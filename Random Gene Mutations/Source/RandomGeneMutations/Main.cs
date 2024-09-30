using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RandomGeneMutations
{
    class RandomGeneMutations : Mod
    {
        public static RandomGeneMutationsSettings settings;

        public RandomGeneMutations(ModContentPack content) : base(content)
        {
            ParseHelper.Parsers<GeneChancesList>.Register(GeneChancesList.FromString);
            settings = GetSettings<RandomGeneMutationsSettings>();
        }

        public override string SettingsCategory()
        {
            return "Random Gene Mutations";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoWindowContents(inRect);
        }
    }
}
