using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class IngestionOutcomeDoer_OffsetAllergenBuildup : IngestionOutcomeDoer
    {
        // Amount to reduce the allergen buildup by
        public float severity = 0.1f;

        // Implement the actual effect of the outcome doer
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingesetedCount)
        {
            if (pawn == null) return;

            // Find the allergen hediff if it exists on the pawn
            Hediff allergenBuildup = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("P42_AllergenBuildup"));
            if (allergenBuildup != null)
            {
                allergenBuildup.Severity = Math.Max(0, allergenBuildup.Severity - severity);
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "allergen buildup reduced", 6f);
            }
        }
    }
}
