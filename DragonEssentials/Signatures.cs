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
        internal string GetPath1 { get; set; }
        internal string GetPath1X { get; set; }

        internal static Dictionary<string, Signatures> VersionSigs = new()
        {
            {
                "likeadragon8.exe", // Like a Dragon: Infinite Wealth
                new Signatures
                {
                    FileErrorString = "66 69 6c 65 20 65 72 72 6f 72 20 3c 25 73 3e",
                    GetPath1 = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 8B 15 ?? ?? ?? ??",
                }
            },
            {
                "likeadragongaiden.exe", // Like a Dragon Gaiden: The Man Who Erased His Name
                new Signatures
                {
                    FileErrorString = "66 69 6c 65 20 65 72 72 6f 72 20 3c 25 73 3e",
                    GetPath1 = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 8B 15 ?? ?? ?? ??",
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
