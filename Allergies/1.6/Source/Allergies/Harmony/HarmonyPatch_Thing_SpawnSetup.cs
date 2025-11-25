using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    [HarmonyPatch(typeof(Thing), "SpawnSetup")]
    public static class HarmonyPatch_Thing_SpawnSetup
    {
        [HarmonyPostfix]
        public static void Postfix(Thing __instance, Map map, bool respawningAfterLoad)
        {
            if (respawningAfterLoad) return;

            if (__instance is Pawn pawn)
            {
                foreach (Hediff_Allergy allergyHediff in Utils.GetPawnAllergies(pawn))
                {
                    allergyHediff.GetAllergy().SetArrivalTickToNow();
                }
            }
        }
    }
}
