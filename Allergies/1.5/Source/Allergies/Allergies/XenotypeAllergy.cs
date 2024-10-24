﻿using RimWorld;
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

        protected override void OnCreate()
        {
            Xenotype = AllergyGenerator.GetRandomXenotype();
        }

        protected override bool IsDirectlyAllergenic(ThingDef thingDef) => false;

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyThingsForPassiveExposure(checkNearbyItems: false, checkApparel: false, checkInventory: false); // needed so OnNearbyPawn gets called
        }

        protected override void OnNearbyPawn(Pawn pawn)
        {
            if (pawn.genes == null) return;
            if (pawn.genes.Xenotype.defName == Xenotype.defName)
                IncreaseAllergenBuildup(ExposureType.MinorPassive, "P42_AllergyCause_BeingNearby".Translate(pawn.LabelShort));
        }

        public override void OnDamageTaken(DamageInfo dinfo)
        {
            if (dinfo.Def.isRanged) return;
            if (dinfo.Def.isExplosive) return;
            if (dinfo.Instigator == Pawn) return;

            if (dinfo.Instigator is Pawn pawn)
            {
                if (pawn.genes == null) return;
                if (pawn.genes.Xenotype.defName == Xenotype.defName)
                    IncreaseAllergenBuildup(ExposureType.StrongEvent, "P42_AllergyCause_DamagedBy".Translate(pawn.LabelShort));
            }
        }
        public override void OnInteractedWith(Pawn pawn)
        {
            if (pawn.genes == null) return;
            if (pawn.genes.Xenotype.defName == Xenotype.defName)
                IncreaseAllergenBuildup(ExposureType.MinorEvent, "P42_AllergyCause_InteractedWith".Translate(pawn.LabelShort));
        }

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is XenotypeAllergy otherXenotypeAllergy && otherXenotypeAllergy.Xenotype == Xenotype);
        }
        public override string TypeLabel => Xenotype.label;
        public override string KeepAwayFromText => Xenotype.label + "s";
        protected override void ExposeExtraData()
        {
            Scribe_Defs.Look(ref Xenotype, "xenotype");
        }
    }
}
