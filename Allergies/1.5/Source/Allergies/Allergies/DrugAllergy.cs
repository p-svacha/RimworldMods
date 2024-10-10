using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class DrugAllergy : Allergy
    {
        public ThingDef Drug;

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyItemsForPassiveExposure(checkApparel: false);
        }

        public override bool IsAllergenic(ThingDef thingDef) => thingDef == Drug;

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is DrugAllergy otherDrugAllergy && otherDrugAllergy.Drug == Drug);
        }
        public override string TypeLabel => Drug.label;
        protected override void ExposeExtraData()
        {
            Scribe_Values.Look(ref Drug, "drug");
        }
    }
}
