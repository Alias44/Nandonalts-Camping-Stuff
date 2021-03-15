using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Camping_Stuff
{
	public class NCS_MiniTent : MinifiedThing
	{
		public NCS_Tent Bag
		{
			get => (NCS_Tent)InnerThing;
		}

		public override string GetInspectString()
		{
			string readyStr;
			if (Bag.Ready)
			{
				readyStr = "Ready";
			}
			else
			{
				readyStr = "Not ready";
			}

			return readyStr + " to deploy\n" + base.GetInspectString();
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
		{
			JobDef jd = DefDatabase<JobDef>.GetNamed("NCS_UnpackBag");
			Thing target = this.SpawnedParentOrMe;

			if (selPawn.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly))
			{
				if (Bag.Cover != null)
				{
					yield return new FloatMenuOption("Unpack " + Bag.Cover.LabelCap, delegate
					{
						jd.driverClass = typeof(JobDriver_UnpackBagCover);
						Job j = JobMaker.MakeJob(jd, target);

						selPawn.jobs.TryTakeOrderedJob(j);
					});
				}

				if (Bag.Poles != null && Bag.Poles.Count > 0)
				{
					foreach (Thing pole in Bag.Poles)
					{
						yield return new FloatMenuOption("Unpack " + pole.LabelCap + (pole.stackCount == 1 ? " x1" : ""), delegate
						{
							jd.driverClass = typeof(JobDriver_UnpackBagPole);
							Job j = JobMaker.MakeJob(jd, target, pole);

							selPawn.jobs.TryTakeOrderedJob(j);
						});
					}

					yield return new FloatMenuOption("Unpack all poles (x" + Bag.PoleCount + ")", delegate
					{
						jd.driverClass = typeof(JobDriver_UnpackBagAllPoles);
						Job j = JobMaker.MakeJob(jd, target);

						selPawn.jobs.TryTakeOrderedJob(j);
					});
				}

				if (Bag.Floor != null)
				{
					yield return new FloatMenuOption("Unpack " + Bag.Floor.LabelCap, delegate
					{
						jd.driverClass = typeof(JobDriver_UnpackBagFloor);
						Job j = JobMaker.MakeJob(jd, target);

						selPawn.jobs.TryTakeOrderedJob(j);
					});
				}

				if (Bag.Cover != null || (Bag.Poles != null && Bag.Poles.Count > 0) || Bag.Floor != null)
				{
					yield return new FloatMenuOption("Unpack all parts", delegate
					{
						jd.driverClass = typeof(JobDriver_UnpackBagAll);
						Job j = JobMaker.MakeJob(jd, target);

						selPawn.jobs.TryTakeOrderedJob(j);
					});
				}
			}
		}
	}
}
