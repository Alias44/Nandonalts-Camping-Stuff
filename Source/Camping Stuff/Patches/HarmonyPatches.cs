using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Camping_Stuff
{
	public partial class HarmonyPatches : Mod
	{
		public HarmonyPatches(ModContentPack content) : base(content)
		{
			var harmony = new Harmony("Nandonalt_CampingStuff.main");
#if !(RELEASE_1_3 || RELEASE_1_2 || RELEASE_1_1)
			harmony.Patch(AccessTools.Method(typeof(DefGenerator), "GenerateImpliedDefs_PreResolve"), new HarmonyMethod(typeof(HarmonyPatches), nameof(TentDefGenerator)));
			harmony.Patch(AccessTools.Method(typeof(BackCompatibility), "BackCompatibleTerrainWithShortHash"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(BackCompatibleTentFloor)));
#endif
			harmony.Patch(AccessTools.Method(typeof(ThingDef), "get_CanHaveFaction"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(TentCanHaveFaction)));
			harmony.Patch(AccessTools.Method(typeof(ThingDefGenerator_Buildings), "NewBlueprintDef_Thing"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(NewBlueprintDef_Tent)));
			harmony.Patch(AccessTools.Method(typeof(Designator_Uninstall), "CanDesignateThing"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(CanDesignateThingTent)));
			harmony.Patch(AccessTools.Method(typeof(GenConstruct), "FirstBlockingThing"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(TentBlueprintRect)));
			harmony.Patch(AccessTools.Method(typeof(ScenPart_ThingCount), "PossibleThingDefs"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(TentScenario)));

#if !RELEASE_1_1
			harmony.Patch(AccessTools.Method(typeof(CaravanUIUtility), "GetTransferableCategory"), null, null, new HarmonyMethod(typeof(HarmonyPatches), nameof(TentTransferCategory)));
#endif

			harmony.Patch(AccessTools.Method(typeof(DebugThingPlaceHelper), "IsDebugSpawnable"), null, null, new HarmonyMethod(typeof(HarmonyPatches), nameof(DebugSpawn)));

			harmony.PatchAll(Assembly.GetExecutingAssembly());

			// Add a custom back compatibility to the conversion chain
			List<BackCompatibilityConverter> compatibilityConverters =
				AccessTools.StaticFieldRefAccess<List<BackCompatibilityConverter>>(typeof(BackCompatibility),
					"conversionChain");

			compatibilityConverters.Add(new BackCompatibilityConverter_LegacyTent());
#if !(RELEASE_1_3 || RELEASE_1_2 || RELEASE_1_1)
			compatibilityConverters.Add(new BackCompatibilityConverter_LegacyFloors());
#endif
		}

		/// <summary>Allows tents to be marked with a faction (why isn't there a boolean override for that?!), resolves issues with setting them as frames</summary>
		[HarmonyPostfix]
		public static void TentCanHaveFaction(ThingDef __instance, ref bool __result)
		{
			if (__instance.Equals(TentDefOf.NCS_TentBag))
			{
				__result = true;
			}
		}

		/// <summary>Assigns the tent blueprint class (without having to be flagged as constructable)</summary>
		[HarmonyPostfix]
		public static void NewBlueprintDef_Tent(ThingDef def, ref ThingDef __result)
		{
			if (def.Equals(TentDefOf.NCS_TentBag) && __result.defName.Equals(ThingDefGenerator_Buildings.BlueprintDefNamePrefix + ThingDefGenerator_Buildings.InstallBlueprintDefNamePrefix + def.defName)) // Tent bag match should only happen once, but just to be sure string match the install blueprint
			{
				__result.thingClass = typeof(TentBlueprint_Install);
				__result.drawerType = DrawerType.RealtimeOnly;
			}
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

		/// <summary>Add tent bag to scenario menu</summary>
		/// <remarks>quick and dirty hack to be repaced by custom ScenPart in future</remarks>
		[HarmonyPostfix]
		public static void TentScenario(ref IEnumerable<ThingDef> __result)
		{
			__result = __result.AddItem(TentDefOf.NCS_TentBag);
		}

		/// <summary>Adds tents in the ready state to the "Travel and Supplies" tab of the Form Caravan page</summary>
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> TentTransferCategory(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		{
			var codes = new List<CodeInstruction>(instructions);

			int insertIndex = -1;

			var tentPatchStart = ilg.DefineLabel();
			Label nextIf = new Label();

			for (int i = 0; i < codes.Count - 2; i++)
			{
				if (codes[i].opcode == OpCodes.Bne_Un_S && codes[i + 2].opcode == OpCodes.Ret)
				{
					nextIf = (Label)codes[i].operand;
					codes[i].operand = tentPatchStart;

					insertIndex = i + 3;
					break;
				}
			}

			if (insertIndex >= 0)
			{
				CodeInstruction[] newInstructions = new[]
				{
					new CodeInstruction(OpCodes.Ldarg_0).WithLabels(tentPatchStart),
					new CodeInstruction(OpCodes.Callvirt, typeof(Transferable).GetMethod("get_AnyThing")),
					new CodeInstruction(OpCodes.Call, typeof(Util).GetMethod("IsTentReady")),
					new CodeInstruction(OpCodes.Brfalse, nextIf),
					new CodeInstruction(OpCodes.Ldc_I4_2),
					new CodeInstruction(OpCodes.Ret)
				};

				codes.InsertRange(insertIndex, newInstructions);
			}

			return codes.AsEnumerable();
		}

		/// <summary>Removes errant tent states from the Debug spawn menu</summary>
		/// <remarks>This would be so much easier if the original function just had "!(def.thingClass == is MinifiedThing)"</remarks>
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> DebugSpawn(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		{
			var codes = new List<CodeInstruction>(instructions);

			int linesBefore = 2;
			int linesToCopy = 6;

			// copy !(def.thingClass == typeof (MinifiedThing) ilcode and check type of NCS_MiniTent / NCS_Tent instead
			for (int i = linesBefore; i < codes.Count - linesToCopy; i++)
			{
				if (codes[i].opcode == OpCodes.Ldtoken && codes[i].operand.Equals(typeof(MinifiedThing)))
				{
					List<CodeInstruction> newInstructions = codes.GetRange(i - linesBefore, linesToCopy);
					newInstructions[linesBefore] = new CodeInstruction(OpCodes.Ldtoken, typeof(NCS_MiniTent));

					int insertIndex = i + (linesToCopy - linesBefore);
					codes.InsertRange(insertIndex, newInstructions);

					newInstructions[linesBefore] = new CodeInstruction(OpCodes.Ldtoken, typeof(NCS_Tent));
					insertIndex += newInstructions.Count;

					codes.InsertRange(insertIndex, newInstructions);

					break;
				}
			}

			return codes.AsEnumerable();
		}
	}
}