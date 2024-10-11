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
        private const float AllergyBuildupDiscoverThreshold = 0.1f;

        private Dictionary<AllergySeverity, float> SeverityMultiplier = new Dictionary<AllergySeverity, float>()
        {
            { AllergySeverity.Mild, 0.5f },
            { AllergySeverity.Moderate, 1f },
            { AllergySeverity.Severe, 2f },
        };


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
            OnInitOrLoad();
        }
        protected virtual void OnInitOrLoad() { }
        public void OnNewAllergyCreated(AllergySeverity severity)
        {
            Severity = severity;
            ResetTicksUntilNaturalSeverityChange();
            ResetTicksUntilAllercureSeverityChange();
        }

        protected abstract bool IsAllergenic(ThingDef thingDef);

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
            if (AllergyIsDiscovered && PawnUtility.ShouldSendNotificationAbout(Pawn))
            {
                Messages.Message("P42_Message_AllergyHealedNaturally".Translate(Pawn.Name.ToStringShort), Pawn, MessageTypeDefOf.PositiveEvent);
            }
        }
        private void ReduceAllergySeverityNaturally()
        {
            Severity = (AllergySeverity)(((int)Severity) - 1);
            if (AllergyIsDiscovered && PawnUtility.ShouldSendNotificationAbout(Pawn))
            {
                Messages.Message("P42_Message_AllergySeverityReducedNaturally".Translate(Pawn.Name.ToStringShort, GetSeverityString()), Pawn, MessageTypeDefOf.PositiveEvent);
            }
        }
        private void IncreaseAllergySeverityNaturally()
        {
            Severity = (AllergySeverity)(((int)Severity) + 1);
            if (AllergyIsDiscovered && PawnUtility.ShouldSendNotificationAbout(Pawn))
            {
                Messages.Message("P42_Message_AllergySeverityIncreasedNaturally".Translate(Pawn.Name.ToStringShort, GetSeverityString()), Pawn, MessageTypeDefOf.NegativeEvent);
            }
        }
        private void SetToExtremeSeverityNaturally()
        {
            Severity = AllergySeverity.Extreme;
            if (AllergyIsDiscovered && PawnUtility.ShouldSendNotificationAbout(Pawn))
            {
                Messages.Message("P42_Message_AllergySeverityIncreasedNaturally".Translate(Pawn.Name.ToStringShort, GetSeverityString()), Pawn, MessageTypeDefOf.NegativeEvent);
            }
        }

        private void HealAllergyWithAllercure()
        {
            Pawn.health.RemoveHediff(AllergyHediff);
            if (AllergyIsDiscovered && PawnUtility.ShouldSendNotificationAbout(Pawn))
            {
                Messages.Message("P42_Message_AllergyHealedAllercure".Translate(Pawn.Name.ToStringShort, Pawn.Possessive()), Pawn, MessageTypeDefOf.PositiveEvent);
            }
        }
        private void ReduceAllergySeverityAllercure()
        {
            Severity = (AllergySeverity)(((int)Severity) - 1);
            if (AllergyIsDiscovered && PawnUtility.ShouldSendNotificationAbout(Pawn))
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

        public void IncreaseAllergenBuildup(ExposureType exposureType, string translatedCause)
        {
            if (Pawn == null || Pawn.Dead) return;

            // Calculate amount
            float baseAmount;

            if (exposureType == ExposureType.MinorPassive) baseAmount = MinorPassiveExposureIncreasePerCheck;
            else if (exposureType == ExposureType.StrongPassive) baseAmount = StrongPassiveExposureIncreasePerCheck;
            else if (exposureType == ExposureType.ExtremePassive) baseAmount = ExtremePassiveExposureIncreasePerCheck;
            else if (exposureType == ExposureType.MinorEvent) baseAmount = MinorExposureEventIncrease;
            else if (exposureType == ExposureType.StrongEvent) baseAmount = StrongExposureEventIncrease;
            else if (exposureType == ExposureType.ExtremeEvent) baseAmount = ExtremeExposureEventIncrease;
            else return;

            float amount = baseAmount;
            if (Severity == AllergySeverity.Extreme) amount = 1;
            else
            {
                amount *= SeverityMultiplier[Severity]; // Scale by allergy severity
                amount *= AllergyUtility.GetAllergicSensitivity(Pawn); // Scale by allergic sensitivity stat
                if (amount <= 0) return;
                if (amount > 1) amount = 1;
            }

            if (Prefs.DevMode) Log.Message($"[Allergies Mod] Increasing allergen buildup of {Pawn.Name} by {amount} (exposure type: {exposureType.ToString()}). Allergy severity: {GetSeverityString()}. Cause: {translatedCause}.");

            // Create the exposure info log
            AllergyExposureInfo info = new AllergyExposureInfo(translatedCause, Find.TickManager.TicksGame);

            // Try to get the allergen buildup hediff from the pawn's health
            Hediff_AllergenBuildup existingAllergenBuildup = (Hediff_AllergenBuildup)Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("P42_AllergenBuildup"));
            float newSeverity;

            // If the hediff exists, increase its severity
            if (existingAllergenBuildup != null)
            {
                if (existingAllergenBuildup.Severity + amount > 1) existingAllergenBuildup.Severity = 1;
                else existingAllergenBuildup.Severity += amount;
                existingAllergenBuildup.AddExposureInfo(info);
                newSeverity = existingAllergenBuildup.Severity;
            }
            else
            {
                // If the hediff is not present, add it to the pawn with the initial severity
                Hediff_AllergenBuildup newBuildup = (Hediff_AllergenBuildup)HediffMaker.MakeHediff(HediffDef.Named("P42_AllergenBuildup"), Pawn);
                newBuildup.Severity = amount;
                Pawn.health.AddHediff(newBuildup);
                newBuildup.AddExposureInfo(info);
                newSeverity = newBuildup.Severity;
            }

            // If the allergy has not been visible so far make it visible
            if (!AllergyIsDiscovered && newSeverity >= AllergyBuildupDiscoverThreshold)
            {
                AllergyHediff.Severity = 0.2f; // makes the allergy hediff visible

                // Create a letter to notify the player of the newly discovered allergy
                if (Pawn.IsColonistPlayerControlled)
                {
                    SendAllergyDiscoveredLetter(translatedCause);
                }
            }
        }

        private void SendAllergyDiscoveredLetter(string translatedCause)
        {
            TaggedString letterLabel = "P42_LetterLabel_NewAllergyDiscovered".Translate();
            TaggedString letterTextStart = "";

            if (Severity == AllergySeverity.Mild)
                letterTextStart = "P42_LetterTextStart_NewAllergyDiscovered_Mild".Translate(translatedCause, Pawn.NameShortColored, Pawn.Possessive());
            if (Severity == AllergySeverity.Moderate)
                letterTextStart = "P42_LetterTextStart_NewAllergyDiscovered_Moderate".Translate(translatedCause, Pawn.NameShortColored, Pawn.Possessive());
            if (Severity == AllergySeverity.Severe)
                letterTextStart = "P42_LetterTextStart_NewAllergyDiscovered_Severe".Translate(translatedCause, Pawn.NameShortColored, Pawn.Possessive());
            if (Severity == AllergySeverity.Extreme)
                letterTextStart = "P42_LetterTextStart_NewAllergyDiscovered_Extreme".Translate(translatedCause, Pawn.NameShortColored, Pawn.Possessive());

            TaggedString letterTextMiddle = "\n\n" + Pawn.NameShortColored + " " + "P42_LetterTextMiddle_AllergyDiscovered".Translate(Pawn.ProSubj(), GetSeverityString()) + " " + FullAllergyName.Colorize(new UnityEngine.Color(0.9f, 1f, 0.6f));
            TaggedString letterTextEnd = "\n\n" + "P42_LetterTextEnd_AllergyDiscovered".Translate(Pawn.ProObj(), TypeLabel);

            TaggedString letterText = letterTextStart + letterTextMiddle + letterTextEnd;
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NegativeEvent, Pawn);
        }

        /// <summary>
        /// Executes the PIE (Passive Item Exposure) check for a certain item and increases allergen buildup accordingly.
        /// </summary>
        protected void CheckNearbyItemsForPassiveExposure(bool checkNearbyItems = true, bool checkApparel = true, bool checkInventory = true, bool checkButcherProducts = false, bool checkPlants = false)
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
                        if (checkNearbyItems)
                        {
                            CheckItemIfAllergenicAndApplyBuildup(item, "P42_AllergyCause_BeingNearby",
                                directExposure: ExposureType.MinorPassive,
                                ingredientExposure: ExposureType.MinorPassive,
                                stuffExposure: ExposureType.MinorPassive,
                                productionIngredientExposure: ExposureType.MinorPassive,
                                butcherProductExposure: checkButcherProducts ? ExposureType.MinorPassive : ExposureType.None,
                                plantExposure: checkPlants ? ExposureType.MinorPassive : ExposureType.None);
                        }

                        // Check nearby pawns
                        if (item is Pawn pawn) OnNearbyPawn(pawn);
                    }
                }

                // Pawn outside
                else
                {
                    foreach (Thing item in room.ContainedAndAdjacentThings)
                    {
                        if (checkNearbyItems)
                        {
                            CheckItemIfAllergenicAndApplyBuildup(item, "P42_AllergyCause_InSameRoom",
                                directExposure: ExposureType.StrongPassive,
                                ingredientExposure: ExposureType.MinorPassive,
                                stuffExposure: ExposureType.MinorPassive,
                                productionIngredientExposure: ExposureType.MinorPassive,
                                butcherProductExposure: checkButcherProducts ? ExposureType.MinorPassive : ExposureType.None,
                                plantExposure: checkPlants ? ExposureType.MinorPassive : ExposureType.None);
                        }

                        // Check nearby pawns
                        if (item is Pawn pawn) OnNearbyPawn(pawn);
                    }
                }
            }

            // Holding item
            if (checkInventory && Pawn.carryTracker.CarriedThing != null)
            {
                Thing carriedThing = Pawn.carryTracker.CarriedThing;

                CheckItemIfAllergenicAndApplyBuildup(carriedThing, "P42_AllergyCause_Holding",
                    directExposure: ExposureType.ExtremePassive,
                    ingredientExposure: ExposureType.StrongPassive,
                    stuffExposure: ExposureType.StrongPassive,
                    productionIngredientExposure: ExposureType.MinorPassive,
                    butcherProductExposure: ExposureType.None,
                    plantExposure: ExposureType.None);
            }

            // Inventory
            if (checkInventory && Pawn.inventory?.innerContainer != null && Pawn.inventory.innerContainer.Count > 0)
            {
                foreach (Thing item in Pawn.inventory.GetDirectlyHeldThings())
                {
                    CheckItemIfAllergenicAndApplyBuildup(item, "P42_AllergyCause_InInventory",
                        directExposure: ExposureType.ExtremePassive,
                        ingredientExposure: ExposureType.StrongPassive,
                        stuffExposure: ExposureType.StrongPassive,
                        productionIngredientExposure: ExposureType.MinorPassive,
                        butcherProductExposure: ExposureType.None,
                        plantExposure: ExposureType.None);
                }
            }


            // Caravan inventory
            if (checkInventory)
            {
                Caravan caravan = Pawn.GetCaravan();
                if (caravan != null)
                {
                    foreach (Thing item in caravan.AllThings)
                    {
                        CheckItemIfAllergenicAndApplyBuildup(item, "P42_AllergyCause_InInventoryCaravan",
                            directExposure: ExposureType.MinorPassive,
                            ingredientExposure: ExposureType.MinorPassive,
                            stuffExposure: ExposureType.MinorPassive,
                            productionIngredientExposure: ExposureType.MinorPassive,
                            butcherProductExposure: ExposureType.None,
                            plantExposure: ExposureType.None);
                    }
                }
            }

            // Apparel
            if (checkApparel && Pawn.apparel != null && Pawn.apparel.WornApparel != null)
            {
                foreach (Apparel apparelItem in Pawn.apparel.WornApparel)
                {
                    if (apparelItem.Stuff != null)
                    {
                        if (IsAllergenic(apparelItem.Stuff))
                        {
                            // Wearing apparel made out of item => extreme passive exposure
                            IncreaseAllergenBuildup(ExposureType.ExtremePassive, "P42_AllergyCause_Wearing".Translate(apparelItem.LabelNoParenthesis));
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets called when nearby another pawn
        /// </summary>
        protected virtual void OnNearbyPawn(Pawn pawn) { }

        /// <summary>
        /// Checks if an item is allergenic and applies the corresponding allergen buildup.
        /// </summary>
        public void CheckItemIfAllergenicAndApplyBuildup(Thing thing, string causeKey, ExposureType directExposure, ExposureType ingredientExposure, ExposureType stuffExposure, ExposureType productionIngredientExposure, ExposureType butcherProductExposure, ExposureType plantExposure)
        {
            ExposureType exposure = GetAllergicExposureOfThing(thing, causeKey, out string translatedCause, directExposure, ingredientExposure, stuffExposure, productionIngredientExposure, butcherProductExposure, plantExposure);
            if(exposure != ExposureType.None) IncreaseAllergenBuildup(exposure, translatedCause);
        }

        /// <summary>
        /// Checks an item, its stuff, ingredients, production ingredients, butcher products and plant yield for if they are allergenic.
        /// Returns the exposure severity and translated cause key based on what about the item is allergenic.
        /// </summary>
        public ExposureType GetAllergicExposureOfThing(Thing thing, string causeKey, out string translatedCause, ExposureType directExposure, ExposureType ingredientExposure, ExposureType stuffExposure, ExposureType productionIngredientExposure, ExposureType butcherProductExposure, ExposureType plantExposure)
        {
            translatedCause = "";

            // Check if item itself is allergenic
            if (directExposure != ExposureType.None && IsAllergenic(thing.def))
            {
                translatedCause = causeKey.Translate(thing.LabelNoParenthesis);
                return directExposure;
            }

            // Check if an ingredient (food) is allergenic
            if (ingredientExposure != ExposureType.None && thing.TryGetComp<CompIngredients>() != null)
            {
                foreach (ThingDef ingredient in thing.TryGetComp<CompIngredients>().ingredients)
                {
                    if (IsAllergenic(ingredient))
                    {
                        translatedCause = causeKey.Translate(thing.LabelNoParenthesis + " (" + "P42_AllergyCause_Suffix_Ingredient".Translate(ingredient.label) + ")");
                        return ingredientExposure;
                    }
                }
            }

            // Check if item stuff is allergenic
            if (stuffExposure != ExposureType.None && thing.Stuff != null && IsAllergenic(thing.Stuff))
            {
                translatedCause = causeKey.Translate(thing.LabelNoParenthesis);
                return stuffExposure;
            }

            // Check if production ingredient is allergenic
            if (productionIngredientExposure != ExposureType.None)
            {
                List<ThingDef> productionIngredients = AllergyUtility.GetProductionIngredients(thing.def);
                foreach (ThingDef ingredient in productionIngredients)
                {
                    if (IsAllergenic(ingredient))
                    {
                        translatedCause = causeKey.Translate(thing.LabelNoParenthesis + " (" + "P42_AllergyCause_Suffix_MadeOutOf".Translate(ingredient.label) + ")");
                        return productionIngredientExposure;
                    }
                }
            }

            // Check butcher products
            if (butcherProductExposure != ExposureType.None && thing.def.butcherProducts != null)
            {
                foreach (ThingDefCountClass tdcc in thing.def.butcherProducts)
                {
                    if (IsAllergenic(tdcc.thingDef))
                    {
                        translatedCause = causeKey.Translate(thing.LabelNoParenthesis);
                        return butcherProductExposure;
                    }
                }
            }

            // Check harvest yield
            if(plantExposure != ExposureType.None && thing.def.plant != null && thing.def.plant.harvestedThingDef != null)
            {
                if (IsAllergenic(thing.def.plant.harvestedThingDef))
                {
                    translatedCause = causeKey.Translate(thing.LabelNoParenthesis);
                    return plantExposure;
                }
            }

            return ExposureType.None;
        }

        // Harmony patches
        public virtual void OnDamageTaken(DamageInfo dinfo) { }
        public virtual void OnInteractedWith(Pawn pawn) { }
        public void OnThingIngested(Thing thing)
        {
            CheckItemIfAllergenicAndApplyBuildup(thing, "P42_AllergyCause_Ingested",
                directExposure: ExposureType.ExtremeEvent,
                ingredientExposure: ExposureType.StrongEvent,
                stuffExposure: ExposureType.StrongEvent,
                productionIngredientExposure: ExposureType.StrongEvent,
                butcherProductExposure: ExposureType.None,
                plantExposure: ExposureType.None);
        }
        public void OnTendedWith(Thing medicine)
        {
            CheckItemIfAllergenicAndApplyBuildup(medicine, "P42_AllergyCause_Tended",
                directExposure: ExposureType.ExtremeEvent,
                ingredientExposure: ExposureType.StrongEvent,
                stuffExposure: ExposureType.StrongEvent,
                productionIngredientExposure: ExposureType.StrongEvent,
                butcherProductExposure: ExposureType.None,
                plantExposure: ExposureType.None);
        }
        public void OnRecipeApplied(Thing recipeIngredient)
        {
            CheckItemIfAllergenicAndApplyBuildup(recipeIngredient, "P42_AllergyCause_RecipeApplied",
                directExposure: ExposureType.ExtremeEvent,
                ingredientExposure: ExposureType.StrongEvent,
                stuffExposure: ExposureType.StrongEvent,
                productionIngredientExposure: ExposureType.StrongEvent,
                butcherProductExposure: ExposureType.None,
                plantExposure: ExposureType.None);
        }

        #endregion

        #region Getters

        /// <summary>
        /// Returns if two allergies are of the exact same type and subtype. Used to avoid generating duplicate allergies.
        /// </summary>
        public abstract bool IsDuplicateOf(Allergy otherAllergy);

        public bool AllergyIsDiscovered => AllergyHediff.Visible;
        public string GetSeverityString()
        {
            if (Severity == AllergySeverity.Mild) return "P42_AllergySeverity_Mild".Translate();
            if (Severity == AllergySeverity.Moderate) return "P42_AllergySeverity_Moderate".Translate();
            if (Severity == AllergySeverity.Severe) return "P42_AllergySeverity_Severe".Translate();
            if (Severity == AllergySeverity.Extreme) return "P42_AllergySeverity_Extreme".Translate();
            return "???";
        }
        public abstract string TypeLabel { get; }
        public void ExposeData()
        {
            Scribe_Values.Look(ref Severity, "allergySeverity");
            Scribe_Values.Look(ref TicksUntilNaturalSeverityChange, "ticksUntilNaturalSeverityChange");
            Scribe_Values.Look(ref TicksUntilAllercureImpact, "ticksUntilAllercureImpact");
            ExposeExtraData();
        }
        protected abstract void ExposeExtraData();

        #endregion
    }
}
