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

            return ThoughtState.ActiveAtStage(allergenBuildup.CurStageIndex);
        }
    }
}
