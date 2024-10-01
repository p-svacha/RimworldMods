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
        Meat,
        Milk,
        Egg,
        Fungus,
        Plants,
        Kibble
    }

    public class FoodTypeAllergy : Allergy
    {
        public override string TypeLabel
        {
            get
            {
                switch(FoodType)
                {
                    case FoodType.Produce: return "P42_AllergyFoodType_Produce".Translate();
                    case FoodType.Meat: return "P42_AllergyFoodType_Meat".Translate();
                    case FoodType.Milk: return "P42_AllergyFoodType_Milk".Translate();
                    case FoodType.Egg: return "P42_AllergyFoodType_Egg".Translate();
                    case FoodType.Fungus: return "P42_AllergyFoodType_Fungus".Translate();
                    case FoodType.Plants: return "P42_AllergyFoodType_Plants".Translate();
                    case FoodType.Kibble: return "P42_AllergyFoodType_Kibble".Translate();
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
                    case FoodType.Meat: return "P42_AllergyFoodType_MeatPlural".Translate();
                    case FoodType.Milk: return "P42_AllergyFoodType_MilkPlural".Translate();
                    case FoodType.Egg: return "P42_AllergyFoodType_EggPlural".Translate();
                    case FoodType.Fungus: return "P42_AllergyFoodType_FungusPlural".Translate();
                    case FoodType.Plants: return "P42_AllergyFoodType_PlantsPlural".Translate();
                    case FoodType.Kibble: return "P42_AllergyFoodType_KibblePlural".Translate();
                    default: return "???";
                }
            }
        }

        public FoodType FoodType;

        public override void Tick()
        {
            base.Tick();

            if(Pawn.IsHashIntervalTick(ExposureCheckInterval))
            {
                // PIE-checks
                DoPieCheck(GetIdentifier());
            }
        }

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is FoodTypeAllergy otherFoodTypeAllergy && otherFoodTypeAllergy.FoodType == FoodType);
        }

        private bool HasFoodTypeFlag(Thing item, FoodTypeFlags flag)
        {
            return item.def.IsIngestible && ((item.def.ingestible.foodType & flag) != 0);
        }
        private bool HasFoodTypeFlags(Thing item, FoodTypeFlags[] flags)
        {
            if (!item.def.IsIngestible) return false;
            foreach(FoodTypeFlags flag in flags)
            {
                if((item.def.ingestible.foodType & flag) == 0) return false;
            }
            return true;
        }

        private Func<Thing, bool> GetIdentifier()
        {
            switch(FoodType)
            {
                case FoodType.Produce: return IsProduce;
                case FoodType.Meat: return IsMeat;
                case FoodType.Fungus: return IsFungus;
                case FoodType.Plants: return IsPlantFood;
                case FoodType.Kibble: return IsKibble;
                case FoodType.Egg: return IsEgg;
                case FoodType.Milk: return IsMilk;
            }
            throw new Exception("Type " + FoodType.ToString() + " not handled.");
        }

        private bool IsProduce(Thing item)
        {
            if (!HasFoodTypeFlag(item, FoodTypeFlags.VegetableOrFruit)) return false;
            if (HasFoodTypeFlag(item, FoodTypeFlags.Fungus)) return false;
            return true;
        }
        private bool IsMeat(Thing item)
        {
            return HasFoodTypeFlag(item, FoodTypeFlags.Meat);
        }
        private bool IsFungus(Thing item)
        {
            return HasFoodTypeFlag(item, FoodTypeFlags.Fungus);
        }
        private bool IsPlantFood(Thing item)
        {
            return HasFoodTypeFlag(item, FoodTypeFlags.Plant);
        }
        private bool IsProcessed(Thing item)
        {
            return HasFoodTypeFlag(item, FoodTypeFlags.Processed);
        }
        private bool IsKibble(Thing item)
        {
            return HasFoodTypeFlag(item, FoodTypeFlags.Kibble);
        }
        private bool IsEgg(Thing item)
        {
            return HasFoodTypeFlag(item, FoodTypeFlags.AnimalProduct) && item.def.defName.ToLower().Contains("egg");
        }
        private bool IsMilk(Thing item)
        {
            return HasFoodTypeFlags(item, new FoodTypeFlags[] { FoodTypeFlags.AnimalProduct, FoodTypeFlags.Fluid }) && item.def.defName.ToLower().Contains("milk");
        }

        protected override void ExposeExtraData()
        {
            base.ExposeExtraData();
            Scribe_Values.Look(ref FoodType, "foodType");
        }
    }

}
