using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEssentials
{
    public struct Signatures
    {
        internal string FileErrorString { get; set; }
        internal string UbikPathString { get; set; }
        internal string GetEntityPath { get; set; }
        internal string GetEntityPathX { get; set; }
        internal string Get_VPath { get; set; }
        internal string GetPath2 { get; set; }
        internal string GetPath2X { get; set; }

        // To find the Get_VPath address, look for shader filepath construction string,
        // i.e. "data/shader/aston_shader_%s.par"
        // find whatever function this string is being given to, and go into that function
        // then that function will in turn call another function, usually with 0x410 as its second argument
        // THAT function is what we want the signature for

        // GetPath2 is vsnprintf, you can eventually reach it by following get_vpath

        // GetEntity path can be found by searching for data/entity/ and following xref
        // the function should check for an "all" string before calling this path

        internal static Dictionary<string, Signatures> VersionSigs = new()
        {
            {
                "likeadragon8.exe", // Like a Dragon: Infinite Wealth
                new Signatures
                {
                    FileErrorString = "66 69 6c 65 20 65 72 72 6f 72 20 3c 25 73 3e",
                    UbikPathString = "64 61 74 61 2F 63 68 61 72 61 2F 75 62 69 6B 2F",
                    Get_VPath = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 8B 15 ?? ?? ?? ??",
                    GetPath2 = "48 8B C4 4C 89 48 ?? 89 50 ?? 55 53 56 57 41 54 41 55 41 56 41 57 48 8D A8 ?? ?? ?? ?? 48 81 EC 58 02 00 00",
                    GetPath2X = "48 8B C4 55 53 56 57 41 54 41 55 41 56 41 57 48 8D A8 ?? ?? ?? ?? 48 81 EC 88 02 00 00",
                    GetEntityPath = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 0F B7 F2 48 8B F9",
                    GetEntityPathX = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 0F B7 F2 48 8B F9",
                }
            },
            {
                "likeadragongaiden.exe", // Like a Dragon Gaiden: The Man Who Erased His Name
                new Signatures
                {
                    Get_VPath = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 8B 15 ?? ?? ?? ??",
                    GetPath2 = "48 8B C4 4C 89 48 ?? 89 50 ?? 55 53 56 57 41 54 41 55 41 56 41 57 48 8D A8 ?? ?? ?? ?? 48 81 EC 58 02 00 00",
                    GetPath2X = "48 8B C4 55 53 56 57 41 54 41 55 41 56 41 57 48 8D A8 ?? ?? ?? ?? 48 81 EC 88 02 00 00",
                    GetEntityPath = "48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 20 44 89 C7 48 89 CB",
                    GetEntityPathX = "48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 20 44 89 C7 48 89 CB",
                    UbikPathString = "64 61 74 61 2F 63 68 61 72 61 2F 75 62 69 6B 2F",
                }
            },
            {
                "LostJudgment.exe", // Lost Judgment
                new Signatures
                {
                    Get_VPath = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 8B 15 ?? ?? ?? ??",
                    GetPath2 = "48 8B C4 4C 89 48 ?? 89 50 ?? 55 53 56 57 41 54 41 55 41 56 41 57 48 8D A8 ?? ?? ?? ?? 48 81 EC 58 02 00 00",
                    GetPath2X = "48 8B C4 4C 89 48 ?? 89 50 ?? 55 53 56 57 41 54 41 55 41 56 41 57 48 8D A8 ?? ?? ?? ?? 48 81 EC 58 02 00 00",
                    GetEntityPath = "48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 20 41 8B F8 48 8B D9",
                    GetEntityPathX = "48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 20 41 8B F8 48 8B D9",
                    UbikPathString = "64 61 74 61 2F 63 68 61 72 61 2F 75 62 69 6B 2F",
                }
            },
            {
                "Judgment.exe", // Judge Eyes
                new Signatures
                {
                    Get_VPath = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 8B 15 ?? ?? ?? ??",
                    GetPath2 = "48 8B C4 4C 89 48 ?? 89 50 ?? 55 53 56 57 41 54 41 55 41 56 41 57 48 8D A8 ?? ?? ?? ?? 48 81 EC 58 02 00 00",
                    GetPath2X = "48 8B C4 4C 89 48 ?? 89 50 ?? 55 53 56 57 41 54 41 55 41 56 41 57 48 8D A8 ?? ?? ?? ?? 48 81 EC 58 02 00 00",
                    GetEntityPath = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 0F B7 FA 48 8B F1",
                    GetEntityPathX = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 0F B7 FA 48 8B F1",
                }
            },
            {
                "BinaryDomain.exe", // Binary Domain
                new Signatures
                {
                    Get_VPath = "56 68 04 01 00 00 FF 15 ?? ?? ?? ??",
                    GetPath2 = "55 8B EC 83 E4 F8 81 EC D4 01 00 00 53 89 54 24 ??",
                    GetEntityPath = "48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 20 41 8B F8 48 8B D9",
                }
            },
        };
    }
}
