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

        private static Dictionary<SpecificMiscItemId, float> SpecificMiscItemWeights = new Dictionary<SpecificMiscItemId, float>()
        {
            { SpecificMiscItemId.Neutroamine, 1f },
            { SpecificMiscItemId.Chemfuel, 1f },
            { SpecificMiscItemId.Smokeleaf, 1f },
            { SpecificMiscItemId.PsychoidLeaves, 1f },
            { SpecificMiscItemId.Chocolate, 0.7f },
        };

        protected override void OnCreate()
        {
            Type = Utils.GetWeightedRandomElement(SpecificMiscItemWeights);
        }
        protected override void OnInitOrLoad()
        {
            // Set allergenic thing
            if(Type == SpecificMiscItemId.Neutroamine) AllergenicThing = ThingDef.Named("Neutroamine");
            else if(Type == SpecificMiscItemId.Smokeleaf) AllergenicThing = ThingDef.Named("SmokeleafLeaves");
            else if(Type == SpecificMiscItemId.Chemfuel) AllergenicThing = ThingDef.Named("Chemfuel");
            else if(Type == SpecificMiscItemId.PsychoidLeaves) AllergenicThing = ThingDef.Named("PsychoidLeaves");
            else if(Type == SpecificMiscItemId.Chocolate) AllergenicThing = ThingDef.Named("Chocolate");

            // Labels
            typeLabel = AllergenicThing.label;
            if (Type == SpecificMiscItemId.Smokeleaf) typeLabel = DefDatabase<ChemicalDef>.GetNamed("Smokeleaf").label.UncapitalizeFirst();
            if (Type == SpecificMiscItemId.PsychoidLeaves) typeLabel = DefDatabase<ChemicalDef>.GetNamed("Psychite").label.UncapitalizeFirst();
        }

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyThingsForPassiveExposure(checkApparel: false, checkPlants: true);
        }

        protected override bool IsAllergenic(ThingDef def) => def == AllergenicThing;

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is SpecificMiscItemAllergy otherSpecificMiscItemAllergy && otherSpecificMiscItemAllergy.Type == Type);
        }
        private string typeLabel;
        public override string TypeLabel => typeLabel;
        public override string KeepAwayFromText => typeLabel;
        protected override void ExposeExtraData()
        {
            Scribe_Values.Look(ref Type, "type");
        }
    }
}
