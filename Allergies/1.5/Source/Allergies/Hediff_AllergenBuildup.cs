using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace P42_Allergies
{
    public class Hediff_AllergenBuildup : HediffWithComps
    {
        private const int ReactionCheckInterval = 300;
        private const float InitialAnaShockSeverityAtMaxSeverity = 0.6f;

        private const float SkinRashMinSeverity = 2f;
        private const float SkinRashMaxSeverity = 7f;

        public override void Tick()
        {
            base.Tick();

            if (pawn == null || pawn.Dead) return;

            if (pawn.IsHashIntervalTick(ReactionCheckInterval))
            {
                AllergenBuildupStage stage = CurStage as AllergenBuildupStage;
                if (stage == null) return;

                TryTriggerSneezingFit(stage);
                TryApplySkinRash(stage);
                
                // Instantly cause an anaphylactic shock at 60% when buildup reaches 100%
                if(Severity >= 1f)
                {
                    Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("P42_AnaphylacticShock"));
                    if(existingHediff == null)
                    {
                        Hediff newHediff = HediffMaker.MakeHediff(HediffDef.Named("P42_AnaphylacticShock"), pawn);
                        newHediff.Severity = InitialAnaShockSeverityAtMaxSeverity;
                        pawn.health.AddHediff(newHediff);
                    }
                }
            }
        }

        private void TryTriggerSneezingFit(AllergenBuildupStage stage)
        {
            if (stage.sneezingFitMtbDays > 0)
            {
                if (Rand.MTBEventOccurs(stage.sneezingFitMtbDays, 60000f, ReactionCheckInterval))
                {
                    StartSneezingFit();
                }
            }
        }

        private void StartSneezingFit()
        {
            Job sneezingJob = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("P42_SneezingFit"), pawn);
            pawn.jobs.StartJob(sneezingJob, JobCondition.InterruptForced, null, resumeCurJobAfterwards: true);
        }

        private void TryApplySkinRash(AllergenBuildupStage stage)
        {
            if (stage.skinRashMtbDays > 0)
            {
                if (Rand.MTBEventOccurs(stage.skinRashMtbDays, 60000f, ReactionCheckInterval))
                {
                    ApplySkinRash();
                }
            }
        }

        private void ApplySkinRash()
        {
            // Get a list of valid outer body parts for applying the skin rash.
            var outerBodyParts = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Outside);
            if (outerBodyParts == null || !outerBodyParts.Any()) return;

            // Choose a random outer body part to apply the rash.
            BodyPartRecord randomBodyPart = outerBodyParts.RandomElement();

            // Create a new injury hediff with the SkinRash HediffDef.
            Hediff_Injury rashInjury = (Hediff_Injury)HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("P42_SkinRash"), pawn, randomBodyPart);

            // Set the severity of the rash randomly between a minor and a more serious injury.
            rashInjury.Severity = Rand.Range(2f, 7f);

            // Add the injury to the pawn.
            pawn.health.AddHediff(rashInjury);
        }
    }
}
