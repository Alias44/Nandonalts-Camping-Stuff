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
			int parts = tent.Cover.TryGetComp<TentCoverComp>().Props.layoutParts;
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

				string str = $"Poles x{AvgPoleFactor(tent, sd)}\nPoles +{DistributedPoleOffset(tent, sd)}";

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
