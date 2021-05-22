using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using Verse.AI;

namespace Camping_Stuff
{
	public class JobDriver_PackBag : JobDriver_Fetch // JobDriver
	{
		private const TargetIndex partTarget = TargetIndex.A;
		private const TargetIndex bagTarget = TargetIndex.B;

		protected Thing Part => this.job.GetTarget(partTarget).Thing;
		protected NCS_MiniTent MiniBag => (NCS_MiniTent) this.job.GetTarget(bagTarget).Thing;

		protected override int Qty => this.GetHaulQty();

		protected override IEnumerable<Toil> MakeNewToils()
		{
			foreach (Toil t in base.MakeNewToils())
			{
				yield return t;
			}

			yield return new Toil
			{
				initAction = delegate ()
				{
					MiniBag.Bag.PackPart(Part.SplitOff(Qty));
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}

		private int GetHaulQty()
		{
			TentPart partType = Part.TryGetComp<CompUsable_TentPart>().Props.partType;

			if (partType == TentPart.pole)
			{
				int m = Math.Min(Part.stackCount, MiniBag.Bag.maxPoles);
  				return m;
			}

			return 1;
		}
	}
}
