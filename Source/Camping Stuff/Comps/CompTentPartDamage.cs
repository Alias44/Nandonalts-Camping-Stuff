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
	internal class SketchEntityComparer : IEqualityComparer<SketchEntity>
	{
		public bool Equals(SketchEntity x, SketchEntity y)
		{
			if (y == null && x == null)
				return true;
			else if (x == null ^ y == null)
				return false;

			return
				x.Equals(y) || (
					x.Label.Equals(y.Label) &&
					x.OccupiedRect.Equals(y.OccupiedRect) &&
					x.SpawnOrder.Equals(y.SpawnOrder) &&
					x.pos.Equals(y.pos)
				);
		}

		public int GetHashCode(SketchEntity obj)
		{
			return new Tuple<string, CellRect, float, IntVec3>(obj.Label, obj.OccupiedRect, obj.SpawnOrder, obj.pos).GetHashCode();
		}
	}

	public class CompTentPartDamage : ThingComp // put damage int here and override protected in child cover?
	{
		public CompProperties_TentPartDamage Props => (CompProperties_TentPartDamage)this.props;

		protected HashSet<SketchEntity> damagedCells = new HashSet<SketchEntity>(new SketchEntityComparer());

		public static int maxTiles = DefDatabase<ThingDef>.AllDefs.Where(d => d.HasComp(typeof(TentCoverComp))).Max(cover => cover.GetCompProperties<CompProperties_TentCover>().tiles);

		//protected virtual int DamageUnit => Math.Round((decimal) (this.parent.MaxHitPoints / maxTiles)); // Hitpoints to subtract per cell in damagedCells
		protected virtual int DamageUnit => (int)Math.Ceiling((1.0 / maxTiles) * this.parent.MaxHitPoints);

		protected virtual double DamageCost => ((double)this.parent.def.costStuffCount / maxTiles);
		public virtual int RepairCost => (int)Math.Ceiling(DamageCost * damagedCells.Count);

		public bool CheckCell(SketchEntity cell, Rot4 sketchRot)
		{
			bool check = damagedCells.Contains(cell.Normalize(sketchRot));
 			return check;
		}

		public bool AddCell(SketchEntity cell, Rot4 sketchRot)
		{
			bool added = damagedCells.Add(cell.Normalize(sketchRot));

			if (added)
			{
				this.parent.HitPoints = this.parent.HitPoints - DamageUnit;
			}

			return added;
		}

		public bool HasDamagedCells()
		{
 			return damagedCells.Count > 0;
		}

		public void ClearCells()
		{
			this.parent.HitPoints = this.parent.MaxHitPoints;
			this.damagedCells.Clear();
		}

		public Thing RepairMaterial(IntVec3 pos, Map map, Pawn selPawn)
		{
			return GenClosest.ClosestThingReachable(pos, map,
				ThingRequest.ForDef(this.parent.Stuff), Verse.AI.PathEndMode.ClosestTouch,
				TraverseParms.For(selPawn, selPawn.NormalMaxDanger())); // validator?
		}

		public Thing RepairMaterial(Pawn selPawn)
		{
			return RepairMaterial(selPawn.Position, selPawn.Map, selPawn);
		}

		public FloatMenuOption RepairMenuOption(Pawn selPawn, JobDef jobDef, LocalTargetInfo destination)
		{
			if (RepairCost > 0)
			{
				Thing material = RepairMaterial(selPawn);
				if (material != null)
				{
					return new FloatMenuOption($"Repair {this.parent.def.label} ({RepairCost} {this.parent.Stuff.label} needed)", delegate
					{
						selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, material, destination));
					});
				}
			}

			return new FloatMenuOption($"Unable to repair {this.parent.LabelNoCount}, need {RepairCost} {this.parent.Stuff.label}", null);
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			yield return RepairMenuOption(selPawn, TentDefOf.NCS_RepairPart, this.parent);
		}

		public override void PostExposeData()
		{
			Scribe_Collections.Look(ref damagedCells, "damagedCells", LookMode.Deep);

			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				damagedCells = damagedCells == null
					? new HashSet<SketchEntity>(new SketchEntityComparer())
					: new HashSet<SketchEntity>(damagedCells, new SketchEntityComparer());
			}
		}

		public override string CompInspectStringExtra()
		{
			return $"{damagedCells.Count} missing";
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