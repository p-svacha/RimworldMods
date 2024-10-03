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
        private const float NewAllergyRandomMtbDays = 300; // How many days on average it takes for a pawn to develop a new allergy
        private const float NewAllergyFromTraitMtbDays = 45; // How many days on average it takes for a pawn with the allergy-prone trait to develop a new allergy

        private const int NewAllergyCheckInterval = 30000; // How often it is checked if pawns should get a new allergy

        public AllergiesGameComponent(Game game) { }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame % NewAllergyCheckInterval == 0)
            {
                List<Pawn> allPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive;
                foreach(Pawn pawn in allPawns)
                {
                    if(ShouldPawnGetNewAllergy(pawn))
                    {
                        AllergyGenerator.GenerateAndApplyRandomAllergy(pawn);
                    }
                }
            }
        }

        private bool ShouldPawnGetNewAllergy(Pawn pawn)
        {
            float mtbDays = -1f;
            if (HasAllergyProneTrait(pawn)) mtbDays = NewAllergyFromTraitMtbDays;
            else mtbDays = NewAllergyRandomMtbDays / pawn.GetStatValue(StatDef.Named("P42_AllergicSensitivity"));

            return (Rand.MTBEventOccurs(mtbDays, 60000f, NewAllergyCheckInterval));
        }

        private bool HasAllergyProneTrait(Pawn pawn)
        {
            return pawn.story?.traits?.HasTrait(TraitDef.Named("P42_Allergy")) == true
                && pawn.story.traits.DegreeOfTrait(TraitDef.Named("P42_Allergy")) == -1;
        }
    }
}
