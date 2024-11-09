using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42Allergies
{
    public class Alert_AllergenBuildup : Alert
    {
        private const float SeverityThreshold = 0.6f;

		private List<Pawn> allergicReactionColonists = new List<Pawn>();

		private List<Pawn> AllergicReactionColonists
		{
			get
			{
				allergicReactionColonists.Clear();
				foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep)
				{
					Hediff firstHediffOfDef = item.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("P42_AllergenBuildup"));
					if (firstHediffOfDef != null && firstHediffOfDef.Severity >= SeverityThreshold)
					{
						allergicReactionColonists.Add(item);
					}
				}
				return allergicReactionColonists;
			}
		}

		public Alert_AllergenBuildup()
		{
			defaultLabel = "P42_Alert_AllergicReactions".Translate();
			defaultPriority = AlertPriority.Medium;
		}

		public override TaggedString GetExplanation()
		{
			string text = allergicReactionColonists.Select((Pawn p) => p.NameShortColored.Resolve()).ToLineList(" - ");
			return "P42_Alert_AllergicReactionsDesc".Translate(text);
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(AllergicReactionColonists);
		}
	}
}