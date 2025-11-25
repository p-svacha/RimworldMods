using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class AllergyExposureInfo : IExposable
    {
        public string Cause;
        public int Tick;
        public int Amount;

        public AllergyExposureInfo() { } // needed for load game
        public AllergyExposureInfo(string cause, int tick)
        {
            Cause = cause;
            Tick = tick;
            Amount = 1;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref Cause, "cause");
            Scribe_Values.Look(ref Tick, "tick");
            Scribe_Values.Look(ref Amount, "amount");
        }
    }
}
