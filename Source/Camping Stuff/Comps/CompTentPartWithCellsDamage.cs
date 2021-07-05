using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

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

	public class CompTentPartWithCellsDamage : CompTentPartDamage
	{
		public CompProperties_CompTentPartWithCellsDamage Props => (CompProperties_CompTentPartWithCellsDamage)this.props;

		protected HashSet<SketchEntity> damagedCells = new HashSet<SketchEntity>(new SketchEntityComparer());

		public static int maxTiles = DefDatabase<ThingDef>.AllDefs.Where(d => d.HasComp(typeof(TentCoverComp))).Max(cover => cover.GetCompProperties<CompProperties_TentCover>().tiles); // maximum cells 

		protected virtual int DamageUnit => (int)Math.Ceiling((double) this.parent.MaxHitPoints / maxTiles); // Hitpoints to subtract per cell in damagedCells

		protected override double DamageCost => ((double)this.parent.def.costStuffCount / maxTiles);
		public override int RepairCost => (int)Math.Ceiling(DamageCost * damagedCells.Count);


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

		public override void Repair()
		{
			base.Repair();
			this.damagedCells.Clear();
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

	public class CompProperties_CompTentPartWithCellsDamage : CompProperties //(Def)
	{
		public CompProperties_CompTentPartWithCellsDamage()
		{
			this.compClass = typeof(CompTentPartWithCellsDamage);
		}
	}
}
