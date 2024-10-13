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

        protected override void OnCreate()
        {
            Drug = Utils.GetRandomDrug();
        }
        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyThingsForPassiveExposure(checkApparel: false, checkPlants: true);
        }

        protected override bool IsAllergenic(ThingDef thingDef) => thingDef == Drug;

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is DrugAllergy otherDrugAllergy && otherDrugAllergy.Drug == Drug);
        }
        public override string TypeLabel => Drug.label;
        public override string KeepAwayFromText => Drug.label;
        protected override void ExposeExtraData()
        {
            Scribe_Defs.Look(ref Drug, "drug");
        }
    }
}
