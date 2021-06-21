using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Camping_Stuff
{
	//[StaticConstructorOnStartup]
	public class HarmonyPatches : Mod
	{
		public HarmonyPatches(ModContentPack content) : base(content)
		{
			var harmony = new Harmony("Nandonalt_CampingStuff.main");
			harmony.Patch(AccessTools.Method(typeof(ThingDefGenerator_Buildings), "NewBlueprintDef_Thing"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(NewBlueprintDef_Tent)));
			harmony.Patch(AccessTools.Method(typeof(GenConstruct), "FirstBlockingThing"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(TentBlueprintRect)));

			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

		[HarmonyPostfix]
		public static void NewBlueprintDef_Tent(ThingDef def, ref ThingDef __result) // Assigns the tent blueprint class (without having to be flagged as constructable)
		{
			if (def.Equals(TentDefOf.NCS_TentBag) && __result.defName.Equals(ThingDefGenerator_Buildings.BlueprintDefNamePrefix + ThingDefGenerator_Buildings.InstallBlueprintDefNamePrefix + def.defName)) // Tent bag match should only happen once, but just to be sure sting match the install blueprint
			{
				__result.thingClass = typeof(TentBlueprint_Install);
			}
		}

		[HarmonyPostfix]
		public static void TentBlueprintRect(Thing constructible, ref Thing __result) // This is more or less FirstBlockingThing, but with the dynamic rect of the tent to be installed
		{
			if (constructible is TentBlueprint_Install tbi && tbi.CustomRectForSelector is CellRect cr)
			{
				foreach (IntVec3 c in cr)
				{
					List<Thing> thingList = c.GetThingList(constructible.Map);
					for (int index = 0; index < thingList.Count; ++index)
					{
						Thing t = thingList[index];
						if (GenConstruct.BlocksConstruction(constructible, t) && t != tbi.MiniToInstallOrBuildingToReinstall && !(t is Pawn p && p.IsColonistPlayerControlled))
							__result = t;
 					}
				}
			}
		}
	}
}