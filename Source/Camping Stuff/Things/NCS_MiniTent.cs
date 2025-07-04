using System.Collections.Generic;

using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Camping_Stuff;

public class NCS_MiniTent : MinifiedThing, IThingHolder
{
	public NCS_Tent Bag
	{
		get => (NCS_Tent)InnerThing;
		set
		{
			// Harmony hack to prevent trying to add things to a null innerContainer
			if (this.GetDirectlyHeldThings() == null)
			{
				AccessTools.Field(typeof(NCS_MiniTent), "innerContainer").SetValue(this, new ThingOwner<Thing>((IThingHolder)this, true));
			}

			base.InnerThing = value;
		}
	}

	public override string GetInspectString()
	{
		string readyStr;
		if (Bag.Ready)
		{
			readyStr = "DeployReady".Translate();
		}
		else
		{
			readyStr = "DeployNotReady".Translate();
		}

		return readyStr + "\n" + base.GetInspectString();
	}

	// Overriding graphic and draw at allows me to draw a graphic instead of the boxed inner thing (if one is specified)
	public override Graphic Graphic =>
		this.def.graphicData != null
			? this.def.graphicData.GraphicColoredFor(this.InnerThing)
			: base.Graphic;

#if RELEASE_1_4 || RELEASE_1_3 || RELEASE_1_2 || RELEASE_1_1
	public override void DrawAt(Vector3 drawLoc, bool flip = false)
#else
	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
#endif
	{
		this.Graphic.Draw(drawLoc, this.Graphic is Graphic_Single ? Rot4.North : Rot4.South, (Thing)this);
	}

#if !(RELEASE_1_2 || RELEASE_1_1)
	public override void Print(SectionLayer layer)
	{
		this.Graphic.Print(layer, this, 0.0f);
	}
#endif

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		if (mode == DestroyMode.KillFinalize)
		{
			this.Bag.EjectAll();
		}

		base.DeSpawn(mode);
	}

#if DEBUG
	/// <summary>
	/// Auto-geneare tent parts when mod is compiled for debug mode
	/// </summary>
	public override void Notify_DebugSpawned()
	{
		base.Notify_DebugSpawned();

		Bag.SpawnParts();
	}
#endif

	public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
	{
		//JobDef jd = DefDatabase<JobDef>.GetNamed("NCS_UnpackBag");
		JobDef jd = TentDefOf.NCS_UnpackBag;
		Thing target = this.SpawnedParentOrMe;

		if (selPawn.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly))
		{
			if (Bag.Cover != null)
			{
				yield return new FloatMenuOption("UnpackOne".Translate(Bag.Cover.LabelCapHpFrac()), delegate
				{
					jd.driverClass = typeof(JobDriver_UnpackBagCover);
					Job j = JobMaker.MakeJob(jd, target);

					selPawn.jobs.TryTakeOrderedJob(j);
				});

				CompTentPartDamage damage = Bag.Cover.TryGetComp<CompTentPartDamage>();

				if (damage != null && damage.CanRepair)
				{
					yield return damage.RepairMenuOption(selPawn, TentDefOf.NCS_RepairCoverInBag, this);
				}
			}

			if (Bag.Poles != null && Bag.Poles.Count > 0)
			{
				foreach (Thing pole in Bag.Poles)
				{
					yield return new FloatMenuOption(
						"UnpackOne".Translate(pole.LabelCap + (pole.stackCount == 1 ? " x1" : "")), delegate
						{
							jd.driverClass = typeof(JobDriver_UnpackBagPole);
							Job j = JobMaker.MakeJob(jd, target, pole);

							selPawn.jobs.TryTakeOrderedJob(j);
						});
				}

				yield return new FloatMenuOption("UnpackAllPoles".Translate(Bag.PoleCount), delegate
				{
					jd.driverClass = typeof(JobDriver_UnpackBagAllPoles);
					Job j = JobMaker.MakeJob(jd, target);

					selPawn.jobs.TryTakeOrderedJob(j);
				});
			}

			if (Bag.Floor != null)
			{
				yield return new FloatMenuOption("UnpackOne".Translate(Bag.Floor.LabelCapHpFrac()), delegate
				{
					jd.driverClass = typeof(JobDriver_UnpackBagFloor);
					Job j = JobMaker.MakeJob(jd, target);

					selPawn.jobs.TryTakeOrderedJob(j);
				});

				CompTentPartDamage damage = Bag.Floor.TryGetComp<CompTentPartDamage>();

				if (damage != null && damage.CanRepair)
				{
					yield return damage.RepairMenuOption(selPawn, TentDefOf.NCS_RepairCoverInBag, this);
				}
			}

			if (Bag.Cover != null || (Bag.Poles != null && Bag.Poles.Count > 0) || Bag.Floor != null)
			{
				yield return new FloatMenuOption("UnpackAll".Translate(), delegate
				{
					jd.driverClass = typeof(JobDriver_UnpackBagAll);
					Job j = JobMaker.MakeJob(jd, target);

					selPawn.jobs.TryTakeOrderedJob(j);
				});
			}
		}

		foreach (var opt in base.GetFloatMenuOptions(selPawn))
		{
			yield return opt;
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			if (gizmo is Designator_Install)
			{
#if RELEASE_1_4 || RELEASE_1_3 || RELEASE_1_2 || RELEASE_1_1
				gizmo.disabled = !Bag.Ready;
#else
				gizmo.Disabled = !Bag.Ready;
#endif

				gizmo.disabledReason = Bag.MissingMsg;
			}

			yield return gizmo;
		}
	}
}
