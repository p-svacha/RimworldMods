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

        public override void Tick()
        {
            base.Tick();
            Allergy.Tick();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref Allergy, "Allergy");

            if (Scribe.mode == LoadSaveMode.PostLoadInit && Allergy != null)
            {
                Allergy.OnInitOrLoad(this); // Re-initialize after loading
            }
        }

        public override bool TryMergeWith(Hediff other)
        {
            return false; // never merge allergies
        }

        public void SetAllergy(Allergy allergy)
        {
            Allergy = allergy;
        }
        public Allergy GetAllergy() => Allergy;

        public override string Label => Allergy.FullAllergyNameCap + " (" + Allergy.Severity.ToString().ToLower() + ")";
        public override string DebugString()
        {
            return base.DebugString() + "\nticks until severity change: " + Allergy.TicksUntilNaturalSeverityChange + "\nticks until allercure impact: " + Allergy.TicksUntilAllercureImpact;
        }
    }
}
