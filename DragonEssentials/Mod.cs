using DragonEssentials.Configuration;
using DragonEssentials.Template;
using DragonEssentials.Interfaces;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using static DragonEssentials.Utils;
using static DragonEssentials.Helpers;
using Reloaded.Memory;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;
using Reloaded.Memory.Interfaces;
using CriFs.V2.Hook.Interfaces;

namespace DragonEssentials
{
    /// <summary>
    /// Your mod logic goes here.
    /// </summary>
    public class Mod : ModBase, IExports // <= Do not Remove.
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

        internal unsafe delegate int GetPath2Delegate(nint file_path, uint a2, nint a3, nint a4);
        internal unsafe delegate int GetEntityPathDelegate(nint file_path, uint e_kind, uint stage_id, uint daynight, nint uid);
        internal bool hasExtractedUBIK = false;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate IntPtr CreateFileWDelegate(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        /// <summary>
        /// The configuration of the currently executing mod.
        /// </summary>
        private readonly IModConfig _modConfig;
        private Dictionary<string, string> _redirections = new();
        private Dictionary<string, string> _redirectionsShort = new();
        private Dictionary<string, string> _redirectionsFull = new();
        private unsafe IHook<GetPath2Delegate> _getPath2Hook;
        private unsafe IHook<GetEntityPathDelegate> _getEntityPathHook;
        private IHook<CreateFileWDelegate> _createFileWHook;

        private IDragonEssentials _api;

        public Mod(ModContext context)
        {
            _modLoader = context.ModLoader;
            _hooks = context.Hooks;
            _logger = context.Logger;
            _owner = context.Owner;
            _configuration = context.Configuration;
            _modConfig = context.ModConfig;

            Initialise(_logger, _configuration, _modLoader);

            // Get Signatures
            var sigs = GetSignatures();

            unsafe
            {
                SigScan(_configuration.isGamePass ? sigs.GetPath2X : sigs.GetPath2, "GetPath2", address =>
                {
                    _getPath2Hook = _hooks.CreateHook<GetPath2Delegate>(GetPath2, address).Activate();
                });

                SigScan(_configuration.isGamePass ? sigs.GetEntityPathX : sigs.GetEntityPath, "GetEntityPath", address =>
                {
                    _getEntityPathHook = _hooks.CreateHook<GetEntityPathDelegate>(GetEntityPath, address).Activate();
                });

                SigScan(sigs.FileErrorString, "FileErrorString", address =>
                {
                    byte[] bytes = new byte[] { 0x25, 0x73, 0x00, 0x00 };
                    var memory = Memory.Instance;
                    memory.SafeWrite((nuint)address, bytes);
                });

                SigScan(sigs.UbikPathString, "UbikPathString", address =>
                {
                    byte[] NewUBIKHex = new byte[] { 0x75, 0x62, 0x69, 0x6B, 0x5F, 0x72, 0x65, 0x64, 0x69, 0x72, 0x65, 0x63, 0x74, 0x2F, 0x00, 0x00 };
                    var memory = Memory.Instance;
                    memory.SafeWrite((nuint)address, NewUBIKHex);
                });
            }

            _modLoader.ModLoading += ModLoading;

            // Expose API
            _api = new Api(AddFolder);
            _modLoader.AddOrReplaceController(context.Owner, _api);

            var criFsController = _modLoader.GetController<ICriFsRedirectorApi>();
            if (criFsController == null || !criFsController.TryGetTarget(out var criFsApi))
            {
                LogError("Unable to load CriFS V2 Library Hooks!");
                return;
            }
            else criFsApi.AddProbingPath("Criware");

            // path fixing and USM redirection
            IntPtr createFileWAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "CreateFileW");
            _createFileWHook = _hooks.CreateHook<CreateFileWDelegate>(CreateFileW, createFileWAddress).Activate();
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
            Log($"Loading mod files from {folder}");

            AddRedirections(folder);
        }

