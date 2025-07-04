using System;
using System.Collections.Generic;

using RimWorld;
using Verse;

namespace Camping_Stuff;

/// <summary>
/// Comp class to handle damage traking repair for tent parts
/// </summary>
public class CompTentPartDamage : ThingComp
{
	public CompProperties_TentPartDamage Props => (CompProperties_TentPartDamage)this.props;

	protected virtual double DamageCost => 1.0 - ((double)this.parent.HitPoints / this.parent.MaxHitPoints);
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
				return new FloatMenuOption("RepairTentPart".Translate(this.parent.def.label, RepairCost, RepairStuff.label), delegate
				{
					selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, material, destination));
				});
			}

			return new FloatMenuOption("CannotRepairTentPart".Translate(this.parent.LabelNoCount, RepairCost, RepairStuff.label), null);
		}

		return new FloatMenuOption("NoRepairNeeded".Translate(this.parent.LabelNoCount), null);
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