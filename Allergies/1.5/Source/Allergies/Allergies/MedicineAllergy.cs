﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class MedicineAllergy : Allergy
    {
        public ThingDef MedicineType;

        protected override void OnCreate()
        {
            MedicineType = AllergyGenerator.GetRandomMedicine();
        }
        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyThingsForPassiveExposure(checkApparel: false, checkProductionIngredients: false, checkPlants: true);
        }

        protected override bool IsDirectlyAllergenic(ThingDef thing) => thing == MedicineType;

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is MedicineAllergy otherMedicineAllergy && otherMedicineAllergy.MedicineType == MedicineType);
        }
        public override string TypeLabel => MedicineType.label;
        public override string KeepAwayFromText => MedicineType.label;
        protected override void ExposeExtraData()
        {
            Scribe_Defs.Look(ref MedicineType, "medicineType");
        }
    }
}

