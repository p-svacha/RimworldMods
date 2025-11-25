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
        public AllergyDef Def;

        private const float AllergyBuildupDiscoverThreshold = 0.1f;
        protected const int AllercureHighCheckInterval = 1000;

        // Development values
        private const int MinTicksUntilNaturalSeverityChange = 20 * 60000; // 20 days
        private const int MaxTicksUntilNaturalSeverityChange = 180 * 60000; // 180 days (3 years)
        private const float ExtremeNaturalSeverityChangeChance = 0.05f; // Chance that an allergy instantly goes to extreme / goes away

        private const int MinTicksUntilAllercureImpact = 5 * 60000; // 5 days
        private const int MaxTicksUntilAllercureImpact = 50 * 60000; // 50 days
        private const float AllercureInstantHealChance = 0.1f;

        // Exposure values
        private const int NearbyItemsSearchRadius = 5;
        private const int NearbyFloorsSearchRadius = 3;

        private const int ExposureCheckInterval = 200;

        private float MinorPassiveExposureIncreasePerCheck;
        private float StrongPassiveExposureIncreasePerCheck;
        private float ExtremePassiveExposureIncreasePerCheck;

        private const float AnaphylacticShockIncreaseFactor = 0.1f; // Each allergen buildup increase also icreases anaphylactic shock severity to a lesser extent (multiplied by this factor)

        protected Hediff_Allergy AllergyHediff;
        
        public AllergySeverity Severity;
        public int TicksUntilNaturalSeverityChange;
        public int TicksUntilAllercureImpact;

        // Cache
        private Dictionary<ThingDef, int> NumChecksPerThing = new Dictionary<ThingDef, int>(); // stores how many times a specific ThingDef has been checked for exposure in a single check
        private Dictionary<TerrainDef, int> NumChecksPerTerrain = new Dictionary<TerrainDef, int>(); // stores how many times a specific TerrainDef has been checked for exposure in a single check

        // Immunity
        private const int ImmunityDurationAfterSpawning = 60000; // 1 day
        public int ArrivalTick;

        public void OnInitOrLoad(Hediff_Allergy hediff)
        {
            AllergyHediff = hediff;
            OnInitOrLoad();

            // Init label and description
            if(LanguageDatabase.activeLanguage?.folderName == "English")
                hediff.description = hediff.def.Description.Replace("an allergen", TypeLabel).Replace("that allergen", KeepAwayFromText);

            // Calculate passive exposure values
            MinorPassiveExposureIncreasePerCheck = Def.minorPassiveExposure_increasePerHour * ((float)ExposureCheckInterval / GenDate.TicksPerHour);
            StrongPassiveExposureIncreasePerCheck = Def.strongPassiveExposure_increasePerHour * ((float)ExposureCheckInterval / GenDate.TicksPerHour);
            ExtremePassiveExposureIncreasePerCheck = Def.extremePassiveExposure_increasePerHour * ((float)ExposureCheckInterval / GenDate.TicksPerHour);
        }
        protected virtual void OnInitOrLoad() { }
        public void OnNewAllergyCreated(AllergyDef def)
        {
            Def = def;
            SetSeverityOnCreation();
            OnCreate();

            ResetTicksUntilNaturalSeverityChange();
            ResetTicksUntilAllercureSeverityChange();
        }

        private void SetSeverityOnCreation()
        {
            Dictionary<AllergySeverity, float> weights = new Dictionary<AllergySeverity, float>();
            weights.Add(AllergySeverity.Mild, Def.severityCommonness_mild);
            weights.Add(AllergySeverity.Moderate, Def.severityCommonness_moderate);
            weights.Add(AllergySeverity.Severe, Def.severityCommonness_severe);
            weights.Add(AllergySeverity.Extreme, Def.severityCommonness_extreme);
            Severity = Utils.GetWeightedRandomElement(weights);
        }

        protected virtual void OnCreate() { }

        protected abstract bool IsDirectlyAllergenic(ThingDef thingDef);

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
            if (IsAllergyDiscovered && Utils.ShouldSendAllergyNotification(Pawn))
            {
                Messages.Message("P42_Message_AllergyHealedNaturally".Translate(Pawn.Name.ToStringShort), Pawn, MessageTypeDefOf.PositiveEvent);
            }

            if(IsAllergyDiscovered) Utils.ApplyMemoryThought(Pawn, "P42_AllergyCured");
        }
        private void ReduceAllergySeverityNaturally()
        {
            Severity = (AllergySeverity)(((int)Severity) - 1);
            if (IsAllergyDiscovered && Utils.ShouldSendAllergyNotification(Pawn))
            {
                Messages.Message("P42_Message_AllergySeverityReducedNaturally".Translate(Pawn.Name.ToStringShort, GetSeverityString()), Pawn, MessageTypeDefOf.PositiveEvent);
            }

            if(IsAllergyDiscovered) Utils.ApplyMemoryThought(Pawn, "P42_AllergyImproved");
        }
        private void IncreaseAllergySeverityNaturally()
        {
            Severity = (AllergySeverity)(((int)Severity) + 1);
            if (IsAllergyDiscovered && Utils.ShouldSendAllergyNotification(Pawn))
            {
                Messages.Message("P42_Message_AllergySeverityIncreasedNaturally".Translate(Pawn.Name.ToStringShort, GetSeverityString()), Pawn, MessageTypeDefOf.NegativeEvent);
            }

            if(IsAllergyDiscovered) Utils.ApplyMemoryThought(Pawn, "P42_AllergyWorsened");
        }
        private void SetToSevereSeverityNaturally()
        {
            Severity = AllergySeverity.Severe;
            if (IsAllergyDiscovered && Utils.ShouldSendAllergyNotification(Pawn))
            {
                Messages.Message("P42_Message_AllergySeverityIncreasedNaturally".Translate(Pawn.Name.ToStringShort, GetSeverityString()), Pawn, MessageTypeDefOf.NegativeEvent);
                
            }

            if(IsAllergyDiscovered) Utils.ApplyMemoryThought(Pawn, "P42_AllergyWorsened");
        }

        private void HealAllergyWithAllercure()
        {
            Pawn.health.RemoveHediff(AllergyHediff);
            if (IsAllergyDiscovered && Utils.ShouldSendAllergyNotification(Pawn))
            {
                Messages.Message("P42_Message_AllergyHealedAllercure".Translate(Pawn.Name.ToStringShort, Pawn.Possessive()), Pawn, MessageTypeDefOf.PositiveEvent);
            }

            if(IsAllergyDiscovered) Utils.ApplyMemoryThought(Pawn, "P42_AllergyCured");
        }
        private void ReduceAllergySeverityAllercure()
        {
            Severity = (AllergySeverity)(((int)Severity) - 1);
            if (IsAllergyDiscovered && Utils.ShouldSendAllergyNotification(Pawn))
            {
                Messages.Message("P42_Message_AllergySeverityReducedAllercure".Translate(Pawn.Name.ToStringShort, Pawn.Possessive(), GetSeverityString()), Pawn, MessageTypeDefOf.PositiveEvent);
            }

            if(IsAllergyDiscovered) Utils.ApplyMemoryThought(Pawn, "P42_AllergyImproved");
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
                float allergicSensitivity = Utils.GetAllergicSensitivity(Pawn);
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
                    if (Severity != AllergySeverity.Extreme && Severity != AllergySeverity.Severe) IncreaseAllergySeverityNaturally();
                }
                else if (!isGoodOutcome && isExtremeOutcome)
                {
                    if (Severity != AllergySeverity.Extreme && Severity != AllergySeverity.Severe) SetToSevereSeverityNaturally();
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
                if (!IsBuildupAboveCap())
                {
                    NumChecksPerThing.Clear();
                    NumChecksPerTerrain.Clear();

                    DoPassiveExposureChecks();
                }
            }
        }
        protected abstract void DoPassiveExposureChecks();

        #endregion

        #region Exposure

        public void IncreaseAllergenBuildup(ExposureType exposureType, string translatedCause)
        {
            if (IsImmuneToBuildup()) return;

            // Calculate amount
            float baseAmount;

            if (exposureType == ExposureType.MinorPassive) baseAmount = MinorPassiveExposureIncreasePerCheck;
            else if (exposureType == ExposureType.StrongPassive) baseAmount = StrongPassiveExposureIncreasePerCheck;
            else if (exposureType == ExposureType.ExtremePassive) baseAmount = ExtremePassiveExposureIncreasePerCheck;
            else if (exposureType == ExposureType.MinorEvent) baseAmount = Def.minorExposureEvent_instantIncrease;
            else if (exposureType == ExposureType.StrongEvent) baseAmount = Def.strongExposureEvent_instantIncrease;
            else if (exposureType == ExposureType.ExtremeEvent) baseAmount = Def.extremeExposureEvent_instantIncrease;
            else return;

            float amount = baseAmount;
            if (Severity == AllergySeverity.Extreme) amount = 1;
            else
            {
                amount *= GetSeverityMultiplier(); // Scale by allergy severity
                amount *= Utils.GetAllergicSensitivity(Pawn); // Scale by allergic sensitivity stat
                if (amount <= 0) return;
                if (amount > 1) amount = 1;
            }

            // Logger.Log($"Increasing allergen buildup of {Pawn.Name} by {amount} (exposure type: {exposureType.ToString()}). Allergy severity: {GetSeverityString()}. Cause: {translatedCause}.");

            // Increase anaphylactic shock severity
            Hediff anaphylacticShock = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("P42_AnaphylacticShock"));
            if(anaphylacticShock != null)
            {
                float anaShockIncrease = (amount * AnaphylacticShockIncreaseFactor);
                anaphylacticShock.Severity += anaShockIncrease;
                // Logger.Log($"Icreasing anaphylactic shock severity of {Pawn.Name} by {anaShockIncrease}.");
            }

            // Create the exposure info log
            AllergyExposureInfo info = new AllergyExposureInfo(translatedCause, Find.TickManager.TicksGame);

            // Try to get the allergen buildup hediff from the pawn's health
            Hediff_AllergenBuildup existingAllergenBuildup = (Hediff_AllergenBuildup)Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("P42_AllergenBuildup"));
            float newSeverity;
            float severityCap = GetBuildupCap();

            // If the hediff exists, increase its severity
            if (existingAllergenBuildup != null)
            {
                if (existingAllergenBuildup.Severity >= severityCap) return;

                if (existingAllergenBuildup.Severity + amount > severityCap) existingAllergenBuildup.Severity = severityCap;
                else existingAllergenBuildup.Severity += amount;
                existingAllergenBuildup.AddExposureInfo(info);
                newSeverity = existingAllergenBuildup.Severity;
            }
            else
            {
                // If the hediff is not present, add it to the pawn with the initial severity
                Hediff_AllergenBuildup newBuildup = (Hediff_AllergenBuildup)HediffMaker.MakeHediff(HediffDef.Named("P42_AllergenBuildup"), Pawn);
                if (amount > severityCap) amount = severityCap;
                newBuildup.Severity = amount;
                Pawn.health.AddHediff(newBuildup);
                newBuildup.AddExposureInfo(info);
                newSeverity = newBuildup.Severity;

                
            }

            // If the allergy has not been visible so far make it visible
            if (!IsAllergyDiscovered && newSeverity >= AllergyBuildupDiscoverThreshold)
            {
                MakeAllergyVisible();

                // Create a letter to notify the player of the newly discovered allergy
                if (Utils.ShouldSendAllergyNotification(Pawn))
                {
                    SendAllergyDiscoveredLetter(translatedCause);
                }

                // Add thought
                Utils.ApplyMemoryThought(Pawn, "P42_AllergyDiscovered");
            }
        }

        private float GetSeverityMultiplier()
        {
            if (Severity == AllergySeverity.Mild) return Def.exposureMultiplier_mild;
            if (Severity == AllergySeverity.Moderate) return Def.exposureMultiplier_moderate;
            if (Severity == AllergySeverity.Severe) return Def.exposureMultiplier_severe;
            if (Severity == AllergySeverity.Extreme) return 2f;

            return 0f;
        }

        public void SetArrivalTickToNow()
        {
            ArrivalTick = Find.TickManager.TicksGame;
        }
        public bool IsImmuneToBuildup()
        {
            if (Pawn == null || Pawn.Dead) return true;

            // Non-player pawns that just appeared on the map are immune
            if (!Pawn.IsColonistPlayerControlled)
            {
                int ticksSinceArrival = Find.TickManager.TicksGame - ArrivalTick;
                if (ticksSinceArrival < ImmunityDurationAfterSpawning) return true;
            }

            return false;
        }

        public void MakeAllergyVisible()
        {
            AllergyHediff.Severity = 0.2f;
        }

        private bool IsBuildupAboveCap()
        {
            float buildupCap = GetBuildupCap();
            Hediff_AllergenBuildup existingAllergenBuildup = (Hediff_AllergenBuildup)Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("P42_AllergenBuildup"));
            if (existingAllergenBuildup != null && existingAllergenBuildup.Severity > buildupCap) return true;
            return false;
        }
        private float GetBuildupCap()
        {
            if (Severity == AllergySeverity.Mild) return Def.mildSeverityReactionsCap;
            if (Severity == AllergySeverity.Moderate) return Def.moderateSeverityReactionsCap;
            return 1f;
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

            TaggedString letterTextMiddle = "\n\n" + Pawn.NameShortColored + " " + "P42_LetterTextMiddle_AllergyDiscovered".Translate(Pawn.ProSubj()) + " " + (GetSeverityString() + " " + FullAllergyName).Colorize(new UnityEngine.Color(0.9f, 1f, 0.6f));
            TaggedString letterTextEnd = "\n\n" + "P42_LetterTextEnd_AllergyDiscovered".Translate(Pawn.ProObj(), KeepAwayFromText) + "\n\n" + "P42_LetterTextEnd2_AllergyDiscovered".Translate(Pawn.NameShortColored, Pawn.ProObj());

            TaggedString letterText = letterTextStart + letterTextMiddle + letterTextEnd;
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NegativeEvent, Pawn);
        }

        /// <summary>
        /// Checks all items nearby a pawn, in the same room, equipped, holding, inventory, caravan and apparel for exposure.
        /// </summary>
        protected void CheckNearbyThingsForPassiveExposure(bool checkNearbyItems = true, bool checkApparel = true, bool checkInventory = true, bool checkProductionIngredients = true, bool checkButcherProducts = false, bool checkPlants = false, bool checkMineableThings = false)
        {
            // Get the current info of the pawn
            Map map = Pawn.Map;
            IntVec3 pawnPosition = Pawn.Position;
            Room room = Pawn.GetRoom();

            // Nearby things
            if (map != null)
            {
                foreach (Thing item in GenRadial.RadialDistinctThingsAround(pawnPosition, map, NearbyItemsSearchRadius, useCenter: true))
                {
                    if (item == Pawn) continue; // No exposure to self
                    if (item.GetRoom() != room) continue; // No exposure to nearby things in other rooms

                    bool inSameRoom = (room != null && !room.IsHuge);
                    string causeKey = inSameRoom ? "P42_AllergyCause_InSameRoom" : "P42_AllergyCause_BeingNearby";
                    ExposureType exposureStrength = inSameRoom ? ExposureType.StrongPassive : ExposureType.MinorPassive;

                    if (checkNearbyItems)
                    {
                        CheckThingIfAllergenicAndApplyBuildup(item, causeKey,
                            directExposure: exposureStrength,
                            ingredientExposure: exposureStrength,
                            stuffExposure: exposureStrength,
                            productionIngredientExposure: checkProductionIngredients ? exposureStrength : ExposureType.None,
                            butcherProductExposure: checkButcherProducts ? exposureStrength : ExposureType.None,
                            plantExposure: checkPlants ? exposureStrength : ExposureType.None,
                            mineableThingExposure: checkMineableThings ? exposureStrength : ExposureType.None);
                    }

                    // Check nearby pawns
                    if (item is Pawn nearbyPawn)
                    {
                        DoPassiveExposureCheckOnNearbyPawn(nearbyPawn, checkInventory, checkApparel, checkProductionIngredients);
                    }
                }
            }

            // Holding item
            if (checkInventory && Pawn.carryTracker != null && Pawn.carryTracker.CarriedThing != null)
            {
                Thing carriedThing = Pawn.carryTracker.CarriedThing;

                CheckThingIfAllergenicAndApplyBuildup(carriedThing, "P42_AllergyCause_Holding",
                    directExposure: ExposureType.ExtremePassive,
                    ingredientExposure: ExposureType.StrongPassive,
                    stuffExposure: ExposureType.StrongPassive,
                    productionIngredientExposure: checkProductionIngredients ? ExposureType.MinorPassive : ExposureType.None);
            }

            // Equipped item
            if (checkInventory && Pawn.equipment != null && Pawn.equipment.Primary != null)
            {
                Thing carriedThing = Pawn.equipment.Primary;

                CheckThingIfAllergenicAndApplyBuildup(carriedThing, "P42_AllergyCause_Equipped",
                    directExposure: ExposureType.ExtremePassive,
                    ingredientExposure: ExposureType.StrongPassive,
                    stuffExposure: ExposureType.StrongPassive,
                    productionIngredientExposure: ExposureType.MinorPassive);
            }

            // Inventory
            if (checkInventory && Pawn.inventory?.innerContainer != null && Pawn.inventory.innerContainer.Count > 0)
            {
                foreach (Thing item in Pawn.inventory.GetDirectlyHeldThings())
                {
                    CheckThingIfAllergenicAndApplyBuildup(item, "P42_AllergyCause_InInventory",
                        directExposure: ExposureType.ExtremePassive,
                        ingredientExposure: ExposureType.StrongPassive,
                        stuffExposure: ExposureType.StrongPassive,
                        productionIngredientExposure: checkProductionIngredients ? ExposureType.MinorPassive : ExposureType.None);
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
                        CheckThingIfAllergenicAndApplyBuildup(item, "P42_AllergyCause_InInventoryCaravan",
                            directExposure: ExposureType.MinorPassive,
                            ingredientExposure: ExposureType.MinorPassive,
                            stuffExposure: ExposureType.MinorPassive,
                            productionIngredientExposure: checkProductionIngredients ? ExposureType.MinorPassive : ExposureType.None);
                    }
                }
            }

            // Apparel
            if (checkApparel && Pawn.apparel != null && Pawn.apparel.WornApparel != null)
            {
                foreach (Apparel apparelItem in Pawn.apparel.WornApparel)
                {
                    CheckThingIfAllergenicAndApplyBuildup(apparelItem, "P42_AllergyCause_Wearing",
                        directExposure: ExposureType.None,
                        ingredientExposure: ExposureType.None,
                        stuffExposure: ExposureType.MinorPassive,
                        productionIngredientExposure: checkProductionIngredients ? ExposureType.MinorPassive : ExposureType.None);
                }
            }
        }
        protected void CheckNearbyFloorsForPassiveExposure()
        {
            Map map = Pawn.Map;
            if (map == null) return;

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(Pawn.Position, NearbyFloorsSearchRadius, useCenter: true))
            {
                if (!cell.InBounds(map)) continue;
                TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
                if (terrain == null) continue;

                // How many times has this specific terrain been checked already this check
                if (NumChecksPerTerrain.ContainsKey(terrain)) NumChecksPerTerrain[terrain]++;
                else NumChecksPerTerrain.Add(terrain, 1);
                if (NumChecksPerTerrain[terrain] > Def.maxPassiveExposureTriggersForSameThingPerCheck) continue;

                // General allergy-specific check
                if(IsTerrainAllergenic(terrain)) IncreaseAllergenBuildup(ExposureType.MinorPassive, "P42_AllergyCause_BeingNearby".Translate(terrain.label));
            }
        }

        public virtual bool IsTerrainAllergenic(TerrainDef terrain)
        {
            if (terrain.costList != null)
            {
                foreach (ThingDefCountClass tdcc in terrain.costList)
                {
                    if (IsDirectlyAllergenic(tdcc.thingDef))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void DoPassiveExposureCheckOnNearbyPawn(Pawn nearbyPawn, bool checkInventory, bool checkApparel, bool checkProductionIngredients)
        {
            OnNearbyPawn(nearbyPawn); // allergy-specific logic

            // Nearby pawn holding item
            if (checkInventory && nearbyPawn.carryTracker != null && nearbyPawn.carryTracker.CarriedThing != null)
            {
                Thing carriedThing = nearbyPawn.carryTracker.CarriedThing;

                CheckThingIfAllergenicAndApplyBuildup(carriedThing, "P42_AllergyCause_NearbyHolding",
                    directExposure: ExposureType.MinorPassive,
                    ingredientExposure: ExposureType.MinorPassive,
                    stuffExposure: ExposureType.MinorPassive,
                    productionIngredientExposure: checkProductionIngredients ? ExposureType.MinorPassive : ExposureType.None);
            }

            // Nearby pawn equipped item
            if (checkInventory && nearbyPawn.equipment != null && nearbyPawn.equipment.Primary != null)
            {
                Thing carriedThing = nearbyPawn.equipment.Primary;

                CheckThingIfAllergenicAndApplyBuildup(carriedThing, "P42_AllergyCause_NearbyEquipped",
                    directExposure: ExposureType.ExtremePassive,
                    ingredientExposure: ExposureType.StrongPassive,
                    stuffExposure: ExposureType.StrongPassive,
                    productionIngredientExposure: checkProductionIngredients ? ExposureType.MinorPassive : ExposureType.None);
            }

            // Nearby pawn apparel
            if (checkApparel && nearbyPawn.apparel != null && nearbyPawn.apparel.WornApparel != null)
            {
                foreach (Apparel apparelItem in nearbyPawn.apparel.WornApparel)
                {
                    CheckThingIfAllergenicAndApplyBuildup(apparelItem, "P42_AllergyCause_NearbyWearing",
                        directExposure: ExposureType.None,
                        ingredientExposure: ExposureType.None,
                        stuffExposure: ExposureType.MinorPassive,
                        productionIngredientExposure: checkProductionIngredients ? ExposureType.MinorPassive : ExposureType.None);
                }
            }
        }

        /// <summary>
        /// Gets called when nearby another pawn
        /// </summary>
        protected virtual void OnNearbyPawn(Pawn nearbyPawn) { }

        /// <summary>
        /// Checks if an item is allergenic and applies the corresponding allergen buildup.
        /// </summary>
        public void CheckThingIfAllergenicAndApplyBuildup(Thing thing, string causeKey, ExposureType directExposure, ExposureType ingredientExposure, ExposureType stuffExposure, ExposureType productionIngredientExposure, ExposureType butcherProductExposure = ExposureType.None, ExposureType plantExposure = ExposureType.None, ExposureType mineableThingExposure = ExposureType.None)
        {
            if (thing == null) return;

            // How many times has this specific thing been checked already this check
            if (NumChecksPerThing.ContainsKey(thing.def)) NumChecksPerThing[thing.def]++;
            else NumChecksPerThing.Add(thing.def, 1);
            if (NumChecksPerThing[thing.def] > Def.maxPassiveExposureTriggersForSameThingPerCheck) return;

            // Get and apply exposure
            ExposureType exposure = GetAllergicExposureOfThing(thing, causeKey, out string translatedCause, directExposure, ingredientExposure, stuffExposure, productionIngredientExposure, butcherProductExposure, plantExposure, mineableThingExposure);
            if(exposure != ExposureType.None) IncreaseAllergenBuildup(exposure, translatedCause);
        }

        /// <summary>
        /// Returns a thing would increase the allergic reactions for a pawn.
        /// </summary>
        public bool IsAllergenic(Thing thing, bool checkFoodIngredients = false, bool checkStuff = false, bool checkProductionIngredients = false, bool checkButcherProducts = false, bool checkPlantYields = false, bool checkMiningYield = false)
        {
            ExposureType exposure = GetAllergicExposureOfThing(thing, "", out _, directExposure: ExposureType.MinorPassive, ingredientExposure: checkFoodIngredients ? ExposureType.MinorPassive : ExposureType.None, stuffExposure: checkStuff ? ExposureType.MinorPassive : ExposureType.None, productionIngredientExposure: checkProductionIngredients ? ExposureType.MinorPassive : ExposureType.None, butcherProductExposure: checkButcherProducts ? ExposureType.MinorPassive : ExposureType.None, plantExposure: checkPlantYields ? ExposureType.MinorPassive : ExposureType.None, mineableThingExposure: checkMiningYield ? ExposureType.MinorPassive : ExposureType.None);
            return exposure != ExposureType.None;
        }

        public bool IsAllergenic(ThingDef def)
        {
            // Directly
            if (IsDirectlyAllergenic(def)) return true;

            // Production ingredients
            List<ThingDef> productionIngredients = Utils.GetProductionIngredients(def);
            foreach (ThingDef ingredient in productionIngredients)
            {
                if (IsDirectlyAllergenic(ingredient)) return true;
            }

            // Butcher products
            if (def.butcherProducts != null)
            {
                foreach (ThingDefCountClass tdcc in def.butcherProducts)
                {
                    if (IsDirectlyAllergenic(tdcc.thingDef)) return true;
                }
            }

            // Plant yield
            if (def.plant != null && def.plant.harvestedThingDef != null && IsDirectlyAllergenic(def.plant.harvestedThingDef)) return true;

            // Mine yield
            if (def.building != null && def.building.mineableThing != null && IsDirectlyAllergenic(def.building.mineableThing)) return true;

            return false;
        }

        /// <summary>
        /// Checks an item, its stuff, ingredients, production ingredients, butcher products and plant yield for if they are allergenic.
        /// Returns the exposure severity and translated cause key based on what about the item is allergenic.
        /// </summary>
        public ExposureType GetAllergicExposureOfThing(Thing thing, string causeKey, out string translatedCause, ExposureType directExposure, ExposureType ingredientExposure, ExposureType stuffExposure, ExposureType productionIngredientExposure, ExposureType butcherProductExposure, ExposureType plantExposure, ExposureType mineableThingExposure)
        {
            translatedCause = "";

            // Check if item itself is allergenic
            if (directExposure != ExposureType.None && IsDirectlyAllergenic(thing.def))
            {
                translatedCause = causeKey.Translate(thing.LabelNoParenthesis);
                return directExposure;
            }

            // Check if an ingredient (food) is allergenic
            if (ingredientExposure != ExposureType.None && thing.TryGetComp<CompIngredients>() != null)
            {
                foreach (ThingDef ingredient in thing.TryGetComp<CompIngredients>().ingredients)
                {
                    if (IsDirectlyAllergenic(ingredient))
                    {
                        translatedCause = causeKey.Translate(thing.LabelNoParenthesis + " (" + "P42_AllergyCause_Suffix_Ingredient".Translate(ingredient.label) + ")");
                        return ingredientExposure;
                    }
                }
            }

            // Check if item stuff is allergenic
            if (stuffExposure != ExposureType.None && thing.Stuff != null && IsDirectlyAllergenic(thing.Stuff))
            {
                translatedCause = causeKey.Translate(thing.LabelNoParenthesis);
                return stuffExposure;
            }

            // Check if production ingredient is allergenic
            if (productionIngredientExposure != ExposureType.None)
            {
                List<ThingDef> productionIngredients = Utils.GetProductionIngredients(thing.def);
                foreach (ThingDef ingredient in productionIngredients)
                {
                    if (IsDirectlyAllergenic(ingredient))
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
                    if (IsDirectlyAllergenic(tdcc.thingDef))
                    {
                        translatedCause = causeKey.Translate(thing.LabelNoParenthesis);
                        return butcherProductExposure;
                    }
                }
            }

            // Check harvest yield
            if(plantExposure != ExposureType.None && thing.def.plant != null && thing.def.plant.harvestedThingDef != null)
            {
                if (IsDirectlyAllergenic(thing.def.plant.harvestedThingDef))
                {
                    translatedCause = causeKey.Translate(thing.LabelNoParenthesis);
                    return plantExposure;
                }
            }

            // Mineable thing
            if(mineableThingExposure != ExposureType.None && thing.def.building != null && thing.def.building.mineableThing != null)
            {
                if (IsDirectlyAllergenic(thing.def.building.mineableThing))
                {
                    translatedCause = causeKey.Translate(thing.LabelNoParenthesis);
                    return mineableThingExposure;
                }
            }

            return ExposureType.None;
        }

        // Harmony patches
        public virtual void OnDamageTaken(DamageInfo dinfo) { }
        public virtual void OnInteractedWith(Pawn pawn) { }
        public void OnThingIngested(Thing thing)
        {
            CheckThingIfAllergenicAndApplyBuildup(thing, "P42_AllergyCause_Ingested",
                directExposure: ExposureType.ExtremeEvent,
                ingredientExposure: ExposureType.StrongEvent,
                stuffExposure: ExposureType.StrongEvent,
                productionIngredientExposure: ExposureType.StrongEvent);
        }
        public void OnTendedWith(Thing medicine)
        {
            CheckThingIfAllergenicAndApplyBuildup(medicine, "P42_AllergyCause_Tended",
                directExposure: ExposureType.ExtremeEvent,
                ingredientExposure: ExposureType.StrongEvent,
                stuffExposure: ExposureType.StrongEvent,
                productionIngredientExposure: ExposureType.StrongEvent);
        }
        public void OnRecipeApplied(Thing recipeIngredient)
        {
            CheckThingIfAllergenicAndApplyBuildup(recipeIngredient, "P42_AllergyCause_RecipeApplied",
                directExposure: ExposureType.ExtremeEvent,
                ingredientExposure: ExposureType.StrongEvent,
                stuffExposure: ExposureType.StrongEvent,
                productionIngredientExposure: ExposureType.StrongEvent);
        }

        #endregion

        #region Getters

        protected Pawn Pawn => AllergyHediff.pawn;
        public string FullAllergyName => TypeLabel + " " + "P42_Allergy".Translate();
        public string FullAllergyNameCap => TypeLabel.CapitalizeFirst() + " " + "P42_Allergy".Translate();

        /// <summary>
        /// Returns if two allergies are of the exact same type and subtype. Used to avoid generating duplicate allergies.
        /// </summary>
        public abstract bool IsDuplicateOf(Allergy otherAllergy);

        public bool IsAllergyDiscovered => AllergyHediff.Visible;

        public string GetSeverityString() => Utils.GetSeverityString(Severity);

        public abstract string KeepAwayFromText { get; }
        public abstract string TypeLabel { get; }
        public void ExposeData()
        {
            Scribe_Defs.Look(ref Def, "allergyDef");
            Scribe_Values.Look(ref Severity, "allergySeverity");
            Scribe_Values.Look(ref TicksUntilNaturalSeverityChange, "ticksUntilNaturalSeverityChange");
            Scribe_Values.Look(ref TicksUntilAllercureImpact, "ticksUntilAllercureImpact");
            Scribe_Values.Look(ref ArrivalTick, "arrivalTick");
            ExposeExtraData();
        }
        protected abstract void ExposeExtraData();

        #endregion
    }
}
