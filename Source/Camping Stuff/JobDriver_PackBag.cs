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
	public class JobDriver_PackBag : JobDriver_UseItem // JobDriver
	{
		private const TargetIndex partTarget = TargetIndex.A;
		private const TargetIndex bagTarget = TargetIndex.B;

		protected Thing Part => this.job.GetTarget(partTarget).Thing;
		protected Thing Bag => this.job.GetTarget(bagTarget).Thing;

		protected int Qty => this.GetHaulQty();

		protected int UseDuration
		{
			get
			{
				return (int)typeof(JobDriver_PackBag).BaseType.GetField("useDuration", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.job.GetTarget(partTarget).Thing, this.job, 1, Qty) && this.pawn.Reserve(this.job.GetTarget(bagTarget).Thing, this.job);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnIncapable<JobDriver_UseItem>(PawnCapacityDefOf.Manipulation);

			this.FailOnDestroyedNullOrForbidden(partTarget);

			Toil reservePart = Toils_Reserve.Reserve(partTarget);
			yield return reservePart;
			yield return Toils_Goto.GotoThing(partTarget, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden<Toil>(partTarget).FailOnSomeonePhysicallyInteracting<Toil>(partTarget);
			pawn.CurJob.count = 1;

			yield return Toils_Haul.StartCarryThing(partTarget).FailOnDespawnedNullOrForbidden<Toil>(partTarget);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(reservePart, partTarget, TargetIndex.None, true);
			yield return Toils_Goto.GotoThing(bagTarget, PathEndMode.ClosestTouch);

			Toil toil = Toils_General.Wait(UseDuration, TargetIndex.None);
			toil.WithProgressBarToilDelay(partTarget, false, -0.5f);
			yield return toil;

			yield return new Toil
			{
				initAction = delegate ()
				{
					Bag.TryGetComp<ThingComp_MiniTentBag>().PackPart(Part.SplitOff(Qty));
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}

		private int GetHaulQty()
		{
			TentPart partType = Part.TryGetComp<CompUsable_TentPart>().Props.partType;

			if (partType == TentPart.pole)
			{
				int m = Math.Min(Part.stackCount, Bag.TryGetComp<ThingComp_MiniTentBag>().MaxPoles);
  				return m;
			}

			return 1;
		}
	}
}
