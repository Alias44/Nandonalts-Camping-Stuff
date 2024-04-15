using System.Collections.Generic;

using Verse;
using Verse.AI;

namespace Camping_Stuff
{
	public class JobDriver_RepairPart : JobDriver_Fetch
	{
		protected Thing Material => this.job.GetTarget(fetch).Thing;
		protected NCS_MiniTent MiniBag => (this.job.GetTarget(target).Thing is NCS_MiniTent mini) ? mini : null;

		protected virtual Thing Part => this.job.GetTarget(target).Thing;

		protected override int DesiredQty => Part.TryGetComp<CompTentPartDamage>().RepairCost;

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
					Material.Destroy();
					Part.TryGetComp<CompTentPartDamage>().Repair();
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
