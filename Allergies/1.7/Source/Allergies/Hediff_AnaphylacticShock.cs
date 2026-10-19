using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class Hediff_AnaphylacticShock : HediffWithComps
    {
		private float intervalFactor;
		private const int SeverityChangeInterval = 5000;
		private const float TendSuccessChanceFactor = 0.65f;
		private const float TendSeverityReduction = 0.3f;

		public override void PostMake()
		{
			base.PostMake();
			intervalFactor = Rand.Range(0.1f, 2f);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref intervalFactor, "intervalFactor", 0f);
		}

		public override void Tick()
		{
			base.Tick();
			if (pawn.IsHashIntervalTick((int)(SeverityChangeInterval * intervalFactor)))
			{
				Severity += Rand.Range(-0.2f, 0.3f);
			}
		}

		public override void Tended(float quality, float maxQuality, int batchPosition = 0)
		{
			base.Tended(quality, maxQuality, 0);
			float num = TendSuccessChanceFactor * quality;
			if (Rand.Value < num)
			{
				if (batchPosition == 0 && pawn.Spawned)
				{
					MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "TextMote_TreatSuccess".Translate(num.ToStringPercent()), 6.5f);
				}
				Severity -= TendSeverityReduction;
			}
			else if (batchPosition == 0 && pawn.Spawned)
			{
				MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "TextMote_TreatFailed".Translate(num.ToStringPercent()), 6.5f);
			}
		}
	}
}
