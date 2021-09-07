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
		private Sketch deployedSketch;

		public int PoleCount { get; private set; } = 0;
		public int maxPoles = DefDatabase<ThingDef>.AllDefs.Where(d => d.HasComp(typeof(TentCoverComp))).Min(cover => cover.GetCompProperties<CompProperties_TentCover>().numPoles);

		//public Dictionary<ThingDef, Thing> poles = new Dictionary<ThingDef, Thing>();
		private List<Thing> poles = new List<Thing>();

		public List<Thing> Poles => poles ?? (poles = new List<Thing>());

		public int PoleKindCount(Thing thing)
		{
			int poleIndex = FindPoleIndex(thing);
			return (poleIndex > -1) ? poles[poleIndex].stackCount : 0;
		}

		private int FindPoleIndex(Thing thing)
		{
			return Poles.FindIndex(p => p.CanStackWith(thing));
		}

		private Thing cover = null;
		public Thing Cover
		{
			get => cover;
			set
			{
				if (!Util.IsTentPart(value, TentPart.cover) && value != null)
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
				if (!Util.IsTentPart(value, TentPart.floor) && value != null)
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
				if (cover == null)
				{
					return "DeployNotReadyNoCover".Translate();
				}
				else if (PoleCount < maxPoles)
				{
					return "DeployNotReadyNeedPoles".Translate(PoleCount, maxPoles, cover.LabelCapNoCount);
				}

				return "";
			}
		}

		public string ContainsMsg
		{
			get
			{
				string msg = "";

				if (Cover != null)
				{
					msg += "ContainsCover".Translate(Cover.LabelCapHpFrac()) + "\n";
				}

				if(PoleCount > 0)
				{
					msg += "ContainsPoles".Translate() + "\n";
					foreach(Thing pole in Poles)
					{
						msg += Util.indent + pole.LabelCap + "\n";
					}
				}

				if(Floor != null)
				{
					msg += "ContainsFloor".Translate(Floor.LabelCapHpFrac()) + "\n";
				}

				return msg;
			}
		}

		// Overriding label and description (while spawned/ installed/ deployed), since minified things don't really have their own.
		public override string LabelNoCount => this.Spawned ? (string) "SpawnedTentLabel".Translate() : base.LabelNoCount;

		public override string DescriptionDetailed => Ready ? (string) "TentDescriptionDetailed".Translate(cover.Stuff.LabelAsStuff, cover.DescriptionDetailed.Replace("A ", "")) : base.DescriptionDetailed;

		public string TentSize => this.Ready ? cover.Label.Replace(" tent cover", "") : "unknown";

		public override string DescriptionFlavor
		{
			get
			{
				if (this.Spawned)
					return "SpawnedTentDescriptionFlavor".Translate();

				return $"{base.DescriptionFlavor}\n{"TentContains".Translate()}\n{ContainsMsg}";
			}
		}

		public override Graphic Graphic => Ready ? this.cover.Graphic : base.Graphic;

		public override CellRect? CustomRectForSelector
		{
			get
			{
				return !this.Spawned ? new CellRect?() : new CellRect?(this.deployedSketch.OccupiedRect.MovedBy(this.Position));
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

			//this.sketch.Rotate(this.Rotation);

			if (!respawningAfterLoad)
 			{
 				if (floor != null && this.floor.TryGetComp<CompTentPartWithCellsDamage>().HasDamagedCells())
				{
					Messages.Message("DamagedMat".Translate(), MessageTypeDefOf.NegativeEvent);
				}

				if (this.cover.TryGetComp<TentCoverComp>().HasDamagedCells())
				{
					Messages.Message("DamagedCover".Translate(), MessageTypeDefOf.NegativeEvent);
				}

				foreach (SketchEntity se in this.sketch.Entities.OrderBy<SketchEntity, float>((Func<SketchEntity, float>)(x => x.SpawnOrder)))
				{
					IntVec3 cell = se.pos + this.Position;

					if (!(se is SketchTerrain && this.floor.TryGetComp<CompTentPartWithCellsDamage>().CheckCell(se, this.Rotation)) && // 
					    !(se is SketchThing && this.cover.TryGetComp<TentCoverComp>().CheckCell(se, this.Rotation)))
					{
						var spawnedThings = new List<Thing>();
 						se.Spawn(cell, this.Map, Faction.OfPlayer, spawnedThings: spawnedThings);

						foreach (var twc in spawnedThings.Where(t => t is ThingWithComps).Cast<ThingWithComps>())
						{
							try
							{
								twc.TryGetComp<TentSpawnedComp>().tent = this;
							}
							catch (NullReferenceException) // Edge case buffer (This is non-ideal, since dynamically added comps aren't replaced on load)
							{
								twc.AllComps.Add(new TentSpawnedComp
								{
									tent = this
								});
							}
							finally // top up the HP after the tent reference has been set (this helps account for any pole factors that would spawn the tent at its base health)
							{
								twc.HitPoints = twc.MaxHitPoints;
							}
						}
					}
				}
			}

			// Prevents respawningAfterLoad from overwriting the deployed sketch if it has already been set (ie by ExposeData or back compatibiity),
			if (deployedSketch == null)
			{
				deployedSketch = sketch.DeepCopy();
			}
		}

		// Trouble with doors that are forced open???
		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			foreach (SketchEntity se in this.deployedSketch.Entities.OrderByDescending<SketchEntity, float>((Func<SketchEntity, float>)(x => x.SpawnOrder)))
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
						this.cover.TryGetComp<TentCoverComp>().AddCell(se, this.Rotation);
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
						this.floor.TryGetComp<CompTentPartWithCellsDamage>().AddCell(terrain, this.Rotation);
					}
				}
			}

			deployedSketch = null;
			base.DeSpawn(mode);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref cover, "tentCover");
			Scribe_Deep.Look(ref floor, "tentFloor");
			Scribe_Collections.Look<Thing>(ref poles, "poleList", LookMode.Deep);
			// Allows the tent to be packed, even if the layout has changed (at the expense of save file space)
			Scribe_Deep.Look<Sketch>(ref deployedSketch, "deployedTentSketch", null);

			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				
				PoleCount += Poles.Sum(pole => pole.stackCount);

				if (cover != null)
				{
					maxPoles = Cover.TryGetComp<TentCoverComp>().Props.numPoles;
				}

				UpdateSketch();
			}
		}

		public void DrawGhost(IntVec3 at, bool placingMode, Rot4 rotation)
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

			var validator = (Func<SketchEntity, IntVec3, List<Thing>, Map, bool>)null;
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

