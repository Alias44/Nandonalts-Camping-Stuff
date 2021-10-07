using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Camping_Stuff
{
	class StatPart_MktValue : AliasContainer
	{
		public StatPart_MktValue() : base(delegate (float f)
		{
			return f.ToStringMoney((string)null);
		}) { }

		public override string ExplanationPart(StatRequest req)
		{
			return this.ExplanationPart(req, StatDefOf.MarketValue);
		}

		public override void TransformValue(StatRequest req, ref float val)
		{
			this.TransformValue(req, ref val, StatDefOf.MarketValue);
		}
	}

	class StatPart_Mass : AliasContainer
	{
		public StatPart_Mass() : base( GenText.ToStringMass)
		{ }

		public override string ExplanationPart(StatRequest req)
		{
			return this.ExplanationPart(req, StatDefOf.Mass);
		}

		public override void TransformValue(StatRequest req, ref float val)
		{
			this.TransformValue(req, ref val, StatDefOf.Mass);
		}
	}

	class StatPart_Favor : AliasContainer
	{
		public StatPart_Favor() : base()
		{ }

		public override string ExplanationPart(StatRequest req)
		{
			return this.ExplanationPart(req, StatDefOf.RoyalFavorValue);
		}

		public override void TransformValue(StatRequest req, ref float val)
		{
			this.TransformValue(req, ref val, StatDefOf.RoyalFavorValue);
		}
	}
}
