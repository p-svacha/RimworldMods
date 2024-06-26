﻿using RimWorld;
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
        public override float GetChangeRatePerDay(ThingWithComps thing)
        {
            PhonosphereActivity activity = thing.GetComp<PhonosphereActivity>();
            CompProperties_PhonosphereActivity props = (CompProperties_PhonosphereActivity)activity.props;

            // Get suppression factors
            List<KeyValuePair<string, float>> suppressionFactors = GetChangeRateFactors(thing, props);
            return suppressionFactors.Sum(x => x.Value);
        }
        public override void GetSummary(ThingWithComps thing, StringBuilder sb)
        {
            PhonosphereActivity activity = thing.TryGetComp<PhonosphereActivity>();
            CompProperties_PhonosphereActivity props = (CompProperties_PhonosphereActivity)activity.props;
            if (activity == null) return;

            sb.Append("\n\n" + string.Format("{0}: {1} / {2}", "P42_TL_ChangeRate".Translate(), GetChangeRatePerDay(thing).ToStringPercent("0"), "day".Translate()));

            List<KeyValuePair<string, float>> suppressionFactors = GetChangeRateFactors(thing, props);
            foreach(var elem in suppressionFactors)
            {
                sb.Append(string.Format("\n - {0}: {1}", elem.Key, elem.Value.ToStringPercent("0")));
            }
        }

        /// <summary>
        /// Returns all factors that affect the phonosphere disturbance change rate as a list.
        /// </summary>
        private List<KeyValuePair<string, float>> GetChangeRateFactors(ThingWithComps thing, CompProperties_PhonosphereActivity props)
        {
            List<KeyValuePair<string, float>> elements = new List<KeyValuePair<string, float>>();

            // Base level
            elements.Add(new KeyValuePair<string, float>("BaseLevel".Translate(), props.changePerDayBase));
           
            Room room = thing.GetRoom();
            if (room != null)
            {
                List<Thing> thingsInRoom = room.ContainedAndAdjacentThings;

                // Find all instruments in a room that are currently being played
                int numPlayedInstruments = 0;
                foreach (Thing thingInRoom in thingsInRoom)
                {
                    if (thingInRoom is Building_MusicalInstrument instrument)
                    {
                        if (instrument.IsBeingPlayed && instrument.def.building.instrumentRange > 0)
                        {
                            numPlayedInstruments++;
                            float suppressionValue = -(props.suppressRatePerDayBase * instrument.def.building.instrumentRange / 12f);
                            elements.Add(new KeyValuePair<string, float>(instrument.LabelCapNoCount, suppressionValue));
                        }
                    }
                }

                // Check how many powered loudspeakers and drums are in the room (this only applies when a dance/drum party is going on)
                int numLoudspeakers = 0;
                int numDrums = 0;
                foreach (Thing thingInRoom in thingsInRoom)
                {
                    if (thingInRoom.def.defName == "Loudspeaker")
                    {
                        var compPower = thingInRoom.TryGetComp<CompPowerTrader>();
                        if (compPower != null)
                        {
                            if (compPower.PowerNet != null && compPower.PowerOn)
                            {
                                numLoudspeakers++;
                            }
                        }
                    }
                    else if (thingInRoom.def.defName == "Drum" && thingInRoom is Building_MusicalInstrument drum)
                    {
                        if (drum.IsBeingPlayed)
                        {
                            numDrums++;
                        }
                    }
                }

                if (numLoudspeakers > props.maxEffectiveLoudspeakers) numLoudspeakers = props.maxEffectiveLoudspeakers;
                if (numLoudspeakers > 0)
                {
                    elements.Add(new KeyValuePair<string, float>(numLoudspeakers + "x " + "P42_TL_Loudspeakers".Translate(), -(numLoudspeakers * props.suppressRatePerLoudspeaker)));
                }

                if (numDrums > props.maxEffectiveDrums) numDrums = props.maxEffectiveDrums;
                if (numDrums > 0)
                {
                    elements.Add(new KeyValuePair<string, float>(numDrums + "x " + "P42_TL_Drums".Translate(), -(numDrums * props.suppressRatePerDrum)));
                }
            }


            // Increased rate when outside
            if (thing.IsOutside() && props.changePerDayOutside.HasValue)
            {
                elements.Add(new KeyValuePair<string, float>("NotInSealedRoom".Translate(), props.changePerDayOutside.Value));
            }

            return elements;
        }
    }
}
