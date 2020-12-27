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
		public int height;
		public int width;

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
			//sketch = new Sketch();
			height = tentLayoutSouth.Count;
			for (int r = 0; r < tentLayoutSouth.Count; r++)
			{
				List<TentLayout> parts = tentLayoutSouth[r].Split(',').Select(val => (TentLayout) Enum.Parse(typeof(TentLayout), val)).ToList();

				layoutS.Add(parts);
				width = Math.Max(width, parts.Count);
				/*for(int c = 0; c < parts.Count; c++)
				{
					IntVec3 pos = new IntVec3(r, c, 0); // r and c may need to be flipped?
					ThingDef thing = null;

					switch (parts[c])
					{
						case TentLayout.empty:
							break; // thing is already null
						case TentLayout.wall:
							thing = TentDefOf.NCS_TentWall;
							break;
						case TentLayout.door:
							thing = TentDefOf.NCS_TentDoor;
							break;
						case TentLayout.pole:
							thing = TentDefOf.NCS_TentBag;
							break;
						case TentLayout.roofedEmpty:
							//thing = TentThingDefOf.NCS_TentWall; // mot sure what to do here (yet)
							break;

						default:
							break;
					}

					if (thing != null) {
						positions.Add(pos);
					}
				}*/
			}
		}
	}
}