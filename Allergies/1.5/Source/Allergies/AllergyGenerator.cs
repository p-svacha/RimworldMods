using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public static class AllergyGenerator
    {
        #region Commonalities

        private static Dictionary<AllergyTypeId, float> AllergyTypeCommonalities = new Dictionary<AllergyTypeId, float>()
        {
            { AllergyTypeId.FoodType, 1f },
            { AllergyTypeId.Ingredient, 1f },
            { AllergyTypeId.Drug, 1f },
            { AllergyTypeId.Animal, 1f },
        };

        private static Dictionary<AllergySeverity, float> AllergySeverityCommonalities = new Dictionary<AllergySeverity, float>()
        {
            { AllergySeverity.Mild, 1f },
            { AllergySeverity.Moderate, 1f },
            { AllergySeverity.Severe, 1f },
            { AllergySeverity.Extreme, 0.05f },
        };

        private static Dictionary<FoodType, float> FoodTypeCommonalities = new Dictionary<FoodType, float>()
        {
            { FoodType.Produce, 1.2f },
            { FoodType.Seed, 0.6f },
            { FoodType.Meat, 0.9f },
            { FoodType.Milk, 1.5f },
            { FoodType.Egg, 1.3f },
            { FoodType.Fungus, 0.7f },
            { FoodType.Kibble, 0.4f },
            { FoodType.Liquor, 0.6f },
            { FoodType.ProcessedMeals, 0.8f },
        };

        #endregion

        private static int MaxAllergiesPerPawn = 5;

        #region Allergy generation

        public static void GenerateAndApplyRandomAllergy(Pawn pawn)
        {
            // Get existing allergies
            List<Hediff_Allergy> existingAllergies = AllergyUtility.GetPawnAllergies(pawn);

            // Check if pawn can get an allergy
            if (!CanApplyAllergy(pawn, existingAllergies)) return;

            // Create a random allergy that the pawn does not yet have
            int tries = 0;
            int maxTries = 20;
            Allergy newAllergy = null;
            do
            {
                tries++;
                newAllergy = CreateRandomAllergy();
            }
            while (tries < maxTries && existingAllergies.Any(x => x.GetAllergy().IsDuplicateOf(newAllergy)));

            if(tries == maxTries)
            {
                if (Prefs.DevMode) Log.Message($"[Allergies Mod] Aborting allergy creation on {pawn.Name}.");
                return;
            }


            // Create new hediff
            Hediff_Allergy allergyHediff = (Hediff_Allergy)HediffMaker.MakeHediff(HediffDef.Named("P42_AllergyHediff"), pawn);
            allergyHediff.Severity = 0.05f;
            allergyHediff.SetAllergy(newAllergy);
            newAllergy.OnInitOrLoad(allergyHediff);
            pawn.health.AddHediff(allergyHediff);
            if (Prefs.DevMode) Log.Message($"[Allergies Mod] Initialized a new allergy: {newAllergy.TypeLabel} with severity {newAllergy.Severity} on {pawn.Name}.");
        }

        private static bool CanApplyAllergy(Pawn pawn, List<Hediff_Allergy> existingAllergies)
        {
            if (pawn == null) return false;
            if (pawn.Dead) return false; // No allergies for dead pawns
            if (pawn.NonHumanlikeOrWildMan()) return false; // Only humanlikes get allergies
            if (AllergyUtility.GetAllergicSensitivity(pawn) <= 0f) return false; // No allergies for pawns with 0 allergic sensitivity
            if (existingAllergies.Count >= MaxAllergiesPerPawn) return false; // max amount of allergies

            return true;
        }

        public static Allergy CreateRandomAllergy()
        {
            AllergyTypeId chosenAllergyType = GetWeightedRandomElement(AllergyTypeCommonalities);
            AllergySeverity chosenAllergySeverity = GetWeightedRandomElement(AllergySeverityCommonalities);

            Allergy newAllergy = CreateAllergyByType(chosenAllergyType);
            newAllergy.OnNewAllergyCreated(chosenAllergySeverity);
            return newAllergy;
        }

        private static Allergy CreateAllergyByType(AllergyTypeId type)
        {
            switch(type)
            {
                case AllergyTypeId.FoodType:
                    FoodTypeAllergy allergy_foodType = new FoodTypeAllergy();
                    allergy_foodType.FoodType = GetWeightedRandomElement(FoodTypeCommonalities);
                    return allergy_foodType;

                case AllergyTypeId.Ingredient:
                    IngredientAllergy allergy_ingredient = new IngredientAllergy();
                    allergy_ingredient.Ingredient = AllergyUtility.GetRandomIngredient();
                    return allergy_ingredient;

                case AllergyTypeId.Drug:
                    DrugAllergy allergy_drug = new DrugAllergy();
                    allergy_drug.Drug = AllergyUtility.GetRandomDrug();
                    return allergy_drug;

                case AllergyTypeId.Animal: return new AnimalAllergy();
            }
            throw new Exception();
        }

        #endregion

        #region Helper

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

        #endregion
    }
}
