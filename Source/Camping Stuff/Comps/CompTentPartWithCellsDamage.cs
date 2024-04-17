using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace Camping_Stuff
{
	/// <summary>
	/// Comparison class for SketchEntities since is uses the default .equals function which cannot do logical equality
	/// </summary>
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

	/// <summary>
	/// Handles damage tracking/ repair where specific cells are involved
	/// </summary>
	public class CompTentPartWithCellsDamage : CompTentPartDamage
	{
		public CompProperties_CompTentPartWithCellsDamage Props => (CompProperties_CompTentPartWithCellsDamage)this.props;

		protected HashSet<SketchEntity> damagedCells = new HashSet<SketchEntity>(new SketchEntityComparer());

		private static TentSpec largestTent = DefDatabase<ThingDef>.AllDefs
			.Where(d => d.HasComp(typeof(TentCoverComp)))
			.Select(cover => cover.GetCompProperties<CompProperties_TentCover>().tentSpec)
			.Aggregate((spec1, spec2) => spec1.tiles > spec2.tiles ? spec1 : spec2);
		public static int maxTiles = largestTent.tiles; // maximum cells 

		protected virtual int DamageUnit => (int)Math.Ceiling((double)this.parent.MaxHitPoints / maxTiles); // Hitpoints to subtract per cell in damagedCells

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
				this.parent.HitPoints = Math.Max(1, this.parent.HitPoints - DamageUnit);
			}

			return added;
		}

		public bool HasDamagedCells()
		{
			return damagedCells.Count > 0;
		}

		/// <summary>
		/// Reallocates the SketchEntities in damagedCells based on the sketch provided
		/// </summary>
		/// <remarks>Allows the comp's damage tracking to function if the shape of tent has been changed</remarks>
		public void Reallocate(Sketch sketch, Rot4 sketchRot)
		{
			var d = sketch.Entities
				.Select(entity => entity.Normalize(sketchRot))
				.GroupBy(entity => entity.GetType()).ToDictionary(group => group.Key, group => group.ToHashSet(new SketchEntityComparer()));

			HashSet<SketchEntity> reallocatedCells = new HashSet<SketchEntity>(new SketchEntityComparer());

			var sketchRect = sketch.OccupiedRect;

			foreach (var cell in damagedCells)
			{
				if (!d[cell.GetType()].Contains(cell))
				{
					int maxRadius = Math.Max(sketchRect.Width, sketchRect.Height);
					for (int radius = 1; radius <= maxRadius; radius++)
					{
						CellRect searchSpace = CellRect.CenteredOn(cell.pos, radius).ClipInsideRect(sketchRect);

						var c = d[cell.GetType()]
							.Where(entity => searchSpace.Contains(entity.pos) &&
									entity.Label.Equals(cell.Label) &&
									!reallocatedCells.Contains(entity) &&
									entity.OccupiedRect.Height == cell.OccupiedRect.Height && // .equals() won't work since Cellrect's are relative to the posistion so the bounds won't line up
									entity.OccupiedRect.Width == cell.OccupiedRect.Width &&
									entity.SpawnOrder.Equals(entity.SpawnOrder))
							.OrderBy(entity => entity.pos.DistanceTo(cell.pos))
							.FirstOrFallback(null);

						if (c != null)
						{
							reallocatedCells.Add(c);
							break;
						}
						if (searchSpace.Equals(sketchRect))
						{
							return;
						}
					}
				}
				else
				{
					reallocatedCells.Add(cell);
				}
			}

			if (reallocatedCells.Count != 0)
			{
				damagedCells.Clear();
				damagedCells = reallocatedCells;
			}
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
			return "DamagedPartCount".Translate(damagedCells.Count);
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
