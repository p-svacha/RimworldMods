using HarmonyLib;
using System;
using System.Reflection;
using Verse;

namespace P42_RealRuinsDynamicWealthCap
{
    /// <summary>
    /// Helper class that provides methods to access and modify the original RealRuins mod's settings.
    /// </summary>
    internal static class RealRuinsHelper
    {
        static readonly Type SettingsType = AccessTools.TypeByName("RealRuins.RealRuins_ModSettings");
        static readonly FieldInfo DefaultScatterOptionsField = AccessTools.Field(SettingsType, "defaultScatterOptions");
        static readonly FieldInfo RuinsCostCapField = AccessTools.Field(SettingsType, "ruinsCostCap");

        /// <summary>
        /// Checks if the necessary types and fields from the RealRuins mod are available. If not, it logs an error message indicating that the mod is incompatible with the current version of RealRuins.
        /// </summary>
        public static bool ValidateOrLogError()
        {
            if (SettingsType == null || DefaultScatterOptionsField == null || RuinsCostCapField == null)
            {
                Log.Error(
                    "[RR - Dynamic Wealth Cap] This mod could not find something it expects inside the Real Ruins mod.\n" +
                    "This means Real Ruins has been updated in a way that Dynamic Wealth Cap doesn't support yet, and it will NOT work currently.\n" +
                    "Real Ruins itself should still work normally on its own.\n" +
                    "Please disable Real Ruins - Dynamic Wealth Cap for now, and report this on the Steam Workshop page so it can be updated."
                );
                return false;
            }

            if (Prefs.DevMode)
            {
                Log.Message($"[RR - Dynamic Wealth Cap] Real Ruins compatibility validated successfully.");
            }

            return true;
        }

        public static float GetRuinsCostCap() => (float)RuinsCostCapField.GetValue(null);
        public static void SetRuinsCostCap(float v) => RuinsCostCapField.SetValue(null, v);

        public static void SetDefaultScatterCapAndFloor(int cap, int floor)
        {
            object opts = DefaultScatterOptionsField.GetValue(null);
            var t = opts.GetType();
            AccessTools.Field(t, "costCap").SetValue(opts, cap);
            AccessTools.Field(t, "minimumCostRequired").SetValue(opts, floor);
        }
    }
}