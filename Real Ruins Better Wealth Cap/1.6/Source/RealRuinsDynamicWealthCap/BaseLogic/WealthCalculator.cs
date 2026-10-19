using RimWorld.Planet;
using Verse;

namespace P42_RealRuinsDynamicWealthCap
{
    public static class WealthCalculator
    {
        /// <summary>
        /// Calculates the total wealth of the player across all maps and caravans.
        /// </summary>
        public static float GetTotalPlayerWealth()
        {
            float total = 0f;

            foreach (Map map in Find.Maps)
            {
                if (map.IsPlayerHome) total += map.PlayerWealthForStoryteller;
            }

            foreach (Caravan caravan in Find.WorldObjects.Caravans)
            {
                total += caravan.PlayerWealthForStoryteller;
            }

            return total;
        }
    }
}