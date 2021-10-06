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
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			if (!(thing is NCS_Tent tent))
				return;
			tent.DrawGhost(center, true, rot);
		}

		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			if(thing is NCS_Tent tent)
			{
				bool existingFloor = tent.CanSafelySpawnFloor(loc, map);
				foreach (SketchEntity entity in tent.sketch.Entities)
				{
					CellRect cellRect = entity.OccupiedRect.MovedBy(loc);
					if (!cellRect.InBounds(map))
						return (AcceptanceReport)false;
					if (cellRect.InNoBuildEdgeArea(map))
						return (AcceptanceReport)"TooCloseToMapEdge".Translate();

					if(!entity.CanBuildOnTerrain(entity.pos + loc, map) && !(entity is SketchTerrain && existingFloor)) // ignore floors if they're going to get skipped on spawn
					{
						if(entity is SketchBuildable sb)
						{
							return (AcceptanceReport)"TerrainCannotSupport_TerrainAffordance".Translate(sb.Buildable, sb.Buildable.GetTerrainAffordanceNeed(sb.Stuff));
						}
						return (AcceptanceReport)"TerrainCannotSupport".Translate(tent);
					}
				}
			}

			return (AcceptanceReport)true;
		}
	}
}
