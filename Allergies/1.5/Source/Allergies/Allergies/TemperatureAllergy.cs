using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class TemperatureAllergy : Allergy
    {
        public bool IsHeatAllergy;

        public const int HeatTreshold_Minor = 30;
        public const int HeatTreshold_Strong = 37;
        public const int HeatTreshold_Extreme = 45;

        public const int ColdThreshold_Minor = -5;
        public const int ColdThreshold_Strong = -12;
        public const int ColdThreshold_Extreme = -20;

        protected override bool IsAllergenic(ThingDef thingDef) => false;

        protected override void DoPassiveExposureChecks()
        {
            ExposureType exposure = GetExposure(Pawn, out string intensity);
            if (exposure != ExposureType.None) IncreaseAllergenBuildup(exposure, "P42_AllergyCause_Heat".Translate(intensity));
        }

        private ExposureType GetExposure(Pawn pawn, out string intensity)
        {
            intensity = "";

            if (pawn.Map == null) return ExposureType.None;

            float temperature = GenTemperature.GetTemperatureForCell(pawn.Position, pawn.Map);

            if (IsHeatAllergy) return GetExposureForHeat(temperature, out intensity);
            else return GetExposureForCold(temperature, out intensity);
        }

        private ExposureType GetExposureForHeat(float temperature, out string intensity)
        {
            intensity = "";
            if (temperature > HeatTreshold_Extreme)
            {
                intensity = "P42_AllergyExposure_Minor".Translate();
                return ExposureType.ExtremePassive;
            }
            if (temperature > HeatTreshold_Strong)
            {
                intensity = "P42_AllergyExposure_Major".Translate();
                return ExposureType.StrongPassive;
            }
            if (temperature > HeatTreshold_Minor)
            {
                intensity = "P42_AllergyExposure_Extreme".Translate();
                return ExposureType.MinorPassive;
            }
            return ExposureType.None;
        }

        private ExposureType GetExposureForCold(float temperature, out string intensity)
        {
            intensity = "";
            if (temperature < ColdThreshold_Extreme)
            {
                intensity = "P42_AllergyExposure_Minor".Translate();
                return ExposureType.ExtremePassive;
            }
            if (temperature < ColdThreshold_Strong)
            {
                intensity = "P42_AllergyExposure_Major".Translate();
                return ExposureType.StrongPassive;
            }
            if (temperature < ColdThreshold_Minor)
            {
                intensity = "P42_AllergyExposure_Extreme".Translate();
                return ExposureType.MinorPassive;
            }
            return ExposureType.None;
        }

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is TemperatureAllergy otherTemperatureAllergy && otherTemperatureAllergy.IsHeatAllergy == IsHeatAllergy);
        }
        public override string TypeLabel
        {
            get
            {
                if (IsHeatAllergy) return "P42_AllergyTemperatureType_Heat".Translate();
                else return "P42_AllergyTemperatureType_Cold".Translate();
            }
        }
        protected override void ExposeExtraData()
        {
            Scribe_Values.Look(ref IsHeatAllergy, "isHeatAllergy");
        }
    }
}
