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
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (!Utils.CheckForAllergies(pawn)) return;

            foreach (Hediff_Allergy allergyHediff in Utils.GetPawnAllergies(pawn))
            {
                foreach (Thing ingredient in ingredients)
                {
                    allergyHediff.GetAllergy().OnRecipeApplied(ingredient);
                }
            }
        }
    }
}
