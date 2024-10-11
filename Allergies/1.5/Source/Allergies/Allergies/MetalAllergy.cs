using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class MetalAllergy : Allergy
    {
        public ThingDef MetalType;

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyItemsForPassiveExposure();
        }

        protected override bool IsAllergenic(ThingDef thingDef) => thingDef == MetalType;

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is MetalAllergy otherMetalAllergy && otherMetalAllergy.MetalType == MetalType);
        }
        public override string TypeLabel => MetalType.label;
        protected override void ExposeExtraData()
        {
            Scribe_Defs.Look(ref MetalType, "metalType");
        }
    }
}
