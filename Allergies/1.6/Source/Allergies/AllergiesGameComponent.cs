using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    /// <summary>
    /// This class handles all checks that need to be done from time to time but don't belong to a specific thing, hediff, etc.
    /// </summary>
    public class AllergiesGameComponent : GameComponent
    {
        private const float NewAllergyRandomMtbDays = 720; // How many days on average it takes for a pawn to develop a new allergy
        private const float NewAllergyFromTraitMtbDays = 180; // How many days on average it takes for a pawn with the allergy-prone trait to develop a new allergy

        private const int NewAllergyCheckInterval = 30000; // How often it is checked if pawns should get a new allergy

        public AllergiesGameComponent(Game game) { }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame % NewAllergyCheckInterval == 0)
            {
                List<Pawn> allPawns = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive;
                foreach(Pawn pawn in allPawns)
                {
                    ReassertGeneAllergies(pawn);
                    if (ShouldPawnGetNewAllergy(pawn))
                    {
                        AllergyGenerator.GenerateAndApplyRandomAllergy(pawn, isVisible: false);
                    }
                }
            }
        }

        private bool ShouldPawnGetNewAllergy(Pawn pawn)
        {
            if (pawn == null || pawn.Dead) return false;

            float allergicSensitivity = Utils.GetAllergicSensitivity(pawn);
            if (allergicSensitivity <= 0f) return false;

            float mtbDays;
            if (HasAllergyProneTrait(pawn)) mtbDays = NewAllergyFromTraitMtbDays;
            else mtbDays = NewAllergyRandomMtbDays / allergicSensitivity;

            // Apply the player-configurable rate multiplier (higher factor = lower MTB = more frequent)
            float factor = Allergies_Settings.newAllergyRateFactor;
            if (factor <= 0f) return false; // 0 effectively disables new allergies
            mtbDays /= factor;

            return Rand.MTBEventOccurs(mtbDays, 60000f, NewAllergyCheckInterval);
        }

        private void ReassertGeneAllergies(Pawn pawn)
        {
            if (pawn == null || pawn.Dead || !ModsConfig.BiotechActive || pawn.genes == null) return;
            if (Utils.HasAllergyImmunity(pawn)) return; // immune pawns: don't re-add forced allergies

            foreach (Gene g in pawn.genes.GenesListForReading)
            {
                if (g is Gene_Allergy ga && g.Active
                    && !Utils.GetPawnAllergies(pawn).Any(h => h.GetAllergy().SourceGeneDefName == g.def.defName))
                {
                    ga.EnsureAllergy(sendReaddMessage: true);
                }
            }
        }

        private bool HasAllergyProneTrait(Pawn pawn)
        {
            return pawn.story?.traits?.HasTrait(TraitDef.Named("P42_Allergy")) == true
                && pawn.story.traits.DegreeOfTrait(TraitDef.Named("P42_Allergy")) == -1;
        }
    }
}
