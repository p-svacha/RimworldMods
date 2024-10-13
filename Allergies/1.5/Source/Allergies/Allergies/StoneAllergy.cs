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

        protected override void OnInitOrLoad()
        {
            // Allergenic things
            AllergenicTerrains = new List<string>();

            string baseStoneName = StoneType.defName.Replace("Blocks", ""); // Extract "Sandstone" from "BlocksSandstone"

            AllergenicTerrains.Add($"{baseStoneName}_Rough");
            AllergenicTerrains.Add($"{baseStoneName}_RoughHewn");
            AllergenicTerrains.Add($"{baseStoneName}_Smooth");

            Logger.Log(TerrainDefOf.Sandstone_Smooth.defName);

            // Labels
            typeLabel = baseStoneName.UncapitalizeFirst();
        }

        protected override void DoPassiveExposureChecks()
        {
            CheckNearbyThingsForPassiveExposure(checkApparel: false, checkButcherProducts: true); // butcher ingredients for stone chunks
            CheckNearbyFloorsForPassiveExposure();
        }
        protected override ExposureType GetDirectExposureOfFloor(TerrainDef def)
        {
            if (AllergenicTerrains.Contains(def.defName)) return ExposureType.MinorPassive;
            return ExposureType.None;
        }

        protected override bool IsAllergenic(ThingDef thingDef) => thingDef == StoneType;

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
