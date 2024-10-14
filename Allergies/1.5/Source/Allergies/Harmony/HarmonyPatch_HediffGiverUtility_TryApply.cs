using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace P42_Allergies
{
    [HarmonyPatch(typeof(HediffGiverUtility), "TryApply")]
    public static class HarmonyPatch_HediffGiverUtility_TryApply
    {
        public static bool Prefix(Pawn pawn, HediffDef hediff, ref bool __result)
        {
            if (hediff.defName == "P42_ChronicSinusitis" && pawn.skills == null)
            {
                __result = false;
                return false;
            }

            // Allow the original method to execute
            return true;
        }
    }
}
