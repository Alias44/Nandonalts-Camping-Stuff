using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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

			harmony.Patch(AccessTools.Method(typeof(Designator_Uninstall), "DesignateThing"), null, null, new HarmonyMethod(typeof(HarmonyPatches), nameof(DesignateThingTranspiler)));
			harmony.Patch(AccessTools.Method(typeof(Designator_Uninstall), "CanDesignateThing"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(CanDesignateThingTent)));

			harmony.Patch(AccessTools.Method(typeof(GenConstruct), "FirstBlockingThing"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(TentBlueprintRect)));

			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

		/// <summary>Assigns the tent blueprint class (without having to be flagged as constructable)</summary>
		[HarmonyPostfix]
		public static void NewBlueprintDef_Tent(ThingDef def, ref ThingDef __result)
		{
			if (def.Equals(TentDefOf.NCS_TentBag) && __result.defName.Equals(ThingDefGenerator_Buildings.BlueprintDefNamePrefix + ThingDefGenerator_Buildings.InstallBlueprintDefNamePrefix + def.defName)) // Tent bag match should only happen once, but just to be sure sting match the install blueprint
			{
				__result.thingClass = typeof(TentBlueprint_Install);
			}
		}

		///<summary>
		/// Converts the second if statement of Designator_Uninstall.DesignateThing to:
		/// (DebugSettings.godMode || (double) t.GetStatValue(StatDefOf.WorkToBuild) == 0.0 || !(t is NCS_Tent) || t.def.IsFrame)
		/// </summary>
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> DesignateThingTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = new List<CodeInstruction>(instructions);
			
			CodeInstruction jumpLoc = null;

			int insertIndex = -1;


			for (int i = 0; i < codes.Count - 2; i++)
			{
				if (codes[i].opcode == OpCodes.Call && codes[i + 2].opcode == OpCodes.Beq_S) // look for (double) t.GetStatValue(StatDefOf.WorkToBuild) == 0.0
				{
					insertIndex = i + 3;
				}

				if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand.ToString().Equals("Boolean get_IsFrame()")) // look for the fail jump of t.def.IsFrame
				{
					jumpLoc = new CodeInstruction(codes[i + 1]);
				}
			}

			if (insertIndex > -1 && jumpLoc != null)
			{
				CodeInstruction loadT = new CodeInstruction(OpCodes.Ldarg_1);
				CodeInstruction isClass = new CodeInstruction(OpCodes.Isinst, typeof(NCS_Tent));

				jumpLoc.opcode = OpCodes.Brtrue;

				codes.InsertRange(insertIndex, new[] {loadT, isClass, jumpLoc});
			}

			return codes.AsEnumerable();
		}


		/// <summary>Allows tents to be selected by the uninstall designator</summary>
		[HarmonyPostfix]
		public static void CanDesignateThingTent(Designator_Uninstall __instance, Thing t, ref AcceptanceReport __result)
		{
			if (t is NCS_Tent)
			{
				__result = __instance.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null ? (AcceptanceReport)false : (AcceptanceReport)true;
			}
		}

		/// <summary>Tent specific adjustments to FirstBlockingThing</summary>
		[HarmonyPostfix]
		public static void TentBlueprintRect(Thing constructible, ref Thing __result) 
		{
			if (constructible is TentBlueprint_Install tbi && tbi.ThingToInstall is NCS_Tent tent)
			{
				__result = null;

				foreach (var se in tent.sketch.Entities)
				{
					foreach (var t in se.OccupiedRect.MovedBy(constructible.Position).SelectMany(c => c.GetThingList(constructible.Map)))
					{
						if (
							(se is SketchRoof && ( //allow early fail out so edge cases only have to be checked once (cells in SketchRoof will be a super set of the cells from other entities)
								 t is Plant || // only check plants under roofs, other stuff will be checked again if it collides with buildables (walls/ doors)
								 !(t is Pawn p && p.IsColonistPlayerControlled)
							)) &&
							t != tbi.MiniToInstallOrBuildingToReinstall &&
							!(t.TryGetComp<TentSpawnedComp>() is TentSpawnedComp tsc && tsc.tent.Equals(tent)) && // Ignore buildings spawned by the tent (since they'll get vanished)
							GenConstruct.BlocksConstruction(constructible, t)
						)
						{
 							__result = t;
							return;
						}
					}
				}
			}
		}
	}
}