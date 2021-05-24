using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace Camping_Stuff
{
	public class JobDriver_RepairTent : JobDriver_Fetch
	{
		private const TargetIndex materialTarget = TargetIndex.A;
		private const TargetIndex partTarget = TargetIndex.B;

		protected Thing Material => this.job.GetTarget(materialTarget).Thing;
		protected Thing Part => this.job.GetTarget(partTarget).Thing;

		protected override int DesiredQty => this.job.GetTarget(partTarget).Thing.TryGetComp<CompTentPartDamage>().RepairCost;

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
					Part.TryGetComp<CompTentPartDamage>().ClearCells();
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
