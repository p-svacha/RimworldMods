using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    [HarmonyPatch(typeof(Thing), "Ingested")]
    public static class HarmonyPatch_ThingIngested
    {
        [HarmonyPostfix]
        public static void Postfix(Thing __instance, Pawn ingester, float nutritionWanted)
        {
            if (!AllergyUtility.CheckForAllergies(ingester)) return;

            foreach (Hediff_Allergy allergyHediff in AllergyUtility.GetPawnAllergies(ingester))
            {
                allergyHediff.GetAllergy().OnThingIngested(__instance);
            }
        }
    }
}
