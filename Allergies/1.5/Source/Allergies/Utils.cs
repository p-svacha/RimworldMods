﻿using P42_Allergies;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld.Planet;
using UnityEngine;

namespace P42_Allergies
{
    public static class Utils
    {
        private static Dictionary<ThingDef, List<ThingDef>> CachedRecipeIngredients = new Dictionary<ThingDef, List<ThingDef>>();

        /// <summary>
        /// Returns very generally if an allergy check should be performed for a pawn.
        /// </summary>
        public static bool CheckForAllergies(Pawn pawn)
        {
            if (pawn.IsWorldPawn()) return false;

            return true;
        }

        public static List<Hediff_Allergy> GetPawnAllergies(Pawn pawn)
        {
            List<Hediff_Allergy> existingAllergies = new List<Hediff_Allergy>();
            pawn.health.hediffSet.GetHediffs<Hediff_Allergy>(ref existingAllergies);
            return existingAllergies;
        }

        public static float GetAllergicSensitivity(Pawn pawn)
        {
            return pawn.GetStatValue(StatDef.Named("P42_AllergicSensitivity"));
        }

        public static void TriggerAnaphylacticShock(Pawn pawn, float severity, string cause)
        {
            Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("P42_AnaphylacticShock"));
            if (existingHediff == null)
            {
                Hediff newHediff = HediffMaker.MakeHediff(HediffDef.Named("P42_AnaphylacticShock"), pawn);
                newHediff.Severity = severity;
                pawn.health.AddHediff(newHediff);

                Find.LetterStack.ReceiveLetter("LetterHealthComplicationsLabel".Translate(pawn.LabelShort, newHediff.LabelBaseCap, pawn.Named("PAWN")).CapitalizeFirst(), "LetterHealthComplications".Translate(pawn.LabelShortCap, newHediff.LabelBaseCap, cause, pawn.Named("PAWN")).CapitalizeFirst(), LetterDefOf.NegativeEvent, pawn);
            }
        }

        /// <summary>
        /// Returns all non-variable / non-replacable ingredients that are required to produce the given product.
        /// </summary>
        public static List<ThingDef> GetProductionIngredients(ThingDef productDef, bool checkIngredientRecipes = true)
        {
            if (CachedRecipeIngredients.TryGetValue(productDef, out List<ThingDef> ingredients)) return ingredients;


            List<ThingDef> ingredientDefs = new List<ThingDef>();
            foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefsListForReading)
            {
                if (recipe.products != null && recipe.products.Any(product => product.thingDef == productDef))
                {
                    // Iterate through each ingredient in the recipe
                    foreach (IngredientCount ingredient in recipe.ingredients)
                    {
                        // Get the allowed ThingDefs for this ingredient filter
                        List<ThingDef> allowedDefs = ingredient.filter.AllowedThingDefs.ToList();

                        // If there's exactly one allowed ThingDef, it is a "fixed" ingredient
                        if (allowedDefs.Count == 1)
                        {
                            if(!ingredientDefs.Contains(allowedDefs[0]))
                                ingredientDefs.Add(allowedDefs[0]);
                        }
                    }
                }
            }

            // Also check what the ingredients are made out of
            if(checkIngredientRecipes)
            {
                List<ThingDef> secondLayerIngredients = new List<ThingDef>();
                foreach(ThingDef ingredient in ingredientDefs)
                {
                    List<ThingDef> ingredientIngredients = GetProductionIngredients(ingredient, checkIngredientRecipes: false);
                    foreach(ThingDef ingredientIngredient in ingredientIngredients)
                    {
                        if(!ingredientDefs.Contains(ingredientIngredient)) secondLayerIngredients.Add(ingredientIngredient);
                    }
                }
                ingredientDefs.AddRange(secondLayerIngredients);
            }

            // Cache and return
            CachedRecipeIngredients.Add(productDef, ingredientDefs);
            if (Prefs.DevMode)
            {
                string s = "";
                foreach (ThingDef t in ingredientDefs) s += " " + t.label + ",";
                s = s.TrimEnd(',');
                Log.Message($"[Allergies Mod] {productDef.label} is made of:{s}");
            }
            return ingredientDefs;
        }

        public static T GetWeightedRandomElement<T>(Dictionary<T, float> weightDictionary)
        {
            float probabilitySum = weightDictionary.Sum(x => x.Value);
            float rng = UnityEngine.Random.Range(0, probabilitySum);
            float tmpSum = 0;
            foreach (KeyValuePair<T, float> kvp in weightDictionary)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            Log.Error($"[Allergies Mod] ERROR in GetWeightedRandomElement<T>");
            return weightDictionary.Keys.First();
        }
    }
}
