using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;
using Verse.AI;
using RimWorld;

namespace P42_Allergies
{
    /// <summary>
    /// Patch that's responsible to create an alert when the player forces a job on a pawn that would expose them to allergens.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_JobTracker), "TryTakeOrderedJob")]
    public static class HarmonyPatch_Pawn_JobTracker_TryTakeOrderedJob
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn_JobTracker __instance, Job job, JobTag? tag, bool requestQueueing, ref bool __result)
        {
            if (!__result) return;
            if (job.targetA == null) return;
            if (job.targetA.Thing == null) return;

            if (job.targetA.Thing is Thing thing)
            {
                Pawn pawn = Utils.GetPawnFromJobTracker(__instance);
                if (Utils.IsKnownAllergenic(pawn, thing))
                {
                    Messages.Message("P42_Message_AllergicJobWarning".Translate(pawn.LabelShort, thing.Label), new LookTargets(pawn, thing), MessageTypeDefOf.NeutralEvent);
                }
            }
        }
    }
}
