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
        Fruit,
        Meat,
        Milk,
        Egg,
        Fungus,
        Plants,
        Processed,
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
                    case FoodType.Fruit: return "fruit";
                    case FoodType.Meat: return "meat";
                    case FoodType.Milk: return "milk";
                    case FoodType.Egg: return "egg";
                    case FoodType.Fungus: return "fungus";
                    case FoodType.Plants: return "plant food";
                    case FoodType.Processed: return "processed food";
                    case FoodType.Kibble: return "kibble";
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
                    case FoodType.Fruit: return "fruits";
                    case FoodType.Meat: return "meat";
                    case FoodType.Milk: return "milk";
                    case FoodType.Egg: return "eggs";
                    case FoodType.Fungus: return "fungi";
                    case FoodType.Plants: return "plant food";
                    case FoodType.Processed: return "processed food";
                    case FoodType.Kibble: return "kibble";
                    default: return "???";
                }
            }
        }

        public FoodType FoodType { get; set; }

        // Tick
        private int tickCounter_itemCheck = 0; // Track the number of ticks since the last execution.
        private const int tickInterval_ItemCheck = 180; // Number of ticks between logic executions (2500 ticks = 1 in-game hour).

        public FoodTypeAllergy(Hediff_Allergy hediff, AllergySeverity severity, FoodType foodType) : base(hediff, severity)
        {
            FoodType = foodType;
        }

        public override void Tick()
        {
            base.Tick();
            tickCounter_itemCheck++;

            if(tickCounter_itemCheck >= tickInterval_ItemCheck)
            {

                tickCounter_itemCheck = 0;

                // PIE-checks
                DoPieCheck(GetIdentifier());
            }
        }

        private bool HasFoodTypeFlag(Thing item, FoodTypeFlags flag)
        {
            return item.def.IsIngestible && item.def.ingestible.HumanEdible &&
                (item.def.ingestible.foodType & flag) != 0;
        }
        private bool HasFoodTypeFlags(Thing item, FoodTypeFlags[] flags)
        {
            if (!item.def.IsIngestible) return false;
            if (!item.def.ingestible.HumanEdible) return false;
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
                case FoodType.Fruit: return IsFruit;
                case FoodType.Meat: return IsMeat;
                case FoodType.Fungus: return IsFungus;
                case FoodType.Plants: return IsPlantFood;
                case FoodType.Processed: return IsProcessed;
                case FoodType.Kibble: return IsKibble;
                case FoodType.Egg: return IsEgg;
                case FoodType.Milk: return IsMilk;
            }
            throw new Exception("Type " + FoodType.ToString() + " not handled.");
        }

        private bool IsFruit(Thing item)
        {
            return HasFoodTypeFlag(item, FoodTypeFlags.VegetableOrFruit);
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
    }

}
