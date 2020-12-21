using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Camping_Stuff
{
	public abstract class AliasContainer: StatPart
	{
		private readonly Func<float, string> format;

		public AliasContainer(Func<float, string> format = null) : base()
		{
			this.format = format;
		}

		public string ExplanationPart(StatRequest req, StatDef sd)
		{
			if (req.HasThing && req.Thing.def.HasComp(typeof(TentBagComp)))
			{
				ThingWithComps t = (ThingWithComps)req.Thing;

				return t.TryGetComp<TentBagComp>().GetExplanation(sd, format);
			}

			return null;
		}

		public void TransformValue(StatRequest req, ref float val, StatDef sd)
		{
			if (req.HasThing && req.Thing.def.HasComp(typeof(TentBagComp)))
			{
				ThingWithComps t = (ThingWithComps)req.Thing;
				val += t.TryGetComp<TentBagComp>().GetValue(sd);
			}
		}
	}
}
