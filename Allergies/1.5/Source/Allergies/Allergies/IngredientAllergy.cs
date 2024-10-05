﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class IngredientAllergy : Allergy
    {
        public ThingDef Ingredient;

        public override void Tick()
        {
            base.Tick();

            if (Pawn.IsHashIntervalTick(ExposureCheckInterval))
            {
                // PIE-checks
                DoPieCheck(IsIngredient);
            }
        }

        public bool IsIngredient(ThingDef thing) => thing == Ingredient;

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is IngredientAllergy otherIngredientAllergy && otherIngredientAllergy.Ingredient == Ingredient);
        }
        public override string TypeLabel => Ingredient.label;
        public override string TypeLabelPlural => Ingredient.label;
    }
}
