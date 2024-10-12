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

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyItemsForPassiveExposure(checkPlants: true);
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
