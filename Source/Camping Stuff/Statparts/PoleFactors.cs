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
	}

	public class PoleBeauty : PoleFactors
	{
		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && req.Thing is ThingWithComps twc && twc.TryGetComp<TentSpawnedComp>() != null)
			{
				NCS_Tent tent = twc.TryGetComp<TentSpawnedComp>().tent;

				string str = $"Poles x{AvgPoleFactor(tent, StatDefOf.Beauty)}\nPoles +{DistributedPoleOffset(tent, StatDefOf.Beauty)}";

				return str;
			}

			return null;
		}

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing && req.Thing is ThingWithComps twc && twc.TryGetComp<TentSpawnedComp>() != null)
			{
				NCS_Tent tent = twc.TryGetComp<TentSpawnedComp>().tent;

				val = (val * AvgPoleFactor(tent, StatDefOf.Beauty)) + DistributedPoleOffset(tent, StatDefOf.Beauty);
			}
		}
	}

	public class PoleMaxHp : PoleFactors
	{
		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && req.Thing is ThingWithComps twc && twc.TryGetComp<TentSpawnedComp>() != null)
			{
				NCS_Tent tent = twc.TryGetComp<TentSpawnedComp>().tent;

				string str = $"Poles x{AvgPoleFactor(tent, StatDefOf.MaxHitPoints)}\nPoles +{DistributedPoleOffset(tent, StatDefOf.MaxHitPoints)}";

				return str;
			}

			return null;
		}

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing && req.Thing is ThingWithComps twc && twc.TryGetComp<TentSpawnedComp>() != null)
			{
				NCS_Tent tent = twc.TryGetComp<TentSpawnedComp>().tent;

				val = (val * AvgPoleFactor(tent, StatDefOf.MaxHitPoints)) + DistributedPoleOffset(tent, StatDefOf.MaxHitPoints);
			}
		}
	}
}
