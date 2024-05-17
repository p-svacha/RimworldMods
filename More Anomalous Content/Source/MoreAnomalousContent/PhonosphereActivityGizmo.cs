using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MoreAnomalousContent
{
    public class PhonosphereActivityGizmo : ActivityGizmo
    {
        private readonly ThingWithComps thing;
        private PhonosphereActivity Comp => thing.GetComp<PhonosphereActivity>();
        public PhonosphereActivityGizmo(ThingWithComps thing) : base(thing)
        {
            this.thing = thing;
        }

        protected override string Title => "P42_TL_Disturbance".Translate();

        protected override string GetTooltip()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("ActivitySuppressionTooltipTitle".Translate().CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor));
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("P42_TL_PhonosphereActivityGizmoDesc".Translate(Comp.suppressIfAbove.ToStringPercent("0").Colorize(ColoredText.TipSectionTitleColor).Named("LEVEL")).Resolve());
            Comp.Props.Worker.GetSummary(thing, stringBuilder);

            if (!Comp.suppressionEnabled)
            {
                stringBuilder.Append("\n\n" + "ActivitySuppressionTooltipDisabled".Translate(thing.Named("PAWN")).CapitalizeFirst().Colorize(ColoredText.FactionColor_Hostile));
            }

            return stringBuilder.ToString();
        }
    }
}
