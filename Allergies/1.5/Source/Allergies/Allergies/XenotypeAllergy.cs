using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class XenotypeAllergy : Allergy
    {
        public XenotypeDef Xenotype;

        public override bool IsAllergenic(ThingDef thingDef) => false;

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyItemsForPassiveExposure(checkNearbyItems: false, checkApparel: false, checkInventory: false); // needed so OnNearbyPawn gets called
        }

        protected override void OnNearbyPawn(Pawn pawn)
        {
            if (pawn.genes == null) return;
            if (pawn.genes.Xenotype.defName == Xenotype.defName)
                IncreaseAllergenBuildup(ExposureType.MinorPassive, "P42_AllergyCause_BeingNearby".Translate(pawn.genes.xenotypeName));
        }

        public override void OnDamageTaken(DamageInfo dinfo)
        {
            if (dinfo.Def.isRanged) return; // Only intereseted in melee hits
            if (dinfo.Def.isExplosive) return; // Only intereseted in melee hits
            if (dinfo.Instigator == Pawn) return; // Only intereseted in melee hits

            if (dinfo.Instigator is Pawn pawn)
            {
                if (pawn.genes == null) return;
                if (pawn.genes.Xenotype.defName == Xenotype.defName)
                    IncreaseAllergenBuildup(ExposureType.StrongEvent, "P42_AllergyCause_DamagedBy".Translate(pawn.genes.xenotypeName));
            }
        }
        public override void OnInteractedWith(Pawn pawn)
        {
            if (pawn.genes == null) return;
            if (pawn.genes.Xenotype.defName == Xenotype.defName)
                IncreaseAllergenBuildup(ExposureType.MinorEvent, "P42_AllergyCause_InteractedWith".Translate(pawn.genes.xenotypeName));
        }

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is XenotypeAllergy otherXenotypeAllergy && otherXenotypeAllergy.Xenotype == Xenotype);
        }
        public override string TypeLabel => Xenotype.label;
        protected override void ExposeExtraData()
        {
            Scribe_Defs.Look(ref Xenotype, "xenotype");
        }
    }
}
