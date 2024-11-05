using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace P42_Allergies
{
    public class AllergyDef : Def
    {
        public Type allergyClass = typeof(Allergy);
        public float commonness = 1f;

        public float mildSeverityReactionsCap = 0.35f;
        public float moderateSeverityReactionsCap = 0.70f;

        public float severityCommonness_mild = 1f;
        public float severityCommonness_moderate = 1f;
        public float severityCommonness_severe = 0.85f;
        public float severityCommonness_extreme = 0.01f;

        public float minorPassiveExposure_increasePerHour = 0.03f;
        public float strongPassiveExposure_increasePerHour = 0.06f;
        public float extremePassiveExposure_increasePerHour = 0.12f;

        public float minorExposureEvent_instantIncrease = 0.10f;
        public float strongExposureEvent_instantIncrease = 0.25f;
        public float extremeExposureEvent_instantIncrease = 0.45f;

        public float exposureMultiplier_mild = 0.6f;
        public float exposureMultiplier_moderate = 0.6f;
        public float exposureMultiplier_severe = 1.6f;

        public int maxPassiveExposureTriggersForSameThingPerCheck = 3;

        public List<string> requiredMods = null;
    }
}
