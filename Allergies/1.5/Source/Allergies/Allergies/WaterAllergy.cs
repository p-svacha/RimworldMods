using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class WaterAllergy : Allergy
    {
        protected override void OnInitOrLoad()
        {
            typeLabel = "P42_AllergyType_Water".Translate();
        }

        protected override bool IsAllergenic(ThingDef thingDef) => thingDef == ThingDefOf.SteamGeyser;

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyThingsForPassiveExposure(checkInventory: false, checkApparel: false);
            CheckNearbyFloorsForPassiveExposure();

            if (IsPawnExposedToRain(Pawn))
            {
                IncreaseAllergenBuildup(ExposureType.ExtremePassive, "P42_AllergyCause_Rain".Translate());
            }
            if (IsPawnExposedToFog(Pawn))
            {
                IncreaseAllergenBuildup(ExposureType.MinorPassive, "P42_AllergyCause_Fog".Translate());
            }
        }

        protected override ExposureType GetDirectExposureOfFloor(TerrainDef def)
        {
            if (def.IsWater || def.defName == "MarshyTerrain") return ExposureType.MinorPassive;
            return ExposureType.None;
        }

        private bool IsPawnExposedToRain(Pawn pawn)
        {
            if (pawn.Map == null) return false;
            if (pawn.Map.weatherManager.curWeather.rainRate > 0 && !pawn.Position.Roofed(pawn.Map)) return true;
            return false;
        }
        private bool IsPawnExposedToFog(Pawn pawn)
        {
            if (pawn.Map == null) return false;
            if (pawn.Map.weatherManager.curWeather == WeatherDef.Named("Fog") && !pawn.Position.Roofed(pawn.Map)) return true;
            return false;
        }

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is WaterAllergy);
        }
        private string typeLabel;
        public override string TypeLabel => typeLabel;
        public override string KeepAwayFromText => typeLabel;
        protected override void ExposeExtraData() { }
    }
}
