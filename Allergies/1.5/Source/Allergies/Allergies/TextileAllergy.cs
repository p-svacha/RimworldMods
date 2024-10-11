using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class TextileAllergy : Allergy
    {
        public ThingDef Textile;

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyItemsForPassiveExposure(checkPlants: true);
        }

        protected override bool IsAllergenic(ThingDef thing) => thing == Textile;

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is TextileAllergy otherTextileAllergy && otherTextileAllergy.Textile == Textile);
        }
        public override string TypeLabel => Textile.label;
        protected override void ExposeExtraData()
        {
            Scribe_Defs.Look(ref Textile, "textile");
        }
    }
}
