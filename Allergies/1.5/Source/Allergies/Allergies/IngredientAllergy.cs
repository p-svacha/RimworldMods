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
            CheckNearbyItemsForPassiveExposure(checkApparel: false, checkPlants: true);
        }

        public override bool IsAllergenic(ThingDef thing) => thing == Ingredient;

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is IngredientAllergy otherIngredientAllergy && otherIngredientAllergy.Ingredient == Ingredient);
        }
        public override string TypeLabel => Ingredient.label;
        protected override void ExposeExtraData()
        {
            Scribe_Defs.Look(ref Ingredient, "ingredient");
        }
    }
}
