using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace P42_Allergies
{
    /// <summary>
    /// The Rimworld hediff element, simply acts as a container to hold an allergy object that handles all the logic.
    /// </summary>
    public class Hediff_Allergy : Hediff
    {
        private Allergy Allergy;

        // PostMake is called after the Hediff has been created by HediffMaker.
        public override void PostMake()
        {
            base.PostMake();
            Allergy = AllergyGenerator.CreateRandomAllergyFor(this, pawn);
        }

        public override void Tick()
        {
            base.Tick();
            Allergy.Tick();
        }

        public override string Label => Allergy.FullAllergyNameCap + " (" + Allergy.Severity.ToString().ToLower() + ")";
    }
}
