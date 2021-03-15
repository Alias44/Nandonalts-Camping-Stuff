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
			if (req.HasThing && req.Thing is NCS_Tent t)
			{
				return t.GetExplanation(sd, format);
			}

			return null;
		}

		public void TransformValue(StatRequest req, ref float val, StatDef sd)
		{
			if (req.HasThing && req.Thing is NCS_Tent t)
			{
				val += t.GetValue(sd);
			}
		}
	}
}
