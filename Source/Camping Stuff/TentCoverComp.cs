using Verse;
using System;
using System.Collections.Generic;

namespace Camping_Stuff
{
	/*public enum TentLayout
	{
		other = -1,
		empty,
		wall,
		door,
		pole,
		roofedEmpty,
	}*/

	public class TentCoverComp : ThingComp //(Thing)
	{
		public CompProperties_TentCover Props => (CompProperties_TentCover)this.props;
	}

	public class CompProperties_TentCover : CompProperties //(Def)
	{
		public int numPoles;
		public List<string> tentLayoutSouth = new List<string>();


		public CompProperties_TentCover()
		{
			this.compClass = typeof(TentCoverComp);
		}

		public CompProperties_TentCover(Type compClass)
		{
			this.compClass = compClass;
		}
	}
}