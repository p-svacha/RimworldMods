using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class IngestionOutcomeDoer_AllercureIngestion : IngestionOutcomeDoer
    {
        public float allergenBuildupSeverityReduction = 0.1f;

        public float anaphylacticShockSeverity = 0.4f;
        public float anaphylacticShockChance = 0.01f;

        // Implement the actual effect of the outcome doer
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingesetedCount)
        {
            if (pawn == null) return;

            // Find the allergen hediff if it exists on the pawn
            Hediff allergenBuildup = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("P42_AllergenBuildup"));
            if (allergenBuildup != null)
            {
                allergenBuildup.Severity = Math.Max(0, allergenBuildup.Severity - allergenBuildupSeverityReduction);
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "P42_Mote_AllergicReactionsDecreased".Translate(), 6f);
            }

            // Chance to trigger anaphylactic shock
            if(Rand.Chance(anaphylacticShockChance))
            {
                Utils.TriggerAnaphylacticShock(pawn, anaphylacticShockSeverity, ingested.LabelCap);
            }
        }
    }
}
