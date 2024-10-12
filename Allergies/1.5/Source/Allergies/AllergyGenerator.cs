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
        #region Weight tables

        private static Dictionary<AllergyTypeId, float> AllergyTypeWeights = new Dictionary<AllergyTypeId, float>()
        {
            { AllergyTypeId.FoodType, 1f },
            { AllergyTypeId.Ingredient, 1f },
            { AllergyTypeId.Drug, 1f },
            { AllergyTypeId.Medicine, 1f },
            { AllergyTypeId.TextileType, 1f },
            { AllergyTypeId.Textile, 1f },
            { AllergyTypeId.Animal, 1f },
            { AllergyTypeId.Plant, 1f },
            { AllergyTypeId.Pollen, 1f },
            { AllergyTypeId.Sunlight, 1f },
            { AllergyTypeId.Dust, 1f },
            { AllergyTypeId.Water, 1f },
            { AllergyTypeId.Temperature, 1f },
            { AllergyTypeId.SpecificMiscItem, 1f },
            { AllergyTypeId.Xenotype, 1f },
            { AllergyTypeId.Stone, 1f },
            { AllergyTypeId.Wood, 1f },
            { AllergyTypeId.Metal, 1f },
        };

        private static Dictionary<AllergySeverity, float> AllergySeverityWeights = new Dictionary<AllergySeverity, float>()
        {
            { AllergySeverity.Mild, 1f },
            { AllergySeverity.Moderate, 1f },
            { AllergySeverity.Severe, 1f },
            { AllergySeverity.Extreme, 0.05f },
        };

        private static Dictionary<FoodType, float> FoodTypeWeights = new Dictionary<FoodType, float>()
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

        private static Dictionary<TextileType, float> TextileTypeWeights = new Dictionary<TextileType, float>()
        {
            { TextileType.Leather, 1f },
            { TextileType.Wool, 0.9f },
            { TextileType.Fabric, 1.1f },
        };

        private static Dictionary<PollenType, float> PollenTypeWeights = new Dictionary<PollenType, float>()
        {
            { PollenType.Flowers, 0.5f },
            { PollenType.Trees, 1f },
            { PollenType.Grass, 0.75f },
        };

        private static Dictionary<SpecificMiscItemId, float> SpecificMiscItemWeights = new Dictionary<SpecificMiscItemId, float>()
        {
            { SpecificMiscItemId.Neutroamine, 1f },
            { SpecificMiscItemId.Chemfuel, 1f },
            { SpecificMiscItemId.Smokeleaf, 1f },
            { SpecificMiscItemId.PsychoidLeaves, 1f },
            { SpecificMiscItemId.Chocolate, 1f },
        };

        #endregion

        private static int MaxAllergiesPerPawn = 6;

        #region Allergy generation

        public static void GenerateAndApplyRandomAllergy(Pawn pawn, bool isVisible)
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
                Logger.Log($"Aborting allergy creation on {pawn.Name}.");
                return;
            }

            // Create new hediff
            Hediff_Allergy allergyHediff = (Hediff_Allergy)HediffMaker.MakeHediff(HediffDef.Named("P42_AllergyHediff"), pawn);
            allergyHediff.Severity = 0.05f;
            allergyHediff.SetAllergy(newAllergy);
            newAllergy.OnInitOrLoad(allergyHediff);
            if (isVisible) newAllergy.MakeAllergyVisible();

            pawn.health.AddHediff(allergyHediff);
            Logger.Log($"Initialized a new allergy: {newAllergy.TypeLabel} ({newAllergy.GetType()}) with severity {newAllergy.Severity} on {pawn.Name}.");
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
            AllergyTypeId chosenAllergyType = GetWeightedRandomElement(AllergyTypeWeights);
            AllergySeverity chosenAllergySeverity = GetWeightedRandomElement(AllergySeverityWeights);

            Allergy newAllergy = CreateAllergyByType(chosenAllergyType);
            newAllergy.OnNewAllergyCreated(chosenAllergySeverity);
            return newAllergy;
        }

        private static Allergy CreateAllergyByType(AllergyTypeId type)
        {
            switch(type)
            {
                case AllergyTypeId.FoodType:
                    FoodTypeAllergy foodTypeAllergy = new FoodTypeAllergy();
                    foodTypeAllergy.FoodType = GetWeightedRandomElement(FoodTypeWeights);
                    return foodTypeAllergy;

                case AllergyTypeId.Ingredient:
                    IngredientAllergy ingredientAllergy = new IngredientAllergy();
                    ingredientAllergy.Ingredient = AllergyUtility.GetRandomIngredient();
                    return ingredientAllergy;

                case AllergyTypeId.Drug:
                    DrugAllergy drugAllergy = new DrugAllergy();
                    drugAllergy.Drug = AllergyUtility.GetRandomDrug();
                    return drugAllergy;

                case AllergyTypeId.Medicine:
                    MedicineAllergy medicineAllergy = new MedicineAllergy();
                    medicineAllergy.MedicineType = AllergyUtility.GetRandomMedicine();
                    return medicineAllergy;

                case AllergyTypeId.TextileType:
                    TextileTypeAllergy textileTypeAllergy = new TextileTypeAllergy();
                    textileTypeAllergy.TextileType = GetWeightedRandomElement(TextileTypeWeights);
                    return textileTypeAllergy;

                case AllergyTypeId.Textile:
                    TextileAllergy textileAllergy = new TextileAllergy();
                    textileAllergy.Textile = AllergyUtility.GetRandomTextile();
                    return textileAllergy;

                case AllergyTypeId.Animal:
                    AnimalAllergy animalAllergy = new AnimalAllergy();
                    animalAllergy.Animal = AllergyUtility.GetRandomAnimal();
                    return animalAllergy;

                case AllergyTypeId.Plant:
                    PlantAllergy plantAllergy = new PlantAllergy();
                    plantAllergy.Plant = AllergyUtility.GetRandomPlant();
                    return plantAllergy;

                case AllergyTypeId.Pollen:
                    PollenAllergy pollenAllergy = new PollenAllergy();
                    pollenAllergy.PollenType = GetWeightedRandomElement(PollenTypeWeights);
                    return pollenAllergy;

                case AllergyTypeId.Sunlight: return new SunlightAllergy();
                case AllergyTypeId.Dust: return new DustAllergy();
                case AllergyTypeId.Water: return new WaterAllergy();

                case AllergyTypeId.Temperature:
                    TemperatureAllergy temperatureAllergy = new TemperatureAllergy();
                    temperatureAllergy.IsHeatAllergy = Rand.Chance(0.5f);
                    return temperatureAllergy;

                case AllergyTypeId.SpecificMiscItem:
                    SpecificMiscItemAllergy specificMiscItemAllergy = new SpecificMiscItemAllergy();
                    specificMiscItemAllergy.Type = GetWeightedRandomElement(SpecificMiscItemWeights);
                    return specificMiscItemAllergy;

                case AllergyTypeId.Xenotype:
                    XenotypeAllergy xenotypeAllergy = new XenotypeAllergy();
                    xenotypeAllergy.Xenotype = AllergyUtility.GetRandomXenotype();
                    return xenotypeAllergy;

                case AllergyTypeId.Stone:
                    StoneAllergy stoneAllergy = new StoneAllergy();
                    stoneAllergy.StoneType = AllergyUtility.GetRandomStone();
                    return stoneAllergy;

                case AllergyTypeId.Wood:
                    WoodAllergy woodAllergy = new WoodAllergy();
                    woodAllergy.WoodType = AllergyUtility.GetRandomWood();
                    return woodAllergy;

                case AllergyTypeId.Metal:
                    MetalAllergy metalAllergy = new MetalAllergy();
                    metalAllergy.MetalType = AllergyUtility.GetRandomMetal();
                    return metalAllergy;

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
