using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MoreAnomalousContent
{
    [HarmonyPatch(typeof(Projectile), "Impact")]
    public static class Projectile_Impact_Patch
    {
        public static void Prefix(Projectile __instance, Thing hitThing)
        {
            if (__instance.def.projectile.damageDef != DamageDefOf.Bullet) return;

            Map map = __instance.Map;
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
                            float distance = thing.Position.DistanceTo(__instance.Position);
                            if (distance < 10)
                            {
                                if (Rand.Chance(0.1f))
                                {
                                    activity.IncreaseActivityDueToNoise("P42_TL_DisturbanceGunshots".Translate(), Rand.Range(0.03f, 0.09f));
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
