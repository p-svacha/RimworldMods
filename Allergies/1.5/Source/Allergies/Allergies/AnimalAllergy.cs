using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace P42_Allergies
{
    public class AnimalAllergy : Allergy
    {
        public PawnKindDef Animal;

        private List<ThingDef> AnimalProducts;

        protected override void OnCreate()
        {
            Animal = AllergyGenerator.GetRandomAnimal();
        }

        protected override void OnInitOrLoad()
        {
            // Logger.Log($"singular: {Animal.label}, plural: {Animal.labelPlural}");
            keepAwayFromText = "P42_LetterTextEnd_AllergyDiscovered_KeepAwayFrom_Animal".Translate(Animal.labelPlural.NullOrEmpty() ? Animal.label : Animal.labelPlural);

            AnimalProducts = new List<ThingDef>();

            // Meat
            if (Animal.race.race.meatDef != null) AnimalProducts.Add(Animal.race.race.meatDef);

            // Leather
            if (Animal.race.race.leatherDef != null) AnimalProducts.Add(Animal.race.race.leatherDef);

            // Wool
            CompProperties_Shearable shearableComp = Animal.race.GetCompProperties<CompProperties_Shearable>();
            if (shearableComp != null && shearableComp.woolDef != null) AnimalProducts.Add(shearableComp.woolDef);

            // Egg
            CompProperties_EggLayer eggLayerComp = Animal.race.GetCompProperties<CompProperties_EggLayer>();
            if (eggLayerComp != null)
            {
                if (eggLayerComp.eggUnfertilizedDef != null) AnimalProducts.Add(eggLayerComp.eggUnfertilizedDef);
                if (eggLayerComp.eggFertilizedDef != null) AnimalProducts.Add(eggLayerComp.eggFertilizedDef);
            }

            // Milk
            CompProperties_Milkable milkableComp = Animal.race.GetCompProperties<CompProperties_Milkable>();
            if (milkableComp != null && milkableComp.milkDef != null) AnimalProducts.Add(milkableComp.milkDef);

            if(Prefs.DevMode)
            {
                string s = "";
                foreach (ThingDef t in AnimalProducts) s += " " + t.label + ",";
                s = s.TrimEnd(',');
                //Logger.Log($"Allergenic animal products for {Animal.label} are:{s}");
            }
        }

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyThingsForPassiveExposure();
        }

        public override void OnDamageTaken(DamageInfo dinfo)
        {
            if (dinfo.Instigator is Pawn pawn)
            {
                if (pawn.kindDef == Animal) IncreaseAllergenBuildup(ExposureType.ExtremeEvent, "P42_AllergyCause_DamagedBy".Translate(Animal.label));
            }
        }
        public override void OnInteractedWith(Pawn pawn)
        {
            if (pawn.kindDef == Animal) IncreaseAllergenBuildup(ExposureType.MinorEvent, "P42_AllergyCause_InteractedWith".Translate(Animal.label));
        }

        protected override bool IsAllergenic(ThingDef thing)
        {
            if (thing == Animal.race) return true;
            if (thing == Animal.race.race.corpseDef) return true;
            if (AnimalProducts.Contains(thing)) return true;

            return false;
        }

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is AnimalAllergy animalAllergy && animalAllergy.Animal == Animal);
        }
        public override string TypeLabel => Animal.label;
        private string keepAwayFromText;
        public override string KeepAwayFromText => keepAwayFromText;
        protected override void ExposeExtraData()
        {
            Scribe_Defs.Look(ref Animal, "animal");
        }
    }
}

