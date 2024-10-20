using P42_Allergies;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;

namespace P42_Allergies
{
    public static class Utils
    {
        private static bool StringsInitialized = false;
        public static string Severity_mild = "";
        public static string Severity_moderate = "";
        public static string Severity_severe = "";
        public static string Severity_extreme = "";

        private static Dictionary<ThingDef, List<ThingDef>> CachedRecipeIngredients = new Dictionary<ThingDef, List<ThingDef>>();

        /// <summary>
        /// Returns very generally if an allergy check should be performed for a pawn.
        /// </summary>
        public static bool CheckForAllergies(Pawn pawn)
        {
            if (pawn.IsWorldPawn()) return false;

            return true;
        }

        /// <summary>
        /// Checks if a thing is allergenic for a pawn and the pawn knows it.
        /// </summary>
        public static bool IsKnownAllergenic(Pawn pawn, Thing t)
        {
            foreach (Hediff_Allergy allergyHediff in GetPawnAllergies(pawn))
            {
                if (!allergyHediff.GetAllergy().IsAllergyDiscovered) continue;

                bool isAllergenic = allergyHediff.GetAllergy().IsAllergenic(t, checkFoodIngredients: true, checkStuff: true, checkProductionIngredients: true, checkButcherProducts: true, checkMiningYield: true, checkPlantYields: true);
                if (isAllergenic) return true;
            }
            return false;
        }

        /// <summary>
        /// This method is used in WorkGiver harmony patches for "JobOnThing". If the thing that the pawn would be interacting with for the job is allergenic to them, this will abort the job. Else the original JobOnThing shall be executed.
        /// </summary>
        public static bool CheckIfPawnShouldAvoidJobOnThing(Pawn pawn, Thing t, bool forced, ref Job job)
        {
            if (!forced && Utils.IsKnownAllergenic(pawn, t))
            {
                job = null;
                return false; // Do not execute original function
            }

            return true; // Execute original function
        }

        public static string GetSeverityString(AllergySeverity severity)
        {
            if (!StringsInitialized)
            {
                Severity_mild = "P42_AllergySeverity_Mild".Translate();
                Severity_moderate = "P42_AllergySeverity_Moderate".Translate();
                Severity_severe = "P42_AllergySeverity_Severe".Translate();
                Severity_extreme = "P42_AllergySeverity_Extreme".Translate();
                StringsInitialized = true;
            }

            if (severity == AllergySeverity.Mild) return Severity_mild;
            if (severity == AllergySeverity.Moderate) return Severity_moderate;
            if (severity == AllergySeverity.Severe) return Severity_severe;
            if (severity == AllergySeverity.Extreme) return Severity_extreme;
            return "???";
        }

        public static void ApplyMemoryThought(Pawn pawn, string defName)
        {
            if (pawn == null || pawn.needs == null || pawn.needs.mood == null || pawn.needs.mood.thoughts == null || pawn.needs.mood.thoughts.memories == null) return;

            ThoughtDef thoughtDef = DefDatabase<ThoughtDef>.GetNamedSilentFail(defName);
            if (thoughtDef == null) return;

            pawn.needs?.mood.thoughts.memories.TryGainMemory(thoughtDef);
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
                Logger.Log($"{productDef.label} is made of:{s}", ignore: true);
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
