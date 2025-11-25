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

        protected override void OnCreate()
        {
            Ingredient = AllergyGenerator.GetRandomIngredient();
        }

        protected override void OnInitOrLoad()
        {
            keepAwayFromText = "P42_LetterTextEnd_AllergyDiscovered_KeepAwayFrom_Food".Translate(Ingredient.label);
        }

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyThingsForPassiveExposure(checkApparel: false, checkPlants: true);
        }

        protected override bool IsDirectlyAllergenic(ThingDef thing) => thing == Ingredient;

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is IngredientAllergy otherIngredientAllergy && otherIngredientAllergy.Ingredient == Ingredient);
        }
        public override string TypeLabel => Ingredient.label;
        private string keepAwayFromText;
        public override string KeepAwayFromText => keepAwayFromText;
        protected override void ExposeExtraData()
        {
            Scribe_Defs.Look(ref Ingredient, "ingredient");
        }
    }
}
