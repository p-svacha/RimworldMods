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

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, PawnGenerationRequest request)
        {
            // No allergies for pawns with 0 allergic sensitivity
            if (Utils.GetAllergicSensitivity(pawn) <= 0f) return;

            // Get base allergy chance
            float allergyChance = Allergies_Settings.baseAllergyChance;

            // Apply any amount of allergies based on chance
            int appliedCount = 0;
            int safetyBreak = 0;

            while (Rand.Chance(allergyChance) && appliedCount < AllergyGenerator.MaxAllergiesPerPawn && safetyBreak <= 5)
            {
                bool isVisible = Rand.Chance(ChanceThatAllergyIsVisible);
                AllergyGenerator.GenerateAndApplyRandomAllergy(pawn, isVisible);

                appliedCount++;
                safetyBreak++; 
            }
        }
    }
}
