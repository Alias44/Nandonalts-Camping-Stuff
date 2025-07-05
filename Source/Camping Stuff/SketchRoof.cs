using System;
using System.Collections.Generic;

using RimWorld;
using UnityEngine;
using Verse;

namespace Camping_Stuff;

class SketchRoof : SketchEntity
{
	public RoofDef roof;

	public override string Label => roof.label;

#if !RELEASE_1_1
	public override string LabelCap => roof.LabelCap;
#endif

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
		try
		{
			return at.GetRoof(map).Equals(roof);
		}
		catch
		{
			return false;
		}
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

#if RELEASE_1_1 || RELEASE_1_2 || RELEASE_1_3 || RELEASE_1_4 || RELEASE_1_5
	public override bool Spawn(IntVec3 at, Map map, Faction faction, Sketch.SpawnMode spawnMode = Sketch.SpawnMode.Normal, bool wipeIfCollides = false, List<Thing> spawnedThings = null, bool dormant = false)
#else
	public override bool Spawn(IntVec3 at, Map map, Faction faction, Sketch.SpawnMode spawnMode = Sketch.SpawnMode.Normal, bool wipeIfCollides = false, bool forceTerrainAffordance = false, List<Thing> spawnedThings = null, bool dormant = false, TerrainDef defaultAffordanceTerrain = null)
#endif

    {
		if(this.IsSpawningBlocked(at, map, (Thing)null, wipeIfCollides))
		{
			return false;
		}
		if (spawnMode == Sketch.SpawnMode.Normal)
		{
			map.roofGrid.SetRoof(at, roof);
		}
		else
		{
			throw new NotImplementedException("Spawn mode " + (object)spawnMode + " not implemented!");
		}

	return true;
}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look<RoofDef>(ref roof, "def");
	}

	public override SketchEntity DeepCopy()
	{
		SketchRoof sketchRoof = (SketchRoof)base.DeepCopy();
		sketchRoof.roof = this.roof;
		return sketchRoof;
	}
}
