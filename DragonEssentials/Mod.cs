using DragonEssentials.Configuration;
using DragonEssentials.Template;
using DragonEssentials.Interfaces;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using static DragonEssentials.Utils;
using Reloaded.Memory;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;
using Reloaded.Memory.Interfaces;
using System.Net;
using System.Security.Cryptography;

namespace DragonEssentials
{
    /// <summary>
    /// Your mod logic goes here.
    /// </summary>
    public class Mod : ModBase // <= Do not Remove.
    {
        private byte[] NullTermBytes = new byte[] { 0x00, 0x00 };

        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        private readonly IModLoader _modLoader;

        /// <summary>
        /// Provides access to the Reloaded.Hooks API.
        /// </summary>
        /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
        private readonly IReloadedHooks? _hooks;

        /// <summary>
        /// Provides access to the Reloaded logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Entry point into the mod, instance that created this class.
        /// </summary>
        private readonly IMod _owner;

        /// <summary>
        /// Provides access to this mod's configuration.
        /// </summary>
        private Config _configuration;

        internal unsafe delegate nint GetPath1Delegate(nint file_path, uint a2, nint a3, nint a4);
        internal unsafe delegate int GetPath2Delegate(nint file_path, uint a2, nint a3, nint a4);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate IntPtr CreateFileWDelegate( string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile
        );

        /// <summary>
        /// The configuration of the currently executing mod.
        /// </summary>
        private readonly IModConfig _modConfig;
        private Dictionary<string, string> _redirections = new();
        private unsafe IHook<GetPath1Delegate> _getPath1Hook;
        private IHook<CreateFileWDelegate> _createFileWHook;

        private IDragonEssentials _api;
        private string _modsPath;

        public Mod(ModContext context)
        {
            _modLoader = context.ModLoader;
            _hooks = context.Hooks;
            _logger = context.Logger;
            _owner = context.Owner;
            _configuration = context.Configuration;
            _modConfig = context.ModConfig;

            Initialise(_logger, _configuration, _modLoader);

            // Setup mods path
            var modPath = new DirectoryInfo(_modLoader.GetDirectoryForModId(_modConfig.ModId));
            _modsPath = modPath.Parent!.FullName;

            // Get Signatures
            var sigs = GetSignatures();

            unsafe
            {
                // Load files from our mod
                SigScan(sigs.GetPath1, "GetPath1", address =>
                {
                    _getPath1Hook = _hooks.CreateHook<GetPath1Delegate>(GetPath1, address).Activate();
                });

                SigScan(sigs.FileErrorString, "FileErrorString", address =>
                {
                    PatchAddress((nuint)address);
                    LogDebug("Patched Error String");
                });

                IntPtr createFileWAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "CreateFileW");

                // Create the hook
                _createFileWHook = _hooks.CreateHook<CreateFileWDelegate>(CreateFileW, createFileWAddress);
                _createFileWHook.Activate();
            }

            _modLoader.ModLoading += ModLoading;

            // Expose API
            _api = new Api(AddFolder);
            _modLoader.AddOrReplaceController(context.Owner, _api);
        }

        private void ModLoading(IModV1 mod, IModConfigV1 modConfig)
        {
            var modsPath = Path.Combine(_modLoader.GetDirectoryForModId(modConfig.ModId), "DragonEssentials");
            if (!Directory.Exists(modsPath))
                return;

            AddFolder(modsPath);
        }

        private void AddFolder(string folder)
        {
            AddRedirections(folder);
            Log($"Loading files from {folder}");
        }

        private static List<string> languages = new List<string> { "de", "en", "es", "fr", "it", "ja", "ko", "pt", "ru", "zh", "zhs", "pt" };

