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
		private const TargetIndex partTarget = TargetIndex.A;
		private const TargetIndex materialTarget = TargetIndex.B;

		protected override int Qty => this.job.GetTarget(partTarget).Thing.TryGetComp<CompTentPartDamage>().RepairCost;

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
					Log.Message("fix a thing");
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
