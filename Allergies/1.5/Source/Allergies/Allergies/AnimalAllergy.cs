using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P42_Allergies
{
    public class AnimalAllergy : Allergy
    {
        public override string TypeLabel => "animal";
        public override string TypeLabelPlural => "animals";

        public AnimalAllergy(Hediff_Allergy hediff, AllergySeverity severity) : base(hediff, severity) { }
    }
}
