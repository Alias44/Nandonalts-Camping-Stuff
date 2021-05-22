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
	public class JobDriver_Fetch : JobDriver_UseItem // JobDriver
	{
		protected const TargetIndex fetch = TargetIndex.A;
		protected const TargetIndex target = TargetIndex.B;

		protected virtual int Qty => 1;

		protected int UseDuration
		{
			get
			{
				try
				{
					return (int)typeof(JobDriver_PackBag).BaseType.GetField("useDuration", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
				}
				catch (NullReferenceException)
				{
					return 1000;
				}
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.job.GetTarget(fetch).Thing, this.job, 1, Qty) && this.pawn.Reserve(this.job.GetTarget(target).Thing, this.job);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnIncapable<JobDriver_UseItem>(PawnCapacityDefOf.Manipulation);

			this.FailOnDestroyedNullOrForbidden(fetch);

			Toil reservePart = Toils_Reserve.Reserve(fetch);
			yield return reservePart;
			yield return Toils_Goto.GotoThing(fetch, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden<Toil>(fetch).FailOnSomeonePhysicallyInteracting<Toil>(fetch);
			pawn.CurJob.count = 1;

			yield return Toils_Haul.StartCarryThing(fetch).FailOnDespawnedNullOrForbidden<Toil>(fetch);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(reservePart, fetch, TargetIndex.None, true);
			yield return Toils_Goto.GotoThing(target, PathEndMode.ClosestTouch);

			Toil toil = Toils_General.Wait(UseDuration, TargetIndex.None);
			toil.WithProgressBarToilDelay(fetch, false, -0.5f);
			yield return toil;
		}
	}
}
