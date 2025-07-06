using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Camping_Stuff;

/// <remarks>Because terrarin/ flooring isn't stuffed</remarks>
/// <see cref="RimWorld.TerrainDefGenerator_Carpet"/>
public static class TerrainDefGenerator_TentFloor
{
#if !(RELEASE_1_3 || RELEASE_1_2 || RELEASE_1_1)
	public static (IEnumerable<ColorDef> colors, IEnumerable<TerrainDef> terrains) ImpliedTerrainDefs(bool hotReload = false)
	{
		var terrainStuff = DefDatabase<ThingDef>.AllDefs
			.Where(def => def.HasComp<TentMatComp>() && def.GetCompProperties<CompProperties_TentMat>().UsesTemplate)
			.GroupBy(def => def.GetCompProperties<CompProperties_TentMat>().spawnedFloorTemplate, tentPart => GenStuff.AllowedStuffsFor(tentPart), (template, stuffs) => new
			{
				Key = template,
				Stuff = stuffs.SelectMany(stuffCategories => stuffCategories).ToHashSet()
			});

		var terrainColors = terrainStuff.SelectMany(obj => obj.Stuff)
			.ToHashSet()
			.ToDictionary(stuff => stuff, ColorDefFromStuff);

		var terrains = terrainStuff.SelectMany(obj => obj.Stuff.Select((stuff, idx) => TentTerrainFromBlueprint(obj.Key, terrainColors[stuff], idx, hotReload)));

		return (terrainColors.Values, terrains);
	}

	public static ColorDef ColorDefFromStuff(ThingDef stuff)
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

#if RELEASE_1_4
		TerrainDef terrainDef = TerrainDefGenerator_Carpet.CarpetFromBlueprint(tp, colorDef, index);
#else
		TerrainDef terrainDef = TerrainDefGenerator_Carpet.CarpetFromBlueprint(tp, colorDef, index, hotReload);
#endif

		terrainDef.defName = tp.defName + colorDef.defName.Replace("_Color", "");
		terrainDef.resourcesFractionWhenDeconstructed = 0;

		return terrainDef;
	}
#endif
}
