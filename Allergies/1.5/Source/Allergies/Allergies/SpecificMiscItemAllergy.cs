using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public enum SpecificMiscItemId
    {
        Neutroamine,
        Smokeleaf,
        Chemfuel,
        PsychoidLeaves,
        Chocolate
    }

    public class SpecificMiscItemAllergy : Allergy
    {
        public SpecificMiscItemId Type;
        private ThingDef AllergenicThing;

        protected override void OnInitOrLoad()
        {
            base.OnInitOrLoad();

            // Set allergenic thing
            if(Type == SpecificMiscItemId.Neutroamine) AllergenicThing = ThingDef.Named("Neutroamine");
            else if(Type == SpecificMiscItemId.Smokeleaf) AllergenicThing = ThingDef.Named("SmokeleafLeaves");
            else if(Type == SpecificMiscItemId.Chemfuel) AllergenicThing = ThingDef.Named("Chemfuel");
            else if(Type == SpecificMiscItemId.PsychoidLeaves) AllergenicThing = ThingDef.Named("PsychoidLeaves");
            else if(Type == SpecificMiscItemId.Chocolate) AllergenicThing = ThingDef.Named("Chocolate");
        }

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyItemsForPassiveExposure(checkApparel: false, checkPlants: true);
        }

        public override bool IsAllergenic(ThingDef def) => def == AllergenicThing;

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is SpecificMiscItemAllergy otherSpecificMiscItemAllergy && otherSpecificMiscItemAllergy.Type == Type);
        }
        public override string TypeLabel => AllergenicThing.label;
        protected override void ExposeExtraData()
        {
            Scribe_Values.Look(ref Type, "type");
        }
    }
}
