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
        protected override void DoPassiveExposureChecks()
        {
            ExposureType waterExposure = GetWaterExposure(Pawn, out string translatedCause);
            if (waterExposure != ExposureType.None) IncreaseAllergenBuildup(waterExposure, translatedCause);
        }

        private ExposureType GetWaterExposure(Pawn pawn, out string translatedCause)
        {
            translatedCause = "";

            if(IsPawnExposedToRain(pawn))
            {
                translatedCause = "P42_AllergyCause_Rain".Translate();
                return ExposureType.ExtremePassive;
            }
            if (IsPawnStandingOnWater(pawn, out string waterLabel))
            {
                translatedCause = "P42_AllergyCause_WalkOn".Translate(waterLabel);
                return ExposureType.StrongPassive;
            }
            if (IsPawnExposedToFog(pawn))
            {
                translatedCause = "P42_AllergyCause_Fog".Translate();
                return ExposureType.MinorPassive;
            }
            if(IsPawnStandingOnMarsh(pawn, out string marshLabel))
            {
                translatedCause = "P42_AllergyCause_WalkOn".Translate(marshLabel);
                return ExposureType.StrongPassive;
            }

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
        private bool IsPawnStandingOnWater(Pawn pawn, out string label)
        {
            label = "";
            if (pawn.Map == null) return false;
            TerrainDef terrain = pawn.Position.GetTerrain(pawn.Map);
            if (terrain == null) return false;
            label = terrain.label;
            return terrain.IsWater;
        }
        private bool IsPawnStandingOnMarsh(Pawn pawn, out string label)
        {
            label = "";
            if (pawn.Map == null) return false;
            TerrainDef terrain = pawn.Position.GetTerrain(pawn.Map);
            if (terrain == null) return false;
            label = terrain.label;
            return terrain.defName == "MarshyTerrain";
        }

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is WaterAllergy);
        }
        public override string TypeLabel => "P42_AllergyType_Water".Translate();
        protected override void ExposeExtraData() { }
    }
}
