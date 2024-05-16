using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace MoreAnomalousContent
{
    public class WorkGiver_Warden_SuppressPhonosphere : WorkGiver_Warden_SuppressActivity
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!ModsConfig.AnomalyActive)
            {
                return null;
            }

            // Get thing that we are suppressing
            Thing thingToSuppress = GetThingToSuppress(t, forced);
            if (thingToSuppress == null)
            {
                return null;
            }
            //Log.Message("WorkGiver_Warden_SuppressPhonosphere: GotThingToSuppress");

            // Get activity component
            CompActivity compActivity = thingToSuppress.TryGetComp<CompActivity>();
            if (compActivity == null)
            {
                return null;
            }
            //Log.Message("WorkGiver_Warden_SuppressPhonosphere: GotActivity");

            // Check if compActivity is of correct type
            if (!(compActivity is PhonosphereActivity))
            {
                return null;
            }
            Log.Message("WorkGiver_Warden_SuppressPhonosphere: GotPhonosphereActivity");

            // Get room that the sphere is in
            Room room = thingToSuppress.GetRoom();
            if (room == null)
            {
                return null;
            }
            if(room.PsychologicallyOutdoors)
            {
                JobFailReason.Is("CannotBeSuppressedOutside".Translate());
                return null;
            }
            Log.Message("WorkGiver_Warden_SuppressPhonosphere: GotRoom");

            // Get instrument in room
            Building musicalInstrument = GetMusicalInstrument(room);
            if(musicalInstrument == null)
            {
                JobFailReason.Is("NoInstrumentInRoom".Translate());
                return null;
            }
            Log.Message("WorkGiver_Warden_SuppressPhonosphere: GotInstrument");

            Job job = JobMaker.MakeJob(JobDefOf.Play_MusicalInstrument, musicalInstrument);
            job.targetB = thingToSuppress;
            job.playerForced = forced;
            return job;
        }

        private Building GetMusicalInstrument(Room room)
        {
            foreach (Thing thing in room.ContainedAndAdjacentThings)
            {
                if (thing is Building_MusicalInstrument instrument)
                {
                    return instrument;
                }
            }
            return null;
        }

        private Thing GetThingToSuppress(Thing thing, bool playerForced)
        {
            /*
            if (!ActivitySuppressionUtility.CanBeSuppressed(thing, considerMinActivity: true, playerForced))
            {
                return null;
            }
            */

            return thing;
        }
    }
}
