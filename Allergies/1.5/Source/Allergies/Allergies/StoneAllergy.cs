using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class StoneAllergy : Allergy
    {
        public ThingDef StoneType;

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyItemsForPassiveExposure(checkButcherProducts: true); // butcher ingredients for stone chunks
        }

        public override bool IsAllergenic(ThingDef thingDef) => thingDef == StoneType;

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is StoneAllergy otherStoneAllergy && otherStoneAllergy.StoneType == StoneType);
        }
        public override string TypeLabel => StoneType.label;
        protected override void ExposeExtraData()
        {
            Scribe_Values.Look(ref StoneType, "stoneType");
        }
    }
}
