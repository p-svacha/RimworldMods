using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public enum FoodType
    {
        Produce,
        Seed,
        Meat,
        Milk,
        Egg,
        Fungus,
        Kibble,
        Liquor,
        ProcessedMeals,
        Fish
    }

    public class FoodTypeAllergy : Allergy
    {
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
            { FoodType.Fish, 1.1f },
        };

        public FoodType FoodType;

        protected override void OnCreate()
        {
            if (!ModsConfig.IsActive("VanillaExpanded.VCEF")) FoodTypeWeights.Remove(FoodType.Fish);

            FoodType = Utils.GetWeightedRandomElement(FoodTypeWeights);
        }

        protected override void OnInitOrLoad()
        {
            typeLabel = GetTypeLabel();
            if (FoodType == FoodType.Liquor || FoodType == FoodType.ProcessedMeals || FoodType == FoodType.Kibble) keepAwayFromText = typeLabel;
            else keepAwayFromText = "P42_LetterTextEnd_AllergyDiscovered_KeepAwayFrom_Food".Translate(typeLabel);
        }
        private string GetTypeLabel()
        {
            switch (FoodType)
            {
                case FoodType.Produce: return "P42_AllergyFoodType_Produce".Translate();
                case FoodType.Seed: return "P42_AllergyFoodType_Seed".Translate();
                case FoodType.Meat: return "P42_AllergyFoodType_Meat".Translate();
                case FoodType.Milk: return "P42_AllergyFoodType_Milk".Translate();
                case FoodType.Egg: return "P42_AllergyFoodType_Egg".Translate();
                case FoodType.Fungus: return "P42_AllergyFoodType_Fungus".Translate();
                case FoodType.Kibble: return "P42_AllergyFoodType_Kibble".Translate();
                case FoodType.Liquor: return "P42_AllergyFoodType_Liquor".Translate();
                case FoodType.ProcessedMeals: return "P42_AllergyFoodType_ProcessedMeals".Translate();
                case FoodType.Fish: return "P42_AllergyFoodType_Fish".Translate();
                default: return "???";
            }
        }

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyThingsForPassiveExposure(checkApparel: false, checkPlants: true);
        }

        /// <summary>
        /// Checks if a thing has all the included food type flags, none of the excluded food type flags, any of the provided cat defs and includes the defNameContains
        /// </summary>
        private bool DoesThingDefConformCriteria(ThingDef thingDef, FoodTypeFlags[] includedFlags = null, FoodTypeFlags[] excludedFlags = null, ThingCategoryDef[] anyOfCatDefs = null, string defNameContains = "")
        {
            if(includedFlags != null)
            {
                if (!thingDef.IsIngestible) return false;
                foreach(FoodTypeFlags flag in includedFlags)
                {
                    if ((thingDef.ingestible.foodType & flag) == 0) return false;
                }
            }
            if(excludedFlags != null && thingDef.IsIngestible)
            {
                foreach (FoodTypeFlags flag in excludedFlags)
                {
                    if ((thingDef.ingestible.foodType & flag) != 0) return false;
                }
            }
            if(anyOfCatDefs != null)
            {
                foreach (ThingCategoryDef cat in anyOfCatDefs)
                {
                    if (!thingDef.thingCategories.Any(x => anyOfCatDefs.Contains(x))) return false;
                }
            }
            if (defNameContains != "" && !thingDef.defName.ToLower().Contains(defNameContains)) return false;
            return true;
        }

        protected override bool IsAllergenic(ThingDef def)
        {
            switch(FoodType)
            {
                case FoodType.Produce: return IsProduce(def);
                case FoodType.Seed: return IsSeed(def);
                case FoodType.Meat: return IsMeat(def);
                case FoodType.Milk: return IsMilk(def);
                case FoodType.Egg: return IsEgg(def);
                case FoodType.Fungus: return IsFungus(def);
                case FoodType.Kibble: return IsKibble(def);
                case FoodType.Liquor: return IsLiquor(def);
                case FoodType.ProcessedMeals: return IsProcessedMeal(def);
                case FoodType.Fish: return IsFish(def);
            }
            return false;
        }

        private bool IsProduce(ThingDef item)
        {
            return item.IsIngestible && item.ingestible.foodType == FoodTypeFlags.VegetableOrFruit;
        }
        private bool IsSeed(ThingDef item)
        {
            return DoesThingDefConformCriteria(item, includedFlags: new[] { FoodTypeFlags.Seed });
        }
        private bool IsMeat(ThingDef item)
        {
            return DoesThingDefConformCriteria(item, includedFlags: new[] { FoodTypeFlags.Meat });
        }
        private bool IsMilk(ThingDef item)
        {
            return DoesThingDefConformCriteria(item, includedFlags: new[] { FoodTypeFlags.AnimalProduct, FoodTypeFlags.Fluid }, defNameContains: "milk");
        }
        private bool IsEgg(ThingDef item)
        {
            return DoesThingDefConformCriteria(item, includedFlags: new FoodTypeFlags[] { FoodTypeFlags.AnimalProduct }, anyOfCatDefs: new[] { ThingCategoryDefOf.EggsFertilized, ThingCategoryDefOf.EggsUnfertilized });
        }
        private bool IsFungus(ThingDef item)
        {
            return item.IsIngestible && item.ingestible.foodType == FoodTypeFlags.Fungus;
        }
        private bool IsKibble(ThingDef item)
        {
            return DoesThingDefConformCriteria(item, includedFlags: new[] { FoodTypeFlags.Kibble });
        }
        private bool IsLiquor(ThingDef item)
        {
            return DoesThingDefConformCriteria(item, includedFlags: new[] { FoodTypeFlags.Liquor });
        }
        private bool IsProcessedMeal(ThingDef item)
        {
            return DoesThingDefConformCriteria(item, includedFlags: new[] { FoodTypeFlags.Meal });
        }
        private bool IsFish(ThingDef thing)
        {
            return thing.thingCategories != null && thing.thingCategories.Contains(ThingCategoryDef.Named("VCEF_RawFishCategory"));
        }



        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is FoodTypeAllergy otherFoodTypeAllergy && otherFoodTypeAllergy.FoodType == FoodType);
        }

        private string typeLabel;
        public override string TypeLabel => typeLabel;
        private string keepAwayFromText;
        public override string KeepAwayFromText => keepAwayFromText;
        protected override void ExposeExtraData()
        {
            Scribe_Values.Look(ref FoodType, "foodType");
        }
    }

}
