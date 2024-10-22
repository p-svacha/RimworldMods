using RimWorld;
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

        protected override void OnCreate()
        {
            Textile = AllergyGenerator.GetRandomTextile();
        }

        protected override void OnInitOrLoad()
        {
            keepAwayFromText = "P42_LetterTextEnd_AllergyDiscovered_KeepAwayFrom_Related".Translate(Textile.label);
        }

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyThingsForPassiveExposure(checkPlants: true);
            CheckNearbyFloorsForPassiveExposure();
        }

        protected override void OnNearbyPawn(Pawn nearbyPawn)
        {
            if(Textile.IsWool)
            {
                CompShearable compShearable = nearbyPawn.TryGetComp<CompShearable>();
                if (compShearable != null && compShearable.Props.woolDef == Textile)
                {
                    IncreaseAllergenBuildup(ExposureType.MinorPassive, "P42_AllergyCause_BeingNearby".Translate(nearbyPawn.Label));
                }
            }
        }

        protected override bool IsDirectlyAllergenic(ThingDef thing) => thing == Textile;

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is TextileAllergy otherTextileAllergy && otherTextileAllergy.Textile == Textile);
        }
        public override string TypeLabel => Textile.label;
        private string keepAwayFromText;
        public override string KeepAwayFromText => keepAwayFromText;
        protected override void ExposeExtraData()
        {
            Scribe_Defs.Look(ref Textile, "textile");
        }
    }
}
