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
            DoPieCheck(GetIdentifier());
        }

        public Func<ThingDef, bool> GetIdentifier()
        {
            switch (TextileType)
            {
                case TextileType.Wool: return IsWool;
                case TextileType.Leather: return IsLeather;
                case TextileType.Fabric: return IsSynthetic;
            }
            return def => false;
        }

        private bool IsWool(ThingDef def)
        {
            return def.thingCategories.Contains(ThingCategoryDefOf.Wools);
        }
        private bool IsLeather(ThingDef def)
        {
            return def.thingCategories.Contains(ThingCategoryDefOf.Leathers);
        }
        private bool IsSynthetic(ThingDef def)
        {
            if (def.thingCategories.Contains(ThingCategoryDefOf.Wools)) return false;
            if (def.thingCategories.Contains(ThingCategoryDefOf.Leathers)) return false;

            if(!def.stuffProps.categories.Contains(StuffCategoryDefOf.Fabric)) return false;

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
        public override string TypeLabelPlural
        {
            get
            {
                switch (TextileType)
                {
                    case TextileType.Leather: return "P42_AllergyTextileType_LeatherPlural".Translate();
                    case TextileType.Wool: return "P42_AllergyTextileType_WoolPlural".Translate();
                    case TextileType.Fabric: return "P42_AllergyTextileType_FabricPlural".Translate();
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
