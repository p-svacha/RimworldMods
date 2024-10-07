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
            DoPieCheck(IsDrug);
        }

        public bool IsDrug(ThingDef thing) => thing == Drug;

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is DrugAllergy drugAllergy && drugAllergy.Drug == Drug);
        }
        public override string TypeLabel => Drug.label;
        public override string TypeLabelPlural => Drug.label;
        protected override void ExposeExtraData()
        {
            Scribe_Values.Look(ref Drug, "drug");
        }
    }
}
