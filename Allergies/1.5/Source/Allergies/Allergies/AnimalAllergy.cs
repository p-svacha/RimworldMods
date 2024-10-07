using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class AnimalAllergy : Allergy
    {
        public PawnKindDef Animal;

        protected override void DoPassiveExposureChecks()
        {
            DoPieCheck(IsAnimal);
        }

        public bool IsAnimal(ThingDef thing) => thing == Animal.race;

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is AnimalAllergy animalAllergy && animalAllergy.Animal == Animal);
        }
        public override string TypeLabel => Animal.label;
        public override string TypeLabelPlural => Animal.labelPlural;
        protected override void ExposeExtraData()
        {
            Scribe_Values.Look(ref Animal, "animal");
        }
    }
}

