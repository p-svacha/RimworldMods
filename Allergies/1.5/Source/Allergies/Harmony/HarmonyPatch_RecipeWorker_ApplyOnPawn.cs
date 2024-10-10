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
    [HarmonyPatch(typeof(RecipeWorker), "ApplyOnPawn")]
    public static class HarmonyPatch_RecipeWorker_ApplyOnPawn
    {
        public static void Postfix(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (!AllergyUtility.CheckForAllergies(pawn)) return;

            foreach (Hediff_Allergy allergyHediff in AllergyUtility.GetPawnAllergies(pawn))
            {
                foreach (Thing ingredient in ingredients)
                {
                    allergyHediff.GetAllergy().OnRecipeApplied(ingredient);
                }
            }
        }
    }
}
