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
        ProcessedMeals
    }

    public class FoodTypeAllergy : Allergy
    {
        public FoodType FoodType;

        protected override void DoPassiveExposureChecks()
        {
            DoPieCheck(GetIdentifier());
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

        public Func<ThingDef, bool> GetIdentifier()
        {
            switch(FoodType)
            {
                case FoodType.Produce: return IsProduce;
                case FoodType.Seed: return IsSeed;
                case FoodType.Meat: return IsMeat;
                case FoodType.Milk: return IsMilk;
                case FoodType.Egg: return IsEgg;
                case FoodType.Fungus: return IsFungus;
                case FoodType.Kibble: return IsKibble;
                case FoodType.Liquor: return IsLiquor;
                case FoodType.ProcessedMeals: return IsProcessedMeal;
            }
            return def => false;
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
            return DoesThingDefConformCriteria(item, includedFlags: new[] { FoodTypeFlags.Fungus }, excludedFlags: new[] { FoodTypeFlags.VegetableOrFruit });
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



        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is FoodTypeAllergy otherFoodTypeAllergy && otherFoodTypeAllergy.FoodType == FoodType);
        }
        public override string TypeLabel
        {
            get
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
                    default: return "???";
                }
            }
        }
        public override string TypeLabelPlural
        {
            get
            {
                switch (FoodType)
                {
                    case FoodType.Produce: return "P42_AllergyFoodType_ProducePlural".Translate();
                    case FoodType.Seed: return "P42_AllergyFoodType_SeedPlural".Translate();
                    case FoodType.Meat: return "P42_AllergyFoodType_MeatPlural".Translate();
                    case FoodType.Milk: return "P42_AllergyFoodType_MilkPlural".Translate();
                    case FoodType.Egg: return "P42_AllergyFoodType_EggPlural".Translate();
                    case FoodType.Fungus: return "P42_AllergyFoodType_FungusPlural".Translate();
                    case FoodType.Kibble: return "P42_AllergyFoodType_KibblePlural".Translate();
                    case FoodType.Liquor: return "P42_AllergyFoodType_LiquorPlural".Translate();
                    case FoodType.ProcessedMeals: return "P42_AllergyFoodType_ProcessedMealsPlural".Translate();
                    default: return "???";
                }
            }
        }
        protected override void ExposeExtraData()
        {
            Scribe_Values.Look(ref FoodType, "foodType");
        }
    }

}
