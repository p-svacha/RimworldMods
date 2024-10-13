using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace P42_Allergies
{
    public class SunlightAllergy : Allergy
    {
        protected override void OnInitOrLoad()
        {
            typeLabel = "P42_AllergyType_Sunlight".Translate();
        }

        protected override bool IsAllergenic(ThingDef thingDef) => false;

        protected override void DoPassiveExposureChecks()
        {
            ExposureType sunlightExposure = GetSunlightExposure(Pawn, out string seasonLabel);
            if (sunlightExposure != ExposureType.None) IncreaseAllergenBuildup(sunlightExposure, "P42_AllergyCause_Sunlight".Translate(seasonLabel));
        }

        private ExposureType GetSunlightExposure(Pawn pawn, out string seasonLabel)
        {
            seasonLabel = "";

            // Check for valid map and position.
            if (pawn.Map == null || pawn.Position == null) return ExposureType.None;

            // Check if the pawn is outside (no roof overhead)
            if (pawn.Map.roofGrid.Roofed(pawn.Position)) return ExposureType.None;

            // Check if the weather allows for sunlight exposure
            bool hasSunlight = pawn.Map.weatherManager.curWeather == WeatherDefOf.Clear;
            if (!hasSunlight) return ExposureType.None;

            bool hasDaylight = GenCelestial.CurCelestialSunGlow(pawn.Map) > 0.5f;
            if (!hasDaylight) return ExposureType.None;

            Vector2 longLat = Find.WorldGrid.LongLatOf(pawn.Map.Tile);
            Season currentSeason = GenDate.Season(Find.TickManager.TicksAbs, longLat);
            seasonLabel = currentSeason.Label();

            if (currentSeason == Season.Winter || currentSeason == Season.PermanentWinter) return ExposureType.MinorPassive;
            else if (currentSeason == Season.Spring || currentSeason == Season.Fall) return ExposureType.StrongPassive;
            else if (currentSeason == Season.Summer || currentSeason == Season.PermanentSummer) return ExposureType.ExtremePassive;

            if (Prefs.DevMode) Log.Message($"[Allergies Mod] Error in getting sunlight exposure.");
            return ExposureType.None;
        }

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is SunlightAllergy);
        }
        private string typeLabel;
        public override string TypeLabel => typeLabel;
        public override string KeepAwayFromText => typeLabel;
        protected override void ExposeExtraData() { }
    }
}
