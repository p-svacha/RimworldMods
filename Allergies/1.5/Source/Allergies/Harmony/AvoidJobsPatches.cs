using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

/// <summary>
/// This file contains all prefix-patches for WorkGivers that are responsible for pawns avoiding things that they are allergic to. (i.e. hauling, plant cutting etc.)
/// </summary>
namespace P42_Allergies
{

    #region General avoidance of things in thing-related jobs / WorkGivers

    // We patch IsForbidden for a pawn, so that everything allergenic is forbidden, but only temporarily during when they look for a new non-forced job.

    [HarmonyPatch(typeof(JobGiver_Work), "TryIssueJobPackage")]
    public static class HarmonyPatch_JobGiver_Work_TryIssueJobPackage
    {
        [HarmonyPrefix]
        public static void Prefix(Pawn pawn, JobIssueParams jobParams)
        {
            HarmonyPatch_ForbidUtility_IsForbidden.InTryIssueJobPackage = true;
        }

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, JobIssueParams jobParams)
        {
            HarmonyPatch_ForbidUtility_IsForbidden.InTryIssueJobPackage = false;
        }
    }

    [HarmonyPatch(typeof(ForbidUtility))]
    [HarmonyPatch("IsForbidden", typeof(Thing), typeof(Pawn))]
    public static class HarmonyPatch_ForbidUtility_IsForbidden
    {
        public static bool InTryIssueJobPackage;

        [HarmonyPostfix]
        public static void Postfix(this Thing t, Pawn pawn, ref bool __result)
        {
            if (__result) return; // Don't need to do anything if already forbidden
            if (!InTryIssueJobPackage) return; // We are not in the TryIssueJobPackage function and therefore shouldn't change anything

            if (Utils.IsKnownAllergenic(pawn, t)) __result = true; // Make the thing forbidden (just for the job search)
        }
    }

    #endregion

    #region Opportunistic hauling

    [HarmonyPatch(typeof(Pawn_JobTracker), "TryOpportunisticJob")]
    public static class HarmonyPatch_Pawn_JobTracker_TryOpportunisticJob
    {
        [HarmonyPrefix]
        public static void Prefix(Pawn_JobTracker __instance, Job finalizerJob, Job job)
        {
            HarmonyPatch_ListerHaulables_ThingsPotentiallyNeedingHauling.InTryOpportunisticJob = true;
            HarmonyPatch_ListerHaulables_ThingsPotentiallyNeedingHauling.JobTracker = __instance;
        }
        [HarmonyPostfix]
        public static void Postfix(Job finalizerJob, Job job)
        {
            HarmonyPatch_ListerHaulables_ThingsPotentiallyNeedingHauling.InTryOpportunisticJob = false;
        }
    }

    [HarmonyPatch(typeof(ListerHaulables), "ThingsPotentiallyNeedingHauling")]
    public static class HarmonyPatch_ListerHaulables_ThingsPotentiallyNeedingHauling
    {
        public static Pawn_JobTracker JobTracker;
        public static bool InTryOpportunisticJob;

        [HarmonyPostfix]
        public static void Postfix(ref List<Thing> __result)
        {
            if (!InTryOpportunisticJob) return;

            FieldInfo fieldInfo = typeof(Pawn_JobTracker).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
            Pawn pawn = (Pawn)fieldInfo.GetValue(JobTracker);
            __result = __result.Where(x => !Utils.IsKnownAllergenic(pawn, x)).ToList();
        }
    }

    #endregion

    #region Construction

    // Smooth wall
    [HarmonyPatch(typeof(WorkGiver_ConstructSmoothWall), "HasJobOnCell")]
    public static class HarmonyPatch_WorkGiver_ConstructSmoothWall_HasJobOnCell
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, IntVec3 c, bool forced, ref bool __result)
        {
            if (!__result) return; // Only interested if pawn has job on cell
            if (forced) return; // Only interested if job isn't forced

            if (Utils.IsKnownAllergenic(pawn, c.GetEdifice(pawn.Map))) __result = false;
        }
    }

    // Smooth / Remove floor
    [HarmonyPatch(typeof(WorkGiver_ConstructAffectFloor), "HasJobOnCell")]
    public static class HarmonyPatch_WorkGiver_ConstructAffectFloor_HasJobOnCell
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, IntVec3 c, bool forced, ref bool __result)
        {
            if (!__result) return; // Only interested if pawn has job on cell
            if (forced) return; // Only interested if job isn't forced

            if (Utils.IsKnownAllergenic(pawn, pawn.Map.terrainGrid.TerrainAt(c))) __result = false;
        }
    }

    #endregion

    #region Bills

    [HarmonyPatch(typeof(WorkGiver_DoBill), "StartOrResumeBillJob")]
    public static class HarmonyPatch_WorkGiver_DoBill_StartOrResumeBillJob
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, IBillGiver giver, bool forced, ref Job __result)
        {
            HarmonyPatch_WorkGiver_DoBill_IsUsableIngredient.IsBillForced = forced;
            HarmonyPatch_WorkGiver_DoBill_IsUsableIngredient.BillPawn = pawn;
            return true; // Execute original function - we just want to save the bill and pawn so we can use it in IsUsableIngredient-Patch
        }
    }
    [HarmonyPatch(typeof(WorkGiver_DoBill), "IsUsableIngredient")]
    public static class HarmonyPatch_WorkGiver_DoBill_IsUsableIngredient
    {
        public static bool IsBillForced;
        public static Pawn BillPawn;
        [HarmonyPostfix]
        public static void Postfix(Thing t, Bill bill, ref bool __result)
        {
            if (!__result) return;

            if (bill.IsFixedOrAllowedIngredient(t) && !IsBillForced && Utils.IsKnownAllergenic(BillPawn, t))
            {
                Logger.Log($"{BillPawn.LabelShort} will not work on bill {bill.Label} with ingredient {t.Label} because they are allergenic to them.");
                __result = false;
            }
        }
    }

    #endregion


    #region Plant work

    // Harvest
    [HarmonyPatch(typeof(WorkGiver_GrowerHarvest), "HasJobOnCell")]
    public static class HarmonyPatch_WorkGiver_GrowerHarvest_HasJobOnCell
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, IntVec3 c, bool forced, ref bool __result)
        {
            if (!__result) return; // Only interested if pawn has job on cell
            if (forced) return; // Only interested if job isn't forced

            if (Utils.IsKnownAllergenic(pawn, c.GetPlant(pawn.Map))) __result = false;
        }
    }

    // Sow
    [HarmonyPatch(typeof(WorkGiver_GrowerSow), "JobOnCell")]
    public static class HarmonyPatch_WorkGiver_GrowerSow_JobOnCell
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, IntVec3 c, bool forced, ref Job __result)
        {
            if (__result == null) return; // Only interested if pawn has job on cell
            if (forced) return; // Only interested if job isn't forced

            Zone_Growing zone_Growing = c.GetZone(pawn.Map) as Zone_Growing;
            if(zone_Growing != null && Utils.IsKnownAllergenic(pawn, zone_Growing.PlantDefToGrow)) __result = null;
        }
    }

    #endregion

    #region Prioritiation (food / apparel)

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
            Logger.Log($"optimality of {foodSource.Label} for {eater.LabelShort} is {__result}: Allergenic? {Utils.IsKnownAllergenic(eater, foodSource)}");
            if (Utils.IsKnownAllergenic(eater, foodSource)) __result -= 200f;
        }
    }

    #endregion
}

