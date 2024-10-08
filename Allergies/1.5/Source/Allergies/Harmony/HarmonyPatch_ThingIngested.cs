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
            if (!AllergyUtility.CheckForAllergies(ingester)) return;

            List<Hediff_Allergy> allergies = AllergyUtility.GetPawnAllergies(ingester);
            foreach(Hediff_Allergy allergyHediff in allergies)
            {
                Allergy allergy = allergyHediff.GetAllergy();

                // Food type allergy
                if(allergy is FoodTypeAllergy foodTypeAllergy)
                {
                    Func<ThingDef, bool> isAllergenic = foodTypeAllergy.GetIdentifier();
                    foodTypeAllergy.CheckItemIfAllergenicAndApplyBuildup(__instance, isAllergenic, "P42_AllergyCause_Ingested",
                        directExposure: ExposureType.ExtremeEvent,
                        ingredientExposure: ExposureType.StrongEvent,
                        stuffExposure: ExposureType.StrongEvent,
                        productionIngredientExposure: ExposureType.StrongEvent);
                }

                // Ingredient allergy
                if (allergy is IngredientAllergy ingredientAllergy)
                {
                    ingredientAllergy.CheckItemIfAllergenicAndApplyBuildup(__instance, ingredientAllergy.IsIngredient, "P42_AllergyCause_Ingested",
                        directExposure: ExposureType.ExtremeEvent,
                        ingredientExposure: ExposureType.StrongEvent,
                        stuffExposure: ExposureType.StrongEvent,
                        productionIngredientExposure: ExposureType.StrongEvent);
                }

                // Drug allergy
                if(allergy is DrugAllergy drugAllergy)
                {
                    drugAllergy.CheckItemIfAllergenicAndApplyBuildup(__instance, drugAllergy.IsDrug, "P42_AllergyCause_Ingested",
                        directExposure: ExposureType.ExtremeEvent,
                        ingredientExposure: ExposureType.StrongEvent,
                        stuffExposure: ExposureType.StrongEvent,
                        productionIngredientExposure: ExposureType.StrongEvent);
                }
            }
        }
    }
}
