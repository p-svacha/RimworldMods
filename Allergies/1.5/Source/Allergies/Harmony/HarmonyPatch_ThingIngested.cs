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
    [HarmonyPatch(typeof(Thing), "Ingested")]
    public class HarmonyPatch_ThingIngested
    {
        [HarmonyPostfix]
        public static void Postfix(Thing __instance, Pawn ingester, float nutritionWanted)
        {
            List<Hediff_Allergy> allergies = AllergyUtility.GetPawnAllergies(ingester);
            foreach(Hediff_Allergy allergyHediff in allergies)
            {
                Allergy allergy = allergyHediff.GetAllergy();

                // Food type allergy
                if(allergy is FoodTypeAllergy foodTypeAllergy)
                {
                    Func<ThingDef, bool> identifier = foodTypeAllergy.GetIdentifier();
                    CheckBuildup(foodTypeAllergy, identifier, __instance);
                }

                // Ingredient allergy
                if (allergy is IngredientAllergy ingredientAllergy)
                {
                    CheckBuildup(ingredientAllergy, ingredientAllergy.IsIngredient, __instance);
                }

                // Drug allergy
                if(allergy is DrugAllergy drugAllergy)
                {
                    if (drugAllergy.IsDrug(__instance.def)) // ingested drug => strong or extreme exposure event
                    {
                        float buildUpValue;

                        if (Rand.Chance(0.5f)) buildUpValue = Allergy.ExtremeExposureEventIncrease;
                        else buildUpValue = Allergy.StrongExposureEventIncrease;

                        allergy.IncreaseAllergenBuildup(buildUpValue, "P42_AllergyCause_Ingested".Translate(__instance.Label));
                    }
                }
            }
        }
        
        private static void CheckBuildup(Allergy allergy, Func<ThingDef, bool> identifier, Thing thing)
        {
            if (identifier(thing.def)) // ingested item => extreme exposure event
            {
                float buildup = Allergy.ExtremeExposureEventIncrease;
                allergy.IncreaseAllergenBuildup(buildup, "P42_AllergyCause_Ingested".Translate(thing.Label));
                return;
            }

            if (thing.TryGetComp<CompIngredients>() != null)
            {
                foreach (ThingDef ingredient in thing.TryGetComp<CompIngredients>().ingredients)
                {
                    if (identifier(ingredient)) // ingested something with item as ingredient => strong exposure event
                    {
                        float buildup = Allergy.StrongExposureEventIncrease;
                        allergy.IncreaseAllergenBuildup(buildup, "P42_AllergyCause_IngestedIngredient".Translate(thing.Label, ingredient.label));
                    }
                }
            }
        }
    }
}
