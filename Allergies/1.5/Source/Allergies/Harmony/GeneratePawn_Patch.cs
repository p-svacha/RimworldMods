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
        private const float BASE_ALLERGY_CHANCE = 1f;

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, PawnGenerationRequest request)
        {
            // Chance to add allergy
            if (Rand.Chance(BASE_ALLERGY_CHANCE))
            {
                Log.Message($"[Allergies Mod] Adding allergy.");
                Hediff allergyHediff = HediffMaker.MakeHediff(HediffDef.Named("P42_AllergyHediff"), pawn);
                allergyHediff.Severity = 0.05f;
                pawn.health.AddHediff(allergyHediff);
            }
        }
    }
}
