﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;


namespace Nandonalt_CampingStuff
{
	public class JobDriver_PackTent : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.TargetA, this.job, 1, -1, null); ;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{

			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil toil = new Toil();
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = 110;
			toil.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);

			yield return toil;
			yield return new Toil
			{
				initAction = delegate
				{
					Pawn actor = this.pawn;
					CompPackTent compUsable = actor.CurJob.targetA.Thing.TryGetComp<CompPackTent>();
					compUsable.UsedBy(actor);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
			yield break;
		}
	}
}
