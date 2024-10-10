using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;

namespace P42_Allergies
{
    [HarmonyPatch(typeof(Pawn), "PostApplyDamage")]
    public static class HarmonyPatch_Pawn_PostApplyDamage
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance, DamageInfo dinfo, float totalDamageDealt)
        {
            if (!AllergyUtility.CheckForAllergies(__instance)) return;

            foreach (Hediff_Allergy allergyHediff in AllergyUtility.GetPawnAllergies(__instance))
            {
                allergyHediff.GetAllergy().OnDamageTaken(dinfo);
            }
        }
    }
}
