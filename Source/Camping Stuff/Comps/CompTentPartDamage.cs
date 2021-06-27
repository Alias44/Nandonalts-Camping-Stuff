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
	public class CompTentPartDamage : ThingComp // put damage int here and override protected in child cover?
	{
		public CompProperties_TentPartDamage Props => (CompProperties_TentPartDamage)this.props;

		protected virtual double DamageCost => 1.0 - ((double) this.parent.HitPoints / this.parent.MaxHitPoints);
		public virtual int RepairCost => (int)Math.Ceiling(this.parent.def.costStuffCount * DamageCost);

		public virtual bool CanRepair => RepairCost > 0;

		protected virtual ThingDef RepairStuff => this.parent.Stuff;

		public FloatMenuOption RepairMenuOption(Pawn selPawn, JobDef jobDef, LocalTargetInfo destination)
		{
			if (CanRepair)
			{
				Thing material = GenClosest.ClosestThingReachable(selPawn.Position, selPawn.Map,
					ThingRequest.ForDef(RepairStuff), Verse.AI.PathEndMode.ClosestTouch,
					TraverseParms.For(selPawn, selPawn.NormalMaxDanger())); // validator?

				if (material != null)
				{
					return new FloatMenuOption($"Repair {this.parent.def.label} ({RepairCost} {RepairStuff.label} needed)", delegate
					{
						selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, material, destination));
					});
				}

				return new FloatMenuOption($"Unable to repair {this.parent.LabelNoCount}, need {RepairCost} {RepairStuff.label}", null);
			}

			return new FloatMenuOption($"{this.parent.LabelNoCount} does not need repairing", null);
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			if (CanRepair)
			{
				yield return RepairMenuOption(selPawn, TentDefOf.NCS_RepairPart, this.parent);
			}
		}

		public virtual void Repair()
		{
			this.parent.HitPoints = this.parent.MaxHitPoints;
		}
	}

	public class CompProperties_TentPartDamage : CompProperties //(Def)
	{
		public CompProperties_TentPartDamage()
		{
			this.compClass = typeof(CompTentPartDamage);
		}
	}
}