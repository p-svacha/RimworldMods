using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class ThoughtWorker_AllergenBuildupThought : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            // Get the hediff of allergen buildup
            Hediff allergenBuildup = p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("P42_AllergenBuildup"));
            if (allergenBuildup == null) return ThoughtState.Inactive;

            // Get the severity of the hediff
            float severity = allergenBuildup.Severity;

            // Determine the thought stage based on severity
            if (severity < 0.04f)
            {
                return ThoughtState.ActiveAtStage(0); // Mildly irritated
            }
            else if (severity < 0.2f)
            {
                return ThoughtState.ActiveAtStage(1); // Annoying allergies
            }
            else if (severity < 0.4f)
            {
                return ThoughtState.ActiveAtStage(2); // Struggling with allergies
            }
            else if (severity < 0.6f)
            {
                return ThoughtState.ActiveAtStage(3); // Severe allergic reaction
            }
            else if (severity >= 0.8f)
            {
                return ThoughtState.ActiveAtStage(4); // Life-threatening allergy
            }

            return ThoughtState.Inactive; // No thought if severity is below 0.04 or no hediff
        }
    }
}
