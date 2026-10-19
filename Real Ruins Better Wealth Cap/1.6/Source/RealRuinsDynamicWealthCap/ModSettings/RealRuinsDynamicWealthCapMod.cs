using UnityEngine;
using Verse;

namespace P42_RealRuinsDynamicWealthCap
{
    public class RealRuinsDynamicWealthCapMod : Mod
    {
        private readonly RealRuinsDynamicWealthCapSettings settings;

        public RealRuinsDynamicWealthCapMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<RealRuinsDynamicWealthCapSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Real Ruins: Better Wealth Cap";
        }
    }
}