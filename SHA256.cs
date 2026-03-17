// SHA256.cs, C# 9.0 with top-level statements
// https://github.com/bwk-germany, 2026

string FileName = "test.txt"; // Define the file name
File.WriteAllText(FileName, "123"); // Create or overwrite the file with "123"

// Compute and print the SHA256 hash of the file content along with the file name:
Console.WriteLine(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(FileName))).ToLower()+" "+FileName); 

