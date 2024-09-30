using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    [StaticConstructorOnStartup]
    public static class ModInitializer
    {
        static ModInitializer()
        {
            // Register harmony patches
            new Harmony("rimworld.mod.phil42.allergies").PatchAll();
        }
    }
}
