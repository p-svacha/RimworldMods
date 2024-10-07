using P42_Allergies;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public static class AllergyUtility
    {
        public static List<Hediff_Allergy> GetPawnAllergies(Pawn pawn)
        {
            List<Hediff_Allergy> existingAllergies = new List<Hediff_Allergy>();
            pawn.health.hediffSet.GetHediffs<Hediff_Allergy>(ref existingAllergies);
            return existingAllergies;
        }

        public static float GetAllergicSensitivity(Pawn pawn)
        {
            return pawn.GetStatValue(StatDef.Named("P42_AllergicSensitivity"));
        }

        public static ThingDef GetRandomIngredient()
        {
            List<ThingDef> candidates = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => IsFoodIngredient(x)).ToList();
            return candidates.RandomElement();
        }
        private static bool IsFoodIngredient(ThingDef def)
        {
            if (!def.IsIngestible) return false;
            if (!def.ingestible.HumanEdible) return false;
            if (def.IsDrug) return false;
            return true;
        }

        public static ThingDef GetRandomDrug()
        {
            List<ThingDef> candidates = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => IsDrug(x)).ToList();
            return candidates.RandomElement();
        }
        private static bool IsDrug(ThingDef def)
        {
            if (!def.IsDrug) return false;
            return true;
        }

        public static ThingDef GetRandomMedicine()
        {
            List<ThingDef> candidates = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => IsMedicine(x)).ToList();
            return candidates.RandomElement();
        }
        private static bool IsMedicine(ThingDef def)
        {
            if (def.thingCategories.Contains(ThingCategoryDefOf.Medicine)) return false;
            return true;
        }

        public static ThingDef GetRandomTextile()
        {
            List<ThingDef> candidates = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => IsTextile(x)).ToList();
            return candidates.RandomElement();
        }
        private static bool IsTextile(ThingDef def)
        {
            if (def.thingCategories.Contains(ThingCategoryDefOf.Textiles)) return false;
            return true;
        }

        public static PawnKindDef GetRandomAnimal()
        {
            List<PawnKindDef> candidates = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => IsAnimal(x)).ToList();
            return candidates.RandomElement();
        }
        private static bool IsAnimal(PawnKindDef def)
        {
            if (def.race.thingCategories.Contains(ThingCategoryDefOf.Animals)) return false;
            return true;
        }
    }
}
