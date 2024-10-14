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

        protected override void OnCreate()
        {
            Plant = AllergyGenerator.GetRandomPlant();
        }

        protected override void OnInitOrLoad()
        {
            keepAwayFromText = "P42_LetterTextEnd_AllergyDiscovered_KeepAwayFrom_Plant".Translate(Plant.label);
        }

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyThingsForPassiveExposure();
        }

        protected override bool IsAllergenic(ThingDef thing)
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
        private string keepAwayFromText;
        public override string KeepAwayFromText => keepAwayFromText;
        protected override void ExposeExtraData()
        {
            Scribe_Defs.Look(ref Plant, "plant");
        }
    }
}
