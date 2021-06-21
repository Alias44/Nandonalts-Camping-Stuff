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

	// Implimnet IThingHolder so packing can be done with DepositHauledThingInContainer ???
	public class NCS_Tent : Building
	{
		public Sketch sketch = new Sketch();
		private Rot4 deployedRot = Rot4.North;
		public int PoleCount { get; private set; } = 0;
		public int maxPoles = DefDatabase<ThingDef>.AllDefs.Where(d => d.HasComp(typeof(TentCoverComp))).Min(cover => cover.GetCompProperties<CompProperties_TentCover>().numPoles);

		//public Dictionary<ThingDef, Thing> poles = new Dictionary<ThingDef, Thing>();
		private List<Thing> poles = new List<Thing>();

		public List<Thing> Poles => poles;

		private Thing cover = null;
		public Thing Cover
		{
			get => cover;
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
			get => floor;
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
				UpdateSketch();
			}
		}

		public bool Ready => cover != null && PoleCount >= cover.TryGetComp<TentCoverComp>().Props.numPoles;

		public string MissingMsg
		{
			get
			{
				string baseMsg = "Tent not ready to deploy: ";
				if (cover == null)
				{
					return baseMsg += "no cover packed";
				}
				else if (PoleCount < maxPoles)
				{
					return baseMsg + $"not enough polls packed ({PoleCount}/{maxPoles} poles for {cover.LabelCapNoCount})";
				}

				return "";
			}
		}

		public string ContainsMsg
		{
			get
			{
				string msg = "\nContains: \n";

				if (Cover != null)
				{
					msg += $"Cover: {Cover.LabelCapHpFrac()}\n";
				}

				if(PoleCount > 0)
				{
					msg += "Poles:\n";
					foreach(Thing pole in Poles)
					{
						msg += "\t" + pole.LabelCap + "\n";
					}
				}

				if(Floor != null)
				{
					msg = $"Floor: {Floor.LabelCapHpFrac()})\n";
				}

				return msg;
			}
		}


		// Overriding label and descritpion (while spawned/ installed/ deployed), since minified things don't really have their own.
		public override string LabelNoCount => this.Spawned ? "Tent" : base.LabelNoCount;

		public override string DescriptionFlavor
		{
			get
			{
				if (this.Spawned)
					return "A temporary structure";


				return base.DescriptionFlavor + ContainsMsg;
			}
		}

		public override Graphic Graphic => Ready ? this.cover.Graphic : base.Graphic;

		public override CellRect? CustomRectForSelector
		{
			get
			{
				this.sketch.Rotate(this.Rotation);
				return !this.Spawned ? new CellRect?() : new CellRect?(this.sketch.OccupiedRect.MovedBy(this.Position));
			}
		}

		public CellRect CandidateRect(IntVec3 pos)
		{
			return this.Ready ? this.sketch.OccupiedRect.MovedBy(pos) : new CellRect();
		}

		public IntVec2 Size => this.sketch.OccupiedSize;

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);

			this.sketch.Rotate(this.Rotation);

  			if (!respawningAfterLoad)
			{
 				if (floor != null && this.floor.TryGetComp<CompTentPartDamage>().HasDamagedCells())
				{
					Messages.Message("DamagedMat".Translate(), MessageTypeDefOf.NegativeEvent);
				}

				if (this.cover.TryGetComp<CompTentPartDamage>().HasDamagedCells())
				{
					Messages.Message("DamagedCover".Translate(), MessageTypeDefOf.NegativeEvent);
				}

				foreach (SketchEntity se in this.sketch.Entities.OrderBy<SketchEntity, float>((Func<SketchEntity, float>)(x => x.SpawnOrder)))
				{
					IntVec3 cell = se.pos + this.Position;

					if (se is SketchTerrain && this.floor.TryGetComp<CompTentPartDamage>().CheckCell(se, this.Rotation))
					{
						// spawn damaged message
					}
					else if (se is SketchThing && this.cover.TryGetComp<CompTentPartDamage>().CheckCell(se, this.Rotation))
					{
						// spawn damaged message
					}
					else
					{
						se.Spawn(cell, this.Map, Faction.OfPlayer);
					}
				}
			}

			deployedRot = this.Rotation;
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			this.sketch.Rotate(deployedRot);

			foreach (SketchEntity se in this.sketch.Entities.OrderByDescending<SketchEntity, float>((Func<SketchEntity, float>)(x => x.SpawnOrder)))
			{
				IntVec3 cell = se.pos + this.Position;

				if (se is SketchRoof sr && sr.IsSameSpawned(cell, this.Map))
				{
					Map.roofGrid.SetRoof(cell, null);
				}
				else if (se is SketchThing thing)
				{
					if (thing.IsSameSpawned(cell, this.Map))
					{
						thing.GetSameSpawned(cell, this.Map).DeSpawn(DestroyMode.Vanish);
					}
					else
					{
						this.cover.TryGetComp<CompTentPartDamage>().AddCell(se, deployedRot);
					}
				}
				else if (se is SketchTerrain terrain)
				{
					if (terrain.IsSameSpawned(cell, this.Map))
					{
						Map.terrainGrid.RemoveTopLayer(terrain.pos + this.Position, false);
					}
					else
					{
						this.floor.TryGetComp<CompTentPartDamage>().AddCell(terrain, deployedRot);
					}
				}
			}

			base.DeSpawn(mode);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref cover, "tentCover");
			Scribe_Deep.Look(ref floor, "tentFloor");
			Scribe_Collections.Look<Thing>(ref poles, "poleList", LookMode.Deep);

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

					return
						loc.InBounds(map) &&
						se.CanBuildOnTerrain(loc, map);
					//GenConstruct.BlocksConstructions()
				};
			}

			sketch.DrawGhost_NewTmp(at, Sketch.SpawnPosType.Unchanged, placingMode, null, validator);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
		}

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
					IntVec3 loc = new IntVec3(c, 0, r) - coverProps.center;

					TentLayout cellLayout = coverProps.layoutS[r][c];

					if(cellLayout != TentLayout.empty && cellLayout != TentLayout.other)
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
							sketch.AddTerrain(TentDefOf.NCS_TentFloor, loc);
						}
					}

					if(cellLayout == TentLayout.wall)
					{
						sketch.AddThing(TentDefOf.NCS_TentWall, loc, Rot4.South, cover.Stuff);
					}

					else if (cellLayout == TentLayout.door)
					{
						sketch.AddThing(TentDefOf.NCS_TentDoor, loc, Rot4.South, cover.Stuff);
					}
				}
			}
		}
	}
}
