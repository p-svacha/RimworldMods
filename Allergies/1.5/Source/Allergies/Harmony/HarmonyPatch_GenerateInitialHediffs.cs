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
    [HarmonyPatch(typeof(PawnGenerator), "GenerateInitialHediffs")]
    public static class HarmonyPatch_GenerateInitialHediffs
    {
        private const float ChanceThatAllergyIsVisible = 0.5f;
        private static int NumAppliedAllergies = 0;

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, PawnGenerationRequest request)
        {
            NumAppliedAllergies = 0;
            TryApplyAllergy(pawn);
        }

        private static void TryApplyAllergy(Pawn pawn)
        {
            if (NumAppliedAllergies > AllergyGenerator.MaxAllergiesPerPawn) return;

            if (Rand.Chance(Allergies_Settings.baseAllergyChance))
            {
                NumAppliedAllergies++;
                bool isVisible = Rand.Chance(ChanceThatAllergyIsVisible);
                AllergyGenerator.GenerateAndApplyRandomAllergy(pawn, isVisible);

                // Add another one if unlucky
                TryApplyAllergy(pawn);
            }
        }
    }
}
