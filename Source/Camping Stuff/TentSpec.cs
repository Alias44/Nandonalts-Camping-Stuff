using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

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

	public class TentSpec
	{
		public int tiles = 0;
		public int layoutParts = 0;
		public Rot4 rotation = Rot4.South; // Towards bottom of screen
		public IntVec3 center;
		public List<List<TentLayout>> layout = new List<List<TentLayout>>();

		public TentSpec(List<String> tentLayout, Rot4 orientation)
		{
			if (layout.Count != 0) // Prevent building the list twice if the method has already been called (shouldn't happen, but just in case)
			{
				return;
			}

			layout = tentLayout.Select(row => row.Split(',').Select(val => (TentLayout)Enum.Parse(typeof(TentLayout), val)).ToList()).ToList();
			rotation = orientation;

			CalculateSpecs();
		}

		public void CalculateSpecs()
		{
			int height = layout.Count; //num rows
			int width = layout.Max(row => row.Count()); // max num cols

			Dictionary<TentLayout, int> partCount = layout.SelectMany(row => row).GroupBy(cell => cell).ToDictionary(group => group.Key, group => group.Count());

			layoutParts = partCount[TentLayout.wall] + partCount[TentLayout.door]; // Count the number of doors and walls (used for deploying damaged covers)
			tiles = partCount.Where(kv => kv.Key != TentLayout.empty && kv.Key != TentLayout.other).Sum(kv => kv.Value); // Count the number of tiles the tent should occupy (used for deploying damaged floors)

			for (int r = 0; r < layout.Count; r++)
			{
				int c = layout[r].IndexOf(TentLayout.pole);
				if (c != -1)
				{
					center = new IntVec3(c, 0, r);
					break;
				}
			}

			if (center == null)
			{
				center = new IntVec3(width / 2, 0, height / 2);
			}
		}

		public Sketch ToSketch(ThingDef coverStuff, ThingDef floorStuff = null)
		{
			Sketch sketch = new Sketch();
			sketch.Rotate(rotation);

			for (int r = 0; r < layout.Count; r++)
			{
				for (int c = 0; c < layout[r].Count; c++)
				{
					IntVec3 loc = new IntVec3(c, 0, r) - center;

					TentLayout cellLayout = layout[r][c];

					if (cellLayout != TentLayout.empty && cellLayout != TentLayout.other)
					{
						// Add roof
						SketchRoof sr = new SketchRoof
						{
							pos = loc,
							roof = TentDefOf.NCS_TentRoof
						};

						sketch.Add(sr, false);

						// Add floor if applicable
						if (floorStuff != null)
						{
							sketch.AddTerrain(TentDefOf.NCS_TentFloor, loc);
						}
					}

					if (cellLayout == TentLayout.wall)
					{
						sketch.AddThing(TentDefOf.NCS_TentWall, loc, rotation, coverStuff);
					}

					else if (cellLayout == TentLayout.door)
					{
						sketch.AddThing(TentDefOf.NCS_TentDoor, loc, rotation, coverStuff);
					}
				}
			}

			return sketch;
		}
	}
}
