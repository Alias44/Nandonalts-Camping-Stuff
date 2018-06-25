using System;
using RimWorld;
using Verse;

namespace Nandonalt_CampingStuff
{
    public class CompHeatPusherRefuelable : ThingComp
    {
        private const int HeatPushInterval = 60;

        public CompProperties_HeatPusher Props
        {
            get
            {
                return (CompProperties_HeatPusher)this.props;
            }
        }

        protected virtual bool ShouldPushHeatNow
        {
            get
            {
                CompRefuelable b = this.parent.GetComp<CompRefuelable>();
                
                if ((b != null && b.HasFuel))
                    return true;
                else return false;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.IsHashIntervalTick(60) && this.ShouldPushHeatNow)
            {
                CompProperties_HeatPusher props = this.Props;
                float temperature = this.parent.Position.GetTemperature(this.parent.Map);
                if (temperature < props.heatPushMaxTemperature && temperature > props.heatPushMinTemperature)
                {
                    GenTemperature.PushHeat(this.parent.Position, this.parent.Map, props.heatPerSecond);
                }
            }
        }
    }
}