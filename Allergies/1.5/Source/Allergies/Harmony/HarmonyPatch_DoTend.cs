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
    [HarmonyPatch(typeof(TendUtility), "DoTend")]
    public static class HarmonyPatch_DoTend
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn doctor, Pawn patient, Medicine medicine)
        {
            if (!AllergyUtility.CheckForAllergies(patient)) return;

            List<Hediff_Allergy> allergies = AllergyUtility.GetPawnAllergies(patient);
            foreach (Hediff_Allergy allergyHediff in allergies)
            {
                Allergy allergy = allergyHediff.GetAllergy();

                if(allergy is MedicineAllergy medicineAllergy)
                {
                    if(medicineAllergy.IsMedicineType(medicine.def)) // getting tended with => extreme exposure event
                    {
                        medicineAllergy.IncreaseAllergenBuildup(ExposureType.ExtremeEvent, "P42_AllergyCause_Tended".Translate(medicine.LabelNoParenthesis));
                    }
                }
            }
        }
    }
}
