using HarmonyLib;
using Verse;

namespace P42_RealRuinsDynamicWealthCap
{
    [StaticConstructorOnStartup]
    public static class ModInitializer
    {
        static ModInitializer()
        {
            new Harmony("rimworld.mod.phil42.realruinsdynamicwealthcap").PatchAll();

            // Validate base mod "API"
            RealRuinsHelper.ValidateOrLogError();
        }
    }
}