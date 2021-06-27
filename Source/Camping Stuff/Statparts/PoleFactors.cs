using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Camping_Stuff
{
	class PoleFactors : StatPart
	{
		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && req.Thing is ThingWithComps twc && twc.TryGetComp<TentSpawnedComp>() != null)
			{
				return "🚀";
			}

			return null;
		}

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing && req.Thing is ThingWithComps twc && twc.TryGetComp<TentSpawnedComp>() != null)
			{
 				NCS_Tent tent = twc.TryGetComp<TentSpawnedComp>().tent;
 				val += tent.Poles.Average(p => p.GetStatValue(StatDefOf.Beauty));
			}
		}
	}
}
