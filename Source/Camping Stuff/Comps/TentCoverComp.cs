using Verse;
using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;

namespace Camping_Stuff
{
	public enum TentLayout
	{
		other = -1,
		empty,
		wall,
		door,
		pole,
		roofedEmpty,
	}

	public class TentCoverComp : CompTentPartDamage //(Thing)
	{
		public CompProperties_TentCover Props => (CompProperties_TentCover)this.props;

		//public HashSet<SketchEntity> dammagedCells = new HashSet<SketchEntity>();

		//float RepairCost => this.dammagedCells.Count / this.Props.layoutParts * this.parent.def.costStuffCount;

		protected override int DamageUnit => Math.Max((int)Math.Floor((1.0 / this.Props.layoutParts) * this.parent.MaxHitPoints), 1);
		protected override double DamageCost => (double)(this.parent.def.costStuffCount / this.Props.layoutParts);
	}

	public class CompProperties_TentCover : CompProperties_TentPartDamage //(Def)
	{
		public int numPoles;
		private List<string> tentLayoutSouth = new List<string>();
		public List<List<TentLayout>> layoutS = new List<List<TentLayout>>();
		public IntVec3 center;
		public int layoutParts = 0;
		public int tiles = 0;

		//public Sketch sketch = null;


		public CompProperties_TentCover()
		{
			this.compClass = typeof(TentCoverComp);
		}

		public CompProperties_TentCover(Type compClass)
		{
			this.compClass = compClass;
		}

		public override void ResolveReferences(ThingDef parentDef)
		{
			int height = tentLayoutSouth.Count; //num rows
			int width = 0; // max num cols
			layoutParts = 0;

			for (int r = 0; r < tentLayoutSouth.Count; r++)
			{
				List<TentLayout> parts = tentLayoutSouth[r].Split(',').Select(val => (TentLayout) Enum.Parse(typeof(TentLayout), val)).ToList();

				layoutS.Add(parts);

				int c = parts.IndexOf(TentLayout.pole);
				if (c != -1)
				{
					center = new IntVec3(c, 0, r);
				}

				width = Math.Max(width, parts.Count);
				layoutParts += parts.Count<TentLayout>(square => square == TentLayout.wall || square == TentLayout.door); // Count the number of doors and walls (used for deploying damaged covers)
				tiles += parts.Count<TentLayout>(square => square != TentLayout.empty && square != TentLayout.other); // Count the number of tiles the tent should occupy (used for deploying damaged floors)
			}

			if(center == null)
			{
				center = new IntVec3(width / 2, 0, height / 2);
			}
		}
	}
}