using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class PlantAllergy : Allergy
    {
        public ThingDef Plant;

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyItemsForPassiveExposure();
        }

        public override bool IsAllergenic(ThingDef thing)
        {
            if (thing == Plant) return true;
            if (thing == Plant.plant.harvestedThingDef) return true;

            return false;
        }

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is PlantAllergy plantAllergy && plantAllergy.Plant == Plant);
        }
        public override string TypeLabel => Plant.label;
        protected override void ExposeExtraData()
        {
            Scribe_Values.Look(ref Plant, "plant");
        }
    }
}
