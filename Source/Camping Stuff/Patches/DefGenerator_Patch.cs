using System.Linq;

using RimWorld;
using Verse;

namespace Camping_Stuff;

public partial class HarmonyPatches
{
#if !(RELEASE_1_4 || RELEASE_1_3 || RELEASE_1_2 || RELEASE_1_1)
	/// <summary>Tent specific def generation</summary>
	public static void TentDefGenerator(bool hotReload)
	{
		foreach (var td in TerrainDefGenerator_TentFloor.ImpliedTerrainDefs())
		{
			DefGenerator.AddImpliedDef(td, hotReload);
		}
	}

#elif (RELEASE_1_4)
	/// <summary>Tent specific def generation</summary>
	public static void TentDefGenerator()
	{
		foreach (var td in TerrainDefGenerator_TentFloor.ImpliedTerrainDefs())
		{
			DefGenerator.AddImpliedDef(td);
		}
	}
#endif
}
