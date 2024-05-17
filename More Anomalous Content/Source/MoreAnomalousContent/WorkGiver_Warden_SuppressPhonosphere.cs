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
            // Get activity component and check type
            CompActivity activity = t.TryGetComp<CompActivity>();
            if (activity == null || !(activity is PhonosphereActivity))
            {
                return null;
            }

            // Get room that the sphere is in
            Room room = t.GetRoom();
            if (room == null)
            {
                return null;
            }
            if(room.PsychologicallyOutdoors)
            {
                JobFailReason.Is("P42_TL_CannotBeSuppressedOutside".Translate());
                return null;
            }

            // Get non-reserved instrument in room
            Building musicalInstrument = GetInstrumentInRoom(room);
            if(musicalInstrument == null)
            {
                JobFailReason.Is("P42_TL_NoInstrumentInRoom".Translate());
                return null;
            }

            // Ensure the musical instrument can be reserved
            if (!pawn.CanReserve(musicalInstrument, 1, -1, null, forced))
            {
                return null;
            }

            // If not forced, pawns will not always suppress it automatically
            if (!forced)
            {
                if (!activity.suppressionEnabled) return null;
                if (activity.ActivityLevel < activity.suppressIfAbove) return null;
            }

            Job job = JobMaker.MakeJob(JobDefOf.Play_MusicalInstrument, musicalInstrument, musicalInstrument.InteractionCell);
            job.expiryInterval = 600; // job is done for 10 seconds
            job.doUntilGatheringEnded = true; // So they do the job for exactly 10 seconds and don't stop when recreation is full
            job.playerForced = forced;
            return job;
        }

        /// <summary>
        /// Returns the first non-reserved instrument in a room
        /// </summary>
        private Building GetInstrumentInRoom(Room room)
        {
            if (room == null) return null;

            foreach (Thing thing in room.ContainedAndAdjacentThings)
            {
                if (thing is Building_MusicalInstrument instrument && !instrument.IsForbidden(Faction.OfPlayer) && !instrument.IsBurning() && instrument.CanBeSeenOver())
                {
                    // Check if the instrument is reserved
                    if (!instrument.Map.reservationManager.IsReserved(instrument))
                    {
                        return instrument;
                    }
                }
            }
            return null;
        }
    }
}
