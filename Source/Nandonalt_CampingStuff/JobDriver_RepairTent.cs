using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using UnityEngine;


namespace Nandonalt_CampingStuff
{
	public class JobDriver_RepairTent : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.TargetB, this.job, 1, -1, null) && this.pawn.Reserve(this.TargetA, this.job, 1, -1, null); ;
		}

		private const TargetIndex BuildingInd = TargetIndex.A;

		private const TargetIndex ClothInd = TargetIndex.B;

		private const int TicksDuration = 1000;

	 private Thing Cloth
		{
			get
			{
				return base.job.GetTarget(TargetIndex.B).Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
		  
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
			Toil toil = new Toil().FailOnDespawnedOrNull(TargetIndex.A);
			toil.defaultDuration = 15 * TargetB.Thing.stackCount;
			toil.WithEffect(EffecterDefOf.ConstructWood, TargetIndex.A);
			toil.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			yield return toil;
			yield return new Toil
			{
				initAction = delegate
				{
					this.TargetA.Thing.HitPoints = this.TargetA.Thing.HitPoints + this.Cloth.stackCount;
					if(this.TargetA.Thing.HitPoints > this.TargetA.Thing.MaxHitPoints)
					{
						this.TargetA.Thing.HitPoints = this.TargetA.Thing.MaxHitPoints;
					}
					this.Cloth.Destroy(DestroyMode.Vanish);
				}
			};
			yield break;
		}
	}
}
