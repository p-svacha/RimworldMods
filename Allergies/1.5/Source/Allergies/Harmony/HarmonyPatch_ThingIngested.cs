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
    public static class HarmonyPatch_ThingIngested
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
                        string cause = "P42_AllergyCause_Ingested".Translate(__instance.Label);

                        if (Rand.Chance(0.5f)) allergy.ExtremeExposureEvent(cause);
                        else allergy.StrongExposureEvent(cause);
                    }
                }
            }
        }
        
        private static void CheckBuildup(Allergy allergy, Func<ThingDef, bool> identifier, Thing thing)
        {
            if (identifier(thing.def)) // ingested item => extreme exposure event
            {
                allergy.ExtremeExposureEvent("P42_AllergyCause_Ingested".Translate(thing.Label));
                return;
            }

            if (thing.TryGetComp<CompIngredients>() != null)
            {
                foreach (ThingDef ingredient in thing.TryGetComp<CompIngredients>().ingredients)
                {
                    if (identifier(ingredient)) // ingested something with item as ingredient => strong exposure event
                    {
                        allergy.StrongExposureEvent("P42_AllergyCause_IngestedIngredient".Translate(thing.Label, ingredient.label));
                    }
                }
            }
        }
    }
}