        private void AddRedirections(string modsPath)
        {
            var modPath = new DirectoryInfo(_modLoader.GetDirectoryForModId(_modConfig.ModId));
            if (!hasExtractedUBIK)
            {
                GetOriginalUBIKFiles(modPath.FullName);
                hasExtractedUBIK = true;
            }

            foreach (var file in Directory.EnumerateFiles(modsPath, "*", SearchOption.AllDirectories))
            {
                if (Path.GetExtension(file).Equals(".ubik", StringComparison.OrdinalIgnoreCase))
                {
                    CopyUBIKFile(file);
                    continue;
                }

                var modFilePath = Path.GetRelativePath(modsPath, file).ToLower();
                var gamePath = Path.Combine(GetGameDirectory(), "data", modFilePath).ToLower(); // recreate what the game would try to load
                var localPath = Path.Combine("data", modFilePath).ToLower(); // recreate what the game would try to load

                if (gamePath.EndsWith(".usm"))
                {
                    LogDebug($"Adding usm {file.ToLower()} as {localPath}");
                    _redirectionsFull[gamePath] = file.ToLower();
                    continue;
                }

                LogDebug($"Adding {file.ToLower()} as {localPath}");

                _redirectionsShort[processLanguageString(localPath).Replace('\\', '/')] = file.ToLower().Replace('\\', '/');
                _redirectionsShort[processLanguageString(gamePath).Replace('\\', '/')] = file.ToLower().Replace('\\', '/');

                if (localPath.Contains(".all"))
                {
                    var newPath = string.Empty;

                    foreach (string lang in languages)
                    {
                        newPath = localPath.Replace(".all", $"\\{lang}").ToLower();

                        _redirectionsShort[newPath.Replace('\\', '/')] = file.ToLower().Replace('\\', '/');
                    }
                }
            }
        }

        private bool TryFindLooseFile(string gameFilePath, out string? looseFile)
        {
            return _redirections.TryGetValue(gameFilePath, out looseFile);
        }

        private bool TryFindLooseFileShort(string gameFilePath, out string? looseFile)
        {
            return _redirectionsShort.TryGetValue(gameFilePath, out looseFile);
        }

        private bool TryFindLooseFileFull(string gameFilePath, out string? looseFile)
        {
            return _redirectionsFull.TryGetValue(gameFilePath, out looseFile);
        }

        private Signatures GetSignatures()
        {
            var fileName = GetExecutableName();

            // Try and find based on file name
            if (Signatures.VersionSigs.TryGetValue(GetExecutableName().ToLower(), out var sigs))
                return sigs;
            else throw new Exception($"Unable to find Signatures for game {fileName}");
        }

        private unsafe int GetPath2(nint file_path, uint a2, nint a3, nint a4)
        {
            int result = _getPath2Hook.OriginalFunction(file_path, a2, a3, a4);

            string target_file = Marshal.PtrToStringAnsi(file_path);

            if (!target_file.Contains("data/")) return result;

            LogAccess($"{target_file}");

            if (!TryFindLooseFileShort(target_file, out var looseFile)) return result;

            LogRedirect($"GetPath2: Redirected file to {looseFile}");

            return ReplaceFilePathWithMod(file_path, looseFile.ToLower());
        }

        private unsafe int GetEntityPath(nint file_path, uint e_kind, uint stage_id, uint daynight, nint uid)
        {
            int result = _getEntityPathHook.OriginalFunction(file_path, e_kind, stage_id, daynight, uid);

            string target_file = Marshal.PtrToStringAnsi(file_path);

            LogAccess($"{target_file}");

            if (!TryFindLooseFileShort(target_file, out var looseFile)) return result;

            LogRedirect($"GetEntityPath: Redirected file to {looseFile}");

            var memory = Memory.Instance;
            memory.SafeWrite((nuint)(file_path + ReplaceFilePathWithMod(file_path, looseFile.ToLower())), NullTermBytes);

            return 0;
        }

        private IntPtr CreateFileW( string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile)
        {
            lpFileName = lpFileName.ToLower();

            // currently this only handles USM redirection or path fixing
            if (TryFindLooseFileFull(lpFileName, out var looseFile))
            {
                LogRedirect($"CreateFileW: Redirected file to {looseFile}");
                return _createFileWHook.OriginalFunction(looseFile, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
            }

            if (lpFileName.Contains("dragonessentials"))
            {
                var targetFile = GetGameDirectory().ToLower().Replace('\\', '/') + '/';

                lpFileName = lpFileName.Replace('\\', '/').Replace(targetFile, "").ToLower();
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
        public Type[] GetTypes() => new[] { typeof(IDragonEssentials) };

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Mod() { }
#pragma warning restore CS8618
        #endregion
    }
}