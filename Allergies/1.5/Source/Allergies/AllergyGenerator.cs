using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public static class AllergyGenerator
    {
        private static Dictionary<AllergyDef, float> AllergyWeights = new Dictionary<AllergyDef, float>();
                private static int MaxAllergiesPerPawn = 6;

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
    }
}
