using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public static class Logger
    {
        public static void Log(string message, bool ignore = false)
        {
            if (!Prefs.DevMode) return;
            if (ignore) return;

            Verse.Log.Message($"[Allergies Mod] {message}");
        }
    }
}
