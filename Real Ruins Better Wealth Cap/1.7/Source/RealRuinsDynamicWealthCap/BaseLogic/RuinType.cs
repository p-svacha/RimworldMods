namespace P42_RealRuinsDynamicWealthCap
{
    /// <summary>
    /// The ruin-generation paths that each have their own wealth cap.
    /// </summary>
    public enum RuinType
    {
        /// <summary>Standard scattered ruins generated on every map (GenStep_ScatterRealRuins).</summary>
        StandardMap,

        /// <summary>Ruins from the "small ruins found" caravan event on pocket maps (GenStep_ScatterMediumRealRuins).</summary>
        Caravan,

        /// <summary>Ruins from the "Pristine ruins found" incident and the RealRuins_AbandonedBase quest.</summary>
        AbandonedBase
    }
}
