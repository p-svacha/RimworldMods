using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace P42_Allergies
{
    [HarmonyPatch(typeof(FoodUtility), "FoodOptimality")]
    public static class HarmonyPatch_FoodUtility_FoodOptimality
    {
		[HarmonyPostfix]
		public static void Postfix(Pawn eater, Thing foodSource, bool takingToInventory, ref float __result)
		{
			if (eater == null || !AllergyUtility.CheckForAllergies(eater)) return;

			foreach (Hediff_Allergy allergyHediff in AllergyUtility.GetPawnAllergies(eater))
			{
				if (!allergyHediff.GetAllergy().IsAllergyDiscovered) continue;

				ExposureType exposure = allergyHediff.GetAllergy().GetAllergicExposureOfThing(foodSource, "", out _,
					directExposure: ExposureType.MinorEvent,
					ingredientExposure: ExposureType.MinorEvent,
					stuffExposure: ExposureType.None,
					productionIngredientExposure: ExposureType.MinorEvent,
					butcherProductExposure: ExposureType.MinorEvent,
					plantExposure: ExposureType.None,
					mineableThingExposure: ExposureType.None);

				bool isAllergic = exposure != ExposureType.None;

				if (isAllergic) __result -= 100f; // Drastically reduce preferability if allergic
			}
		}
	}
}
