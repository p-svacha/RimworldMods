using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public abstract class Allergy : IExposable
    {
        private Dictionary<AllergySeverity, float> SeverityMultiplier = new Dictionary<AllergySeverity, float>()
        {
            { AllergySeverity.Mild, 0.5f },
            { AllergySeverity.Moderate, 1f },
            { AllergySeverity.Severe, 2f },
        };

        public abstract string TypeLabel { get; }
        public abstract string TypeLabelPlural { get; }

        protected Hediff_Allergy AllergyHediff;
        protected Pawn Pawn => AllergyHediff.pawn;
        public string FullAllergyName => TypeLabel + " allergy";
        public string FullAllergyNameCap => TypeLabel.CapitalizeFirst() + " allergy";

        protected const int ExposureCheckInterval = 200;
        protected const int AllercureHighCheckInterval = 1000;

        private const int MinTicksUntilNaturalSeverityChange = 15 * 60000; // 15 days
        private const int MaxTicksUntilNaturalSeverityChange = 180 * 60000; // 180 days (3 years)
        private const float ExtremeNaturalSeverityChangeChance = 0.1f; // Chance that an allergy instantly goes to extreme / goes away

        private const int MinTicksUntilAllercureImpact = 5 * 60000; // 5 days
        private const int MaxTicksUntilAllercureImpact = 60 * 60000; // 60 days (1 year)
        private const float AllercureInstantHealChance = 0.1f;
        

        public AllergySeverity Severity;
        public int TicksUntilNaturalSeverityChange;
        public int TicksUntilAllercureImpact;
        
        public void OnInitOrLoad(Hediff_Allergy hediff)
        {
            AllergyHediff = hediff;
        }

        public void OnNewAllergyCreated(AllergySeverity severity)
        {
            Severity = severity;
            ResetTicksUntilNaturalSeverityChange();
            ResetTicksUntilAllercureSeverityChange();
        }

        private void ResetTicksUntilNaturalSeverityChange()
        {
            TicksUntilNaturalSeverityChange = 2500;
            //TODO: TicksUntilSeverityChange = Rand.Range(MinTicksUntilSeverityChange, MaxTicksUntilSeverityChange);
        }
        private void ResetTicksUntilAllercureSeverityChange()
        {
            TicksUntilAllercureImpact = 2500;
            //TODO: TicksUntilAllercureSeverityChange = Rand.Range(MinTicksUntilAllercureImpact, MaxTicksUntilAllercureImpact);
        }

        public virtual void Tick()
        {
            // Check for natural severity change
            TicksUntilNaturalSeverityChange--;
            if(TicksUntilNaturalSeverityChange <= 0)
            {
                // Define what kind of change should occur
                float chanceForGoodOutcome = 0.5f;
                float allergicSensitivity = Pawn.GetStatValue(StatDef.Named("P42_AllergicSensitivity"));
                if (allergicSensitivity == 0f) chanceForGoodOutcome = 1f;
                else if (allergicSensitivity < 1f) chanceForGoodOutcome = 0.6f;
                else if (allergicSensitivity > 1f) chanceForGoodOutcome = 0.4f;

                bool isGoodOutcome = Rand.Chance(chanceForGoodOutcome);
                bool isExtremeOutcome = Rand.Chance(ExtremeNaturalSeverityChangeChance);

                // Do the change
                if(isGoodOutcome && isExtremeOutcome)
                {
                    HealAllergyNaturally();
                    return;
                }
                else if(isGoodOutcome && !isExtremeOutcome)
                {
                    if (Severity == AllergySeverity.Mild)
                    {
                        HealAllergyNaturally();
                        return;
                    }
                    else ReduceAllergySeverityNaturally();
                }
                else if(!isGoodOutcome && !isExtremeOutcome)
                {
                    if (Severity != AllergySeverity.Extreme) IncreaseAllergySeverityNaturally();
                }
                else if(!isGoodOutcome && isExtremeOutcome)
                {
                    if (Severity != AllergySeverity.Extreme) SetToExtremeSeverityNaturally();
                }

                ResetTicksUntilNaturalSeverityChange();
            }

            // Check for allercure severity change
            if(Pawn.IsHashIntervalTick(AllercureHighCheckInterval))
            {
                Hediff allercureHigh = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("P42_AllercureHigh"));
                if(allercureHigh != null)
                {
                    TicksUntilAllercureImpact -= AllercureHighCheckInterval;
                    if(TicksUntilAllercureImpact <= 0)
                    {
                        bool isInstantHeal = Rand.Chance(AllercureInstantHealChance);

                        if(isInstantHeal)
                        {
                            HealAllergyWithAllercure();
                            return;
                        }
                        else
                        {
                            if(Severity == AllergySeverity.Mild)
                            {
                                HealAllergyWithAllercure();
                                return;
                            }
                            else ReduceAllergySeverityAllercure();
                        }

                        ResetTicksUntilAllercureSeverityChange();
                    }
                }
            }
        }

        private void HealAllergyNaturally()
        {
            Pawn.health.RemoveHediff(AllergyHediff);
            if (AllergyHediff.Visible && PawnUtility.ShouldSendNotificationAbout(Pawn))
            {
                Messages.Message("P42_Message_AllergyHealedNaturally".Translate(Pawn.Name.ToStringShort), Pawn, MessageTypeDefOf.PositiveEvent);
            }
        }
        private void ReduceAllergySeverityNaturally()
        {
            Severity = (AllergySeverity)(((int)Severity) - 1);
            if (AllergyHediff.Visible && PawnUtility.ShouldSendNotificationAbout(Pawn))
            {
                Messages.Message("P42_Message_AllergySeverityReducedNaturally".Translate(Pawn.Name.ToStringShort, Severity.ToString().ToLower()), Pawn, MessageTypeDefOf.PositiveEvent);
            }
        }
        private void IncreaseAllergySeverityNaturally()
        {
            Severity = (AllergySeverity)(((int)Severity) + 1);
            if (AllergyHediff.Visible && PawnUtility.ShouldSendNotificationAbout(Pawn))
            {
                Messages.Message("P42_Message_AllergySeverityIncreasedNaturally".Translate(Pawn.Name.ToStringShort, Severity.ToString().ToLower()), Pawn, MessageTypeDefOf.NegativeEvent);
            }
        }
        private void SetToExtremeSeverityNaturally()
        {
            Severity = AllergySeverity.Extreme;
            if (AllergyHediff.Visible && PawnUtility.ShouldSendNotificationAbout(Pawn))
            {
                Messages.Message("P42_Message_AllergySeverityIncreasedNaturally".Translate(Pawn.Name.ToStringShort, Severity.ToString().ToLower()), Pawn, MessageTypeDefOf.NegativeEvent);
            }
        }

        private void HealAllergyWithAllercure()
        {
            Pawn.health.RemoveHediff(AllergyHediff);
            if (AllergyHediff.Visible && PawnUtility.ShouldSendNotificationAbout(Pawn))
            {
                Messages.Message("P42_Message_AllergyHealedAllercure".Translate(Pawn.Name.ToStringShort, Pawn.Possessive()), Pawn, MessageTypeDefOf.PositiveEvent);
            }
        }
        private void ReduceAllergySeverityAllercure()
        {
            Severity = (AllergySeverity)(((int)Severity) - 1);
            if (AllergyHediff.Visible && PawnUtility.ShouldSendNotificationAbout(Pawn))
            {
                Messages.Message("P42_Message_AllergySeverityReducedAllercure".Translate(Pawn.Name.ToStringShort, Pawn.Possessive(), Severity.ToString().ToLower()), Pawn, MessageTypeDefOf.PositiveEvent);
            }
        }

        /// <summary>
        /// Returns if two allergies are of the exact same type and subtype. Used to avoid generating duplicate allergies.
        /// </summary>
        public abstract bool IsDuplicateOf(Allergy otherAllergy);

        /// <summary>
        /// Executes the PIE (Passive Item Exposure) check for a certain item and increases allergen buildup accordingly.
        /// </summary>
        protected void DoPieCheck(Func<Thing, bool> ItemIdentifier)
        {
            // Get the current map and position of the pawn.
            Map map = Pawn.Map;
            IntVec3 pawnPosition = Pawn.Position;

            if (map == null) return;

            // Define a search radius of 5 tiles around the pawn.
            int searchRadius = 5;

            // Find all things (items) in the defined radius.
            foreach (Thing item in GenRadial.RadialDistinctThingsAround(pawnPosition, map, searchRadius, useCenter: true))
            {
                // Check if the item contains allergen
                if (ItemIdentifier(item))
                {
                    Log.Message($"Pawn {Pawn.Name} is within {searchRadius} tiles of allergenic item: {item.Label}");
                    IncreaseAllergenBuildup(0.01f, "being nearby " + item.Label);
                    break;
                }
            }
        }
        protected void IncreaseAllergenBuildup(float baseAmount, string reasonForIncrease)
        {
            if (Pawn == null || Pawn.Dead) return;

            // Calculate amount
            float amount = baseAmount;
            if (Severity == AllergySeverity.Extreme) amount = 1;
            else
            {
                amount *= SeverityMultiplier[Severity]; // Scale by allergy severity
                amount *= Pawn.GetStatValue(StatDef.Named("P42_AllergicSensitivity")); // Scale by allergic sensitivity stat
                if (amount <= 0) return;
                if (amount > 1) amount = 1;
            }

            // Try to get the allergen buildup hediff from the pawn's health
            Hediff existingAllergenBuildup = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("P42_AllergenBuildup"));

            // If the hediff exists, increase its severity
            if (existingAllergenBuildup != null)
            {
                if (existingAllergenBuildup.Severity + amount > 1) existingAllergenBuildup.Severity = 1;
                else existingAllergenBuildup.Severity += amount;
            }
            else
            {
                // If the hediff is not present, add it to the pawn with the initial severity
                Hediff newBuildup = HediffMaker.MakeHediff(HediffDef.Named("P42_AllergenBuildup"), Pawn);
                newBuildup.Severity = amount;
                Pawn.health.AddHediff(newBuildup);
            }

            // If the allergy has not been visible so far make it visible
            if (!AllergyHediff.Visible)
            {
                AllergyHediff.Severity = 0.2f; // makes it visible

                // Create a letter to notify the player of the newly discovered allergy
                if (Pawn.IsColonistPlayerControlled)
                {
                    string letterLabel = "P42_LetterLabel_NewAllergyDiscovered".Translate();
                    string letterTextStart = "";

                    if (Severity == AllergySeverity.Mild)
                        letterTextStart = "P42_LetterTextStart_NewAllergyDiscovered_Mild".Translate(reasonForIncrease, Pawn.NameShortColored, Pawn.Possessive());
                    if (Severity == AllergySeverity.Moderate)
                        letterTextStart = "P42_LetterTextStart_NewAllergyDiscovered_Moderate".Translate(reasonForIncrease, Pawn.NameShortColored, Pawn.Possessive());
                    if (Severity == AllergySeverity.Severe)
                        letterTextStart = "P42_LetterTextStart_NewAllergyDiscovered_Severe".Translate(reasonForIncrease, Pawn.NameShortColored, Pawn.Possessive());
                    if (Severity == AllergySeverity.Extreme)
                        letterTextStart = "P42_LetterTextStart_NewAllergyDiscovered_Extreme".Translate(reasonForIncrease, Pawn.NameShortColored, Pawn.Possessive());

                    string letterTextEnd = "\n\n"+"P42_LetterTextEnd_AllergyDiscovered".Translate(Pawn.NameShortColored, Pawn.ProSubj(), Severity.ToString().ToLower(), FullAllergyName, Pawn.ProObj(), TypeLabelPlural);

                    string letterText = letterTextStart + letterTextEnd;
                    Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NegativeEvent, Pawn);
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref Severity, "allergySeverity");
            Scribe_Values.Look(ref TicksUntilNaturalSeverityChange, "ticksUntilNaturalSeverityChange");
            Scribe_Values.Look(ref TicksUntilAllercureImpact, "ticksUntilAllercureImpact");
            ExposeExtraData();
        }
        protected virtual void ExposeExtraData() { }
    }
}
