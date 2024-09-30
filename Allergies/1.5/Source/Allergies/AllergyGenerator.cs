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
            { AllergyTypeId.FoodType, 10f },
            { AllergyTypeId.Ingredient, 1f },
            { AllergyTypeId.Animal, 1f },
        };

        private static Dictionary<AllergySeverity, float> AllergySeverityCommonalities = new Dictionary<AllergySeverity, float>()
        {
            { AllergySeverity.Mild, 1f },
            { AllergySeverity.Moderate, 1f },
            { AllergySeverity.Severe, 0.8f },
        };

        private static Dictionary<FoodType, float> FoodTypeCommonalities = new Dictionary<FoodType, float>()
        {
            { FoodType.Fruit, 1.2f },
            { FoodType.Meat, 0.8f },
            { FoodType.Milk, 1.5f },
            { FoodType.Egg, 1.3f },
            { FoodType.Fungus, 0.7f },
            { FoodType.Plants, 0.7f },
            { FoodType.Processed, 0.6f },
            { FoodType.Kibble, 0.4f },
        };

        #endregion

        #region Allergy generation

        public static Allergy CreateRandomAllergyFor(Hediff_Allergy hediff, Pawn pawn)
        {
            // Choose a random allergy type
            AllergyTypeId chosenAllergyType = GetWeightedRandomElement(AllergyTypeCommonalities);
            AllergySeverity chosenAllergySeverity = GetWeightedRandomElement(AllergySeverityCommonalities);

            Allergy newAllergy = CreateAllergy(hediff, chosenAllergyType, chosenAllergySeverity);

            Log.Message($"[Allergies Mod] Initialized a new allergy: {newAllergy.TypeLabel} with severity {newAllergy.Severity} on {pawn.Name}");
            return newAllergy;
        }

        private static Allergy CreateAllergy(Hediff_Allergy hediff, AllergyTypeId type, AllergySeverity severity)
        {
            switch(type)
            {
                case AllergyTypeId.FoodType:
                    FoodType foodType = GetWeightedRandomElement(FoodTypeCommonalities);
                    return new FoodTypeAllergy(hediff, severity, foodType);

                case AllergyTypeId.Ingredient: return new IngredientAllergy(hediff, severity);
                case AllergyTypeId.Animal: return new AnimalAllergy(hediff, severity);
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
            throw new System.Exception();
        }

        #endregion
    }
}
