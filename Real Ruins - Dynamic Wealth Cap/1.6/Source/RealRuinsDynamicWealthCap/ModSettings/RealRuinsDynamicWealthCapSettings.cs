using UnityEngine;
using Verse;

namespace P42_RealRuinsDynamicWealthCap
{
    public class RealRuinsDynamicWealthCapSettings : ModSettings
    {
        public const float MinMultiplier = 0.10f;
        public const float MaxMultiplier = 3.0f;
        public const float DefaultMultiplier = 1.0f;

        private float wealthMultiplier = DefaultMultiplier;

        private static RealRuinsDynamicWealthCapSettings instance;

        public static float WealthMultiplier => instance?.wealthMultiplier ?? DefaultMultiplier;

        public RealRuinsDynamicWealthCapSettings()
        {
            instance = this;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref wealthMultiplier, "wealthMultiplier", DefaultMultiplier);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);

            // Explanation Text
            listing.Label(
                "Controls how much value the ruins from Real Ruins contain relative to your colony's wealth.\n" +
                "Lower values mean ruins contain less value, higher values mean ruins contain more value.\n" +
                "The multiplier is based on the numbers this mod was balanced around, with 1.0x being the default. "
            );
            listing.Gap();


            // Slider
            wealthMultiplier = listing.SliderLabeled(
                $"Ruin wealth multiplier: {wealthMultiplier:0.00}x",
                wealthMultiplier,
                MinMultiplier,
                MaxMultiplier
            );

            listing.Gap();

            // Reset to Defaults Button
            if (listing.ButtonText("Reset to default", widthPct: 0.3f))
            {
                wealthMultiplier = DefaultMultiplier;
            }

            listing.End();
        }
    }
}