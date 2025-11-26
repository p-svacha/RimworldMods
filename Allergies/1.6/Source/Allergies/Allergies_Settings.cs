using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace P42_Allergies
{
    public class Allergies_Settings : ModSettings
    {
		private const float baseAllergyChance_DefaultValue = 0.09f;
        public static float baseAllergyChance = 0.09f;

		public static Dictionary<string, bool> enabledAllergyTypes = new Dictionary<string, bool>();
		private List<string> allergyKeys;
		private List<bool> boolValues;

		private static Vector2 scrollPosition = Vector2.zero;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref baseAllergyChance, "baseAllergyChance", baseAllergyChance_DefaultValue);
			Scribe_Collections.Look(ref enabledAllergyTypes, "enabledAllergyTypes", LookMode.Value, LookMode.Value, ref allergyKeys, ref boolValues);
		}

		public void DoSettingsWindowContents(Rect inRect)
		{
			foreach (AllergyDef def in DefDatabase<AllergyDef>.AllDefsListForReading)
			{
				if (enabledAllergyTypes == null)
				{
					enabledAllergyTypes = new Dictionary<string, bool>();
				}
				if (!enabledAllergyTypes.ContainsKey(def.defName))
				{
					enabledAllergyTypes[def.defName] = true;
				}
			}

			//Rect rect = new Rect(0f, 0f, inRect.width - 16f, inRect.height + 50f);
			//Vector2 scrollPosition = Vector2.zero;
			//Widgets.BeginScrollView(inRect, ref scrollPosition, rect);

			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.Begin(inRect);
			CreatePercentSlider(listing_Standard, "P42_Allergies_Settings_BaseAllergyChance".Translate(), ref baseAllergyChance, baseAllergyChance_DefaultValue);
			listing_Standard.GapLine();

			listing_Standard.Label("P42_Allergies_Settings_EnableAllergyTypes".Translate() + ":");
			List<string> list = enabledAllergyTypes.Keys.OrderByDescending(x => x).ToList();
			for (int num = list.Count - 1; num >= 0; num--)
			{
				bool checkOn = enabledAllergyTypes[list[num]];
				listing_Standard.CheckboxLabeled(list[num], ref checkOn);
				enabledAllergyTypes[list[num]] = checkOn;
			}

			listing_Standard.End();

			Write();
		}

		private void CreatePercentSlider(Listing_Standard listingStandard, string labelName, ref float value, float defaultValue)
		{
			Rect rectBase = listingStandard.GetRect(Text.LineHeight);

			float widthLabel = rectBase.width * 0.35f;
			float widthSlider = rectBase.width * 0.35f;
			float widthSliderValueLabel = rectBase.width * 0.10f;
			float widthButton = rectBase.width * 0.20f;

			Rect rectLabel = new Rect(rectBase.x, rectBase.y, widthLabel, rectBase.height);
			Rect rectSlider = new Rect(rectLabel.xMax, rectBase.y, widthSlider, rectBase.height);
			Rect rectSliderValueLabel = new Rect(rectSlider.xMax, rectBase.y, widthSliderValueLabel, rectBase.height);
			Rect rectButton = new Rect(rectSliderValueLabel.xMax, rectBase.y, widthButton, rectBase.height);

			Widgets.Label(rectLabel, labelName);
			value = Widgets.HorizontalSlider(rectSlider, value, 0f, 1f, middleAlignment: true);
			Widgets.Label(rectSliderValueLabel, $"{value * 100f:F0}%");
			if (Widgets.ButtonText(rectButton, "P42_Allergies_Settings_Reset".Translate()))
            {
				value = defaultValue;
            }
		}
	}
}
