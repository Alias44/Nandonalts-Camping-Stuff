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

		protected override int DesiredQty
		{
			get
			{
				TentPart partType = Part.TryGetComp<CompUsable_TentPart>().Props.partType;

				if (partType == TentPart.pole)
				{
					return MiniBag.Bag.maxPoles;
				}

				return 1;
			}
		}

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
					MiniBag.Bag.PackPart(Part.SplitOff(AvailQty));
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
