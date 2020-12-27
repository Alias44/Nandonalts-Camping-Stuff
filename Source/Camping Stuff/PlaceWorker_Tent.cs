using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Camping_Stuff
{
	class PlaceWorker_Tent : PlaceWorker
	{
		//private List<Thing> tentThings = new List<Thing>();

		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			TentBagComp bagComp = thing.TryGetComp<TentBagComp>();
			if (bagComp == null)
				return;
			bagComp.DrawGhost_NewTmp(center, true, rot);
		}
	}
}
