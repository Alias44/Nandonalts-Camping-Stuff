using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace Camping_Stuff
{
	class JobDriver_UnpackBag : JobDriver
	{
		protected const TargetIndex bagTarget = TargetIndex.A;

		protected Thing Bag => this.job.GetTarget(bagTarget).Thing;
		protected int UseDuration
		{
			get
			{
				return 200;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.job.GetTarget(bagTarget).Thing, this.job);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnIncapable<JobDriver>(PawnCapacityDefOf.Manipulation);

			this.FailOnDestroyedNullOrForbidden(bagTarget);

			yield return Toils_Goto.GotoThing(bagTarget, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden<Toil>(bagTarget).FailOnSomeonePhysicallyInteracting<Toil>(bagTarget);
			pawn.CurJob.count = 1;

			Toil toil = Toils_General.Wait(UseDuration, TargetIndex.None);
			toil.WithProgressBarToilDelay(bagTarget, false, -0.5f);
			yield return toil;
		}
	}

	class JobDriver_UnpackBagCover : JobDriver_UnpackBag
	{
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
					Bag.TryGetComp<ThingComp_MiniTentBag>().EjectCover();
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}

	class JobDriver_UnpackBagFloor : JobDriver_UnpackBag
	{
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
					Bag.TryGetComp<ThingComp_MiniTentBag>().EjectFloor();
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}

	class JobDriver_UnpackBagPole : JobDriver_UnpackBag
	{
		protected const TargetIndex itemTarget = TargetIndex.B;
		protected Thing pole => this.job.GetTarget(itemTarget).Thing;
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
					//find pole in list

					Bag.TryGetComp<ThingComp_MiniTentBag>().EjectPole(pole);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}

	class JobDriver_UnpackBagAllPoles : JobDriver_UnpackBag
	{
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
					Bag.TryGetComp<ThingComp_MiniTentBag>().EjectAllPoles();
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}

	class JobDriver_UnpackBagAll : JobDriver_UnpackBag
	{
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
					Bag.TryGetComp<ThingComp_MiniTentBag>().EjectAll();
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
