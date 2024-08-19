using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DragonEssentials.Utils;

namespace DragonEssentials
{
    internal unsafe class Helpers
    {
        static string newUBIKPath = Path.Combine(GetGameDirectory().ToLower(), "ubik_redirect/");

        internal static List<string> languages = new List<string> { "de", "en", "es", "fr", "it", "ja", "ko", "pt", "ru", "zh", "zhs", "pt" };
        internal static string processLanguageString(string input)
        {
            foreach (string lang in languages)
            {
                if (input.Contains($".{lang}"))
                {
                    input = input.Replace($".{lang}", $"\\{lang}");
                }
            }

            if (input.Contains("entity_elvis"))
            {
                input = input.Replace("entity_elvis", "entity");
            }
            else if (input.Contains("entity_aston"))
            {
                input = input.Replace("entity_aston", "entity");
            }
            else if (input.Contains("entity_yazawa"))
            {
                input = input.Replace("entity_yazawa", "entity");
            }

            return input.ToLower();
        }

        internal static void GetOriginalUBIKFiles(string modPath)
        {
            LogDebug($"New ubik dir target is {newUBIKPath}");

            if (Directory.Exists(newUBIKPath))
            {
                Directory.Delete(newUBIKPath, true);
                // clear out directory and remake it
            }

            Directory.CreateDirectory(newUBIKPath);

            var originalUBIKFiles = Path.Combine(modPath, "ubik", GetExecutableName().Replace(".exe", ".zlib"));
            DecompressZlibFile(originalUBIKFiles, newUBIKPath);
            LogDebug($"Extracting original UBIK from {originalUBIKFiles} into {newUBIKPath}");
        }

        internal static void CopyUBIKFile(string filePath)
        {
            File.Copy(filePath, Path.Combine(newUBIKPath, Path.GetFileName(filePath)));
            LogDebug($"Copying mod UBIK file from {filePath} to {Path.Combine(newUBIKPath, Path.GetFileName(filePath))}");
        }

        internal static string GetExecutableName()
        {
            var CurrentProcess = Process.GetCurrentProcess();
            var mainModule = CurrentProcess.MainModule;
            return Path.GetFileName(mainModule!.FileName);
        }

        internal static string GetGameDirectory()
        {
            var CurrentProcess = Process.GetCurrentProcess();
            var mainModule = CurrentProcess.MainModule;
            string returnval = Path.GetDirectoryName(mainModule!.FileName);
            // LogDebug($"Game base directory is {returnval}");
            return returnval;
        }

        static void DecompressZlibFile(string zlibFilePath, string outputFolderPath)
        {
            Directory.CreateDirectory(outputFolderPath);

            using (var inputFileStream = new FileStream(zlibFilePath, FileMode.Open, FileAccess.Read))
            using (var decompressionStream = new ZLibStream(inputFileStream, CompressionMode.Decompress))
            using (var tempFileStream = new MemoryStream())
            {
                decompressionStream.CopyTo(tempFileStream);
                tempFileStream.Position = 0;
                new ZipArchive(tempFileStream, ZipArchiveMode.Read).ExtractToDirectory(outputFolderPath);
            }
        }
    }
}
