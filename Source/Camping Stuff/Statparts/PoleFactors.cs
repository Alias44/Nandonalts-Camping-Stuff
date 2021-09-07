using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Camping_Stuff
{
	public abstract class PoleFactors : StatPart
	{
		protected float AvgPoleFactor(NCS_Tent tent, StatDef sd)
		{
			return tent.Poles.Sum(p =>
				p.Stuff.stuffProps.statFactors.GetStatFactorFromList(sd) * p.stackCount) / tent.PoleCount;
		}

		protected float DistributedPoleOffset(NCS_Tent tent, StatDef sd)
		{
			int parts = tent.Cover.TryGetComp<TentCoverComp>().Props.tentSpec.layoutParts;
			float totalOffset = tent.Poles.Sum(p =>
				p.Stuff.stuffProps.statOffsets.GetStatOffsetFromList(sd) * p.stackCount);

			float distributedOffset = totalOffset / parts;

			return distributedOffset;
		}

		public string ExplanationPart(StatRequest req, StatDef sd)
		{
			try
			{
				NCS_Tent tent = req.Thing.TryGetComp<TentSpawnedComp>().tent;

				float avg = AvgPoleFactor(tent, sd);
				float offset = DistributedPoleOffset(tent, sd);
				float coverMultiplier = (float) 1.0 / tent.Cover.TryGetComp<TentCoverComp>().Props.tentSpec.layoutParts;

				string factorDesc = "";
				string offsetDesc = "";

				string str = "";

				foreach (var pole in tent.Poles)
				{
					float statFactorFromList = pole.Stuff.stuffProps.statFactors.GetStatFactorFromList(sd);
					float weight = (float) pole.stackCount / tent.PoleCount;

					factorDesc +=
						$"{Util.indent}{"StatsReport_Material".Translate()} ({pole.Stuff.LabelCap}): {statFactorFromList.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor)} ({"HealthFactorPercentImpact".Translate(weight.ToStringPercentEmptyZero())})\n";
					
					float statOffsetFromList = pole.Stuff.stuffProps.statOffsets.GetStatOffsetFromList(sd);
					offsetDesc +=
						$"{Util.indent}{"StatsReport_Material".Translate()} ({pole.Stuff.LabelCap}): {statOffsetFromList.ToStringByStyle(sd.toStringStyle, ToStringNumberSense.Offset)} ({"HealthOffsetScale".Translate(pole.stackCount + "x")})\n";
				}

				if ((double)Math.Abs(avg - 1f) > 1.40129846432482E-45) // Avg != 1.0
				{
					factorDesc +=
						$"{Util.indent}{"StatsReport_FinalValue".Translate()}: {avg.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor)}\n";

					str += $"{"ContainsPoles".Translate()}\n{factorDesc}\n";
				}

				if ((double) offset != 0.0)
				{
					offsetDesc +=
						$"{Util.indent}{"StatsReport_TentSize".Translate(tent.TentSize)}: {coverMultiplier.ToStringByStyle(ToStringStyle.PercentTwo, ToStringNumberSense.Factor)}\n" +
						$"{Util.indent}{"StatsReport_FinalValue".Translate()}: {offset.ToStringByStyle(sd.toStringStyle, ToStringNumberSense.Offset)}\n";

					str += $"{"ContainsPoles".Translate()}\n{offsetDesc}\n";
				}

				return str;
			}
			catch (Exception) { } // do nothing

			return null;
		}

		public void TransformValue(StatRequest req, ref float val, StatDef sd)
		{
			try
			{
				NCS_Tent tent = req.Thing.TryGetComp<TentSpawnedComp>().tent;

				val = (val * AvgPoleFactor(tent, sd)) + DistributedPoleOffset(tent, sd);
			}
			catch (Exception) { } // do nothing
		}
	}

	public class PoleBeauty : PoleFactors
	{
		public override string ExplanationPart(StatRequest req)
		{
			return ExplanationPart(req, StatDefOf.Beauty);
		}

		public override void TransformValue(StatRequest req, ref float val)
		{
			this.TransformValue(req, ref val, StatDefOf.Beauty);
		}
	}

	public class PoleMaxHp : PoleFactors
	{
		public override string ExplanationPart(StatRequest req)
		{
			return ExplanationPart(req, StatDefOf.MaxHitPoints);
		}

		public override void TransformValue(StatRequest req, ref float val)
		{
			this.TransformValue(req, ref val, StatDefOf.MaxHitPoints);
		}
	}
}
