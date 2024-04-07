using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Camping_Stuff
{
	public partial class HarmonyPatches
	{
#if !(RELEASE_1_3 || RELEASE_1_2 || RELEASE_1_1)
		/// <summary>Tent specific def generation</summary>
		public static void TentDefGenerator(bool hotReload)
		{
			var matColors = GenStuff.AllowedStuffsFor(TentDefOf.NCS_TentPart_Floor).ToHashSet()
				.Select(stuff => ColorFromStuff(stuff))
				.ToList();

			var tentFloors = DefDatabase<ThingDef>.AllDefs
				.Where(def => def.HasComp<TentMatComp>())
				.Select(def => def.GetCompProperties<CompProperties_TentMat>().spawnedFloorTemplate);

			matColors.ForEach(colorDef =>
			{
				DefGenerator.AddImpliedDef(colorDef, hotReload);

				int index = 0;
				foreach (var item in tentFloors)
				{
					TerrainDef td = TentTerrainFromBlueprint(item, colorDef, index, false);
					++index;

					DefGenerator.AddImpliedDef(td, hotReload);
				}
			});
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
#endif
	}
}
