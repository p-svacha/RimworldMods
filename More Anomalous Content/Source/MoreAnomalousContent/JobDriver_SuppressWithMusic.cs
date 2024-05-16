using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace MoreAnomalousContent
{
    public class JobDriver_SuppressWithMusic : JobDriver
    {
        private const int SuppressionDuration = 600; // Duration in ticks (10 seconds)

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // Reserve the instrument
            yield return Toils_Reserve.Reserve(TargetIndex.A);

            // Go to the instrument
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            // Play the instrument
            Toil playInstrument = new Toil
            {
                initAction = () =>
                {
                    Pawn actor = GetActor();
                    actor.jobs.curDriver.ticksLeftThisToil = SuppressionDuration;
                },
                tickAction = () =>
                {
                    // Add any custom tick behavior here, such as playing animations
                    Pawn actor = GetActor();
                    actor.skills.Learn(SkillDefOf.Artistic, 0.1f); // Example: Increase artistic skill slightly
                },
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = SuppressionDuration
            };
            playInstrument.AddFinishAction(() =>
            {
                // Suppress the activity level here
                Thing targetThing = job.targetB.Thing;
                CompActivity compActivity = targetThing.TryGetComp<CompActivity>();
                if (compActivity != null)
                {
                    compActivity.AdjustActivity(-0.1f); // Example suppression value
                }
            });
            yield return playInstrument;

            // End with cleanup if needed
            yield return Toils_Reserve.Release(TargetIndex.A);
        }
    }
}
