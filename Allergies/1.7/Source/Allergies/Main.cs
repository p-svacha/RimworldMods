using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace P42_Allergies
{
	[StaticConstructorOnStartup]
	public class Main : Mod
    {
		private Allergies_Settings settings;

		public Main(ModContentPack content) : base(content)
		{
			settings = GetSettings<Allergies_Settings>();
			ApplyHarmonyPatches();
		}

		private static void ApplyHarmonyPatches()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			Harmony val = new Harmony("Phil42.Allergies");
			val.PatchAll();
		}

		public override string SettingsCategory()
		{
			return "Allergies";
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			settings.DoSettingsWindowContents(inRect);
		}

        public override void WriteSettings()
        {
            base.WriteSettings();
			AllergyGenerator.RecreateAllergyWeightsTable();
        }
    }
}
