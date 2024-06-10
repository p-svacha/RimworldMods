using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MoreAnomalousContent
{
    public class PhonosphereActivity : CompActivity
    {
		private PhonosphereActivityGizmo disturbanceGizmo;

		private int lastNoiseIncreaseTick = -1;
		private int noiseIncreaseCooldownTicks;

		private const int MIN_NOISE_INCREASE_COOLDOWN = 900;
		private const int MAX_NOISE_INCREASE_COOLDOWN = 6300;

		public PhonosphereActivity()
		{
			// Initialize the cooldown to a random value between 30 and 90 seconds
			noiseIncreaseCooldownTicks = Rand.Range(MIN_NOISE_INCREASE_COOLDOWN, MAX_NOISE_INCREASE_COOLDOWN);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Deactivated)
			{
				yield break;
			}
			if (disturbanceGizmo == null)
			{
				disturbanceGizmo = new PhonosphereActivityGizmo(parent);
			}
			if (Find.Selector.SelectedObjects.Count == 1 && IsDormant)
			{
				yield return disturbanceGizmo;
			}

			// Dev mode
			if (DebugSettings.godMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEV: Activity -5%",
					action = delegate
					{
						AdjustActivity(-0.05f);
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "DEV: Activity +5%",
					action = delegate
					{
						AdjustActivity(0.05f);
					}
				};
			}
		}

		public override string CompInspectStringExtra()
		{
			if (Deactivated)
			{
				return "Deactivated".Translate();
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(string.Format("{0}: {1} ({2} / {3})", "P42_TL_Disturbance".Translate(), ActivityLevel.ToStringPercent("0"), Props.Worker.GetChangeRatePerDay(parent).ToStringPercentSigned("0"), "day".Translate()));
			return stringBuilder.ToString();
		}

		public void IncreaseActivityDueToNoise(string message, float amount)
        {
			if (Find.TickManager.TicksGame < lastNoiseIncreaseTick + noiseIncreaseCooldownTicks) return;
			if (ActivityLevel > (1f - amount - 0.05f)) return; // avoid instant explosion of sphere

			AdjustActivity(amount);
			Messages.Message(message, parent, MessageTypeDefOf.NegativeEvent); // Display a notification message

			// re-randomize cooldown
			lastNoiseIncreaseTick = Find.TickManager.TicksGame;
			noiseIncreaseCooldownTicks = Rand.Range(MIN_NOISE_INCREASE_COOLDOWN, MAX_NOISE_INCREASE_COOLDOWN);
		}
	}
}
