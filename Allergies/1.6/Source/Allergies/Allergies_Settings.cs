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
        // Base chance setting
		private const float baseAllergyChance_DefaultValue = 0.09f;
        public static float baseAllergyChance = 0.09f;

        // Toxic Resistance Effect Setting
        private const float toxicResEffect_DefaultValue = 0.25f;
        public static float toxicResEffect = 0.25f;

        // Enable/Disable settings
        public static Dictionary<string, bool> enabledAllergyTypes = new Dictionary<string, bool>();
		private List<string> allergyKeys;
		private List<bool> boolValues;

		// Commonness settings
        public static Dictionary<string, float> allergyCommonness = new Dictionary<string, float>();
        private List<string> commonnessKeys;
        private List<float> commonnessValues;

        private static Vector2 scrollPosition = Vector2.zero;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref baseAllergyChance, "baseAllergyChance", baseAllergyChance_DefaultValue);
            Scribe_Values.Look(ref toxicResEffect, "toxicResEffect", toxicResEffect_DefaultValue);

            Scribe_Collections.Look(ref enabledAllergyTypes, "enabledAllergyTypes", LookMode.Value, LookMode.Value, ref allergyKeys, ref boolValues);
            Scribe_Collections.Look(ref allergyCommonness, "allergyCommonness", LookMode.Value, LookMode.Value, ref commonnessKeys, ref commonnessValues);
        }

		public void DoSettingsWindowContents(Rect inRect)
		{
            // Ensure dictionaries are initialized
            if (enabledAllergyTypes == null) enabledAllergyTypes = new Dictionary<string, bool>();
            if (allergyCommonness == null) allergyCommonness = new Dictionary<string, float>();

			// Init window content
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);

			// Base allergy chance slider
            CreatePercentSlider(listing_Standard, "P42_Allergies_Settings_BaseAllergyChance".Translate(), ref baseAllergyChance, baseAllergyChance_DefaultValue);

            // Toxic Resistance Effect slider with Tooltip
            CreatePercentSlider(
                listing_Standard,
                "P42_Allergies_Settings_ToxicResEffect".Translate(),
                ref toxicResEffect,
                toxicResEffect_DefaultValue,
                "P42_Allergies_Settings_ToxicResEffect_Desc".Translate()
            );

            listing_Standard.GapLine();
            listing_Standard.Label("P42_Allergies_Settings_EnableAllergyTypes".Translate() + ":");
            listing_Standard.Gap(Text.LineHeight / 2);

            // Iterate through all AllergyDefs
            foreach (AllergyDef def in DefDatabase<AllergyDef>.AllDefsListForReading.OrderBy(x => x.label))
			{
                // Ensure defaults exist for this specific def
                if (!enabledAllergyTypes.ContainsKey(def.defName)) enabledAllergyTypes[def.defName] = true;

                // Ensure commonness dictionary has value, otherwise use original
                if (!allergyCommonness.ContainsKey(def.defName)) allergyCommonness[def.defName] = def.defaultCommonness;

                // Set the actual commonness according to the settings
                def.commonness = allergyCommonness[def.defName];

                // Draw the custom row
                DrawAllergyRow(listing_Standard, def);
            }

			// Finalize window content
			listing_Standard.End();
		}

        /// <summary>
        /// Draws a row containing: Checkbox | Label | Slider | Value | Reset Button
        /// </summary>
        private void DrawAllergyRow(Listing_Standard listing, AllergyDef def)
		{
            Rect rect = listing.GetRect(30f);

            // Define column widths
            float checkboxWidth = 24f;
            float labelWidth = 150f;
            float sliderWidth = 200f;
            float valLabelWidth = 50f;
            float buttonWidth = 80f;

            // Checkbox and Label (Enable/Disable)
            bool isEnabled = enabledAllergyTypes[def.defName];
            Rect checkRect = new Rect(rect.x, rect.y, checkboxWidth, rect.height);
            Rect labelRect = new Rect(checkRect.xMax + 5f, rect.y, labelWidth, rect.height);

            Widgets.Checkbox(checkRect.position, ref isEnabled);
            Widgets.Label(labelRect, def.LabelCap);
            if (!string.IsNullOrEmpty(def.description)) TooltipHandler.TipRegion(labelRect, def.description); // Tooltip

            enabledAllergyTypes[def.defName] = isEnabled;

            // Commonness Slider (0.0 to 2.0)
            Rect sliderRect = new Rect(labelRect.xMax + 10f, rect.y + 5f, sliderWidth, 24f);
            float currentCommonness = allergyCommonness[def.defName];

            // Only draw slider if enabled
            if (isEnabled)
            {
                float newCommonness = Widgets.HorizontalSlider(sliderRect, currentCommonness, 0f, 2f, middleAlignment: true);

                // Value Label
                Rect valRect = new Rect(sliderRect.xMax + 5f, rect.y, valLabelWidth, rect.height);
                Widgets.Label(valRect, newCommonness.ToString("0.00"));

                // Reset Button
                Rect btnRect = new Rect(valRect.xMax + 10f, rect.y, buttonWidth, rect.height);
                if (Widgets.ButtonText(btnRect, "P42_Allergies_Settings_Reset".Translate())) newCommonness = def.defaultCommonness;

                // Apply changes
                if (newCommonness != currentCommonness)
                {
                    allergyCommonness[def.defName] = newCommonness;
                    def.commonness = newCommonness;
                }
            }
        }


        private void CreatePercentSlider(Listing_Standard listingStandard, string labelName, ref float value, float defaultValue, string tooltip = null)
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

            // Add Tooltip if provided
            if (!string.IsNullOrEmpty(tooltip)) TooltipHandler.TipRegion(rectLabel, tooltip);

            value = Widgets.HorizontalSlider(rectSlider, value, 0f, 1f, middleAlignment: true);
			Widgets.Label(rectSliderValueLabel, $"{value * 100f:F0}%");
			if (Widgets.ButtonText(rectButton, "P42_Allergies_Settings_Reset".Translate()))
            {
				value = defaultValue;
            }
		}
    }
}
