using RimWorld;
using RimWorld.Planet;
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
        private const float SkinRashMaxSeverity = 6f;

        private const float TightThroatMinSeverity = 0.2f;
        private const float TightThroatMaxSeverity = 1f;

        private const float PinkEyeMinSeverity = 0.2f;
        private const float PinkEyeMaxSeverity = 1f;

        private int DontCheckFullSeverityFor = 0;
        private const int StopCheckingFullSeverityFor = 1000;

        private const int IncreaseCausesLogLimit = 3;
        private List<AllergyExposureInfo> LastIncreaseCauses = new List<AllergyExposureInfo>();

        private Dictionary<string, float> SkinRashBodyPartTable = new Dictionary<string, float>()
        {
            { "Head", 1f },
            { "Neck", 0.6f },
            { "Torso", 1.5f },
            { "Arm", 1.3f },
            { "Hand", 0.5f },
            { "Leg", 1.3f },
            { "Foot", 0.5f },
        };

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
                TryApplyTightThroat(stage);
                TryApplyPinkEye(stage);
            }

            // Instantly cause an anaphylactic shock at 60% when buildup reaches 100%
            if (Severity >= 1f)
            {
                if (DontCheckFullSeverityFor == 0)
                {
                    Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("P42_AnaphylacticShock"));
                    if (existingHediff == null)
                    {
                        Hediff newHediff = HediffMaker.MakeHediff(HediffDef.Named("P42_AnaphylacticShock"), pawn);
                        newHediff.Severity = InitialAnaShockSeverityAtMaxSeverity;
                        pawn.health.AddHediff(newHediff);

                        Send100PercentShockLetter();
                    }
                    DontCheckFullSeverityFor = StopCheckingFullSeverityFor;
                }
            }

            if (DontCheckFullSeverityFor > 0) DontCheckFullSeverityFor--;
        }

        private void Send100PercentShockLetter()
        {
            // todo
            ChoiceLetter let = LetterMaker.MakeLetter(label, text, textLetterDef, lookTargets, relatedFaction, quest, hyperlinkThingDefs);
            Find.LetterStack.ReceiveLetter(let, debugInfo, delayTicks, playSound);

            // or just

            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NegativeEvent, Pawn);
        }

        private void TryTriggerSneezingFit(AllergenBuildupStage stage)
        {
            if (stage.sneezingFitMtbDays > 0)
            {
                if (Rand.MTBEventOccurs(0.1f, 60000f, ReactionCheckInterval)) // TODO: stage.sneezingFitMtbDays
                {
                    StartSneezingFit();
                }
            }
        }

        private void StartSneezingFit()
        {
            if (!pawn.Awake()) return;
            if (pawn.IsCaravanMember()) return;
            if (pawn.Downed) return;
            if (pawn.InCryptosleep) return;

            Job sneezingJob = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("P42_SneezingFit"), pawn);
            pawn.jobs.StartJob(sneezingJob, JobCondition.InterruptForced, null, resumeCurJobAfterwards: true);
        }

        private void TryApplySkinRash(AllergenBuildupStage stage)
        {
            if (stage.skinRashMtbDays > 0)
            {
                if (Rand.MTBEventOccurs(0.1f, 60000f, ReactionCheckInterval)) // TODO: stage.skinRashMtbDays
                {
                    ApplySkinRash();
                }
            }
        }

        private void ApplySkinRash()
        {
            string chosenBodyPartString = AllergyGenerator.GetWeightedRandomElement(SkinRashBodyPartTable);
            BodyPartRecord bodyPart = pawn.health.hediffSet.GetBodyPartRecord(DefDatabase<BodyPartDef>.GetNamed(chosenBodyPartString));

            Hediff_Injury rashInjury = (Hediff_Injury)HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("P42_SkinRash"), pawn, bodyPart);
            rashInjury.Severity = Rand.Range(SkinRashMinSeverity, SkinRashMaxSeverity);
            rashInjury.sourceLabel = "source?";
            rashInjury.combatLogText = "clt";
            pawn.health.AddHediff(rashInjury);
        }

        private void TryApplyTightThroat(AllergenBuildupStage stage)
        {
            if (stage.throatTightnessMtbDays > 0)
            {
                if (Rand.MTBEventOccurs(0.1f, 60000f, ReactionCheckInterval)) // TODO: stage.throatTightnessMtbDays
                {
                    ApplyTightThroat();
                }
            }
        }
        private void ApplyTightThroat()
        {
            float severity = Rand.Range(TightThroatMinSeverity, TightThroatMaxSeverity);
            BodyPartRecord bodyPart = pawn.health.hediffSet.GetBodyPartRecord(DefDatabase<BodyPartDef>.GetNamed("Neck"));

            Hediff existingHediff = pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.Part == bodyPart && x.def == HediffDef.Named("P42_TightThroat"));
            if (existingHediff == null)
            {
                Hediff newHediff = HediffMaker.MakeHediff(HediffDef.Named("P42_TightThroat"), pawn, bodyPart);
                newHediff.Severity = severity;
                newHediff.sourceLabel = "source?";
                newHediff.combatLogText = "clt";
                pawn.health.AddHediff(newHediff);
            }
            else existingHediff.Severity = Math.Max(existingHediff.Severity, severity);
        }


        private void TryApplyPinkEye(AllergenBuildupStage stage)
        {
            if (stage.pinkEyeMtbDays > 0)
            {
                if (Rand.MTBEventOccurs(0.1f, 60000f, ReactionCheckInterval)) // TODO: stage.pinkEyeMtbDays
                {
                    ApplyPinkEye();
                }
            }
        }
        private void ApplyPinkEye()
        {
            float severity = Rand.Range(PinkEyeMinSeverity, PinkEyeMaxSeverity);
            BodyPartRecord bodyPart = pawn.health.hediffSet.GetBodyPartRecord(DefDatabase<BodyPartDef>.GetNamed("Eye"));

            Hediff existingHediff = pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.Part == bodyPart && x.def == HediffDef.Named("P42_PinkEye"));
            if (existingHediff == null)
            {
                Hediff newHediff = HediffMaker.MakeHediff(HediffDef.Named("P42_PinkEye"), pawn, bodyPart);
                newHediff.Severity = severity;
                newHediff.sourceLabel = "source?";
                newHediff.combatLogText = "clt";
                pawn.health.AddHediff(newHediff);
            }
            else existingHediff.Severity = Math.Max(existingHediff.Severity, severity);
        }

        public void AddExposureInfo(AllergyExposureInfo info)
        {
            AllergyExposureInfo existingInfo = LastIncreaseCauses.FirstOrDefault(x => x.Cause == info.Cause);
            if (existingInfo != null)
            {
                existingInfo.Amount++;
                existingInfo.Tick = info.Tick;
            }
            else
            {
                LastIncreaseCauses.Add(info);
                if (LastIncreaseCauses.Count > IncreaseCausesLogLimit) LastIncreaseCauses.RemoveAt(0);
            }
        }

        public override string Description
        {
            get
            {
                if (LastIncreaseCauses == null || LastIncreaseCauses.Count == 0) return def.description;
                else
                {
                    string s = "";
                    foreach(AllergyExposureInfo info in LastIncreaseCauses)
                    {
                        string amount = info.Amount == 1 ? "" : " x" + info.Amount;
                        int ticksAgo = Find.TickManager.TicksGame - info.Tick;
                        string timeAgo = GenDate.ToStringTicksToPeriod(ticksAgo);
                        s += "\n   - " + info.Cause + amount +" (" + timeAgo + " ago)";
                    }
                    return def.description + "\n\nLast causes for increase:" + s;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref LastIncreaseCauses, "lastIncreaseCauses");
            Scribe_Values.Look(ref DontCheckFullSeverityFor, "dontCheckFullSeverityFor");
        }
        public override string DebugString()
        {
            return base.DebugString() + "\ndontCheckFullSeverityFor: " + DontCheckFullSeverityFor;
        }
    }
}
