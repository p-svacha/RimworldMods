using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MoreAnomalousContent
{
    public class PhonosphereActivity : CompActivity
    {
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Log.Message("PhonosphereActivity: Initialized with properties");
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Log.Message("PhonosphereActivity: PostSpawnSetup called for " + this.parent.Label);

            // Example: Set initial activity level only if not respawning after load
            if (!respawningAfterLoad)
            {
                this.SetActivity(this.Props.startingRange.RandomInRange);
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            //Log.Message("PhonosphereActivity: CompTick called");
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompInspectStringExtra());
            stringBuilder.Append("\nPhonosphere specific information.");
            return stringBuilder.ToString();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            yield return new Command_Action
            {
                defaultLabel = "DEV: Phonosphere Test Action",
                action = () =>
                {
                    Log.Message("PhonosphereActivity: Test Action executed");
                }
            };
        }
    }
}
