using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MoreAnomalousContent
{
    [HarmonyPatch(typeof(WeatherEvent_LightningStrike), "FireEvent")]
    public static class WeatherEvent_LightningStrike_FireEvent_Patch
    {
        public static void Postfix(WeatherEvent_LightningStrike __instance)
        {
            FieldInfo mapField = typeof(WeatherEvent_LightningStrike).GetField("map", BindingFlags.NonPublic | BindingFlags.Instance);
            Map map = (Map)mapField.GetValue(__instance);

            if (map != null)
            {
                List<Thing> thingsInRange = map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver);
                foreach (Thing thing in thingsInRange)
                {
                    if (thing.def.defName == "P42_Phonosphere")
                    {
                        PhonosphereActivity activity = thing.TryGetComp<PhonosphereActivity>();
                        if (activity != null)
                        {
                            if (Rand.Chance(0.4f))
                            {
                                activity.IncreaseActivityDueToNoise("P42_TL_DisturbanceIncreaseThunder".Translate(), Rand.Range(0.03f, 0.9f));
                            }
                        }
                        break;
                    }
                }
            }
        }
    }
}
