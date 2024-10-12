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
        private const float BaseAllergyChance = 0.4f; // TODO: 0.15f
        private const float ChanceThatAllergyIsVisible = 0.5f;

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, PawnGenerationRequest request)
        {
            TryApplyAllergy(pawn);
        }

        private static void TryApplyAllergy(Pawn pawn)
        {
            if (Rand.Chance(BaseAllergyChance))
            {
                bool isVisible = Rand.Chance(ChanceThatAllergyIsVisible);
                AllergyGenerator.GenerateAndApplyRandomAllergy(pawn, isVisible);

                // Add another one if unlucky
                TryApplyAllergy(pawn);
            }
        }
    }
}
