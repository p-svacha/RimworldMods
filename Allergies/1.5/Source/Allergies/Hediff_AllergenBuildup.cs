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

        private const float SkinRashMinSeverity = 1.6f;
        private const float SkinRashMaxSeverity = 5.1f;

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
                    Utils.TriggerAnaphylacticShock(pawn, InitialAnaShockSeverityAtMaxSeverity, LabelCap);
                    DontCheckFullSeverityFor = StopCheckingFullSeverityFor;
                }
            }

            if (DontCheckFullSeverityFor > 0) DontCheckFullSeverityFor--;
        }

        private void TryTriggerSneezingFit(AllergenBuildupStage stage)
        {
            if (stage.sneezingFitMtbDays > 0)
            {
                if (Rand.MTBEventOccurs(0.05f, 60000f, ReactionCheckInterval)) // todo: stage.sneezingFitMtbDays
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
                if (Rand.MTBEventOccurs(stage.skinRashMtbDays, 60000f, ReactionCheckInterval))
                {
                    ApplySkinRash();
                }
            }
        }

        private void ApplySkinRash()
        {
            string chosenBodyPartString = Utils.GetWeightedRandomElement(SkinRashBodyPartTable);
            BodyPartDef bodyPartDef = DefDatabase<BodyPartDef>.GetNamed(chosenBodyPartString);
            List<BodyPartRecord> naturalBodyParts = pawn.health.hediffSet.GetNotMissingParts().Where(x => x.def == bodyPartDef
                && !pawn.health.hediffSet.hediffs.Any(h => h.Part == x && !h.def.countsAsAddedPartOrImplant)).ToList();

            if (naturalBodyParts.Count == 0) return;
            BodyPartRecord bodyPart = naturalBodyParts.RandomElement();

            float chosenSeverity = Rand.Range(SkinRashMinSeverity, SkinRashMaxSeverity);

            if (Prefs.DevMode) Log.Message($"[Allergies Mod] Trying to apply skin rash onto {bodyPart.Label}. There were {naturalBodyParts.Count} options.");

            if (pawn.health.hediffSet.GetPartHealth(bodyPart) <= chosenSeverity) return; // Don't apply skin rash if it would destory the body part

            Hediff_Injury rashInjury = (Hediff_Injury)HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("P42_SkinRash"), pawn, bodyPart);
            rashInjury.Severity = chosenSeverity;
            pawn.health.AddHediff(rashInjury);
        }

        private void TryApplyTightThroat(AllergenBuildupStage stage)
        {
            if (stage.throatTightnessMtbDays > 0)
            {
                if (Rand.MTBEventOccurs(stage.throatTightnessMtbDays, 60000f, ReactionCheckInterval))
                {
                    ApplyTightThroat();
                }
            }
        }
        private void ApplyTightThroat()
        {
            float severity = Rand.Range(TightThroatMinSeverity, TightThroatMaxSeverity);
            BodyPartRecord bodyPart = pawn.health.hediffSet.GetBodyPartRecord(DefDatabase<BodyPartDef>.GetNamed("Neck"));
            if (bodyPart == null) return;

            Hediff existingHediff = pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.Part == bodyPart && x.def == HediffDef.Named("P42_TightThroat"));
            if (existingHediff == null)
            {
                Hediff newHediff = HediffMaker.MakeHediff(HediffDef.Named("P42_TightThroat"), pawn, bodyPart);
                newHediff.Severity = severity;
                pawn.health.AddHediff(newHediff);
            }
            else existingHediff.Severity = Math.Max(existingHediff.Severity, severity);
        }


        private void TryApplyPinkEye(AllergenBuildupStage stage)
        {
            if (stage.pinkEyeMtbDays > 0)
            {
                if (Rand.MTBEventOccurs(stage.pinkEyeMtbDays, 60000f, ReactionCheckInterval))
                {
                    ApplyPinkEye();
                }
            }
        }
        private void ApplyPinkEye()
        {
            float severity = Rand.Range(PinkEyeMinSeverity, PinkEyeMaxSeverity);

            BodyPartDef eyeDef = DefDatabase<BodyPartDef>.GetNamed("Eye");
            List<BodyPartRecord> naturalEyes = pawn.health.hediffSet.GetNotMissingParts().Where(x => x.def == eyeDef
                && !pawn.health.hediffSet.hediffs.Any(h => h.Part == x && h.def.countsAsAddedPartOrImplant)).ToList();
            if (naturalEyes.Count == 0) return; 

            BodyPartRecord bodyPart = naturalEyes.RandomElement();
            if (Prefs.DevMode) Log.Message($"[Allergies Mod] Trying to apply pink eye onto {bodyPart.Label}. There were {naturalEyes.Count} options.");

            Hediff existingHediff = pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.Part == bodyPart && x.def == HediffDef.Named("P42_PinkEye"));
            if (Prefs.DevMode) Log.Message($"[Allergies Mod] Found pink eye on same body part.");
            if (existingHediff == null)
            {
                Hediff newHediff = HediffMaker.MakeHediff(HediffDef.Named("P42_PinkEye"), pawn, bodyPart);
                newHediff.Severity = severity;
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
                    return def.description + "\n\n" + "P42_AllergyCause_Desc".Translate().CapitalizeFirst() + ":" + s;
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
