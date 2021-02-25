using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Camping_Stuff
{
	class Designator_Tent : Designator_Place
	{
		public override BuildableDef PlacingDef => throw new NotImplementedException();

		public override AcceptanceReport CanDesignateCell(IntVec3 loc)
		{
			throw new NotImplementedException();
		}
	}
}
