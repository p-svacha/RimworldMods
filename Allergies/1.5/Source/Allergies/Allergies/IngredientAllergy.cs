using System;
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

        protected override void DoPassiveExposureChecks()
        {
            DoPieCheck(IsIngredient);
        }

        public bool IsIngredient(ThingDef thing) => thing == Ingredient;

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is IngredientAllergy otherIngredientAllergy && otherIngredientAllergy.Ingredient == Ingredient);
        }
        public override string TypeLabel => Ingredient.label;
        public override string TypeLabelPlural => Ingredient.label;
        protected override void ExposeExtraData()
        {
            Scribe_Values.Look(ref Ingredient, "ingredient");
        }
    }
}
