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
            List<Hediff_Allergy> allergies = AllergyUtility.GetPawnAllergies(pawn);
            foreach (Hediff_Allergy allergyHediff in allergies)
            {
                Allergy allergy = allergyHediff.GetAllergy();

                if (allergy is MedicineAllergy medicineAllergy)
                {
                    foreach (Thing ingredient in ingredients)
                    {
                        if (medicineAllergy.IsMedicineType(ingredient.def)) // getting operated with => extreme exposure event
                        {
                            medicineAllergy.ExtremeExposureEvent("P42_AllergyCause_Tended".Translate(ingredient.Label));
                            break;
                        }
                    }
                }
            }
        }
    }
}
