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
                if(allergyHediff.GetAllergy() is FoodTypeAllergy foodTypeAllergy)
                {
                    Func<ThingDef, bool> identifier = foodTypeAllergy.GetIdentifier();
                    if(identifier(__instance.def))
                    {
                        float buildup = Allergy.ExtremeExposureEventIncrease;
                        foodTypeAllergy.IncreaseAllergenBuildup(buildup, "P42_AllergyCause_Ingested".Translate(__instance.Label));
                        break;
                    }

                    if (__instance.TryGetComp<CompIngredients>() != null)
                    {
                        foreach (ThingDef ingredient in __instance.TryGetComp<CompIngredients>().ingredients)
                        {
                            if (identifier(ingredient)) // Something with item as ingredient in caravan inventory => minor passive exposure
                            {
                                float buildup = Allergy.StrongExposureEventIncrease;
                                foodTypeAllergy.IncreaseAllergenBuildup(buildup, "P42_AllergyCause_IngestedIngredient".Translate(__instance.Label, ingredient.label));
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
