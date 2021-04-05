using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Camping_Stuff
{
	class SketchRoof : SketchEntity
	{
		public RoofDef roof;

		public override string Label
		{
			get
			{
				return roof.label;
			}
		}

		public override string LabelCap
		{
			get
			{
				return roof.LabelCap;
			}
		}

		public override CellRect OccupiedRect
		{
			get
			{
				return new CellRect(this.pos.x, this.pos.z, 1, 1);
			}
		}

		public override float SpawnOrder
		{
			get
			{
				return float.MaxValue;
			}
		}

		public override bool CanBuildOnTerrain(IntVec3 at, Map map)
		{
			return true; // Assume true since other sketch entities aren't known at this time
			/*try
			{
				return at.GetFirstBuilding(map).def.holdsRoof;
			}
			catch
			{
				return false;
			}*/
		}

		public override void DrawGhost(IntVec3 at, Color color)
		{
			return; // No ghost for roofs;
		}

		public override bool IsSameSpawned(IntVec3 at, Map map)
		{
			return at.GetRoof(map).Equals(roof);
		}

		public override bool IsSameSpawnedOrBlueprintOrFrame(IntVec3 at, Map map)
		{
			return this.IsSameSpawned(at, map);
		}

		public override bool IsSpawningBlocked(IntVec3 at, Map map, Thing thingToIgnore = null, bool wipeIfCollides = false)
		{
			return this.IsSpawningBlockedPermanently(at, map, thingToIgnore, wipeIfCollides) || at.GetRoof(map) != null;
		}

		public override bool IsSpawningBlockedPermanently(IntVec3 at, Map map, Thing thingToIgnore = null, bool wipeIfCollides = false)
		{
			return !at.InBounds(map) || !this.CanBuildOnTerrain(at, map);
		}

		public override bool SameForSubtracting(SketchEntity other)
		{
			if (!(other is SketchRoof sketchRoof))
				return false;
			if (sketchRoof == this)
				return true;
			if(this.roof == sketchRoof.roof)
			{
				return true;
			}
			return false;
		}

		public override bool Spawn(IntVec3 at, Map map, Faction faction, Sketch.SpawnMode spawnMode = Sketch.SpawnMode.Normal, bool wipeIfCollides = false, List<Thing> spawnedThings = null, bool dormant = false)
		{
			if (this.IsSpawningBlocked(at, map, (Thing)null, wipeIfCollides))
			{
				return false;
			}
			if(spawnMode == Sketch.SpawnMode.Normal)
			{
				map.roofGrid.SetRoof(at, roof);
			}
			else
			{
				throw new NotImplementedException("Spawn mode " + (object)spawnMode + " not implemented!");
			}

			return true;
		}
	}
}
