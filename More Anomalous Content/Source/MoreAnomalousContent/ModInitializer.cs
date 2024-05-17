using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MoreAnomalousContent
{
    [StaticConstructorOnStartup]
    public static class YourModInitializer
    {
        static YourModInitializer()
        {
            var harmony = new Harmony("rimworld.mod.phil42.moreanomalouscontent");
            harmony.PatchAll();
        }
    }
}
