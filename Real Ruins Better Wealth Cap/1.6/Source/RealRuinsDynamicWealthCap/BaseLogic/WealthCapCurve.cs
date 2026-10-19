using UnityEngine;

namespace P42_RealRuinsDynamicWealthCap
{
    /// <summary>
    /// Defines a wealth-scaling curve for one ruin-generation path (standard map ruins, caravan ruins, or abandoned-base ruins). 
    /// </summary>
    public struct WealthCapCurveConfig
    {
        /// <summary>Which ruin-generation path this curve belongs to. Used to look up the absolute cap in absolute mode.</summary>
        public RuinType Type;

        /// <summary>Wealth factor applied at player wealth 0.</summary>
        public float WfMin;

        /// <summary>Wealth factor applied at 100'000 player wealth. The midpoint of the interpolation.</summary>
        public float WfMed;

        /// <summary>Wealth factor applied at 500'000 player wealth.</summary>
        public float WfMax;

        /// <summary>Hard floor on the computed ruin wealth cap, regardless of wF * wealth. Prevents ruins from becoming worthless.</summary>
        public int AbsoluteMin;

        /// <summary>Hard ceiling on the computed ruin wealth cap, regardless of wF * wealth. Prevents runaway values once player wealth gets very large.</summary>
        public int AbsoluteMax;

        /// <summary>Fraction of the computed cap used as the minimum blueprint cost when selecting a ruin. Shared across all paths rather than configured per-path.</summary>
        public const float FloorFraction = 0.1f;

        /// <summary>Curve for standard scattered ruins generated on every map (GenStep_ScatterRealRuins).</summary>
        public static readonly WealthCapCurveConfig StandardMapRuins =
            new WealthCapCurveConfig { Type = RuinType.StandardMap, WfMin = 0.35f, WfMed = 0.08f, WfMax = 0.03f, AbsoluteMin = 1000, AbsoluteMax = 20000 };

        /// <summary>Curve for the "small ruins found" caravan event (GenStep_ScatterMediumRealRuins) on small pocket maps.</summary>
        public static readonly WealthCapCurveConfig CaravanRuins =
            new WealthCapCurveConfig { Type = RuinType.Caravan, WfMin = 0.30f, WfMed = 0.07f, WfMax = 0.02f, AbsoluteMin = 1000, AbsoluteMax = 15000 };

        /// <summary>Curve for the "Pristine ruins found" incident and the equivalent RealRuins_AbandonedBase quest.</summary>
        public static readonly WealthCapCurveConfig AbandonedBaseRuins =
            new WealthCapCurveConfig { Type = RuinType.AbandonedBase, WfMin = 0.60f, WfMed = 0.30f, WfMax = 0.10f, AbsoluteMin = 5000, AbsoluteMax = 100000 };


        /// <summary>
        /// Returns a copy of this curve with WfMin/WfMed/WfMax and AbsoluteMin/AbsoluteMax scaled by the factor defined in the mod settings.
        /// </summary>
        public WealthCapCurveConfig Scaled(float factor)
        {
            return new WealthCapCurveConfig
            {
                Type = Type,
                WfMin = WfMin * factor,
                WfMed = WfMed * factor,
                WfMax = WfMax * factor,
                AbsoluteMin = Mathf.Max(1, Mathf.RoundToInt(AbsoluteMin * factor)),
                AbsoluteMax = Mathf.Max(1, Mathf.RoundToInt(AbsoluteMax * factor)),
            };
        }
    }

    /// <summary>
    /// Computes a dynamic ruin wealth cap + floor from current player wealth, using a 3-point piecewise-linear curve (0 -> MidWealthAnchor -> HighWealthAnchor).
    /// </summary>
    public static class WealthCapCurve
    {
        public const float MidWealthAnchor = 100000f;
        public const float HighWealthAnchor = 500000f;
        private const float MidToHighSpan = HighWealthAnchor - MidWealthAnchor;

        /// <summary>
        /// Computes the wealth cap and floor. In absolute mode the configured cap for the ruin type is used; in dynamic mode the curve is scaled by the global multiplier.
        /// </summary>
        public static (int cap, int floor) Calculate(float wealth, WealthCapCurveConfig baseCfg)
        {
            if (RealRuinsDynamicWealthCapSettings.Mode == WealthCapMode.Absolute)
            {
                int absoluteCap = RealRuinsDynamicWealthCapSettings.GetAbsoluteCap(baseCfg.Type);
                return (absoluteCap, (int)(absoluteCap * WealthCapCurveConfig.FloorFraction));
            }

            WealthCapCurveConfig cfg = baseCfg.Scaled(RealRuinsDynamicWealthCapSettings.WealthMultiplier);

            float wF;
            if (wealth <= MidWealthAnchor)
            {
                wF = Mathf.Lerp(cfg.WfMin, cfg.WfMed, wealth / MidWealthAnchor);
            }
            else
            {
                wF = Mathf.Lerp(cfg.WfMed, cfg.WfMax, Mathf.Clamp01((wealth - MidWealthAnchor) / MidToHighSpan));
            }

            int cap = Mathf.Clamp((int)(wealth * wF), cfg.AbsoluteMin, cfg.AbsoluteMax);
            int floor = (int)(cap * WealthCapCurveConfig.FloorFraction);
            return (cap, floor);
        }
    }
}