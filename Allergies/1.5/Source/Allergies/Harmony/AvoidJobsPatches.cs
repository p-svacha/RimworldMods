using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

/// <summary>
/// This file contains all prefix-patches for WorkGivers that are responsible for pawns avoiding things that they are allergic to. (i.e. hauling, plant cutting etc.)
/// </summary>
namespace P42_Allergies
{
    // Haul
    [HarmonyPatch(typeof(WorkGiver_Haul), "JobOnThing")]
    public static class HarmonyPatch_WorkGiver_Haul_JobOnThing
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Thing t, bool forced, ref Job __result)
        {
            return Utils.CheckIfPawnShouldAvoidJobOnThing(pawn, t, forced, ref __result);
        }
    }
    [HarmonyPatch(typeof(WorkGiver_EmptyEggBox), "JobOnThing")]
    public static class HarmonyPatch_WorkGiver_EmptyEggBox_JobOnThing
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Thing t, bool forced, ref Job __result)
        {
            CompEggContainer compEggContainer = t.TryGetComp<CompEggContainer>();
            if (compEggContainer != null && compEggContainer.ContainedThing != null)
            {
                return Utils.CheckIfPawnShouldAvoidJobOnThing(pawn, compEggContainer.ContainedThing, forced, ref __result);
            }
            return true; // Execute original function
        }
    }

    // Construct
    [HarmonyPatch(typeof(WorkGiver_ConstructDeliverResourcesToBlueprints), "JobOnThing")]
    public static class HarmonyPatch_WorkGiver_ConstructDeliverResourcesToBlueprints_JobOnThing
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Thing t, bool forced, ref Job __result)
        {
            return Utils.CheckIfPawnShouldAvoidJobOnThing(pawn, t, forced, ref __result);
        }
    }
    [HarmonyPatch(typeof(WorkGiver_ConstructDeliverResourcesToFrames), "JobOnThing")]
    public static class HarmonyPatch_WorkGiver_ConstructDeliverResourcesToFrames_JobOnThing
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Thing t, bool forced, ref Job __result)
        {
            return Utils.CheckIfPawnShouldAvoidJobOnThing(pawn, t, forced, ref __result);
        }
    }
    [HarmonyPatch(typeof(WorkGiver_ConstructFinishFrames), "JobOnThing")]
    public static class HarmonyPatch_WorkGiver_ConstructFinishFrames_JobOnThing
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Thing t, bool forced, ref Job __result)
        {
            return Utils.CheckIfPawnShouldAvoidJobOnThing(pawn, t, forced, ref __result);
        }
    }
    [HarmonyPatch(typeof(WorkGiver_RemoveBuilding), "JobOnThing")]
    public static class HarmonyPatch_WorkGiver_RemoveBuilding_JobOnThing
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Thing t, bool forced, ref Job __result)
        {
            return Utils.CheckIfPawnShouldAvoidJobOnThing(pawn, t, forced, ref __result);
        }
    }
    [HarmonyPatch(typeof(WorkGiver_Repair), "JobOnThing")]
    public static class HarmonyPatch_WorkGiver_Repair_JobOnThing
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Thing t, bool forced, ref Job __result)
        {
            return Utils.CheckIfPawnShouldAvoidJobOnThing(pawn, t, forced, ref __result);
        }
    }

    // Bill
    [HarmonyPatch(typeof(WorkGiver_DoBill), "StartOrResumeBillJob")]
    public static class HarmonyPatch_WorkGiver_DoBill_StartOrResumeBillJob
    {
        public static bool IsBillForced;
        public static Pawn BillPawn;

        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, IBillGiver giver, bool forced, ref Job __result)
        {
            IsBillForced = forced;
            BillPawn = pawn;

            /*
            string s1 = "PriorityWork";
            foreach (WorkGiverDef wgd in pawn.mindState.priorityWork.WorkGiver.workType.workGiversByPriority) s1 += "\n" + wgd.defName;
            Logger.Log(s1);

            string s2 = "WorkSettings";
            foreach (WorkGiver wg in pawn.workSettings.WorkGiversInOrderNormal) s2 += "\n" + wg.def.defName;
            Logger.Log(s2);
            */

            return true; // Execute original function - we just want to save the bill and pawn so we can use it in IsUsableIngredient-Patch
        }
    }
    [HarmonyPatch(typeof(WorkGiver_DoBill), "IsUsableIngredient")]
    public static class HarmonyPatch_WorkGiver_DoBill_IsUsableIngredient
    {
        [HarmonyPrefix]
        public static bool Prefix(Thing t, Bill bill, ref bool __result)
        {
            if (bill.IsFixedOrAllowedIngredient(t) && !HarmonyPatch_WorkGiver_DoBill_StartOrResumeBillJob.IsBillForced && Utils.IsKnownAllergenic(HarmonyPatch_WorkGiver_DoBill_StartOrResumeBillJob.BillPawn, t))
            {
                Logger.Log($"{HarmonyPatch_WorkGiver_DoBill_StartOrResumeBillJob.BillPawn.LabelShort} will not work on bill {bill.Label} with ingredient {t.Label} because they are allergenic to them.");
                __result = false;
                return false; // Do not execute original function
            }

            return true; // Execute original function
        }
    }

    // Animal work
    [HarmonyPatch(typeof(WorkGiver_GatherAnimalBodyResources), "JobOnThing")]
    public static class HarmonyPatch_WorkGiver_GatherAnimalBodyResources_JobOnThing
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Thing t, bool forced, ref Job __result)
        {
            return Utils.CheckIfPawnShouldAvoidJobOnThing(pawn, t, forced, ref __result);
        }
    }
    [HarmonyPatch(typeof(WorkGiver_Slaughter), "JobOnThing")]
    public static class HarmonyPatch_WorkGiver_Slaughter_JobOnThing
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Thing t, bool forced, ref Job __result)
        {
            return Utils.CheckIfPawnShouldAvoidJobOnThing(pawn, t, forced, ref __result);
        }
    }
    [HarmonyPatch(typeof(WorkGiver_Tame), "JobOnThing")]
    public static class HarmonyPatch_WorkGiver_Tame_JobOnThing
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Thing t, bool forced, ref Job __result)
        {
            return Utils.CheckIfPawnShouldAvoidJobOnThing(pawn, t, forced, ref __result);
        }
    }
    [HarmonyPatch(typeof(WorkGiver_TakeToPen), "JobOnThing")]
    public static class HarmonyPatch_WorkGiver_TakeToPen_JobOnThing
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Thing t, bool forced, ref Job __result)
        {
            return Utils.CheckIfPawnShouldAvoidJobOnThing(pawn, t, forced, ref __result);
        }
    }

    // Mine
    [HarmonyPatch(typeof(WorkGiver_Miner), "JobOnThing")]
    public static class HarmonyPatch_WorkGiver_Miner_JobOnThing
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Thing t, bool forced, ref Job __result)
        {
            return Utils.CheckIfPawnShouldAvoidJobOnThing(pawn, t, forced, ref __result);
        }
    }

    // Plant work
    [HarmonyPatch(typeof(WorkGiver_PlantsCut), "JobOnThing")]
    public static class HarmonyPatch_WorkGiver_PlantsCut_JobOnThing
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Thing t, bool forced, ref Job __result)
        {
            return Utils.CheckIfPawnShouldAvoidJobOnThing(pawn, t, forced, ref __result);
        }
    }
    [HarmonyPatch(typeof(WorkGiver_PlantSeed), "JobOnThing")]
    public static class HarmonyPatch_WorkGiver_PlantSeed_JobOnThing
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Thing t, bool forced, ref Job __result)
        {
            return Utils.CheckIfPawnShouldAvoidJobOnThing(pawn, t, forced, ref __result);
        }
    }

    // Apparel
    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "ApparelScoreRaw")]
    public static class HarmonyPatch_JobGiver_OptimizeApparel_ApparelScoreRaw
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, Apparel ap, ref float __result)
        {
            if (Utils.IsKnownAllergenic(pawn, ap)) __result -= 50f;
        }
    }

    // Food
    [HarmonyPatch(typeof(FoodUtility), "FoodOptimality")]
    public static class HarmonyPatch_FoodUtility_FoodOptimality
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn eater, Thing foodSource, bool takingToInventory, ref float __result)
        {
            if (Utils.IsKnownAllergenic(eater, foodSource)) __result -= 100f;
        }
    }
}

