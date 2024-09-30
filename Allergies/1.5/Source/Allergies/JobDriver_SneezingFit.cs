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
        private const int MIN_DURATION = 150; // 150 ticks = 2.5 seconds
        private const int MAX_DURATION = 600; // 600 ticks = 10 seconds

		private const float SINGLE_SNEEZE_DURATION = 150;
		private const float REST_FALL_PER_SNEEZE = 0.03f;

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
				ticksLeft = Rand.Range(MIN_DURATION, MAX_DURATION);
				int num = 0;
				IntVec3 intVec;
				do
				{
					intVec = pawn.Position + GenAdj.AdjacentCellsAndInside[Rand.Range(0, 9)];
					num++;
					if (num > 12)
					{
						intVec = pawn.Position;
						break;
					}
				}
				while (!intVec.InBounds(pawn.Map) || !intVec.Standable(pawn.Map));
				job.targetA = intVec;
				pawn.pather.StopDead();

				// Add thought
				if (pawn.needs.mood != null)
				{
					ThoughtDef sneezingFitThought = DefDatabase<ThoughtDef>.GetNamed("P42_HadSneezingFit");
					pawn.needs.mood.thoughts.memories.TryGainMemory(sneezingFitThought);
				}
			};

			toil.tickAction = delegate
			{	
				if (ticksLeft % SINGLE_SNEEZE_DURATION == (SINGLE_SNEEZE_DURATION - 1)) // Single sneeze
				{
					FilthMaker.TryMakeFilth(job.targetA.Cell, base.Map, ThingDefOf.Filth_Slime, pawn.LabelIndefinite());
					Need_Rest need_Rest = pawn.needs?.TryGetNeed<Need_Rest>();
					if (need_Rest != null && need_Rest.CurLevelPercentage > REST_FALL_PER_SNEEZE)
					{
						need_Rest.CurLevel -= REST_FALL_PER_SNEEZE;
					}
				}
				ticksLeft--;
				if (ticksLeft <= 0)
				{
					ReadyForNextToil();
					// TaleRecorder.RecordTale(TaleDefOf.Vomited, pawn);
				}
			};

			toil.defaultCompleteMode = ToilCompleteMode.Never;
			// toil.WithEffect(EffecterDefOf.Vomit, TargetIndex.A);
			// toil.PlaySustainerOrSound(() => SoundDefOf.Vomit);
			yield return toil;
		}
    }
}
