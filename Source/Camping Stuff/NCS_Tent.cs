using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Camping_Stuff
{
	public enum TentPart
	{
		other = -1,
		bag,
		pole,
		cover,
		floor
	}

	public class NCS_Tent : Building
	{
		public Sketch sketch = new Sketch();

		public int PoleCount { get; private set; } = 0;
		public int maxPoles = DefDatabase<ThingDef>.AllDefs.Where(d => d.HasComp(typeof(TentCoverComp))).Min(cover => cover.GetCompProperties<CompProperties_TentCover>().numPoles);

		//public Dictionary<ThingDef, Thing> poles = new Dictionary<ThingDef, Thing>();
		private List<Thing> poles = new List<Thing>();

		public List<Thing> Poles
		{
			get { return poles; }
		}

		private Thing cover = null;
		public Thing Cover
		{
			get { return cover; }
			set
			{
				if (!IsTentPart(value, TentPart.cover) && value != null)
				{
					return;
				}

				if (this.cover != null)
				{
					Eject(this.cover);
				}

				this.cover = value;

				if (this.cover == null)
				{
					maxPoles = DefDatabase<ThingDef>.AllDefs.Where(d => d.HasComp(typeof(TentCoverComp))).Min(cover => cover.GetCompProperties<CompProperties_TentCover>().numPoles);
				}
				else
				{
					maxPoles = Cover.TryGetComp<TentCoverComp>().Props.numPoles;
				}

				AdjustPoles();
				UpdateSketch();
			}
		}

		private Thing floor = null;
		public Thing Floor
		{
			get { return floor; }
			set
			{
				if (!IsTentPart(value, TentPart.floor) && value != null)
				{
					return;
				}

				if (this.floor != null)
				{
					Eject(this.floor);
				}

				this.floor = value;
			}
		}

		public bool Ready
		{
			get
			{
				return cover != null && PoleCount >= cover.TryGetComp<TentCoverComp>().Props.numPoles;
			}
		}

		/*public override string GetInspectString()
		{
			string readyStr;
			if (Ready)
			{
				readyStr = "Ready";
			}
			else
			{
				readyStr = "Not ready";
			}

			return readyStr + " to deploy\n" + base.GetInspectString();
		}*/

		public override CellRect? CustomRectForSelector
		{
			get
			{
				return !this.Spawned ? new CellRect?() : new CellRect?(this.sketch.OccupiedRect.MovedBy(this.Position));
			}
		}

		public IntVec2 Size
		{
			get
			{
				return this.sketch.OccupiedSize;
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				this.sketch.Rotate(this.Rotation);
				this.sketch.Spawn(this.Map, this.Position, Faction.OfPlayer, Sketch.SpawnPosType.Unchanged, Sketch.SpawnMode.Normal, false, false, (List<Thing>)null, false, false, (Func<SketchEntity, IntVec3, bool>)null, (Action<IntVec3, SketchEntity>)null);
			}

			Log.Message("SpawnSetup");
		}

		public override void ExposeData()
		{
			base.ExposeData();

			//Scribe_Values.Look(ref PoleCount, "PoleCount");
			//Scribe_Values.Look(ref deployed, "deployed", false);
			Scribe_Deep.Look(ref cover, "tentCover");
			Scribe_Deep.Look(ref floor, "tentFloor");
			Scribe_Collections.Look<Thing>(ref poles, "poleList", LookMode.Deep);
			//Scribe_Deep.Look<Sketch>(ref this.sketch, "sketch"); //Saving the sketch so the tent can still be packed if the layout changed

			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (poles == null)
				{
					poles = new List<Thing>();
				}

				//PoleCount = 0;
				foreach (Thing pole in poles)
				{
					PoleCount += pole.stackCount;
				}

				UpdateSketch();
			}
		}

		public void DrawGhost_NewTmp(IntVec3 at, bool placingMode, Rot4 rotation)
		{
			if (!Ready)
			{
				return;
			}
			if (sketch.Empty)
			{
				UpdateSketch();
			}

			sketch.Rotate(rotation);

			Func<SketchEntity, IntVec3, List<Thing>, Map, bool> validator = (Func<SketchEntity, IntVec3, List<Thing>, Map, bool>)null;
			if (placingMode)
			{
				validator = delegate (SketchEntity se, IntVec3 targetLoc, List<Thing> list, Map map)
				{
					IntVec3 loc = se.pos + targetLoc;
					Building bldg = loc.GetFirstBuilding(map);
					Plant plant = loc.GetPlant(map);

					return
					loc.InBounds(map) &&
					se.CanBuildOnTerrain(loc, map) &&
					!(loc.GetFirstItem(map) != null ||
					loc.GetFirstPawn(map) != null ||
					loc.GetFirstHaulable(map) != null) &&
					!(bldg != null && bldg.def.IsEdifice()) &&
					!(plant != null && plant.def.plant.IsTree);
				};
			}

			sketch.DrawGhost_NewTmp(at, Sketch.SpawnPosType.Unchanged, placingMode, null, validator);
		}

		/*public override IEnumerable<Gizmo> GetGizmos()
		{
			if(deployed)
			{
				//pack
			}
			else if(Ready)
			{
				// deploy (unless install isnt' in the set???
			}
		}*/

		#region PartPacking
		private bool IsTentPart(Thing t, TentPart partType)
		{
			CompUsable_TentPart partComp = t.TryGetComp<CompUsable_TentPart>();

			if (partComp == null)
			{
				return false;
			}

			return partComp.Props.partType == partType;
		}

		public void PackPart(Thing part)
		{
			TentPart partType = TentPart.other;

			if (part.TryGetComp<CompUsable_TentPart>() != null)
			{
				partType = part.TryGetComp<CompUsable_TentPart>().Props.partType;
			}

			switch (partType)
			{
				case TentPart.pole:
					int typeIndex = poles.FindIndex(p => p.Stuff.Equals(part.Stuff));

					if (typeIndex >= 0)
					{
						poles[typeIndex].stackCount += part.stackCount;
					}
					else
					{
						poles.Add(part);
					}
					PoleCount += part.stackCount;
					AdjustPoles();
					break;

				case TentPart.cover:
					Cover = part;
					break;

				case TentPart.floor:
					Floor = part;
					break;
			}

			if (part.Spawned == true)
			{
				part.DeSpawn(DestroyMode.Vanish);
			}
		}

		private void Eject(Thing t)
		{
			if (t != null)
			{
				GenPlace.TryPlaceThing(t, this.SpawnedParentOrMe.PositionHeld, this.SpawnedParentOrMe.MapHeld, ThingPlaceMode.Near);
			}
		}

		public void EjectPole(int poleIndex)
		{
			EjectPole(poleIndex, poles[poleIndex].stackCount);
		}

		public void EjectPole(Thing pole)
		{
			for (int i = 0; i < poles.Count; i++)
			{
				if (poles[i].Equals(pole))
				{
					EjectPole(i);
					break;
				}
			}
		}

		private void EjectPole(int poleIndex, int qtyOut)
		{
			Thing src = poles[poleIndex];
			Thing polesOut = src.SplitOff(qtyOut);

			if (src.Equals(polesOut) || src.stackCount == 0) // splitoff returns a clone if the qty == the satck count
			{
				poles.RemoveAt(poleIndex);
			}
			else
			{
				poles[poleIndex] = src;
			}

			Eject(polesOut);

			PoleCount -= qtyOut;
		}

		public void EjectAllPoles()
		{
			for (int i = 0; i < poles.Count;) // no modifier op as count should zero out
			{
				EjectPole(i);
			}
		}

		public void EjectAll()
		{
			Cover = null;
			EjectAllPoles();
			Floor = null;
		}

		private void AdjustPoles()
		{
			for (int i = 0; i < poles.Count && PoleCount > maxPoles; i++)
			{
				EjectPole(i, Math.Min(PoleCount - maxPoles, poles[i].stackCount));
			}
		}
		#endregion
		public float GetValue(StatDef sd)
		{
			if (sd == null)
			{
				return 0;
			}

			float val = 0;

			if (cover != null)
			{
				val += cover.GetStatValue(sd);
			}

			if (poles != null && poles.Count != 0)
			{
				//defDesc += "Poles:\n";
				foreach (Thing pole in poles)
				{
					val += (pole.GetStatValue(sd) * pole.stackCount);
				}
			}

			if (floor != null)
			{
				val += floor.GetStatValue(sd);
			}
			return val;
		}

		public string GetExplanation(StatDef sd, Func<float, string> format = null)
		{
			if (sd == null)
			{
				return "";
			}

			if (format == null)
			{
				format = delegate (float f)
				{
					return f.ToString();
				};
			}

			string massDesc = "";

			if (cover != null)
			{
				massDesc += cover.LabelCap + ": " + format(cover.GetStatValue(sd)) + "\n";
			}

			if (poles != null && poles.Count != 0)
			{
				foreach (Thing pole in poles)
				{
					massDesc += pole.LabelCap + ": " + format(pole.GetStatValue(sd) * pole.stackCount) + "\n";
				}
			}

			if (floor != null)
			{
				massDesc += floor.LabelCap + ": " + format(floor.GetStatValue(sd)) + "\n";
			}

			return massDesc;
		}

		private void UpdateSketch()
		{
			if (cover == null)
			{
				return;
			}

			sketch = new Sketch();
			sketch.Rotate(Rot4.South);

			CompProperties_TentCover coverProps = this.cover.TryGetComp<TentCoverComp>().Props;

			for (int r = 0; r < coverProps.layoutS.Count; r++)
			{
				for (int c = 0; c < coverProps.layoutS[r].Count; c++)
				{
					ThingDef thing = null;
					ThingDef stuff = cover.Stuff; //this.parent.Stuff
					IntVec3 loc = new IntVec3(c, 0, r) - coverProps.center;

					switch (coverProps.layoutS[r][c])
					{
						case TentLayout.other:
							break;
						case TentLayout.empty:
							break;
						case TentLayout.wall:
							thing = TentDefOf.NCS_TentWall;
							break;
						case TentLayout.door:
							thing = TentDefOf.NCS_TentDoor;
							break;
						case TentLayout.pole:
							//thing = TentDefOf.NCS_TentBag;
							break;
						case TentLayout.roofedEmpty:
							break;
						default:
							break;
					}

					if (thing != null)
					{
						sketch.AddThing(thing, loc, Rot4.South, stuff);
					}
				}
			}
		}
	}
}
