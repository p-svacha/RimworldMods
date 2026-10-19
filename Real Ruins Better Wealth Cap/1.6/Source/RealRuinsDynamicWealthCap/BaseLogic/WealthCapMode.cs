namespace P42_RealRuinsDynamicWealthCap
{
    /// <summary>
    /// How the wealth cap of generated ruins is determined.
    /// </summary>
    public enum WealthCapMode
    {
        /// <summary>The cap scales with the player's wealth, adjusted by a global multiplier.</summary>
        Dynamic,

        /// <summary>The cap is a fixed value per ruin type, as set in the mod settings, independent of player wealth.</summary>
        Absolute
    }
}
