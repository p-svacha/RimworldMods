using HarmonyLib;
using P42_RealRuinsDynamicWealthCap;
using System.Reflection;
using Verse;

namespace P42_RealRuinsDynamicWealthCap
{
    /// <summary>
    /// This patch modifies the behavior of the RealRuins mod to dynamically adjust the wealth cap for scattered ruins based on the player's total wealth.
    /// </summary>
    [HarmonyPatch]
    public static class ScatterRealRuinsWealthCapPatch
    {
        static MethodBase TargetMethod() =>
            AccessTools.Method(AccessTools.TypeByName("RealRuins.GenStep_ScatterRealRuins"), "Generate");

        static void Prefix()
        {
            float wealth = WealthCalculator.GetTotalPlayerWealth();
            var (cap, floor) = WealthCapCurve.Calculate(wealth, WealthCapCurveConfig.StandardMapRuins);

            if (Prefs.DevMode) Log.Message($"{RealRuinsHelper.LOG_PREFIX}StandardMapRuins: wealth={wealth:F0} cap={cap} floor={floor}");

            RealRuinsHelper.SetDefaultScatterCapAndFloor(cap, floor);
        }
    }

    /// <summary>
    /// This patch modifies the behavior of the RealRuins mod to dynamically adjust the wealth cap for medium scattered ruins (caravan event) based on the player's total wealth.
    /// </summary>
    [HarmonyPatch]
    public static class ScatterMediumRealRuinsWealthCapPatch
    {
        static MethodBase TargetMethod() =>
            AccessTools.Method(AccessTools.TypeByName("RealRuins.GenStep_ScatterMediumRealRuins"), "Generate");

        static void Prefix()
        {
            float wealth = WealthCalculator.GetTotalPlayerWealth();
            var (cap, floor) = WealthCapCurve.Calculate(wealth, WealthCapCurveConfig.CaravanRuins);

            if (Prefs.DevMode) Log.Message($"{RealRuinsHelper.LOG_PREFIX}CaravanRuins: wealth={wealth:F0} cap={cap} floor={floor}");

            RealRuinsHelper.SetDefaultScatterCapAndFloor(cap, floor);
        }
    }
}