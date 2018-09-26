using System.Collections.Generic;
using System;
using RimWorld;
using Verse;

namespace Nandonalt_CampingStuff
{
	public class CompProperties_Tent : CompProperties_Targetable
	{
		public List<string> tentLayoutSouth = new List<string>();

		public CompProperties_Tent()
		{
			this.compClass = typeof(CompTargetable_Tent);
		}
	}
}
