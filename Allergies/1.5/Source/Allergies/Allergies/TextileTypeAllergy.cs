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

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyItemsForPassiveExposure(checkPlants: true);
        }

        public override bool IsAllergenic(ThingDef thingDef)
        {
            switch (TextileType)
            {
                case TextileType.Wool: return IsWool(thingDef);
                case TextileType.Leather: return IsLeather(thingDef);
                case TextileType.Fabric: return IsSynthetic(thingDef);
            }
            return false;
        }

        private bool IsWool(ThingDef def)
        {
            return def.IsWool;
        }
        private bool IsLeather(ThingDef def)
        {
            return def.IsLeather;
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
        public override string TypeLabel
        {
            get
            {
                switch(TextileType)
                {
                    case TextileType.Leather: return "P42_AllergyTextileType_Leather".Translate();
                    case TextileType.Wool: return "P42_AllergyTextileType_Wool".Translate();
                    case TextileType.Fabric: return "P42_AllergyTextileType_Fabric".Translate();
                    default: return "???";
                }
            }
        }
        protected override void ExposeExtraData()
        {
            Scribe_Values.Look(ref TextileType, "textileType");
        }
    }
}
