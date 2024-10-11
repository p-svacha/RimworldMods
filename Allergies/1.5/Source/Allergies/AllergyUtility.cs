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

namespace P42_Allergies
{
    public static class AllergyUtility
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

        public static ThingDef GetRandomIngredient()
        {
            List<ThingDef> candidates = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => IsFoodIngredient(x)).ToList();
            return candidates.RandomElement();
        }
        private static bool IsFoodIngredient(ThingDef def)
        {
            if (!def.IsIngestible) return false;
            if (!def.ingestible.HumanEdible) return false;
            if (def.IsDrug) return false;
            if ((def.ingestible.foodType & FoodTypeFlags.Meal) != 0) return false;
            if (def.IsCorpse) return false;
            return true;
        }

        public static ThingDef GetRandomDrug()
        {
            List<ThingDef> candidates = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => IsDrug(x)).ToList();
            return candidates.RandomElement();
        }
        private static bool IsDrug(ThingDef def)
        {
            if (!def.IsDrug) return false;
            if (def.tradeTags.Contains("Serum")) return false; // exclude serums
            return true;
        }

        public static ThingDef GetRandomMedicine()
        {
            List<ThingDef> candidates = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => IsMedicine(x)).ToList();
            return candidates.RandomElement();
        }
        private static bool IsMedicine(ThingDef def)
        {
            if (!def.IsMedicine) return false;
            return true;
        }

        public static ThingDef GetRandomTextile()
        {
            List<ThingDef> candidates = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => IsTextile(x)).ToList();
            return candidates.RandomElement();
        }
        private static bool IsTextile(ThingDef def)
        {
            if (def.thingCategories == null) return false;
            if (!def.thingCategories.Contains(ThingCategoryDefOf.Textiles)) return false;
            return true;
        }

        public static PawnKindDef GetRandomAnimal()
        {
            List<PawnKindDef> candidates = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => IsAnimal(x)).ToList();
            return candidates.RandomElement();
        }
        private static bool IsAnimal(PawnKindDef def)
        {
            if (def.race == null) return false;
            if (def.race.thingCategories == null) return false;
            if (!def.race.thingCategories.Contains(ThingCategoryDefOf.Animals)) return false;
            if (def.race.tradeTags.Contains("AnimalDryad")) return false; // exclude dryads

            return true;
        }

        public static ThingDef GetRandomPlant()
        {
            List<ThingDef> candidates = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => IsPlant(x)).ToList();
            return candidates.RandomElement();
        }
        private static bool IsPlant(ThingDef def)
        {
            if (def.plant == null) return false;

            return true;
        }

        public static XenotypeDef GetRandomXenotype()
        {
            List<XenotypeDef> candidates = DefDatabase<XenotypeDef>.AllDefsListForReading;
            return candidates.RandomElement();
        }

        private static bool HasStuffProp(ThingDef def, StuffCategoryDef stuffCategory)
        {
            if (def.stuffProps == null) return false;
            if (def.stuffProps.categories == null) return false;
            if (!def.stuffProps.categories.Contains(stuffCategory)) return false;

            return true;
        }
        public static ThingDef GetRandomStone()
        {
            List<ThingDef> candidates = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => HasStuffProp(x, StuffCategoryDefOf.Stony)).ToList();
            return candidates.RandomElement();
        }
        public static ThingDef GetRandomWood()
        {
            List<ThingDef> candidates = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => HasStuffProp(x, StuffCategoryDefOf.Woody)).ToList();
            return candidates.RandomElement();
        }
        public static ThingDef GetRandomMetal()
        {
            List<ThingDef> candidates = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => HasStuffProp(x, StuffCategoryDefOf.Metallic)).ToList();
            return candidates.RandomElement();
        }

    }
}
