using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class TraitsManager : GameComponent
    {
        private const float NewAllergyMtbDays = 0.1f; // TODO: 45
        private const int NewAllergyCheckInterval = 1800;

        public TraitsManager(Game game) { }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame % NewAllergyCheckInterval == 0)
            {
                // Log.Message($"[Allergies Mod] Checking for new allergy by trait.");
                CheckForNewAllergies();
            }
        }

        private void CheckForNewAllergies()
        {
            // Get a list of all pawns currently spawned in the world.
            var allPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive;

            foreach (Pawn pawn in allPawns)
            {
                // Check if the pawn has the "allergyprone" trait.
                if (HasAllergyProneTrait(pawn))
                {
                    // Log.Message($"[Allergies Mod] Checking for new allergy by trait on {pawn.Name}.");

                    // Calculate the chance based on the MTB days defined.
                    if (Rand.MTBEventOccurs(NewAllergyMtbDays, 60000f, NewAllergyCheckInterval))
                    {
                        AllergyGenerator.GenerateAndApplyRandomAllergy(pawn);
                    }
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
