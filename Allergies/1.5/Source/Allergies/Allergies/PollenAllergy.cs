using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public enum PollenType
    {
        Flowers,
        Trees,
        Grass
    }

    public class PollenAllergy : Allergy
    {
        public PollenType PollenType;

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyItemsForPassiveExposure(checkApparel: false, checkInventory: false);
        }

        protected override bool IsAllergenic(ThingDef thingDef)
        {
            switch (PollenType)
            {
                case PollenType.Flowers: return IsFlower(thingDef);
                case PollenType.Trees: return IsTree(thingDef);
                case PollenType.Grass: return IsGrass(thingDef);
            }
            return false;
        }

        private bool IsFlower(ThingDef def)
        {
            return def.plant != null && def.plant.purpose == RimWorld.PlantPurpose.Beauty;
        }
        private bool IsTree(ThingDef def)
        {
            return def.plant != null && def.plant.treeCategory == RimWorld.TreeCategory.Full;
        }
        private bool IsGrass(ThingDef def)
        {
            return def.plant != null && def.plant.purpose != RimWorld.PlantPurpose.Beauty && !def.selectable;
        }

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is PollenAllergy otherPollenAllergy && otherPollenAllergy.PollenType == PollenType);
        }
        public override string TypeLabel
        {
            get
            {
                switch (PollenType)
                {
                    case PollenType.Flowers: return "P42_AllergyPollenType_Flower".Translate();
                    case PollenType.Trees: return "P42_AllergyPollenType_Tree".Translate();
                    case PollenType.Grass: return "P42_AllergyPollenType_Grass".Translate();
                    default: return "???";
                }
            }
        }
        protected override void ExposeExtraData()
        {
            Scribe_Values.Look(ref PollenType, "pollenType");
        }
    }
}
