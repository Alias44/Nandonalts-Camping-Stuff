using System;

using RimWorld;
using Verse;

namespace Camping_Stuff;

public class TentMatComp : CompTentPartWithCellsDamage //(Thing)
{
	public CompProperties_TentMat Props => (CompProperties_TentMat)this.props;

#if RELEASE_1_3 || RELEASE_1_2 || RELEASE_1_1
	public TerrainDef Spawns => this.Props.spawnedFloor;
#else
	public TerrainDef Spawns => this.Props.spawnedFloor ?? DefDatabase<TerrainDef>.GetNamedSilentFail(this.Props.spawnedFloorTemplate.defName + this.parent.Stuff.defName);
#endif
}

public class CompProperties_TentMat : CompProperties_CompTentPartWithCellsDamage //(Def)
{
	public TerrainDef spawnedFloor;
#if !(RELEASE_1_3 || RELEASE_1_2 || RELEASE_1_1)
	public TerrainTemplateDef spawnedFloorTemplate;

	public bool UsesTemplate => spawnedFloor == null && spawnedFloorTemplate != null;
#endif

	public CompProperties_TentMat()
	{
		this.compClass = typeof(TentMatComp);
	}

	public CompProperties_TentMat(Type compClass)
	{
		this.compClass = compClass;
	}
}