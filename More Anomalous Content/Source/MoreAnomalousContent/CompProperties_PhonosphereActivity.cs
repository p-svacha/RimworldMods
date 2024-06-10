using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;

namespace MoreAnomalousContent
{
    public class CompProperties_PhonosphereActivity : CompProperties_Activity
    {
        public float suppressRatePerDayBase;
        public int maxEffectiveLoudspeakers;
        public float suppressRatePerLoudspeaker;
        public int maxEffectiveDrums;
        public float suppressRatePerDrum;

        public CompProperties_PhonosphereActivity()
        {
            compClass = typeof(PhonosphereActivity);
        }
    }
}
