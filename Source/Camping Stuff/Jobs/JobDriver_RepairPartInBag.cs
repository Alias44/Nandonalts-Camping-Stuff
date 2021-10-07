using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

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
