using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class StoneAllergy : Allergy
    {
        public ThingDef StoneType;

        private List<string> AllergenicTerrains;
        private List<ThingDef> AllergicMountainThings; // mountains made out of this stone, both rough and smoothed

        protected override void OnCreate()
        {
            StoneType = AllergyGenerator.GetRandomStone();
        }
        protected override void OnInitOrLoad()
        {
            string baseStoneName = StoneType.defName.Replace("Blocks", ""); // Extract "Sandstone" from "BlocksSandstone"

            // Allergenic terrain
            AllergenicTerrains = new List<string>();

            AllergenicTerrains.Add($"{baseStoneName}_Rough");
            AllergenicTerrains.Add($"{baseStoneName}_RoughHewn");
            AllergenicTerrains.Add($"{baseStoneName}_Smooth");

            // Allergic things
            AllergicMountainThings = new List<ThingDef>();

            ThingDef mountainRock = DefDatabase<ThingDef>.GetNamedSilentFail($"{baseStoneName}");
            if(mountainRock != null) AllergicMountainThings.Add(mountainRock);
            ThingDef smoothedMountainRock = DefDatabase<ThingDef>.GetNamedSilentFail($"Smoothed{baseStoneName}");
            if (smoothedMountainRock != null) AllergicMountainThings.Add(smoothedMountainRock);

            // Labels
            typeLabel = baseStoneName.UncapitalizeFirst();
        }

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyThingsForPassiveExposure(checkApparel: false, checkButcherProducts: true, checkMineableThings: true); // butcher ingredients are for stone chunks
            CheckNearbyFloorsForPassiveExposure();
        }
        public override bool IsTerrainAllergenic(TerrainDef terrain)
        {
            if (AllergenicTerrains.Contains(terrain.defName)) return true;
            return base.IsTerrainAllergenic(terrain);
        }

        protected override bool IsDirectlyAllergenic(ThingDef thingDef) => thingDef == StoneType || AllergicMountainThings.Contains(thingDef);

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is StoneAllergy otherStoneAllergy && otherStoneAllergy.StoneType == StoneType);
        }
        private string typeLabel;
        public override string TypeLabel => typeLabel;
        public override string KeepAwayFromText => typeLabel;
        protected override void ExposeExtraData()
        {
            Scribe_Defs.Look(ref StoneType, "stoneType");
        }
    }
}
