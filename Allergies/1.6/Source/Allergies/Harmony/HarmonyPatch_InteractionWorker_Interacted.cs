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
    [HarmonyPatch(typeof(InteractionWorker), "Interacted")]
    public static class HarmonyPatch_InteractionWorker_Interacted
    {
        [HarmonyPrefix]
        public static void Prefix(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;

            if (Utils.CheckForAllergies(initiator))
            {
                foreach (Hediff_Allergy allergyHediff in Utils.GetPawnAllergies(initiator))
                {
                    allergyHediff.GetAllergy().OnInteractedWith(recipient);
                }
            }

            if (Utils.CheckForAllergies(recipient))
            {
                foreach (Hediff_Allergy allergyHediff in Utils.GetPawnAllergies(recipient))
                {
                    allergyHediff.GetAllergy().OnInteractedWith(initiator);
                }
            }
        }
    }
}
