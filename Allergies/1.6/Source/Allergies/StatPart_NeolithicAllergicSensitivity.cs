using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class StatPart_NeolithicAllergicSensitivity : StatPart
    {
        public float factor;

        private bool ActiveFor(Thing t)
        {
            if (t is Pawn pawn)
            {
                if (pawn.Faction == null) return false;
                if (pawn.Faction.def == null) return false;
                if (pawn.Faction.def.techLevel != TechLevel.Neolithic) return false;

                return true;
            }
            return false;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing || !ActiveFor(req.Thing)) return;

            val *= factor;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing || !ActiveFor(req.Thing)) return null;

            return "P42_StatsReport_Tribal".Translate() + ": x" + factor.ToStringPercent();
        }
    }
}
