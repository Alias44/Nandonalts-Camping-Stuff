using HarmonyLib;

using Verse;

namespace Camping_Stuff;

#if !(RELEASE_1_3 || RELEASE_1_2 || RELEASE_1_1)
public partial class HarmonyPatches
{
	/// <summary>Add tent bag to scenario menu</summary>
	/// <remarks>quick and dirty hack to be repaced by custom ScenPart in future</remarks>
	[HarmonyPostfix]
	public static void BackCompatibleTentFloor(ushort hash, ref TerrainDef __result)
	{
		if (__result == null && hash == (ushort)20659)
		{
			__result = TentDefOf.NCS_TentFloorRed;
		}
	}
}
#endif
