using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MoreAnomalousContent
{
    public class PhonosphereActivityWorker : ActivityWorker
    {
        public PhonosphereActivityWorker() { }
        public override void GetSummary(ThingWithComps thing, StringBuilder sb)
        {
            PhonosphereActivity activity = thing.TryGetComp<PhonosphereActivity>();
            if(activity != null)
            {
                sb.Append("\nCan be suppressed: " + activity.CanBeSuppressed + "\nsuppressIfAbove: " + activity.suppressIfAbove + "\nstate: " + activity.State.ToString());
            }
        }
    }
}
