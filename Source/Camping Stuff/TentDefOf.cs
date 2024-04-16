using RimWorld;
using Verse;

namespace Camping_Stuff
{
	[DefOf]
	public static class TentDefOf
	{
		public static ThingDef NCS_TentDoor;
		public static ThingDef NCS_TentWall;

		public static ThingDef NCS_TentPart_Cover_Small;
		public static ThingDef NCS_TentPart_Cover_Med;
		public static ThingDef NCS_TentPart_Cover_Large;
		public static ThingDef NCS_TentPart_Cover_Long;

		public static ThingDef NCS_TentPart_Pole;
		public static ThingDef NCS_TentPart_Floor;

		public static ThingDef NCS_TentBag;
		public static ThingDef NCS_MiniTentBag;

		public static RoofDef NCS_TentRoof;

		//public static TerrainDef NCS_TentFloor;
#if !(RELEASE_1_3 || RELEASE_1_2 || RELEASE_1_1)
		public static TerrainDef NCS_TentFloorRed;
#endif

		public static JobDef NCS_PackBag;
		public static JobDef NCS_UnpackBag;
		public static JobDef NCS_RepairPart;
		public static JobDef NCS_RepairCoverInBag;
		public static JobDef NCS_RepairFloorInBag;
	}
}
