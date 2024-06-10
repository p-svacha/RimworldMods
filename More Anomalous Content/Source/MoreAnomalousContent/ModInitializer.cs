using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MoreAnomalousContent
{
    [StaticConstructorOnStartup]
    public static class MoreAnomalousContent_Initializer
    {
        static MoreAnomalousContent_Initializer()
        {
            // Register harmony patches
            new Harmony("rimworld.mod.phil42.moreanomalouscontent").PatchAll();
        }
    }
}
