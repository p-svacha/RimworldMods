using HarmonyLib;
using P42_RealRuinsDynamicWealthCap;
using Verse;

namespace P42_RealRuinsDynamicWealthCap
{

    /// <summary>
    /// This patch modifies the behavior of the RealRuins mod's IncidentWorker_RuinsFound class to dynamically adjust the cost cap for generating ruins based on the player's total wealth.
    /// </summary>
    [HarmonyPatch(typeof(RealRuins.IncidentWorker_RuinsFound), "TryExecuteWorker")]
    public static class IncidentWorkerRuinsFoundWealthCapPatch
    {
        static void Prefix(out float __state)
        {
            __state = RealRuinsHelper.GetRuinsCostCap();

            float wealth = WealthCalculator.GetTotalPlayerWealth();
            var (cap, _) = WealthCapCurve.Calculate(wealth, WealthCapCurveConfig.AbandonedBaseRuins);

            if (Prefs.DevMode) Log.Message($"{RealRuinsHelper.LOG_PREFIX}AbandonedBaseRuins: wealth={wealth:F0} cap={cap}");

            RealRuinsHelper.SetRuinsCostCap(cap);
        }

        static void Postfix(float __state) => RealRuinsHelper.SetRuinsCostCap(__state);
    }

    /// <summary>
    /// This patch modifies the behavior of the RealRuins mod's QuestNode_GenerateRuinsObject class to dynamically adjust the cost cap for generating ruins based on the player's total wealth.
    /// </summary>
    [HarmonyPatch(typeof(RealRuins.QuestNode_GenerateRuinsObject), "TryGenerateWorldObject")]
    public static class QuestGenerateRuinsWealthCapPatch
    {
        static void Prefix(out float __state)
        {
            __state = RealRuinsHelper.GetRuinsCostCap();

            float wealth = WealthCalculator.GetTotalPlayerWealth();
            var (cap, _) = WealthCapCurve.Calculate(wealth, WealthCapCurveConfig.AbandonedBaseRuins);

            if (Prefs.DevMode) Log.Message($"{RealRuinsHelper.LOG_PREFIX}AbandonedBaseRuins: wealth={wealth:F0} cap={cap}");

            RealRuinsHelper.SetRuinsCostCap(cap);
        }

        static void Postfix(float __state) => RealRuinsHelper.SetRuinsCostCap(__state);
    }
}