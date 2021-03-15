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
			if (!(thing is NCS_Tent tent))
				return;
			tent.DrawGhost_NewTmp(center, true, rot);
		}

		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			if(thing is NCS_Tent tent)
			{
				CellRect occupiedRect = tent.sketch.OccupiedRect;
				CellRect rect = occupiedRect.MovedBy(loc);
				foreach (SketchEntity entity in tent.sketch.Entities)
				{
					occupiedRect = entity.OccupiedRect;
					CellRect cellRect = occupiedRect.MovedBy(loc);
					if (!cellRect.InBounds(map))
						return (AcceptanceReport)false;
					if (cellRect.InNoBuildEdgeArea(map))
						return (AcceptanceReport)"TooCloseToMapEdge".Translate();
					foreach (IntVec3 intVec3 in cellRect)
					{
						if (!entity.CanBuildOnTerrain(intVec3, map))
							return (AcceptanceReport)"CannotPlaceMonumentOnTerrain".Translate((NamedArgument)intVec3.GetTerrain(map).LabelCap);
					}
				}
			}

			return (AcceptanceReport)true;
		}
	}
}
