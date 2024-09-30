using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace P42_Allergies
{
    public abstract class Allergy
    {
        private Dictionary<AllergySeverity, float> SEVERITY_MULTIPLIER = new Dictionary<AllergySeverity, float>()
        {
            { AllergySeverity.Mild, 0.5f },
            { AllergySeverity.Moderate, 1f },
            { AllergySeverity.Severe, 2f },
        };

        protected Hediff_Allergy AllergyHediff;
        protected Pawn Pawn => AllergyHediff.pawn;
        public abstract string TypeLabel { get; }
        public abstract string TypeLabelPlural {get; }
        public string FullAllergyName => TypeLabel + " allergy";
        public string FullAllergyNameCap => TypeLabel.CapitalizeFirst() + " allergy";
        public AllergySeverity Severity;

        public Allergy(Hediff_Allergy hediff, AllergySeverity severity)
        {
            AllergyHediff = hediff;
            Severity = severity;
        }

        public virtual void Tick() { }

        /// <summary>
        /// Executes the PIE (Passive Item Exposure) check for a certain item and increases allergen buildup accordingly.
        /// </summary>
        protected void DoPieCheck(Func<Thing, bool> ItemIdentifier)
        {
            // Get the current map and position of the pawn.
            Map map = Pawn.Map;
            IntVec3 pawnPosition = Pawn.Position;

            if (map == null) return;

            // Define a search radius of 5 tiles around the pawn.
            int searchRadius = 5;

            // Find all things (items) in the defined radius.
            foreach (Thing item in GenRadial.RadialDistinctThingsAround(pawnPosition, map, searchRadius, useCenter: true))
            {
                // Check if the item contains allergen
                if (ItemIdentifier(item))
                {
                    Log.Message($"Pawn {Pawn.Name} is within {searchRadius} tiles of allergenic item: {item.Label}");
                    IncreaseAllergenBuildup(0.01f, "being nearby " + item.Label);
                    break;
                }
            }
        }

        protected void IncreaseAllergenBuildup(float baseAmount, string reasonForIncrease)
        {
            if (Pawn == null || Pawn.Dead) return;

            // Calculate amount
            float amount = baseAmount;
            amount *= SEVERITY_MULTIPLIER[Severity]; // Scale by allergy severity
            amount *= Pawn.GetStatValue(StatDef.Named("P42_AllergicSensitivity")); // Scale by allergic sensitivity stat

            // Try to get the allergen buildup hediff from the pawn's health
            Hediff allergenBuildup = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("P42_AllergenBuildup"));

            // If the hediff exists, increase its severity
            if (allergenBuildup != null) allergenBuildup.Severity += amount;
            else
            {
                // If the hediff is not present, add it to the pawn with the initial severity
                Hediff newBuildup = HediffMaker.MakeHediff(HediffDef.Named("P42_AllergenBuildup"), Pawn);
                newBuildup.Severity = amount;
                Pawn.health.AddHediff(newBuildup);

                // If the allergy has not been visible so far make it visible
                if(!AllergyHediff.Visible)
                {
                    AllergyHediff.Severity = 0.2f; // makes it visible

                    // Create a letter to notify the player of the newly discovered allergy
                    if (Pawn.IsColonistPlayerControlled)
                    {
                        string letterLabel = "New Allergy Discovered";
                        string letterTextStart = "";

                        if (Severity == AllergySeverity.Mild)
                            letterTextStart = $"When {reasonForIncrease}, {Pawn.NameShortColored} felt a mild irritation on {Pawn.Possessive()} skin.";
                        if (Severity == AllergySeverity.Moderate)
                            letterTextStart = $"When {reasonForIncrease}, {Pawn.NameShortColored} had to sneeze repeatedly and {Pawn.Possessive()} skin and eyes started to itch.";
                        if (Severity == AllergySeverity.Severe)
                            letterTextStart = $"When {reasonForIncrease}, {Pawn.NameShortColored} felt {Pawn.Possessive()} throat tightening, had to sneeze violently and {Pawn.Possessive()} eyes started to burn.";

                        string letterTextEnd = $"\n\n{Pawn.NameShortColored} discovered that {Pawn.ProSubj()} has a {Severity.ToString().ToLower()} {FullAllergyName}." +
                                            $"\n\nKeep {Pawn.ProObj()} away from {TypeLabelPlural} or try to treat the allergy to avoid health complications.";

                        string letterText = letterTextStart + letterTextEnd;
                        Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NegativeEvent, Pawn);
                    }
                }
            }
        }
    }
}
