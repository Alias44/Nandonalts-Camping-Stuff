using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

namespace Camping_Stuff;

public class JobDriver_PackBag : JobDriver_Fetch // JobDriver
{
	protected Thing Part => this.job.GetTarget(fetch).Thing;
	protected NCS_MiniTent MiniBag => (NCS_MiniTent)this.job.GetTarget(target).Thing;

	protected override int DesiredQty
	{
		get
		{
			TentPart partType = Part.TryGetComp<CompUsable_TentPart>().Props.partType;

			if (partType == TentPart.pole)
			{
				return MiniBag.Bag.maxPoles - MiniBag.Bag.PoleKindCount(Part);
			}

			return 1;
		}
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (AvailQty == 0)
		{
			Messages.Message(MiniBag.Bag.PoleFullMsg, MessageTypeDefOf.NeutralEvent);
		}

		foreach (Toil t in base.MakeNewToils())
		{
			yield return t;
		}

		yield return new Toil
		{
			initAction = delegate ()
			{
				MiniBag.Bag.PackPart(Part.SplitOff(AvailQty));
			},
			defaultCompleteMode = ToilCompleteMode.Instant
		};
	}
}
