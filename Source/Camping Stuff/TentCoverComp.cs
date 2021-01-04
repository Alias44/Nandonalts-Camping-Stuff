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

	public class TentCoverComp : ThingComp //(Thing)
	{
		public CompProperties_TentCover Props => (CompProperties_TentCover)this.props;
	}

	public class CompProperties_TentCover : CompProperties //(Def)
	{
		public int numPoles;
		private List<string> tentLayoutSouth = new List<string>();
		public List<List<TentLayout>> layoutS = new List<List<TentLayout>>();
		public IntVec3 center;
		//public int height;
		//public int width;

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
			//height = tentLayoutSouth.Count;
			for (int r = 0; r < tentLayoutSouth.Count; r++)
			{
				List<TentLayout> parts = tentLayoutSouth[r].Split(',').Select(val => (TentLayout) Enum.Parse(typeof(TentLayout), val)).ToList();

				layoutS.Add(parts);

				int c = parts.IndexOf(TentLayout.pole);
				if (c != -1)
				{
					center = new IntVec3(c, 0, r);
				}
				//width = Math.Max(width, parts.Count);
			}
		}
	}
}