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

        protected override void OnCreate()
        {
            WoodType = Utils.GetRandomWood();
        }
        protected override void OnInitOrLoad()
        {
            keepAwayFromText = "P42_LetterTextEnd_AllergyDiscovered_KeepAwayFrom_Related".Translate(WoodType.label);
        }

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyThingsForPassiveExposure(checkPlants: true);
            CheckNearbyFloorsForPassiveExposure();
        }

        protected override bool IsAllergenic(ThingDef thingDef) => thingDef == WoodType;

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is WoodAllergy otherWoodAllergy && otherWoodAllergy.WoodType == WoodType);
        }
        public override string TypeLabel => WoodType.label;
        private string keepAwayFromText;
        public override string KeepAwayFromText => keepAwayFromText;
        protected override void ExposeExtraData()
        {
            Scribe_Defs.Look(ref WoodType, "woodType");
        }
    }
}
