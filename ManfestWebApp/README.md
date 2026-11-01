# ManifestCreate / ManifestCheck – Browser-Version

## Start
  index.html mit Microsoft Edge oder Google Chrome öffnen.
  Alle vier Dateien (index.html, style.css, sha256.js, app.js) müssen
  im gleichen Ordner liegen. Es sind keine Installation und kein Server nötig.

## ManifestCreate
  Verzeichnis wählen, dann "Manifest erstellen und herunterladen".
  Die erzeugte XML-Datei wird über den Browser heruntergeladen.
  Der Name lautet <Verzeichnis>.sha256.<SHA256>.xml.

## ManifestCheck
  XML-Datei und Datenverzeichnis wählen, Prüfung starten.
  Das Ergebnis wird angezeigt; das Protokoll kann heruntergeladen werden.

## Datenschutz
  Die Seite verwendet keine externen Skripte, keine Uploads, keine
  Netzwerkanfragen und keine Analyse-Dienste. Die Dateien bleiben lokal.

## Kompatibilität
  XML-Elemente und Attribute entsprechen dem C#-Modell DocumentManifest,
  ManifestVersion=1, HashAlgorithm=SHA-256, Document mit Index/Name/Hash/Size.
  ManifestCheck verifiziert den SHA-256 über die ORIGINALBYTES der XML-Datei.
  Damit können C#-erzeugte XML-Dateien geprüft werden, ohne dass sie neu
  serialisiert werden müssen. ManifestCreate erzeugt eigene, schema-kompatible
  XML-Bytes; deren SHA-256 wird für den Dateinamen verwendet.
  Die XML-Bytefolge ist nicht garantiert identisch zur Ausgabe des .NET
  XmlSerializer (u.a. Kommentar/Whitespace/Attributreihenfolge).

## Technische Hinweise
  - Rekursive Verzeichnisauswahl über webkitdirectory (Edge/Chrome).
  - SHA-256 blockweise mit eigenständiger JavaScript-Implementierung.
  - Keine Verzeichnisse ohne Dateien im Manifest (wie beim C#-Original).
  - Die Browser-Downloadfunktion schreibt nicht automatisch neben den
    ausgewählten Verzeichnisordner; Speicherort vom Browser abhängig.
  - Die Verzeichnisauswahl erfordert die ausdrückliche Benutzerfreigabe.
  - Browser-Sicherheitsgrenzen verhindern direktes Überschreiben von Dateien.

---

Developed by: Bernhard Simon Bock
Published on GitHub by: Bund der Ingenieure für Wasserwirtschaft, Abfallwirtschaft und Kulturbau ([BWK](https://www.bwk-bund.de/)), Germany
License: MIT
