using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Camping_Stuff;

/// <see cref="RimWorld.TerrainDefGenerator_Carpet"/>
public static class TerrainDefGenerator_TentFloor
{
#if !(RELEASE_1_4 || RELEASE_1_3 || RELEASE_1_2 || RELEASE_1_1)
	public static IEnumerable<TerrainDef> ImpliedTerrainDefs(bool hotReload = false)
	{
		var matColors = GenStuff.AllowedStuffsFor(TentDefOf.NCS_TentPart_Floor).ToHashSet()
			.Select(stuff => ColorFromStuff(stuff))
			.ToList();


		var tentFloors = DefDatabase<ThingDef>.AllDefs
			.Where(def => def.HasComp<TentMatComp>())
			.Select(def => def.GetCompProperties<CompProperties_TentMat>().spawnedFloorTemplate);

		foreach (var colorDef in matColors)
		{
			DefGenerator.AddImpliedDef(colorDef, hotReload);

			int index = 0;
			foreach (var item in tentFloors)
			{
				TerrainDef td = TentTerrainFromBlueprint(item, colorDef, index, false);
				++index;

				yield return td;
			}
		}
	}

	public static ColorDef ColorFromStuff(ThingDef stuff)
	{
		return new ColorDef
		{
			defName = stuff.defName + "_Color",
			color = stuff.stuffProps.color,
			colorType = ColorType.Misc,
			label = stuff.label,
			displayOrder = -1,
			displayInStylingStationUI = false
		};
	}

	public static TerrainDef TentTerrainFromBlueprint(
		TerrainTemplateDef tp,
		ColorDef colorDef,
		int index,
		bool hotReload = false)
	{
		TerrainDef terrainDef = TerrainDefGenerator_Carpet.CarpetFromBlueprint(tp, colorDef, index, hotReload);

		terrainDef.defName = tp.defName + colorDef.defName.Replace("_Color", "");
		terrainDef.resourcesFractionWhenDeconstructed = 0;
		  
		return terrainDef;
	}


	/// <summary>Tent specific def generation</summary>
	public static void TentDefGenerator(bool hotReload)
	{
		foreach (var td in TerrainDefGenerator_TentFloor.ImpliedTerrainDefs())
		{
			DefGenerator.AddImpliedDef(td, hotReload);
		}
	}

#elif (RELEASE_1_4)
	public static IEnumerable<TerrainDef> ImpliedTerrainDefs()
	{
		var matColors = GenStuff.AllowedStuffsFor(TentDefOf.NCS_TentPart_Floor).ToHashSet()
			.Select(stuff => ColorFromStuff(stuff))
			.ToList();

		var tentFloors = DefDatabase<ThingDef>.AllDefs
			.Where(def => def.GetCompProperties<CompProperties_TentMat>() != null)
			.Select(def => def.GetCompProperties<CompProperties_TentMat>().spawnedFloorTemplate);

		foreach (var colorDef in matColors)
		{
			DefGenerator.AddImpliedDef(colorDef);

			int index = 0;
			foreach (var item in tentFloors)
			{
				TerrainDef td = TentTerrainFromBlueprint(item, colorDef, index, false);
				++index;

				yield return td;
			}
		}
	}

	public static ColorDef ColorFromStuff(ThingDef stuff)
	{
		return new ColorDef
		{
			defName = stuff.defName + "_Color",
			color = stuff.stuffProps.color,
			colorType = ColorType.Misc,
			label = stuff.label,
			displayOrder = -1,
			displayInStylingStationUI = false
		};
	}

	public static TerrainDef TentTerrainFromBlueprint(
		TerrainTemplateDef tp,
		ColorDef colorDef,
		int index,
		bool hotReload = false)
	{
		TerrainDef terrainDef = TerrainDefGenerator_Carpet.CarpetFromBlueprint(tp, colorDef, index);

		terrainDef.defName = tp.defName + colorDef.defName.Replace("_Color", "");
		terrainDef.resourcesFractionWhenDeconstructed = 0;

		return terrainDef;
	}
#endif
}
