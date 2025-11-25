using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class DustAllergy : Allergy
    {
        protected override void OnInitOrLoad()
        {
            typeLabel = "P42_AllergyType_Dust".Translate();
            keepAwayFromText = "P42_LetterTextEnd_AllergyDiscovered_KeepAwayFrom_Dust".Translate();
        }

        protected override bool IsDirectlyAllergenic(ThingDef thingDef) => false;

        protected override void DoPassiveExposureChecks()
        {
            ExposureType dustExosure = GetDustExposure(Pawn, out string cleanlinessLabel);
            if (dustExosure != ExposureType.None) IncreaseAllergenBuildup(dustExosure, "P42_AllergyCause_Dust".Translate(cleanlinessLabel));
        }

        private ExposureType GetDustExposure(Pawn pawn, out string cleanlinessLabel)
        {
            cleanlinessLabel = "";

            if (pawn == null || pawn.Map == null) return ExposureType.None;

            // Get the room the pawn is currently in
            Room room = pawn.Position.GetRoom(pawn.Map);
            if (room == null) return ExposureType.None;

            // Get the cleanliness stat of the room
            float cleanlinessValue = room.GetStat(RoomStatDefOf.Cleanliness);
            RoomStatScoreStage cleanlinessStage = RoomStatDefOf.Cleanliness.GetScoreStage(cleanlinessValue);
            if (cleanlinessStage == null) return ExposureType.None;
            cleanlinessLabel = cleanlinessStage.label;
            
            if (cleanlinessStage.untranslatedLabel == "very dirty") return ExposureType.ExtremePassive;
            if (cleanlinessStage.untranslatedLabel == "dirty") return ExposureType.StrongPassive;
            if (cleanlinessStage.untranslatedLabel == "slightly dirty") return ExposureType.MinorPassive;

            return ExposureType.None;
        }

        public override bool IsDuplicateOf(Allergy otherAllergy)
        {
            return (otherAllergy is DustAllergy);
        }
        private string typeLabel;
        public override string TypeLabel => typeLabel;
        private string keepAwayFromText;
        public override string KeepAwayFromText => keepAwayFromText;

        protected override void ExposeExtraData() { }
    }
}
