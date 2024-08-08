using DragonEssentials.Configuration;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Reloaded.Memory;
using Reloaded.Memory.Interfaces;

namespace DragonEssentials
{
    internal unsafe class Utils
    {
        private static ILogger _logger;
        private static Config _config;
        private static IStartupScanner _startupScanner;
        internal static nint BaseAddress { get; private set; }

        internal static bool Initialise(ILogger logger, Config config, IModLoader modLoader)
        {
            _logger = logger;
            _config = config;
            using var thisProcess = Process.GetCurrentProcess();
            BaseAddress = thisProcess.MainModule!.BaseAddress;

            var startupScannerController = modLoader.GetController<IStartupScanner>();
            if (startupScannerController == null || !startupScannerController.TryGetTarget(out _startupScanner))
            {
                LogError($"Unable to get controller for Reloaded SigScan Library, stuff won't work :(");
                return false;
            }

            return true;
        }

        internal static void LogDebug(string message)
        {
            if (_config.DebugEnabled)
                _logger.WriteLineAsync($"[Dragon Essentials] {message}");
        }

        internal static void Log(string message)
        {
            _logger.WriteLineAsync($"[Dragon Essentials] {message}");
        }

        internal static void LogError(string message, Exception e)
        {
            _logger.WriteLineAsync($"[Dragon Essentials] {message}: {e.Message}", System.Drawing.Color.Red);
        }

        internal static void LogError(string message)
        {
            _logger.WriteLineAsync($"[Dragon Essentials] {message}", System.Drawing.Color.Red);
        }

        internal static void SigScan(string pattern, string name, Action<nint> action)
        {
            if (pattern != null)
            {
                _startupScanner.AddMainModuleScan(pattern, result =>
                {
                    if (!result.Found)
                    {
                        LogError($"Unable to find {name}, stuff won't work :(");
                        return;
                    }
                    LogDebug($"Found {name} at 0x{result.Offset + BaseAddress:X}");

                    action(result.Offset + BaseAddress);
                });
            }
            else
            {
                LogError($"{name} doesn't exist, stuff won't work :(");
            }
        }

        /// <summary>
        /// Gets the address of a global from something that references it
        /// </summary>
        /// <param name="ptrAddress">The address to the pointer to the global (like in a mov instruction or something)</param>
        /// <returns>The address of the global</returns>
        internal static unsafe nuint GetGlobalAddress(nint ptrAddress)
        {
            return (nuint)((*(int*)ptrAddress) + ptrAddress + 4);
        }

        internal static void PatchAddress(nuint adr)
        {
            var memory = Memory.Instance;
            byte[] bytes = new byte[] { 0x25, 0x73, 0x00, 0x00 };
            memory.SafeWrite(adr, bytes);
        }
    }
}
