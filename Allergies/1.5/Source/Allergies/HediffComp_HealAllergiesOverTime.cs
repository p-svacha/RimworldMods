using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class HediffComp_HealAllergiesOverTime : HediffComp
    {
		private int ticksToHeal;

		public override void CompPostMake()
		{
			base.CompPostMake();
			ResetTicksToHeal();
		}

		private void ResetTicksToHeal()
		{
			ticksToHeal = Rand.Range(15, 30) * 60000;
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			ticksToHeal--;
			if (ticksToHeal <= 0)
			{
				TryToHealAllergy(base.Pawn, parent.LabelCap);
				ResetTicksToHeal();
			}
		}

		public static void TryToHealAllergy(Pawn pawn, string cause)
		{
			List<Hediff> existingAllergies = pawn.health.hediffSet.hediffs.Where(x => x.GetType() == typeof(Hediff_Allergy)).ToList();

			if (existingAllergies.TryRandomElement(out var result))
			{
				HealthUtility.Cure(result);
				if (PawnUtility.ShouldSendNotificationAbout(pawn))
				{
					Messages.Message("P42_MessageAllergyHealed".Translate(cause, pawn.LabelShort, result.Label), pawn, MessageTypeDefOf.PositiveEvent);
				}
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.Look(ref ticksToHeal, "ticksToHeal", 0);
		}

		public override string CompDebugString()
		{
			return "ticksToHeal: " + ticksToHeal;
		}
	}
}
