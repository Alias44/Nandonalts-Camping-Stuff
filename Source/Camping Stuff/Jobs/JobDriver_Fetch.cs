using System;
using System.Collections.Generic;

using RimWorld;
using Verse.AI;

namespace Camping_Stuff
{
	public class JobDriver_Fetch : JobDriver // JobDriver
	{
		protected const TargetIndex fetch = TargetIndex.A;
		protected const TargetIndex target = TargetIndex.B;

		protected virtual int DesiredQty => 1;
		protected virtual int AvailQty => Math.Min(DesiredQty, this.job.GetTarget(fetch).Thing.stackCount);

		protected int UseDuration => 1000;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.job.GetTarget(fetch).Thing, this.job, 1, AvailQty) && this.pawn.Reserve(this.job.GetTarget(target).Thing, this.job);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn<JobDriver>(() => AvailQty == 0);
			this.FailOnIncapable<JobDriver>(PawnCapacityDefOf.Manipulation);

			this.FailOnDestroyedNullOrForbidden(fetch);

			Toil reservePart = Toils_Reserve.Reserve(fetch);
			yield return reservePart;
			yield return Toils_Goto.GotoThing(fetch, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden<Toil>(fetch).FailOnSomeonePhysicallyInteracting<Toil>(fetch);
			pawn.CurJob.count = 1;

			yield return Toils_Haul.StartCarryThing(fetch).FailOnDespawnedNullOrForbidden<Toil>(fetch);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(reservePart, fetch, TargetIndex.None, true, t =>
			{
				return pawn.carryTracker.CarriedThing.stackCount < DesiredQty;
			});
			yield return Toils_Goto.GotoThing(target, PathEndMode.ClosestTouch);

			Toil toil = Toils_General.Wait(UseDuration, TargetIndex.None);
			toil.WithProgressBarToilDelay(fetch, false, -0.5f);
			yield return toil;
		}
	}
}
