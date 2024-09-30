using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MoreAnomalousContent
{
    public class QuestNode_Root_MysteriousCargo_Phonosphere : QuestNode_Root_MysteriousCargo
    {
        protected override bool RequiresPawn => false;
        protected override Thing GenerateThing(Pawn pawn)
        {
            return ThingMaker.MakeThing(ThingDef.Named("P42_Phonosphere"));
        }
    }
}
