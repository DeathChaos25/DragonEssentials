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
        internal string GetPath1 { get; set; }
        internal string GetPath2 { get; set; }
        internal string GetPath1X { get; set; }
        // To find the GetPath1 address, look for shader filepath construction string,
        // i.e. "data/shader/aston_shader_%s.par"
        // find whatever function this string is being given to, and go into that function
        // then that function will in turn call another function, usually with 0x410 as its second argument
        // THAT function is what we want the signature for

        internal static Dictionary<string, Signatures> VersionSigs = new()
        {
            {
                "likeadragon8.exe", // Like a Dragon: Infinite Wealth
                new Signatures
                {
                    FileErrorString = "66 69 6c 65 20 65 72 72 6f 72 20 3c 25 73 3e",
                    UbikPathString = "64 61 74 61 2f 63 68 61 72 61 2f 75 62 69 6b 2f 00",
                    GetPath1 = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 8B 15 ?? ?? ?? ??",
                    GetPath2 = "48 8B C4 4C 89 48 ?? 89 50 ?? 55 53 56 57 41 54 41 55 41 56 41 57 48 8D A8 ?? ?? ?? ?? 48 81 EC 58 02 00 00",
                }
            },
            {
                "likeadragongaiden.exe", // Like a Dragon Gaiden: The Man Who Erased His Name
                new Signatures
                {
                    GetPath1 = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 8B 15 ?? ?? ?? ??",
                    GetPath2 = "48 8B C4 4C 89 48 ?? 89 50 ?? 55 53 56 57 41 54 41 55 41 56 41 57 48 8D A8 ?? ?? ?? ?? 48 81 EC 58 02 00 00",
                }
            },
            {
                "LostJudgment.exe", // Lost Judgment
                new Signatures
                {
                    FileErrorString = "66 69 6c 65 20 65 72 72 6f 72 20 3c 25 73 3e",
                    GetPath1 = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 8B 15 ?? ?? ?? ??",
                }
            },
        };
    }
}
