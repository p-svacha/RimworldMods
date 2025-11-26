using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace P42_Allergies
{
    /// <summary>
    /// StatPart for P42_AllergicSensitivity stat that reduces the allergic sensitivity based on the pawns ToxicEnvironmentResistance stat value.
    /// </summary>
    public class StatPart_ToxicEnvAllergicSensitivity : StatPart
    {

        /// <summary>
        /// Allergic sensitivity is reduced by (factor * ToxicEnvironmentResistance).
        /// A factor of 0.25 would mean that allergic sensitivity is reduced by 25% if a pawn has 100% toxic environment resistance.
        /// </summary>
        public float factor = 0.25f;

        private bool ActiveFor(Thing t)
        {
            if (t is Pawn pawn)
            {
                // Get the pawns toxic environmental resistance
                float toxicEnvRes = GetToxicEnvironmentResistance(pawn);

                // Only active is pawn has toxic environmental resistance
                return toxicEnvRes > 0f;
            }
            return false;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing || !ActiveFor(req.Thing)) return;

            Pawn pawn = (Pawn)req.Thing;

            // Calculate the allergic sensitivity reduction amount
            float reductionAmount = GetAllergicSensitivityReductionAmount(pawn);

            // Reduce value (not below 0)
            val = Mathf.Max(val - reductionAmount, 0f);
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing || !ActiveFor(req.Thing)) return null;

            Pawn pawn = (Pawn)req.Thing;

            // Calculate the allergic sensitivity reduction amount
            float reductionAmount = GetAllergicSensitivityReductionAmount(pawn);

            if (reductionAmount == 0f) return null; // Don't mention if it has no effect

            return $"{StatDefOf.ToxicEnvironmentResistance.LabelCap}: -{reductionAmount.ToStringPercent()}";
        }

        private float GetToxicEnvironmentResistance(Pawn pawn) => Mathf.Clamp01(pawn.GetStatValue(StatDefOf.ToxicEnvironmentResistance, applyPostProcess: true));
        private float GetAllergicSensitivityReductionAmount(Pawn pawn) => GetToxicEnvironmentResistance(pawn) * factor;
    }
}
