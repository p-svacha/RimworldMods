using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace P42_Allergies
{
    public static class AllergyGenerator
    {
        private static Dictionary<AllergyDef, float> AllergyWeights = new Dictionary<AllergyDef, float>();
        private static int MaxAllergiesPerPawn = 6;

        // Lists containing all valid allergy types for ThingDef allergies
        private static List<ThingDef> FoodIngredients;
        private static List<ThingDef> Drugs;
        private static List<ThingDef> Medicines;
        private static List<ThingDef> Textiles;
        private static List<ThingDef> Plants;
        private static List<ThingDef> Woods;
        private static List<ThingDef> Stones;
        private static List<ThingDef> Metals;

        private static List<PawnKindDef> Animals;
        private static List<XenotypeDef> Xenotypes;


        #region Initialization

        private static bool IsInitialized = false;

        private static void Initialize()
        {
            if (IsInitialized) return;

            // Add all allergy defs
            AllergyWeights.Clear();
            foreach(AllergyDef def in DefDatabase<AllergyDef>.AllDefs)
            {
                if(def.requiredMods != null && def.requiredMods.Any(x => !ModsConfig.IsActive(x)))
                {
                    Logger.Log($"Removing {def.defName} from allergy pool because a required mod is missing");
                    continue;
                }

                AllergyWeights.Add(def, def.commonness);
            }

            // DLC checks
            if (!ModsConfig.BiotechActive) AllergyWeights.Remove(DefDatabase<AllergyDef>.GetNamed("Xenotype"));

            // Initialize all valid allergy thingdef-subtype lists
            FoodIngredients = new List<ThingDef>();
            Drugs = new List<ThingDef>();
            Medicines = new List<ThingDef>();
            Textiles = new List<ThingDef>();
            Plants = new List<ThingDef>();
            Woods = new List<ThingDef>();
            Stones = new List<ThingDef>();
            Metals = new List<ThingDef>();

            Animals = new List<PawnKindDef>();

            foreach(ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (IsFoodIngredient(def)) FoodIngredients.Add(def);
                if (IsDrug(def)) Drugs.Add(def);
                if (IsMedicine(def)) Medicines.Add(def);
                if (IsTextile(def)) Textiles.Add(def);
                if (IsPlant(def)) Plants.Add(def);
                if (HasStuffProp(def, StuffCategoryDefOf.Woody)) Woods.Add(def);
                if (HasStuffProp(def, StuffCategoryDefOf.Stony)) Stones.Add(def);
                if (HasStuffProp(def, StuffCategoryDefOf.Metallic)) Metals.Add(def);
            }

            foreach(PawnKindDef def in DefDatabase<PawnKindDef>.AllDefsListForReading)
            {
                if (IsAnimal(def)) Animals.Add(def);
            }

            if (ModsConfig.BiotechActive)
            {
                Xenotypes = new List<XenotypeDef>();
                foreach(XenotypeDef def in DefDatabase<XenotypeDef>.AllDefsListForReading)
                {
                    if (def != XenotypeDefOf.Baseliner) Xenotypes.Add(def);
                }
            }

            IsInitialized = true;
        }

        #endregion

        #region Allergy generation

        public static void GenerateAndApplyRandomAllergy(Pawn pawn, bool isVisible)
        {
            Initialize();

            // Get existing allergies
            List<Hediff_Allergy> existingAllergies = Utils.GetPawnAllergies(pawn);

            // Check if pawn can get an allergy
            if (!CanApplyAllergy(pawn, existingAllergies)) return;

            // Create a random allergy that the pawn does not yet have
            int tries = 0;
            int maxTries = 20;
            Allergy newAllergy = null;
            do
            {
                tries++;
                newAllergy = CreateRandomAllergy();
            }
            while (tries < maxTries && !CanApplyAllergy(pawn, newAllergy, existingAllergies));

            if(tries == maxTries)
            {
                Logger.Log($"Aborting allergy creation on {pawn.Name}.");
                return;
            }

            // Create new hediff
            Hediff_Allergy allergyHediff = (Hediff_Allergy)HediffMaker.MakeHediff(HediffDef.Named("P42_AllergyHediff"), pawn);
            allergyHediff.Severity = 0.05f;
            allergyHediff.SetAllergy(newAllergy);
            newAllergy.OnInitOrLoad(allergyHediff);
            if (isVisible) newAllergy.MakeAllergyVisible();

            pawn.health.AddHediff(allergyHediff);
            Logger.Log($"Initialized a new allergy: {newAllergy.TypeLabel} ({newAllergy.GetType()}) with severity {newAllergy.Severity} on {pawn.Name}.");
        }

        /// <summary>
        /// Checks and returns if it is generally possible to apply a new allergy to a pawn.
        /// </summary>
        private static bool CanApplyAllergy(Pawn pawn, List<Hediff_Allergy> existingAllergies)
        {
            if (pawn == null) return false;
            if (pawn.Dead) return false; // No allergies for dead pawns
            if (pawn.NonHumanlikeOrWildMan()) return false; // Only humanlikes get allergies
            if (Utils.GetAllergicSensitivity(pawn) <= 0f) return false; // No allergies for pawns with 0 allergic sensitivity
            if (existingAllergies.Count >= MaxAllergiesPerPawn) return false; // max amount of allergies

            return true;
        }

        /// <summary>
        /// Checks and returns if a specific allergy can be applied to a pawn.
        /// </summary>
        private static bool CanApplyAllergy(Pawn pawn, Allergy newAllergy, List<Hediff_Allergy> existingAllergies)
        {
            if (existingAllergies.Any(x => x.GetAllergy().IsDuplicateOf(newAllergy))) return false;

            // Don't allow being allergic to own xenotype
            if (newAllergy is XenotypeAllergy xenotypeAllergy && pawn.genes != null && pawn.genes.Xenotype != null && pawn.genes.Xenotype == xenotypeAllergy.Xenotype) return false;

            return true;
        }

        public static Allergy CreateRandomAllergy()
        {
            Initialize();

            AllergyDef chosenDef = Utils.GetWeightedRandomElement(AllergyWeights);
            Type allergyClass = chosenDef.allergyClass;
            Allergy newAllergy = (Allergy)Activator.CreateInstance(allergyClass);
            newAllergy.OnNewAllergyCreated(chosenDef);
            
            return newAllergy;
        }

        #endregion

        #region Lists for random Defs


        public static ThingDef GetRandomIngredient() => FoodIngredients.RandomElement();
        private static bool IsFoodIngredient(ThingDef def)
        {
            if (!def.IsIngestible) return false;
            if (!def.ingestible.HumanEdible) return false;
            if (def.IsDrug) return false;
            if ((def.ingestible.foodType & FoodTypeFlags.Meal) != 0) return false;
            if (def.IsCorpse) return false;
            if (def.IsEgg) return false;
            return true;
        }

        public static ThingDef GetRandomDrug() => Drugs.RandomElement();
        private static bool IsDrug(ThingDef def)
        {
            if (!def.IsDrug) return false;
            if (def.tradeTags.Contains("Serum")) return false;
            return true;
        }

        public static ThingDef GetRandomMedicine() => Medicines.RandomElement();
        private static bool IsMedicine(ThingDef def)
        {
            if (!def.IsMedicine) return false;
            return true;
        }

        public static ThingDef GetRandomTextile() => Textiles.RandomElement();
        private static bool IsTextile(ThingDef def)
        {
            if (def.thingCategories == null) return false;
            if (!def.thingCategories.Contains(ThingCategoryDefOf.Textiles)) return false;
            return true;
        }



        public static ThingDef GetRandomPlant() => Plants.RandomElement();
        private static bool IsPlant(ThingDef def)
        {
            if (def.plant == null) return false;
            if (def.plant.isStump) return false;

            return true;
        }

        private static bool HasStuffProp(ThingDef def, StuffCategoryDef stuffCategory)
        {
            if (def.stuffProps == null) return false;
            if (def.stuffProps.categories == null) return false;
            if (!def.stuffProps.categories.Contains(stuffCategory)) return false;

            return true;
        }
        public static ThingDef GetRandomStone()=> Stones.RandomElement();
        public static ThingDef GetRandomWood() => Woods.RandomElement();
        public static ThingDef GetRandomMetal() => Metals.RandomElement();


        public static PawnKindDef GetRandomAnimal() => Animals.RandomElement();

        private static bool IsAnimal(PawnKindDef def)
        {
            if (def.race == null) return false;
            if (def.race.thingCategories == null) return false;
            if (!def.race.thingCategories.Contains(ThingCategoryDefOf.Animals)) return false;
            if (def.race.tradeTags.Contains("AnimalDryad")) return false; // exclude dryads

            return true;
        }
        public static XenotypeDef GetRandomXenotype() => Xenotypes.RandomElement();

        #endregion
    }
}
