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

		private const float RestNeedReduction = 0.15f;
		private const float AllergenBuildupReduction = 0.60f;

		public override void DoEffect(Pawn user)
		{
			if (user == null || user.Dead) return;

			TryRemoveHediff(user, "Heatstroke");
			TryRemoveHediff(user, "Hypothermia");
			TryRemoveHediff(user, "ToxicBuildup");
			TryRemoveHediff(user, "FoodPoisoning");
			TryRemoveHediff(user, "P42_AnaphylacticShock");
			TryReduceHediffSeverity(user, "P42_AllergenBuildup", AllergenBuildupReduction);

			TryApplyNausea(user);
            TryApplyHeartAttack(user);

			TryApplyAntishockInjectorHigh(user);

			AddInjectionThought(user);

			ReduceRestNeed(user);

			TaleRecorder.RecordTale(DefDatabase<TaleDef>.GetNamed("P42_Tale_AntishockInjection"), user);
		}

		private void TryRemoveHediff(Pawn pawn, string defName)
        {
			Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(defName));
			if (existingHediff != null) HealHediffWithMoteText(pawn, existingHediff);
		}
		private void TryReduceHediffSeverity(Pawn pawn, string defName, float amount)
		{
			Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(defName));
			if (existingHediff != null)
			{
				if (existingHediff.Severity <= amount) HealHediffWithMoteText(pawn, existingHediff);
				else existingHediff.Severity -= amount;
			}
		}

		private void HealHediffWithMoteText(Pawn pawn, Hediff hediff)
        {
			HealthUtility.Cure(hediff);
			MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, $"{hediff.LabelBaseCap.CapitalizeFirst()} removed", 6f);
		}

		private void TryApplyNausea(Pawn pawn)
        {
			if(Rand.Chance(NauseaChance))
            {
				Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("P42_AntishockNausea"));
				if (existingHediff == null)
				{
					Hediff newHediff = HediffMaker.MakeHediff(HediffDef.Named("P42_AntishockNausea"), pawn);
					newHediff.Severity = Rand.Range(NauseaMinSeverity, NauseaMaxSeverity);
					pawn.health.AddHediff(newHediff);
				}
				else existingHediff.Severity = Math.Max(existingHediff.Severity, Rand.Range(NauseaMinSeverity, NauseaMaxSeverity));
			}
        }

		private void TryApplyHeartAttack(Pawn pawn)
        {
			if (Rand.Chance(HeartAttackChance))
			{
				BodyPartRecord bodyPart = pawn.health.hediffSet.GetBodyPartRecord(DefDatabase<BodyPartDef>.GetNamed("Heart"));
				Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("HeartAttack"));

				if (existingHediff == null)
				{
					Hediff newHediff = HediffMaker.MakeHediff(HediffDef.Named("HeartAttack"), pawn, bodyPart);
					pawn.health.AddHediff(newHediff);

					if (Utils.ShouldSendAllergyNotification(pawn))
					{
						Find.LetterStack.ReceiveLetter("LetterHealthComplicationsLabel".Translate(pawn.LabelShort, newHediff.LabelBaseCap, pawn.Named("PAWN")).CapitalizeFirst(), "LetterHealthComplications".Translate(pawn.LabelShortCap, newHediff.LabelBaseCap, parent.LabelNoParenthesisCap, pawn.Named("PAWN")).CapitalizeFirst(), LetterDefOf.NegativeEvent, pawn);
					}
				}
			}
		}

		private void TryApplyAntishockInjectorHigh(Pawn pawn)
        {
			Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("P42_AntishockInjectorHigh"));
			if (existingHediff == null)
			{
				Hediff newHediff = HediffMaker.MakeHediff(HediffDef.Named("P42_AntishockInjectorHigh"), pawn);
				newHediff.Severity = 1f;
				pawn.health.AddHediff(newHediff);
			}
			else existingHediff.Severity = 1f;
		}

		private void AddInjectionThought(Pawn pawn)
        {
			Utils.ApplyMemoryThought(pawn, "P42_AntishockInjection");
		}

		private void ReduceRestNeed(Pawn pawn)
        {
			if (pawn.needs?.rest != null)
			{
				pawn.needs.rest.CurLevel += RestNeedReduction;
			}
		}
	}
}
