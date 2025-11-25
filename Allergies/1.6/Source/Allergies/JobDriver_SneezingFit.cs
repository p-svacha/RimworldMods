using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace P42_Allergies
{
    public class JobDriver_SneezingFit : JobDriver
    {
        private const int MinSneezeFitDuration = 300; // 300 ticks = 5 seconds
        private const int MaxSneezeFitDuration = 600; // 600 ticks = 10 seconds

		private const float SingleSneezeDuration = 150;
		private const float RestFallPerSneeze = 0.03f;
		private float SlimeChancePerSneeze = 0.3f;

		private int ticksLeft;

        public override void SetInitialPosture()
        {
            // Set the initial posture to standing
            pawn.jobs.posture = PawnPosture.Standing;
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true; // No reservations needed for sneezing
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
			Toil toil = ToilMaker.MakeToil("MakeNewToils");
			toil.initAction = delegate
			{
				ticksLeft = Rand.Range(MinSneezeFitDuration, MaxSneezeFitDuration);
				job.targetA = pawn.Position;
				pawn.pather.StopDead();

				// Add thought
				Utils.ApplyMemoryThought(pawn, "P42_HadSneezingFit");
			};

			toil.tickAction = delegate
			{	
				if (ticksLeft % SingleSneezeDuration == (SingleSneezeDuration - 1)) // Single sneeze
				{
					if (Rand.Chance(SlimeChancePerSneeze))
					{
						FilthMaker.TryMakeFilth(job.targetA.Cell, base.Map, ThingDefOf.Filth_Slime, pawn.LabelIndefinite());
					}

					Need_Rest need_Rest = pawn.needs?.TryGetNeed<Need_Rest>();
					if (need_Rest != null && need_Rest.CurLevelPercentage > RestFallPerSneeze)
					{
						need_Rest.CurLevel -= RestFallPerSneeze;
					}
				}

				ticksLeft--;
				if (ticksLeft <= 0)
				{
					ReadyForNextToil();
					TaleRecorder.RecordTale(DefDatabase<TaleDef>.GetNamed("P42_Tale_HadSneezingFit"), pawn);
				}
			};

			toil.defaultCompleteMode = ToilCompleteMode.Never;
			toil.WithEffect(DefDatabase<EffecterDef>.GetNamed("P42_Sneeze"), TargetIndex.A);
			toil.PlaySustainerOrSound(() => SoundDef.Named("P42_Sneeze"));
			yield return toil;
		}
    }
}
