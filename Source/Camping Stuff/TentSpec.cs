using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Camping_Stuff
{
	public enum TentLayout : int
	{
		other = -1,
		empty,
		wall,
		door,
		pole,
		roofedEmpty,
	}

	public class TentSpec : IExposable
	{
		public int tiles = 0;
		public int layoutParts = 0;
		public Rot4 rotation = Rot4.South; // Towards bottom of screen
		public IntVec3 center;
		public List<List<TentLayout>> layout = new List<List<TentLayout>>();
		private int height;
		private int width;

		private Dictionary<TentLayout, ThingDef> spawns = new Dictionary<TentLayout, ThingDef>();

		public TentSpec() { }

		public TentSpec(List<string> tentLayout, Rot4 orientation)
		{
			Initalize(tentLayout, orientation);
		}

		/// <summary>pseudo constructor (allows the layout to be reconstructed from the ExposeData())</summary>
		private void Initalize(List<string> tentLayout, Rot4 orientation)
		{
			if (layout.Count != 0) // Prevent building the list twice if the method has already been called (shouldn't happen, but just in case)
			{
				return;
			}

			layout = tentLayout.Select(row => row.Split(',').Select(val => (TentLayout)Enum.Parse(typeof(TentLayout), val)).ToList()).ToList();
			rotation = orientation;

			CalculateDimensions();
			SmoothLists();
			NoramlizeList(rotation.AsInt);
			center = FindCenter();
			CountParts();
		}

		public void AssignSpawns(Dictionary<TentLayout, ThingDef> tentSpawns)
		{
			spawns = tentSpawns;

			// Default tent configs
			if (!spawns.ContainsKey(TentLayout.wall))
			{
				spawns[TentLayout.wall] = TentDefOf.NCS_TentWall;
			}
			if (!spawns.ContainsKey(TentLayout.door))
			{
				spawns[TentLayout.door] = TentDefOf.NCS_TentDoor;
			}
		}

		private void CalculateDimensions()
		{
			height = layout.Count; //num rows
			width = layout.Max(row => row.Count()); // max num cols
		}

		private void SwapDimensions()
		{
			int tmp = height;
			height = width;
			width = tmp;
		}

		private IntVec3 FindCenter()
		{
			for (int r = 0; r < layout.Count; r++)
			{
				int c = layout[r].IndexOf(TentLayout.pole);
				if (c != -1)
				{
					return new IntVec3(c, 0, r);
				}
			}

			return new IntVec3(width / 2, 0, height / 2);
		}

		private void CountParts()
		{
			Dictionary<TentLayout, int> partCount = layout.SelectMany(row => row).GroupBy(cell => cell).ToDictionary(group => group.Key, group => group.Count());

			layoutParts = partCount[TentLayout.wall] + partCount[TentLayout.door]; // Count the number of doors and walls (used for deploying damaged covers)
			tiles = partCount.Where(kv => kv.Key != TentLayout.empty && kv.Key != TentLayout.other).Sum(kv => kv.Value); // Count the number of tiles the tent should occupy (used for deploying damaged floors)
		}

		private void SmoothLists()
		{
			for (int r = 0; r < height; r++)
			{
				while (layout[r].Count < width)
				{
					layout[r].Add(TentLayout.empty);
				}
			}
		}

		private void NoramlizeList(int roataionInt)
		{
			switch (roataionInt)
			{
				case 0: // North
					layout.ForEach(row => row.Reverse());
					layout.Reverse();
					break;

				case 1: // East
					layout = Enumerable.Range(0, width).Select(c => layout.Select(row => row[c]).Reverse().ToList()).ToList();
					SwapDimensions();
					break;

				case 2: // South
					break;

				case 3: // West
					layout = Enumerable.Range(0, width).Reverse().Select(c => layout.Select(row => row[c]).ToList()).ToList();
					SwapDimensions();
					break;

				default:
					break;
			}
		}

		public Sketch ToSketch(ThingDef coverStuff, TerrainDef floor = null)
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
						if (floor != null)
						{
							sketch.AddTerrain(floor, loc);
						}
					}

					if (cellLayout == TentLayout.wall || cellLayout == TentLayout.door)
					{
						sketch.AddThing(spawns[cellLayout], loc, rotation, coverStuff);
					}
				}
			}

			return sketch;
		}

		private List<string> JoinRows()
		{
			return layout.Select(row => string.Join(",", row.Cast<int>())).ToList();
		}

		// nullcheck prevents errors when loading tentlayouts without embeded spawns (<1.5)
		public override string ToString()
		{
			return string.Join(Environment.NewLine, JoinRows()) +
				(spawns == null ? string.Empty : Environment.NewLine + spawns.ToStringFullContents());
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public void ExposeData()
		{
			List<string> saveable = new List<string>();

			if(Scribe.mode == LoadSaveMode.Saving)
			{
				saveable = JoinRows();
			}
			else
			{
				if (spawns == null)
				{
					spawns = new Dictionary<TentLayout, ThingDef>();
				}
			}

			Scribe_Collections.Look(ref saveable, "tentSpec", LookMode.Value);
			Scribe_Collections.Look(ref spawns, "tentSpawns", LookMode.Value, LookMode.Def);

			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				Initalize(saveable, Rot4.South);
			}
			else if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (spawns == null)
				{
					AssignSpawns(new Dictionary<TentLayout, ThingDef>());
				}
			}
		}
	}
}
