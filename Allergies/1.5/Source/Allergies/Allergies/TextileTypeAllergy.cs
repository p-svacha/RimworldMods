using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace P42_Allergies
{
    public enum TextileType
    {
        Wool,
        Leather,
        Fabric
    }

    public class TextileTypeAllergy : Allergy
    {
        public TextileType TextileType;

        private static Dictionary<TextileType, float> TextileTypeWeights = new Dictionary<TextileType, float>()
        {
            { TextileType.Leather, 1.1f },
            { TextileType.Wool, 0.85f },
            { TextileType.Fabric, 1f },
        };

        protected override void OnCreate()
        {
            TextileType = Utils.GetWeightedRandomElement(TextileTypeWeights);
        }
        protected override void OnInitOrLoad()
        {
            typeLabel = GetTypeLabel();
            keepAwayFromText = "P42_LetterTextEnd_AllergyDiscovered_KeepAwayFrom_Related".Translate(typeLabel);
        }
        private string GetTypeLabel()
        {
            switch (TextileType)
            {
                case TextileType.Leather: return "P42_AllergyTextileType_Leather".Translate();
                case TextileType.Wool: return "P42_AllergyTextileType_Wool".Translate();
                case TextileType.Fabric: return "P42_AllergyTextileType_Fabric".Translate();
                default: return "???";
            }
        }

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyThingsForPassiveExposure(checkPlants: true);
            CheckNearbyFloorsForPassiveExposure();
        }

        protected override void OnNearbyPawn(Pawn nearbyPawn)
        {
            if (TextileType == TextileType.Wool)
            {
                CompShearable compShearable = nearbyPawn.TryGetComp<CompShearable>();
                if (compShearable != null && compShearable.Props.woolDef.IsWool)
                {
                    IncreaseAllergenBuildup(ExposureType.MinorPassive, "P42_AllergyCause_BeingNearby".Translate(nearbyPawn.Label));
                }
            }
        }

        protected override bool IsAllergenic(ThingDef thingDef)
        {
            switch (TextileType)
            {
                case TextileType.Wool: return thingDef.IsWool;
                case TextileType.Leather: return thingDef.IsLeather;
                case TextileType.Fabric: return IsSynthetic(thingDef);
            }
            return false;
        }

        private bool IsSynthetic(ThingDef def)
        {
            if (def.thingCategories == null) return false;

            if (def.thingCategories.Contains(ThingCategoryDefOf.Wools)) return false;
            if (def.thingCategories.Contains(ThingCategoryDefOf.Leathers)) return false;
            if(!def.thingCategories.Contains(ThingCategoryDefOf.Textiles)) return false;

            return true;
        }

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is TextileTypeAllergy textileTypeAllergy && textileTypeAllergy.TextileType == TextileType);
        }
        private string typeLabel;
        public override string TypeLabel => typeLabel;
        private string keepAwayFromText;
        public override string KeepAwayFromText => keepAwayFromText;
        protected override void ExposeExtraData()
        {
            Scribe_Values.Look(ref TextileType, "textileType");
        }
    }
}
