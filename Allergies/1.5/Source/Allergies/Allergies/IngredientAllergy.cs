using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P42_Allergies
{
    public class IngredientAllergy : Allergy
    {
        public override string TypeLabel => "ingredient";
        public override string TypeLabelPlural => "ingredients";

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return true;
        }
    }
}
