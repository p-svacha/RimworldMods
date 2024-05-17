using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace MoreAnomalousContent
{
    [HarmonyPatch(typeof(Explosion), "StartExplosion")]
    public static class Explosion_StartExplosion_Patch
    {
        public static void Postfix(Explosion __instance)
        {
            Map map = __instance.Map;
            if (map != null)
            {
                List<Thing> thingsInRange = map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver);
                foreach (Thing thing in thingsInRange)
                {
                    if(thing.def.defName == "P42_Phonosphere")
                    {
                        CompActivity activity = thing.TryGetComp<CompActivity>();
                        if (activity != null && activity is PhonosphereActivity phonosphereActivity)
                        {
                            float distance = thing.Position.DistanceTo(__instance.Position);
                            if (distance < __instance.radius * 3)
                            {
                                if (Rand.Chance(0.8f))
                                {
                                    phonosphereActivity.IncreaseActivityDueToNoise("P42_TL_DisturbanceIncreaseExplosion".Translate(), Rand.Range(0.05f, 0.12f));
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }
    }
}
