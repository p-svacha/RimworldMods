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
    public static class GeneratePawn_Patch
    {
        private const float BaseAllergyChance = 1f; // TODO: 0.1f

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, PawnGenerationRequest request)
        {
            // Chance to add allergy
            if (Rand.Chance(BaseAllergyChance))
            {
                AllergyGenerator.GenerateAndApplyRandomAllergy(pawn);
            }
        }
    }
}
