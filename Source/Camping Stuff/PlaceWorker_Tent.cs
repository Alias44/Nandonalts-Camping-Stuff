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
				// tents have the following
				//  -walls/ doors
				//  -roof
				//  -floor (terrain)

				// walls/ doors/ floors(?) -> check for items since spawining would wipe them out/ not make sense
				// roof check for trees
				// floors check for shorter plants


				/*CellRect occupiedRect = tent.sketch.OccupiedRect;
				CellRect rect = occupiedRect.MovedBy(loc);*/
				foreach (SketchEntity entity in tent.sketch.Entities)
				{
					CellRect cellRect = entity.OccupiedRect.MovedBy(loc);
					if (!cellRect.InBounds(map))
						return (AcceptanceReport)false;
					if (cellRect.InNoBuildEdgeArea(map))
						return (AcceptanceReport)"TooCloseToMapEdge".Translate();

					/*foreach (IntVec3 intVec3 in cellRect)
					{
						Thing item = intVec3.GetFirstItem(map);
						Building bldg = intVec3.GetFirstBuilding(map);
						Plant plant = intVec3.GetPlant(map);

						if (!entity.CanBuildOnTerrain(intVec3, map))
							return (AcceptanceReport)"CannotPlaceMonumentOnTerrain".Translate((NamedArgument)intVec3.GetTerrain(map).LabelCap);

						if(entity is SketchTerrain || entity is SketchBuildable)
						{
							if(plant!= null && plant.def.hideAtSnowDepth > 0.5)
								return (AcceptanceReport)"CannotPlaceItemOver".Translate((NamedArgument)entity.LabelCap, (NamedArgument)plant.LabelCap);
						}

						if(entity is SketchBuildable)
						{
							if(item != null && !item.GetInnerIfMinified().Equals(tent))
								return (AcceptanceReport)"CannotPlaceTentOver".Translate((NamedArgument)item.LabelCap);
							if (bldg != null) // Add exclusions for tent walls
								return (AcceptanceReport)"CannotPlaceTentOver".Translate((NamedArgument)bldg.LabelCap);
						}

						if(entity is SketchRoof)
						{
							if (plant!= null && plant.def.plant.interferesWithRoof)
								return (AcceptanceReport)"CannotPlaceTentOver".Translate((NamedArgument)plant.LabelCap);
						}
					}*/
				}
			}

			return (AcceptanceReport)true;
		}
	}
}
