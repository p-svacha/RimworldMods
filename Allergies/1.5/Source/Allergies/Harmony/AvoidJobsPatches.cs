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
    #region Hauling

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

    #endregion

    #region Construction

    [HarmonyPatch(typeof(WorkGiver_ConstructSmoothWall), "JobOnCell")]
    public static class HarmonyPatch_WorkGiver_ConstructSmoothWall_JobOnCell
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, IntVec3 c, bool forced, ref Job __result)
        {
            bool isAllergenic = Utils.IsKnownAllergenic(pawn, c.GetEdifice(pawn.Map));
            Logger.Log($"WorkGiver_ConstructDeliverResources {pawn.LabelShort} - {c.GetEdifice(pawn.Map).Label}: allgergenic? {isAllergenic}. __result before: {__result}");

            return Utils.CheckIfPawnShouldAvoidJobOnThing(pawn, c.GetEdifice(pawn.Map), forced, ref __result);
        }
    }

    [HarmonyPatch(typeof(WorkGiver_ConstructDeliverResources), "ResourceValidator")]
    public static class HarmonyPatch_WorkGiver_ConstructDeliverResources_ResourceValidator
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, ThingDefCountClass need, Thing th, ref bool __result)
        {
            bool isAllergenic = Utils.IsKnownAllergenic(pawn, th);
            Logger.Log($"WorkGiver_ConstructDeliverResources {pawn.LabelShort} - {th.Label}: allgergenic? {isAllergenic}. __result before: {__result}");

            if (!__result) return;

            if (Utils.IsKnownAllergenic(pawn, th)) __result = false;
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

    #region Animal work

    [HarmonyPatch(typeof(WorkGiver_GatherAnimalBodyResources), "JobOnThing")]
    public static class HarmonyPatch_WorkGiver_GatherAnimalBodyResources_JobOnThing
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

    // Slaughter
    [HarmonyPatch(typeof(WorkGiver_Slaughter), "JobOnThing")]
    public static class HarmonyPatch_WorkGiver_Slaughter_JobOnThing
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Thing t, bool forced, ref Job __result)
        {
            return Utils.CheckIfPawnShouldAvoidJobOnThing(pawn, t, forced, ref __result);
        }
    }
    [HarmonyPatch(typeof(WorkGiver_Slaughter), "ShouldSkip")]
    public static class HarmonyPatch_WorkGiver_Slaughter_ShouldSkip
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, bool forced, ref bool __result)
        {
            if (!__result) return;
            if (!forced && pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Slaughter).First().target.Thing..AnimalsToSlaughter.All(x => Utils.IsKnownAllergenic(pawn, x))) __result = true;
        }
    }

    #endregion

    #region Mining

    [HarmonyPatch(typeof(WorkGiver_Miner), "JobOnThing")]
    public static class HarmonyPatch_WorkGiver_Miner_JobOnThing
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Thing t, bool forced, ref Job __result)
        {
            return Utils.CheckIfPawnShouldAvoidJobOnThing(pawn, t, forced, ref __result);
        }
    }

    #endregion

    #region Plant work

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

    // Grow (Harvest & Sow)
    [HarmonyPatch(typeof(WorkGiver_GrowerHarvest), "JobOnCell")]
    public static class HarmonyPatch_WorkGiver_GrowerHarvest_JobOnCell
    {
        [HarmonyPostfix]
        public static bool Prefix(Pawn pawn, IntVec3 c, bool forced, ref Job __result)
        {
            Plant plant = c.GetPlant(pawn.Map);
            return Utils.CheckIfPawnShouldAvoidJobOnThing(pawn, plant, forced, ref __result);
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
            if (Utils.IsKnownAllergenic(eater, foodSource)) __result -= 100f;
        }
    }

    #endregion
}

