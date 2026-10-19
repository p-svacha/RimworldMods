using UnityEngine;
using Verse;

namespace P42_RealRuinsDynamicWealthCap
{
    public class RealRuinsDynamicWealthCapSettings : ModSettings
    {
        public const float MinMultiplier = 0.10f;
        public const float MaxMultiplier = 3.0f;
        public const float DefaultMultiplier = 1.0f;

        public const int MinAbsoluteCap = 1000;
        public const int MaxAbsoluteCap = 200000;
        public const int AbsoluteCapStep = 1000;
        public const int DefaultStandardCap = 12000;
        public const int DefaultCaravanCap = 8000;
        public const int DefaultAbandonedBaseCap = 50000;

        private WealthCapMode mode = WealthCapMode.Dynamic;
        private float wealthMultiplier = DefaultMultiplier;
        private int standardCap = DefaultStandardCap;
        private int caravanCap = DefaultCaravanCap;
        private int abandonedBaseCap = DefaultAbandonedBaseCap;

        private static RealRuinsDynamicWealthCapSettings instance;

        public static WealthCapMode Mode => instance?.mode ?? WealthCapMode.Dynamic;
        public static float WealthMultiplier => instance?.wealthMultiplier ?? DefaultMultiplier;

        public static int GetAbsoluteCap(RuinType type)
        {
            switch (type)
            {
                case RuinType.Caravan: return instance?.caravanCap ?? DefaultCaravanCap;
                case RuinType.AbandonedBase: return instance?.abandonedBaseCap ?? DefaultAbandonedBaseCap;
                default: return instance?.standardCap ?? DefaultStandardCap;
            }
        }

        public RealRuinsDynamicWealthCapSettings()
        {
            instance = this;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref mode, "mode", WealthCapMode.Dynamic);
            Scribe_Values.Look(ref wealthMultiplier, "wealthMultiplier", DefaultMultiplier);
            Scribe_Values.Look(ref standardCap, "standardCap", DefaultStandardCap);
            Scribe_Values.Look(ref caravanCap, "caravanCap", DefaultCaravanCap);
            Scribe_Values.Look(ref abandonedBaseCap, "abandonedBaseCap", DefaultAbandonedBaseCap);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);

            if (listing.RadioButton("Dynamic", mode == WealthCapMode.Dynamic,
                tooltip: "The ruin wealth cap scales with your colony's wealth."))
            {
                mode = WealthCapMode.Dynamic;
            }
            if (listing.RadioButton("Absolute", mode == WealthCapMode.Absolute,
                tooltip: "The ruin wealth cap is a fixed value per ruin type, regardless of your colony's wealth."))
            {
                mode = WealthCapMode.Absolute;
            }
            listing.GapLine();

            if (mode == WealthCapMode.Dynamic)
            {
                DoDynamicSettings(listing);
            }
            else
            {
                DoAbsoluteSettings(listing);
            }

            listing.Gap();

            if (listing.ButtonText("Reset to default", widthPct: 0.3f))
            {
                ResetCurrentMode();
            }

            listing.End();
        }

        private void DoDynamicSettings(Listing_Standard listing)
        {
            wealthMultiplier = listing.SliderLabeled(
                $"Ruin wealth multiplier: {wealthMultiplier:0.00}x",
                wealthMultiplier,
                MinMultiplier,
                MaxMultiplier,
                tooltip:
                "Controls how much value ruins contain relative to your colony's wealth.\n" +
                "Lower values mean less valuable ruins, higher values mean more valuable ruins.\n" +
                "1.0x is the value this mod was balanced around."
            );
        }

        private void DoAbsoluteSettings(Listing_Standard listing)
        {
            standardCap = AbsoluteSlider(listing, "Standard map ruins", standardCap,
                "Ruins that spawn on regular maps, e.g. your home map and other maps you settle or do quests on.");
            caravanCap = AbsoluteSlider(listing, "Caravan ruins", caravanCap,
                "Ruins from the \"small ruins found\" caravan event.");
            abandonedBaseCap = AbsoluteSlider(listing, "Abandoned base ruins", abandonedBaseCap,
                "Ruins from the \"Pristine ruins found\" incident and the abandoned base quest.");
        }

        private static int AbsoluteSlider(Listing_Standard listing, string label, int value, string tooltip)
        {
            float v = listing.SliderLabeled($"{label}: {value:N0}", value, MinAbsoluteCap, MaxAbsoluteCap, tooltip: tooltip);
            int stepped = Mathf.RoundToInt(v / AbsoluteCapStep) * AbsoluteCapStep;
            return Mathf.Clamp(stepped, MinAbsoluteCap, MaxAbsoluteCap);
        }

        private void ResetCurrentMode()
        {
            if (mode == WealthCapMode.Dynamic)
            {
                wealthMultiplier = DefaultMultiplier;
            }
            else
            {
                standardCap = DefaultStandardCap;
                caravanCap = DefaultCaravanCap;
                abandonedBaseCap = DefaultAbandonedBaseCap;
            }
        }
    }
}