        private void AddRedirections(string modsPath)
        {
            foreach (var file in Directory.EnumerateFiles(modsPath, "*", SearchOption.AllDirectories))
            {
                var modFilePath = Path.GetRelativePath(modsPath, file);
                var gamePath = Path.Combine(GetGameDirectory(), "data", modFilePath); // recreate what the game would try to load

                _redirections[processLanguageString(gamePath).Replace('\\', '/')] = file.ToLower().Replace('\\', '/');
                _redirections[processLanguageString(gamePath)] = file.ToLower();

                if (gamePath.Contains(".all"))
                {
                    var newPath = string.Empty;

                    foreach (string lang in languages)
                    {
                        newPath = gamePath.Replace(".all", $"\\{lang}").ToLower();

                        _redirections[newPath.Replace('\\', '/')] = file.ToLower().Replace('\\', '/');
                        _redirections[newPath] = file.ToLower();
                    }
                }
            }
        }

        private string processLanguageString( string input )
        {
            foreach (string lang in languages)
            {
                if (input.Contains($".{lang}"))
                {
                    input = input.Replace($".{lang}", $"\\{lang}");
                }
            }
            return input.ToLower();
        }

        private Signatures GetSignatures()
        {
            var fileName = GetExecutableName();

            // Try and find based on file name
            if (Signatures.VersionSigs.TryGetValue(GetExecutableName().ToLower(), out var sigs))
                return sigs;
            else throw new Exception($"Unable to find Signatures for game {fileName}");
        }

        string GetExecutableName()
        {
            var CurrentProcess = Process.GetCurrentProcess();
            var mainModule = CurrentProcess.MainModule;
            return Path.GetFileName(mainModule!.FileName);
        }

        string GetGameDirectory()
        {
            var CurrentProcess = Process.GetCurrentProcess();
            var mainModule = CurrentProcess.MainModule;
            return Path.GetDirectoryName(mainModule!.FileName);
        }

        private bool TryFindLooseFile(string gameFilePath, out string? looseFile)
        {
            return _redirections.TryGetValue(gameFilePath, out looseFile);
        }

        private unsafe nint GetPath1(nint file_path, uint a2, nint a3, nint a4)
        {
            nint result = _getPath1Hook.OriginalFunction(file_path, a2, a3, a4);

            string target_file = Marshal.PtrToStringAnsi(result);

            if (!target_file.Contains("data/")) return result;

            if (_configuration.FileAccessLog) Log($"{target_file}");

            if (!TryFindLooseFile(target_file, out var looseFile)) return result;

            var memory = Memory.Instance;
            memory.SafeWrite((nuint)(file_path + ReplaceFilePathWithMod(file_path, looseFile.ToLower())), NullTermBytes);

            LogDebug($"GetPath1: Redirected file to {looseFile}");

            return result;
        } // currently this hook catches every normal game file EXCEPT dds files such as character model textures

        private IntPtr CreateFileW( string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile)
        {
            // Log or modify the file name
            if (_configuration.FileAccessLog) Log($"File opened: {lpFileName}");

            if (TryFindLooseFile(lpFileName.ToLower(), out var looseFile))
            {
                LogDebug($"CreateFileW: Redirected file to {looseFile}");
                return _createFileWHook.OriginalFunction(looseFile, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
            }

            // Call the original function with the original parameters
            return _createFileWHook.OriginalFunction(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
        }

        unsafe static int ReplaceFilePathWithMod(nint target, string newString)
        {
            var strBuffer = Marshal.StringToHGlobalAnsi(newString);

            Buffer.MemoryCopy((void*)strBuffer, (void*)target, newString.Length + 1, newString.Length + 1);

            Marshal.FreeHGlobal(strBuffer);

            return newString.Length + 1;
        }

        // Required imports for function address retrieval
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #region Standard Overrides
        public override void ConfigurationUpdated(Config configuration)
        {
            // Apply settings from configuration.
            // ... your code here.
            _configuration = configuration;
            _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
        }
        #endregion

        #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Mod() { }
#pragma warning restore CS8618
        #endregion
    }
}