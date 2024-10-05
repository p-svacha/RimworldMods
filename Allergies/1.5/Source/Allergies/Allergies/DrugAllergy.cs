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

        public override void Tick()
        {
            base.Tick();

            if (Pawn.IsHashIntervalTick(ExposureCheckInterval))
            {
                // PIE-checks
                DoPieCheck(IsDrug);
            }
        }

        public bool IsDrug(ThingDef thing) => thing == Drug;

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is DrugAllergy drugAllergy && drugAllergy.Drug == Drug);
        }
        public override string TypeLabel => Drug.label;
        public override string TypeLabelPlural => Drug.label;
    }
}
