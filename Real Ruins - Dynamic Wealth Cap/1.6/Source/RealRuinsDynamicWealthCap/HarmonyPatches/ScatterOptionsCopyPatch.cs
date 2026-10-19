using HarmonyLib;
using System.Reflection;

namespace P42_RealRuinsDynamicWealthCap
{
    /// <summary>
    /// This patch ensures that when the RealRuins ScatterOptions.Copy method is called, the costCap field is copied correctly to the new instance.
    /// </summary>
    [HarmonyPatch]
    public static class ScatterOptionsCopyPatch
    {
        static MethodBase TargetMethod() =>
            AccessTools.Method(AccessTools.TypeByName("RealRuins.ScatterOptions"), "Copy");

        static void Postfix(object __instance, object __result)
        {
            var t = __instance.GetType();
            var costCapField = AccessTools.Field(t, "costCap");
            costCapField.SetValue(__result, costCapField.GetValue(__instance));
        }
    }
}