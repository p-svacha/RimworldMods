using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class AllergyGeneExtension : DefModExtension
    {
        public AllergyDef allergyDef;
        public string allergyTarget;
        public AllergySeverity severity = AllergySeverity.Moderate;
    }

    public class Gene_Allergy : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            EnsureAllergy();
        }

        public override void PostRemove()
        {
            base.PostRemove();
            RemoveAllergy();
        }

        public void EnsureAllergy(bool sendReaddMessage = false)
        {
            if (pawn?.health == null) return;

            AllergyGeneExtension ext = def.GetModExtension<AllergyGeneExtension>();
            if (ext?.allergyDef == null) return;

            if (pawn.NonHumanlikeOrWildMan() || pawn.IsMutant) return;

            // Make sure everything is initialized
            AllergyGenerator.Initialize();

            // Build a candidate with the forced target so we can dup-check via existing logic
            Allergy candidate = (Allergy)System.Activator.CreateInstance(ext.allergyDef.allergyClass);
            candidate.OnNewAllergyCreated(ext.allergyDef);
            candidate.ApplyGenePreset(ext.allergyTarget);
            candidate.Severity = ext.severity;

            // If the pawn already has a matching allergy (e.g. a random one), adopt it instead of duplicating
            Hediff_Allergy match = Utils.GetPawnAllergies(pawn)
                .FirstOrDefault(h => h.GetAllergy().IsDuplicateOf(candidate));
            if (match != null)
            {
                Allergy existing = match.GetAllergy();
                existing.IsFromGene = true;
                existing.SourceGeneDefName = def.defName;
                existing.Severity = ext.severity;
                existing.MakeAllergyVisible();
                return;
            }

            Hediff_Allergy hediff = (Hediff_Allergy)HediffMaker.MakeHediff(HediffDef.Named("P42_AllergyHediff"), pawn);
            hediff.Severity = 0.05f;
            hediff.SetAllergy(candidate);
            candidate.IsFromGene = true;
            candidate.SourceGeneDefName = def.defName;
            candidate.OnInitOrLoad(hediff);   // after preset, so labels reflect the forced target
            candidate.MakeAllergyVisible();
            pawn.health.AddHediff(hediff);

            if (sendReaddMessage) SendReaddMessage(candidate);
        }

        private void SendReaddMessage(Allergy allergy)
        {
            if (!Utils.ShouldSendAllergyNotification(pawn)) return;

            Messages.Message(
                "P42_Message_GeneAllergyReadded".Translate(pawn.Name.ToStringShort, allergy.FullAllergyName),
                pawn, MessageTypeDefOf.NeutralEvent);
        }

        private void RemoveAllergy()
        {
            if (pawn?.health == null) return;
            foreach (Hediff_Allergy h in Utils.GetPawnAllergies(pawn).ToList())
            {
                if (h.GetAllergy().SourceGeneDefName == def.defName) pawn.health.RemoveHediff(h);
            }
        }
    }
}
