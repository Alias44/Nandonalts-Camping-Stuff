using Verse;

namespace Camping_Stuff
{
	public class JobDriver_RepairCoverInBag : JobDriver_RepairPart
	{
		protected override Thing Part => MiniBag.Bag.Cover;
	}

	public class JobDriver_RepairFloorInBag : JobDriver_RepairPart
	{
		protected override Thing Part => MiniBag.Bag.Floor;
	}
}
