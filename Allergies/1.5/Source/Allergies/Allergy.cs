using RimWorld;
using RimWorld.Planet;
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

        // Exposure values
        private const float MinorPassiveExposureIncreasePerHour = 0.05f;
        private const float StrongPassiveExposureIncreasePerHour = 0.12f;
        private const float ExtremePassiveExposureIncreasePerHour = 0.30f;

        private const int TicksPerHour = 2500;
        private const int ExposureCheckInterval = 200;

        private const float MinorPassiveExposureIncreasePerCheck = MinorPassiveExposureIncreasePerHour * ((float)ExposureCheckInterval / TicksPerHour);
        private const float StrongPassiveExposureIncreasePerCheck = StrongPassiveExposureIncreasePerHour * ((float)ExposureCheckInterval / TicksPerHour);
        private const float ExtremePassiveExposureIncreasePerCheck = ExtremePassiveExposureIncreasePerHour * ((float)ExposureCheckInterval / TicksPerHour);

        private const float MinorExposureEventIncrease = 0.10f;
        private const float StrongExposureEventIncrease = 0.25f;
        private const float ExtremeExposureEventIncrease = 0.45f;

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

        #region Development

        private void ResetTicksUntilNaturalSeverityChange()
        {
            TicksUntilNaturalSeverityChange = Rand.Range(MinTicksUntilNaturalSeverityChange, MaxTicksUntilNaturalSeverityChange);
        }
        private void ResetTicksUntilAllercureSeverityChange()
        {
            TicksUntilAllercureImpact = Rand.Range(MinTicksUntilAllercureImpact, MaxTicksUntilAllercureImpact);
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
                Messages.Message("P42_Message_AllergySeverityReducedNaturally".Translate(Pawn.Name.ToStringShort, GetSeverityString()), Pawn, MessageTypeDefOf.PositiveEvent);
            }
        }
        private void IncreaseAllergySeverityNaturally()
        {
            Severity = (AllergySeverity)(((int)Severity) + 1);
            if (AllergyHediff.Visible && PawnUtility.ShouldSendNotificationAbout(Pawn))
            {
                Messages.Message("P42_Message_AllergySeverityIncreasedNaturally".Translate(Pawn.Name.ToStringShort, GetSeverityString()), Pawn, MessageTypeDefOf.NegativeEvent);
            }
        }
        private void SetToExtremeSeverityNaturally()
        {
            Severity = AllergySeverity.Extreme;
            if (AllergyHediff.Visible && PawnUtility.ShouldSendNotificationAbout(Pawn))
            {
                Messages.Message("P42_Message_AllergySeverityIncreasedNaturally".Translate(Pawn.Name.ToStringShort, GetSeverityString()), Pawn, MessageTypeDefOf.NegativeEvent);
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
                Messages.Message("P42_Message_AllergySeverityReducedAllercure".Translate(Pawn.Name.ToStringShort, Pawn.Possessive(), GetSeverityString()), Pawn, MessageTypeDefOf.PositiveEvent);
            }
        }

        #endregion

        #region Tick

        public void Tick()
        {
            // Check for natural severity change
            TicksUntilNaturalSeverityChange--;
            if (TicksUntilNaturalSeverityChange <= 0)
            {
                // Define what kind of change should occur
                float chanceForGoodOutcome = 0.5f;
                float allergicSensitivity = AllergyUtility.GetAllergicSensitivity(Pawn);
                if (allergicSensitivity == 0f) chanceForGoodOutcome = 1f;
                else if (allergicSensitivity < 1f) chanceForGoodOutcome = 0.6f;
                else if (allergicSensitivity > 1f) chanceForGoodOutcome = 0.4f;

                bool isGoodOutcome = Rand.Chance(chanceForGoodOutcome);
                bool isExtremeOutcome = Rand.Chance(ExtremeNaturalSeverityChangeChance);

                // Do the change
                if (isGoodOutcome && isExtremeOutcome)
                {
                    HealAllergyNaturally();
                    return;
                }
                else if (isGoodOutcome && !isExtremeOutcome)
                {
                    if (Severity == AllergySeverity.Mild)
                    {
                        HealAllergyNaturally();
                        return;
                    }
                    else ReduceAllergySeverityNaturally();
                }
                else if (!isGoodOutcome && !isExtremeOutcome)
                {
                    if (Severity != AllergySeverity.Extreme) IncreaseAllergySeverityNaturally();
                }
                else if (!isGoodOutcome && isExtremeOutcome)
                {
                    if (Severity != AllergySeverity.Extreme) SetToExtremeSeverityNaturally();
                }

                ResetTicksUntilNaturalSeverityChange();
            }

            // Check for allercure severity change
            if (Pawn.IsHashIntervalTick(AllercureHighCheckInterval))
            {
                Hediff allercureHigh = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("P42_AllercureHigh"));
                if (allercureHigh != null)
                {
                    TicksUntilAllercureImpact -= AllercureHighCheckInterval;
                    if (TicksUntilAllercureImpact <= 0)
                    {
                        bool isInstantHeal = Rand.Chance(AllercureInstantHealChance);

                        if (isInstantHeal)
                        {
                            HealAllergyWithAllercure();
                            return;
                        }
                        else
                        {
                            if (Severity == AllergySeverity.Mild)
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

            // Allergy-specific passive exposure checks
            if (Pawn.IsHashIntervalTick(ExposureCheckInterval))
            {
                DoPassiveExposureChecks();
            }
        }
        protected abstract void DoPassiveExposureChecks();

        #endregion

        #region Exposure

        private void MinorPassiveExposure(string cause)
        {
            float buildup = MinorPassiveExposureIncreasePerCheck;
            IncreaseAllergenBuildup(buildup, cause);
        }
        private void StrongPassiveExposure(string cause)
        {
            float buildup = StrongPassiveExposureIncreasePerCheck;
            IncreaseAllergenBuildup(buildup, cause);
        }
        private void ExtremePassiveExposure(string cause)
        {
            float buildup = ExtremePassiveExposureIncreasePerCheck;
            IncreaseAllergenBuildup(buildup, cause);
        }
        public void MinorExposureEvent(string cause)
        {
            float buildup = MinorExposureEventIncrease;
            IncreaseAllergenBuildup(buildup, cause);
        }
        public void StrongExposureEvent(string cause)
        {
            float buildup = StrongExposureEventIncrease;
            IncreaseAllergenBuildup(buildup, cause);
        }
        public void ExtremeExposureEvent(string cause)
        {
            float buildup = ExtremeExposureEventIncrease;
            IncreaseAllergenBuildup(buildup, cause);
        }

        private void IncreaseAllergenBuildup(float baseAmount, string reasonForIncrease)
        {
            if (Pawn == null || Pawn.Dead) return;

            // Calculate amount
            float amount = baseAmount;
            if (Severity == AllergySeverity.Extreme) amount = 1;
            else
            {
                amount *= SeverityMultiplier[Severity]; // Scale by allergy severity
                amount *= AllergyUtility.GetAllergicSensitivity(Pawn); // Scale by allergic sensitivity stat
                if (amount <= 0) return;
                if (amount > 1) amount = 1;
            }

            if (Prefs.DevMode) Log.Message($"[Allergies Mod] Increasing allergen buildup of {Pawn.Name} by {amount}. Allergy severity: {GetSeverityString()}. Cause: {reasonForIncrease}.");

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

                    string letterTextEnd = "\n\n"+"P42_LetterTextEnd_AllergyDiscovered".Translate(Pawn.NameShortColored, Pawn.ProSubj(), GetSeverityString(), FullAllergyName, Pawn.ProObj(), TypeLabelPlural);

                    string letterText = letterTextStart + letterTextEnd;
                    Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NegativeEvent, Pawn);
                }
            }
        }

        /// <summary>
        /// Executes the PIE (Passive Item Exposure) check for a certain item and increases allergen buildup accordingly.
        /// </summary>
        protected void DoPieCheck(Func<ThingDef, bool> ItemIdentifier)
        {
            // Get the current info of the pawn
            Map map = Pawn.Map;
            IntVec3 pawnPosition = Pawn.Position;
            Room room = Pawn.GetRoom();

            if (map != null)
            {
                // Pawn inside
                if (room == null || room.IsHuge)
                {
                    int searchRadius = 5;
                    foreach (Thing item in GenRadial.RadialDistinctThingsAround(pawnPosition, map, searchRadius, useCenter: true))
                    {
                        if (ItemIdentifier(item.def)) // Being nearby item => minor passive exposure
                        {
                            MinorPassiveExposure("P42_AllergyCause_BeingNearby".Translate(item.Label));
                            break;
                        }

                        if (item.TryGetComp<CompIngredients>() != null)
                        {
                            foreach (ThingDef ingredient in item.TryGetComp<CompIngredients>().ingredients)
                            {
                                if (ItemIdentifier(ingredient)) // Being nearby something with item as ingredient => minor passive exposure
                                {
                                    MinorPassiveExposure("P42_AllergyCause_BeingNearbyIngredient".Translate(item.Label, ingredient.label));
                                    break;
                                }
                            }
                        }
                    }
                }

                // Pawn outside
                else
                {
                    foreach (Thing item in room.ContainedAndAdjacentThings)
                    {
                        if (ItemIdentifier(item.def)) // Being in same room as item => strong passive exposure
                        {
                            StrongPassiveExposure("P42_AllergyCause_InSameRoom".Translate(item.Label));
                            break;
                        }

                        if (item.TryGetComp<CompIngredients>() != null)
                        {
                            foreach (ThingDef ingredient in item.TryGetComp<CompIngredients>().ingredients)
                            {
                                if (ItemIdentifier(ingredient)) // Being in same room as something with item as ingredient => minor passive exposure
                                {
                                    MinorPassiveExposure("P42_AllergyCause_InSameRoomIngredient".Translate(item.Label, ingredient.label));
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // Holding item
            if (Pawn.carryTracker.CarriedThing != null)
            {
                Thing carriedThing = Pawn.carryTracker.CarriedThing;
                if (carriedThing != null && ItemIdentifier(carriedThing.def)) // Holding item => extreme passive exposure
                {
                    ExtremePassiveExposure("P42_AllergyCause_Holding".Translate(carriedThing.Label));
                }

                if (carriedThing.TryGetComp<CompIngredients>() != null)
                {
                    foreach (ThingDef ingredient in carriedThing.TryGetComp<CompIngredients>().ingredients)
                    {
                        if (ItemIdentifier(ingredient)) // Holding something with item as ingredient => strong passive exposure
                        {
                            StrongPassiveExposure("P42_AllergyCause_HoldingIngredient".Translate(carriedThing.Label, ingredient.label));
                            break;
                        }
                    }
                }
            }

            // Inventory
            if (Pawn.inventory?.innerContainer != null && Pawn.inventory.innerContainer.Count > 0)
            {
                foreach (Thing item in Pawn.inventory.GetDirectlyHeldThings())
                {
                    if (ItemIdentifier(item.def)) // Having item in inventory => extreme passive exposure
                    {
                        ExtremePassiveExposure("P42_AllergyCause_InInventory".Translate(item.Label, Pawn.Possessive()));
                        break;
                    }

                    if (item.TryGetComp<CompIngredients>() != null)
                    {
                        foreach (ThingDef ingredient in item.TryGetComp<CompIngredients>().ingredients)
                        {
                            if (ItemIdentifier(ingredient)) // Having something with item as ingredient in inventory => strong passive exposure
                            {
                                StrongPassiveExposure("P42_AllergyCause_InInventoryIngredient".Translate(item.Label, Pawn.Possessive(), ingredient.label));
                                break;
                            }
                        }
                    }
                }
            }


            // Caravan inventory
            Caravan caravan = Pawn.GetCaravan();
            if (caravan != null)
            {
                foreach (Thing item in caravan.AllThings)
                {
                    if (ItemIdentifier(item.def)) // Item is in caravan inventory => minor passive exposure
                    {
                        MinorPassiveExposure("P42_AllergyCause_InInventoryCaravan".Translate(item.Label));
                        break;
                    }

                    if (item.TryGetComp<CompIngredients>() != null)
                    {
                        foreach (ThingDef ingredient in item.TryGetComp<CompIngredients>().ingredients)
                        {
                            if (ItemIdentifier(ingredient)) // Something with item as ingredient in caravan inventory => minor passive exposure
                            {
                                MinorPassiveExposure("P42_AllergyCause_InInventoryCaravanIngredient".Translate(item.Label, ingredient.label));
                                break;
                            }
                        }
                    }
                }
            }



        }

        #endregion

        /// <summary>
        /// Returns if two allergies are of the exact same type and subtype. Used to avoid generating duplicate allergies.
        /// </summary>
        public abstract bool IsDuplicateOf(Allergy otherAllergy);

        public string GetSeverityString()
        {
            if (Severity == AllergySeverity.Mild) return "P42_AllergySeverity_Mild".Translate();
            if (Severity == AllergySeverity.Moderate) return "P42_AllergySeverity_Moderate".Translate();
            if (Severity == AllergySeverity.Severe) return "P42_AllergySeverity_Severe".Translate();
            if (Severity == AllergySeverity.Extreme) return "P42_AllergySeverity_Extreme".Translate();
            return "???";
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref Severity, "allergySeverity");
            Scribe_Values.Look(ref TicksUntilNaturalSeverityChange, "ticksUntilNaturalSeverityChange");
            Scribe_Values.Look(ref TicksUntilAllercureImpact, "ticksUntilAllercureImpact");
            ExposeExtraData();
        }
        protected abstract void ExposeExtraData();
    }
}
