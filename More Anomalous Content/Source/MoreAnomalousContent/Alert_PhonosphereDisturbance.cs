using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MoreAnomalousContent
{
    class Alert_PhonosphereDisturbance : Alert_Critical
    {
		private PhonosphereActivity disturbedPhonosphere;

		private List<Thing> GetDisturbedPhonospheres()
		{
			List<Thing> disturbedPhonospheres = new List<Thing>();
			disturbedPhonosphere = null;
			foreach (Map map in Find.Maps)
			{
				foreach (Thing thing in map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver))
				{
					if (thing.def.defName == "P42_Phonosphere")
					{
						PhonosphereActivity activity = thing.TryGetComp<PhonosphereActivity>();
						if (activity.ActivityLevel > 0.6f)
						{
							disturbedPhonospheres.Add(thing);
							disturbedPhonosphere = activity;
						}
					}
				}
			}
			return disturbedPhonospheres;
		}

		public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(GetDisturbedPhonospheres());
        }

        public override string GetLabel()
        {
            return "P42_TL_PhonosphereDisturbanceAlert".Translate() + ": " + disturbedPhonosphere.ActivityLevel.ToStringPercent("0");
		}

		public override TaggedString GetExplanation()
		{
			return "P42_TL_PhonosphereDisturbanceAlertDesc".Translate();
		}
	}
}
