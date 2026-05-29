using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public class Gene_AllergyImmunity : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn?.health == null) return;

            // Clear every allergy, including gene-forced ones. Forcing genes won't re-add while this gene is active (EnsureAllergy + ReassertGeneAllergies both bail).
            foreach (Hediff_Allergy h in Utils.GetPawnAllergies(pawn).ToList())
                pawn.health.RemoveHediff(h);
        }
    }
}
