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
        public override string TypeLabel => "P42_AllergyType_Dust".Translate();
        protected override void ExposeExtraData() { }
    }
}