#if RELEASE_1_2
			sketch.DrawGhost_NewTmp(at, Sketch.SpawnPosType.Unchanged, placingMode, null, validator);
#else
			sketch.DrawGhost(at, Sketch.SpawnPosType.Unchanged, placingMode, null, validator);
#endif
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
		}

#region PartPacking
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
 					int typeIndex = this.FindPoleIndex(part);

					if (typeIndex >= 0)
					{
						poles[typeIndex].stackCount += part.stackCount;
					}
					else
					{
						poles.Add(part);
					}
					PoleCount += part.stackCount;
					AdjustPoles(typeIndex);
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
			for (int i = 0; i < Poles.Count;) // no modifier op as count should zero out
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

		private void AdjustPoles(int excludeIndex = -1)
		{
			for (int i = 0; i < Poles.Count && PoleCount > maxPoles; i++)
			{
				if (i != excludeIndex)
				{
					EjectPole(i, Math.Min(PoleCount - maxPoles, poles[i].stackCount));
				}
			}

			if (PoleCount > maxPoles)
			{
				AdjustPoles();
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

			if (Poles.Count != 0)
			{
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
				format = f => f.ToString();
			}

			string massDesc = "";

			if (cover != null)
			{
				massDesc += cover.LabelCap + ": " + format(cover.GetStatValue(sd)) + "\n";
			}

			if (Poles.Count != 0)
			{
				massDesc = poles.Aggregate(massDesc, (current, pole) => current + (pole.LabelCap + ": " + format(pole.GetStatValue(sd) * pole.stackCount) + "\n"));
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

			sketch = this.cover.TryGetComp<TentCoverComp>().Props.tentSpec.ToSketch(cover.Stuff, floor?.Stuff);
			sketch.Rotate(this.Rotation);
		}

		public void SetDeployedSketch(TentSpec spec)
		{
			deployedSketch = spec.ToSketch(cover.Stuff, floor?.Stuff);
			var sketchRot = this.Rotation;
			sketchRot.AsInt += 2;

			deployedSketch.Rotate(this.Rotation);
		}

		public void SpawnParts(ThingDef cover)
		{
			Cover = ThingMaker.MakeThing(cover, Stuff);

			var poleStack = ThingMaker.MakeThing(TentDefOf.NCS_TentPart_Pole, ThingDefOf.WoodLog);
			poleStack.stackCount = maxPoles;
			PackPart(poleStack);
		}
	}
}
