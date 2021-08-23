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

		public override string Label => roof.label;

		public override string LabelCap => roof.LabelCap;

		public override CellRect OccupiedRect => new CellRect(this.pos.x, this.pos.z, 1, 1);

		public override float SpawnOrder => float.MaxValue;

		public override bool CanBuildOnTerrain(IntVec3 at, Map map)
		{
			return true; // Assume true since other sketch entities aren't known at this time
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
			return !wipeIfCollides && (this.IsSpawningBlockedPermanently(at, map, thingToIgnore, wipeIfCollides) || at.GetRoof(map) != null);
		}

		public override bool IsSpawningBlockedPermanently(IntVec3 at, Map map, Thing thingToIgnore = null, bool wipeIfCollides = false)
		{
			return !wipeIfCollides && (!at.InBounds(map) || !this.CanBuildOnTerrain(at, map));
		}

		public override bool SameForSubtracting(SketchEntity other)
		{
			if (!(other is SketchRoof sketchRoof))
				return false;
			if (sketchRoof == this)
				return true;
			return this.roof == sketchRoof.roof;
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
