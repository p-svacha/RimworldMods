using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace P42_Allergies
{
    public class CompUseEffect_AntishockInjector : CompUseEffect
    {
		private const float NauseaChance = 0.5f;
		private const float NauseaMinSeverity = 0.2f;
		private const float NauseaMaxSeverity = 1f;

		private const float HeartAttackChance = 0.02f;
		private const float ChemicalDamageChance = 0.02f;

		public override void DoEffect(Pawn user)
		{
			if (user == null || user.Dead) return;

			TryRemoveHediff(user, "Heatstroke");
			TryRemoveHediff(user, "Hypothermia");
			TryRemoveHediff(user, "ToxicBuildup");
			TryRemoveHediff(user, "P42_AllergenBuildup");
			TryRemoveHediff(user, "FoodPoisoning");
			TryRemoveHediff(user, "P42_AnaphylacticShock");

			TryApplyNausea(user);
            TryApplyHeartAttack(user);
			TryApplyChemicalDamage(user);
		}

		private void TryRemoveHediff(Pawn pawn, string defName)
        {
			Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(defName));
			if(existingHediff != null)
            {
				HealthUtility.Cure(existingHediff);
				MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, $"{existingHediff.Label.CapitalizeFirst()} removed", 6f);
			}
		}

		private void TryApplyNausea(Pawn pawn)
        {
			if(Rand.Chance(NauseaChance))
            {
				Hediff newHediff = HediffMaker.MakeHediff(HediffDef.Named("P42_AntishockNausea"), pawn);
				newHediff.Severity = Rand.Range(NauseaMinSeverity, NauseaMaxSeverity);
				pawn.health.AddHediff(newHediff);
			}
        }

		private void TryApplyHeartAttack(Pawn pawn)
        {
			if (Rand.Chance(HeartAttackChance))
			{
				Hediff newHediff = HediffMaker.MakeHediff(HediffDef.Named("HeartAttack"), pawn);
				pawn.health.AddHediff(newHediff);
			}
		}

		private void TryApplyChemicalDamage(Pawn pawn)
		{
			if (Rand.Chance(ChemicalDamageChance))
			{
				Hediff newHediff = HediffMaker.MakeHediff(HediffDef.Named("ChemicalDamage"), pawn);
				pawn.health.AddHediff(newHediff);
			}
		}
	}
}
