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
    [HarmonyPatch(typeof(TendUtility), "DoTend")]
    public static class HarmonyPatch_DoTend
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn doctor, Pawn patient, Medicine medicine)
        {
            if (!Utils.CheckForAllergies(patient)) return;

            foreach (Hediff_Allergy allergyHediff in Utils.GetPawnAllergies(patient))
            {
                allergyHediff.GetAllergy().OnTendedWith(medicine);
            }
        }
    }
}
