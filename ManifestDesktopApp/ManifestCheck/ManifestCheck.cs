// ManifestCheck.cs, C# 9.0 with Top-level statements
// ManifestCreate / ManifestCheck
// Copyright (c) 2026 Bernhard Simon Bock
// SPDX-License-Identifier: MIT


using System.Security.Cryptography; 
using System.Xml;
using System.Xml.Serialization;

using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

if (args.Length < 2) {MessageBox.Show("Aufruf: ManifestCheck.exe <ManifestFileName> <DataPath>  oder  ManifestCheck.exe <DataPath>  <ManifestFileName>","ManifestCheck",MessageBoxButtons.OK,MessageBoxIcon.Error);return;}

string? manifestFileName = null;
string? dataPath = null;

foreach (string arg in args)
        {if (     File.Exists(arg)) manifestFileName = arg;
    else if (Directory.Exists(arg)) dataPath         = arg;
        }

if (manifestFileName == null || dataPath == null) {MessageBox.Show("Fehler: Bitte genau eine Manifestdatei und ein Verzeichnis übergeben.","ManifestCheck",MessageBoxButtons.OK,MessageBoxIcon.Error);return;}

ManifestCheck(manifestFileName, dataPath);

static void ManifestCheck(string manifestFileName, string dataPath)
{
    string directory = Path.GetDirectoryName(manifestFileName)!;
    string nameWithoutExtension = Path.GetFileNameWithoutExtension(manifestFileName);
    string logFile = Path.Combine(directory, nameWithoutExtension + ".check.log");


    List<string> log = new();

    log.Add("Manifest check");
    log.Add($"Date/Time    : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    log.Add($"Manifest file: {Path.GetFileName(manifestFileName)}");
    log.Add($"Data path    : {new DirectoryInfo(dataPath).Name}");
    log.Add("");

    bool ok = true;

    // 1. Grundprüfung Dateiformat, XML-Inhalt, Version, Dateiname
    log.Add("1. Format, XML content, version and file name");

    if (!     File.Exists(manifestFileName)) {MessageBox.Show("Fehler: Manifest file does not exist.","ManifestCheck",MessageBoxButtons.OK,MessageBoxIcon.Error);return;}
    if (!Directory.Exists(dataPath        )) {MessageBox.Show("Fehler: Data path does not exist."    ,"ManifestCheck",MessageBoxButtons.OK,MessageBoxIcon.Error);return;}

    string fileNameOnly = Path.GetFileName(manifestFileName);

    Match match = Regex.Match(fileNameOnly,@"^(.+)\.sha256\.([a-fA-F0-9]{64})\.xml$");

    string? sha256FromFileName = null;

    if (!match.Success) {ok = false;log.Add("ERROR: File name does not match pattern '<name>.sha256.<64 hex chars>.xml'.");}
    else                {sha256FromFileName = match.Groups[2].Value.ToLowerInvariant();log.Add("OK: File name contains SHA-256 checksum.");}

    DocumentManifest? manifest = null;

    try
    {
        XmlSerializer serializer = new(typeof(DocumentManifest));

        using FileStream stream = File.OpenRead(manifestFileName);

        manifest = (DocumentManifest?)serializer.Deserialize(stream);

             if (manifest == null){ok = false;log.Add("ERROR: XML could not be deserialized.");}
        else if (manifest.ManifestVersion != 1) {ok = false;log.Add($"ERROR: Unsupported manifest version: {manifest.ManifestVersion}");}
        else if (!string.Equals(manifest.HashAlgorithm, "SHA-256", StringComparison.OrdinalIgnoreCase)) {ok = false;log.Add($"ERROR: Unsupported hash algorithm: {manifest.HashAlgorithm}");}
        else    {log.Add("OK: XML content, manifest version and hash algorithm are valid.");}
    }
    catch (Exception ex) {ok = false;log.Add("ERROR: XML deserialization failed.");log.Add(ex.Message);}

    log.Add("");

    if (manifest == null)
    {
        WriteLog(logFile, log);
        return;
    }

    // 2. SHA-256 der Manifestdatei gegen Dateinamen prüfen
    log.Add("2. Manifest checksum from file name");

    if (sha256FromFileName != null)
    {
        string actualManifestHash = GetSha256(manifestFileName);

        if (actualManifestHash == sha256FromFileName)
        {
            log.Add("OK: SHA-256 checksum in file name matches manifest file content.");
        }
        else
        {
            ok = false;
            log.Add("ERROR: SHA-256 checksum in file name does not match manifest file content.");
            log.Add($"Expected: {sha256FromFileName}");
            log.Add($"Actual  : {actualManifestHash}");
        }
    }

    log.Add("");

    // Manifest-Dateien nach Name indizieren
    Dictionary<string, DocumentEntry> manifestFiles = manifest.Document
        .ToDictionary(d => NormalizeRelativePath(d.Name), StringComparer.OrdinalIgnoreCase);

    // Tatsächliche Dateien im Datenverzeichnis ermitteln
    Dictionary<string, string> realFiles = Directory
        .EnumerateFiles(dataPath, "*", SearchOption.AllDirectories)
        .ToDictionary(
            f => NormalizeRelativePath(Path.GetRelativePath(dataPath,f)),
            f => f,
            StringComparer.OrdinalIgnoreCase);


    log.Add("3. File existence");

    foreach (string name in manifestFiles.Keys)
    {
        if (!realFiles.ContainsKey(name))
        {
            ok = false;
            log.Add($"ERROR: Missing file: {name}");
        }
    }

    foreach (string name in realFiles.Keys) {if (!manifestFiles.ContainsKey(name)) {ok = false;log.Add($"ERROR: Extra file not listed in manifest: {name}");}}

    if (manifestFiles.Keys.All(realFiles.ContainsKey) && realFiles.Keys.All(manifestFiles.ContainsKey)) {log.Add("OK: All files exist and no extra files were found.");}

    log.Add("");//-----------------------------------------------------------------------------------------------
    log.Add("4. File size");
    bool sizeOk = true;

    foreach (DocumentEntry entry in manifest.Document)
            {string name = NormalizeRelativePath(entry.Name);
             if (!realFiles.TryGetValue(name, out string? file)) continue;
             long actualSize = new FileInfo(file).Length;
             if (actualSize != entry.Size)
                {ok = false;
                 sizeOk = false;
                 log.Add($"ERROR: Size mismatch: {entry.Name}");
                 log.Add($"Expected: {entry.Size}");
                 log.Add($"Actual  : {actualSize}");
                }
            }

    if (sizeOk) log.Add("OK: All file sizes match.");

    log.Add("");//-----------------------------------------------------------------------------------------------
    log.Add("5. File SHA-256 checksums");

    bool hashOk = true;

    foreach (DocumentEntry entry in manifest.Document)
    {
        string name = NormalizeRelativePath(entry.Name);

        if (!realFiles.TryGetValue(name, out string? file))
            continue;

        string actualHash = GetSha256(file);
        string expectedHash = entry.Hash.ToLowerInvariant();

        if (actualHash != expectedHash)
        {
            ok = false;
            hashOk = false;
            log.Add($"ERROR: SHA-256 mismatch: {entry.Name}");
            log.Add($"Expected: {expectedHash}");
            log.Add($"Actual  : {actualHash}");
        }
    }

    if (hashOk)
        log.Add("OK: All SHA-256 checksums match.");

    log.Add("");//-----------------------------------------------------------------------------------------------
    log.Add("6. Result");

    if (ok)  log.Add("OK: Manifest check successful.");
    else     log.Add("ERROR: Manifest check failed.");

    WriteLog(logFile, log);

if (ok) MessageBoxEx.Show("Der Verzeichnisinhalt von\r\n\r\n"+(new DirectoryInfo(dataPath).Name)+"\r\n\r\nstimmt mit der Manifestdatei\r\n\r\n"+Path.GetFileName(manifestFileName)+"\r\n\r\nüberein.\r\nSiehe Log-Datei\r\n\r\n"+Path.GetFileName(logFile),"ManifestCheck",800,300,MessageBoxIcon.Information);//MessageBoxButtons.OK,MessageBoxIcon.Asterisk);
else    MessageBoxEx.Show("Es gibt Abweichungen zwischen dem Verzeichnisinhalt von\r\n\r\n"+(new DirectoryInfo(dataPath).Name)+"\r\n\r\nund der Manifestdatei\r\n\r\n"+Path.GetFileName(manifestFileName)+"\r\n\r\nSiehe Log-Datei\r\n\r\n"+Path.GetFileName(logFile),"ManifestCheck",800,300,MessageBoxIcon.Error);
}

static string GetSha256(string fileName) {using FileStream stream = File.OpenRead(fileName); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();}
static string NormalizeRelativePath(string path){return path.Replace('\\', '/').Trim();}

static void WriteLog(string logFile, List<string> log)
{

    File.WriteAllLines(logFile, log, Encoding.UTF8);

  //  Console.WriteLine($"Log written to: {logFile}");

}

