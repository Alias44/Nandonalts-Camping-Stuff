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
	public class SketchEntityWraper : IExposable
	{
		private SketchEntity entity;

		public SketchEntityWraper()
		{
			entity = null;
		}

		public SketchEntityWraper(SketchEntity se)
		{
			entity = se;
		}

		public SketchEntityWraper(SketchEntity se, Rot4 sketchRot)
		{
			entity = se.DeepCopy();
			 
			int newRot = Rot4.North.AsInt - sketchRot.AsInt;
			if (newRot < 0)
				newRot += 4;
			Rot4 rot1 = new Rot4(newRot);

 			entity.pos = entity.pos.RotatedBy(rot1);
		}

		public override bool Equals(object obj)
		{
			SketchEntity se = null;
			if(obj is SketchEntityWraper sew)
			{
				se = sew.entity;
			}
			else if(obj is SketchEntity)
			{
				se = (SketchEntity) obj;
			}

			if(se != null)
			{
				return
					entity.Equals(se) || (
						entity.Label.Equals(se.Label) &&
						entity.OccupiedRect.Equals(se.OccupiedRect) &&
						entity.SpawnOrder.Equals(se.SpawnOrder) &&
						entity.pos.Equals(se.pos)
					);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return new Tuple<string, CellRect, float, IntVec3>(entity.Label, entity.OccupiedRect, entity.SpawnOrder, entity.pos).GetHashCode();
		}

		public void ExposeData()
		{
			Scribe_Deep.Look(ref entity, "WrapedSketchEntity");
		}

	}

	public class CompTentPartDamage : ThingComp // put damage int here and override protected in child cover?
	{
		public CompProperties_TentPartDamage Props => (CompProperties_TentPartDamage)this.props;

		protected HashSet<SketchEntityWraper> damagedCells = new HashSet<SketchEntityWraper>();

		public static int maxTiles = DefDatabase<ThingDef>.AllDefs.Where(d => d.HasComp(typeof(TentCoverComp))).Max(cover => cover.GetCompProperties<CompProperties_TentCover>().tiles);

		//protected virtual int DamageUnit => Math.Round((decimal) (this.parent.MaxHitPoints / maxTiles)); // Hitpoints to subtract per cell in damagedCells
		protected virtual int DamageUnit => (int)Math.Ceiling((1.0 / maxTiles) * this.parent.MaxHitPoints);

		protected virtual double DamageCost => (double)(this.parent.def.costStuffCount / maxTiles);
		public virtual int RepairCost => (int)Math.Ceiling(DamageCost * damagedCells.Count);

		public bool CheckCell(SketchEntity cell, Rot4 sketchRot)
		{
			bool check = damagedCells.Contains(new SketchEntityWraper(cell, sketchRot));
 			return check;
		}

		public bool AddCell(SketchEntity cell, Rot4 sketchRot)
		{
			bool added = damagedCells.Add(new SketchEntityWraper(cell, sketchRot));

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

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			if (RepairCost > 0)
			{
				yield return new FloatMenuOption($"Repair {this.parent.def.label} ({RepairCost} {this.parent.Stuff.label} needed)", delegate
				{
					Thing material = GenClosest.ClosestThingReachable(this.parent.Position, this.parent.Map, ThingRequest.ForDef(this.parent.Stuff), Verse.AI.PathEndMode.ClosestTouch, TraverseParms.For(selPawn, selPawn.NormalMaxDanger())); // validator?

					//selPawn.jobs.TryTakeOrderedJob(HaulAIUtility.HaulToContainerJob(selPawn, material, this.parent));
					selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(TentDefOf.NCS_RepairPart, material, this.parent));
				});
			}
		}

		public override void PostExposeData()
		{
			Scribe_Collections.Look(ref damagedCells, "damagedCells", LookMode.Deep);

			if (Scribe.mode == LoadSaveMode.PostLoadInit && damagedCells == null)
			{
				damagedCells = new HashSet<SketchEntityWraper>();
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