using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class WoodAllergy : Allergy
    {
        public ThingDef WoodType;

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyItemsForPassiveExposure(checkPlants: true);
        }

        public override bool IsAllergenic(ThingDef thingDef) => thingDef == WoodType;

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is WoodAllergy otherWoodAllergy && otherWoodAllergy.WoodType == WoodType);
        }
        public override string TypeLabel => WoodType.label;
        protected override void ExposeExtraData()
        {
            Scribe_Values.Look(ref WoodType, "woodType");
        }
    }
}
