using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;


namespace Nandonalt_CampingStuff
{
    public class JobDriver_DeployTent : JobDriver
    {
               protected override IEnumerable<Toil> MakeNewToils()
        {
            this.pawn.jobs.curJob.count = 1;
            this.FailOnDestroyedOrNull(TargetIndex.A);
            if (!base.TargetThingA.IsForbidden(this.pawn))
            {
                this.FailOnForbidden(TargetIndex.A);
            }
            yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
            Toil toil = Toils_Reserve.Reserve(TargetIndex.A, 1);
            yield return toil;
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false);
            if (base.CurJob.haulOpportunisticDuplicates)
            {
                yield return Toils_Haul.CheckForGetOpportunityDuplicate(toil, TargetIndex.A, TargetIndex.B, false, null);
            }
            Toil toil3 = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
            yield return toil3;
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, toil3, true);

           

            Toil toil2 = new Toil();
            toil2.defaultCompleteMode = ToilCompleteMode.Delay;
            toil2.defaultDuration = 100;
            toil2.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            toil2.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return toil2;
            yield return new Toil
            {
                initAction = delegate
                {
                    Pawn actor = this.pawn;
                    CompUsable compUsable = actor.CurJob.targetA.Thing.TryGetComp<CompUsable>();
                    compUsable.UsedBy(actor);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield break;
        }
    }
}
