﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using RimWorld;
using System.Diagnostics.Eventing.Reader;
using System.Reflection;
using UnityEngine;

namespace Camping_Stuff
{
	public enum TentPart
	{
		other = -1,
		bag,
		pole,
		cover,
		floor
	}

	public class TentBagComp : ThingComp //(Thing)
	{
		ThingComp_MiniTentBag Inner => this.parent.SpawnedParentOrMe.TryGetComp<ThingComp_MiniTentBag>();

		int poleCount = 0;
		public int maxPoles = DefDatabase<ThingDef>.AllDefs.Where(d => d.HasComp(typeof(TentCoverComp))).Min(cover => cover.GetCompProperties<CompProperties_TentCover>().numPoles);

		//public Dictionary<ThingDef, Thing> poles = new Dictionary<ThingDef, Thing>();
		private List<Thing> poles = new List<Thing>();
		private Thing cover;
		private Thing floor;

		private Sketch sketch = new Sketch();

		public void PackPart(Thing part)
		{
			TentPart partType = TentPart.other;

			if (part.TryGetComp<CompUsable_TentPart>() != null)
			{
				partType = part.TryGetComp<CompUsable_TentPart>().Props.partType;
			}

			switch (partType)
			{
				case TentPart.pole:
					int typeIndex = poles.FindIndex(p => p.Stuff.Equals(part.Stuff));

					if (typeIndex >= 0)
					{
						poles[typeIndex].stackCount += part.stackCount;
					}
					else
					{
						poles.Add(part);
					}
					poleCount += part.stackCount; //Excess poles will be ejected as part of the update function
					break;

				case TentPart.cover:
					ReplaceCover(part);
					break;

				case TentPart.floor:
					ReplaceFloor(part);
					break;
			}

			if (part.Spawned == true)
			{
				part.DeSpawn(DestroyMode.Vanish);
			}
		}

		private void Eject(Thing t)
		{
			if (t != null)
			{
				GenPlace.TryPlaceThing(t, this.parent.PositionHeld, this.parent.MapHeld, ThingPlaceMode.Near);
			}
		}

		public bool Ready()
		{
			return cover != null && poleCount >= cover.TryGetComp<TentCoverComp>().Props.numPoles;
		}

		public void ReplaceCover(Thing newCover)
		{
			Thing ret;

			if (this.cover != null)
			{
				ret = this.cover;
				//ret.stackCount = newCover.stackCount;
				Eject(ret);
			}

			this.cover = newCover;

			if (this.cover == null)
			{
				maxPoles = DefDatabase<ThingDef>.AllDefs.Where(d => d.HasComp(typeof(TentCoverComp))).Min(cover => cover.GetCompProperties<CompProperties_TentCover>().numPoles);
			}
			else
			{
				maxPoles = cover.TryGetComp<TentCoverComp>().Props.numPoles;
			}

			AdjustPoles();
			UpdateSketch();
		}

		public void EjectCover()
		{
			ReplaceCover(null);
		}

		private void ReplaceFloor(Thing newFloor)
		{
			Thing ret;

			if (this.floor != null)
			{
				ret = this.floor;
				//ret.stackCount = newCover.stackCount;
				Eject(ret);
			}

			this.floor = newFloor;
		}

		public void EjectFloor()
		{
			ReplaceFloor(null);
		}

		public void EjectPole(int poleIndex)
		{
			EjectPole(poleIndex, poles[poleIndex].stackCount);
		}

		public void EjectPole(Thing pole)
		{
			for (int i = 0; i < poles.Count; i++)
			{
				if (poles[i].Equals(pole))
				{
					EjectPole(i);
					break;
				}
			}
		}

		private void EjectPole(int poleIndex, int qtyOut)
		{
			Thing src = poles[poleIndex];
			Thing polesOut = src.SplitOff(qtyOut);

			if (src.Equals(polesOut) || src.stackCount == 0) // splitoff returns a clone if the qty == the satck count
			{
				poles.RemoveAt(poleIndex);
			}
			else
			{
				poles[poleIndex] = src;
			}

			Eject(polesOut);

			poleCount -= qtyOut;
		}

		public void EjectAllPoles()
		{
			for (int i = 0; i < poles.Count;) // no modifier op as count should zero out
			{
				EjectPole(i);
			}
		}

		public void EjectAll()
		{
			EjectCover();
			EjectAllPoles();
			EjectFloor();
		}

		private void AdjustPoles()
		{
			for (int i = 0; i < poles.Count && poleCount > maxPoles; i++)
			{
				EjectPole(i, Math.Min(poleCount - maxPoles, poles[i].stackCount));
			}
		}

		private void UpdateSketch()
		{
			if (cover == null)
			{
				return;
			}

			sketch = new Sketch();
			sketch.Rotate(Rot4.South);

			CompProperties_TentCover coverProps = this.cover.TryGetComp<TentCoverComp>().Props;

			for (int r = 0; r < coverProps.layoutS.Count; r++)
			{
				for (int c = 0; c < coverProps.layoutS[r].Count; c++)
				{
					ThingDef thing = null;
					ThingDef stuff = cover.Stuff; //this.parent.Stuff
					IntVec3 loc = new IntVec3(c, 0, r) - coverProps.center;

					switch (coverProps.layoutS[r][c])
					{
						case TentLayout.other:
							break;
						case TentLayout.empty:
							break;
						case TentLayout.wall:
							thing = TentDefOf.NCS_TentWall;
							break;
						case TentLayout.door:
							thing = TentDefOf.NCS_TentDoor;
							break;
						case TentLayout.pole:
							//thing = TentDefOf.NCS_TentBag;
							break;
						case TentLayout.roofedEmpty:
							break;
						default:
							break;
					}

					if (thing != null) {
						sketch.AddThing(thing, loc, Rot4.South, stuff); 
					}
				}
			}
			
			//sketch.MoveOccupiedCenterToZero(); // does a thing?
		}

		public void DrawGhost_NewTmp(IntVec3 at, bool placingMode, Rot4 rotation)
		{
			if (!Ready())
			{
				return;
			}
			if(sketch.Empty)
			{
				UpdateSketch();
			}

			sketch.Rotate(rotation);

			Func<SketchEntity, IntVec3, List<Thing>, Map, bool> validator = (Func<SketchEntity, IntVec3, List<Thing>, Map, bool>)null;
			if (placingMode)
			{
				//validator = (Func<SketchEntity, IntVec3, List<Thing>, Map, bool>)((entity, offset, things, map) => MonumentMarkerUtility.GetFirstAdjacentBuilding(entity, offset, things, map) == null);
			}

			sketch.DrawGhost_NewTmp(at, Sketch.SpawnPosType.Unchanged, false, null, validator);
		}

		public float GetValue(StatDef sd)
		{
			if (sd == null)
			{
				return 0;
			}

			float mass = 0;

			if (cover != null)
			{
				mass += cover.GetStatValue(sd);
			}

			if (poles != null && poles.Count != 0)
			{
				//defDesc += "Poles:\n";
				foreach (Thing pole in poles)
				{
					mass += pole.GetStatValue(sd);
				}
			}

			if (floor != null)
			{
				mass += floor.GetStatValue(sd);
			}
			return mass;
		}

		public override string CompInspectStringExtra()
		{
			if (Ready())
			{
				return "Ready to deploy";
			}

			return "Not ready to deploy";
		}

		public override string GetDescriptionPart()
		{
			string defDesc = ""; //parent.DescriptionDetailed;

			if (cover != null)
			{
				defDesc += "Cover:\n\t" + cover.LabelCap + "\n\n";
			}

			if (poles != null && poles.Count != 0)
			{
				defDesc += "Poles:\n";
				foreach (Thing pole in poles)
				{
					defDesc += "\t" + pole.LabelCap + (pole.stackCount == 1 ? " x1" : "") + "\n";
				}
				defDesc += "\n";
			}

			if (floor != null)
			{
				defDesc += "Floor:\n\t" + floor.LabelCap + "\n";
			}

			return defDesc;
		}

		public string GetExplanation(StatDef sd, Func<float, string> format = null)
		{
			if (sd == null)
			{
				return "";
			}

			if (format == null)
			{
				format = delegate (float f)
				{
					return f.ToString();
				};
			}

			string massDesc = "";

			if (cover != null)
			{
				massDesc += cover.LabelCap + ": " + format(cover.GetStatValue(sd)) + "\n";
			}

			if (poles != null && poles.Count != 0)
			{
				//defDesc += "Poles:\n";
				foreach (Thing pole in poles)
				{
					massDesc += pole.LabelCap + ": " + format(pole.GetStatValue(sd)) + "\n";
				}
				massDesc += "\n";
			}

			if (floor != null)
			{
				massDesc += floor.LabelCap + ": " + format(floor.GetStatValue(sd)) + "\n";
			}

			return massDesc;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();

			Scribe_Values.Look(ref poleCount, "poleCount");
			Scribe_Deep.Look(ref cover, "tentCover");
			Scribe_Deep.Look(ref floor, "tentFloor");
			Scribe_Collections.Look<Thing>(ref poles, "poleList", LookMode.Deep);
			//Scribe_Deep.Look<Sketch>(ref this.sketch, "sketch"); //Saving the sketch so the tent can still be packed if the layout changed

			if (Scribe.mode != LoadSaveMode.Saving)
			{
				if (poles == null)
				{
					poles = new List<Thing>();
				}

				if (sketch == null)
				{
					UpdateSketch();
				}
			}
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			JobDef jd = DefDatabase<JobDef>.GetNamed("NCS_UnpackBag");
			Thing target = this.parent.SpawnedParentOrMe;

			if (selPawn.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly))
			{
				if (cover != null)
				{
					yield return new FloatMenuOption("Unpack " + cover.LabelCap, delegate
					{
						jd.driverClass = typeof(JobDriver_UnpackBagCover);
						Job j = JobMaker.MakeJob(jd, target);

						selPawn.jobs.TryTakeOrderedJob(j);
					});
				}

				if (poles != null && poles.Count > 0)
				{
					foreach (Thing pole in poles)
					{
						yield return new FloatMenuOption("Unpack " + pole.LabelCap + (pole.stackCount == 1 ? " x1" : ""), delegate
						{
							jd.driverClass = typeof(JobDriver_UnpackBagPole);
							Job j = JobMaker.MakeJob(jd, target, pole);

							selPawn.jobs.TryTakeOrderedJob(j);
						});
					}

					yield return new FloatMenuOption("Unpack all poles (x" + poleCount + ")", delegate
					{
						jd.driverClass = typeof(JobDriver_UnpackBagAllPoles);
						Job j = JobMaker.MakeJob(jd, target);

						selPawn.jobs.TryTakeOrderedJob(j);
					});
				}

				if (floor != null)
				{
					yield return new FloatMenuOption("Unpack " + floor.LabelCap, delegate
					{
						jd.driverClass = typeof(JobDriver_UnpackBagFloor);
						Job j = JobMaker.MakeJob(jd, target);

						selPawn.jobs.TryTakeOrderedJob(j);
					});
				}

				if (cover != null || (poles != null && poles.Count > 0) || floor != null)
				{
					yield return new FloatMenuOption("Unpack all parts", delegate
					{
						jd.driverClass = typeof(JobDriver_UnpackBagAll);
						Job j = JobMaker.MakeJob(jd, target);

						selPawn.jobs.TryTakeOrderedJob(j);
					});
				}
			}
		}
	}

	public class CompProperties_TentBag : CompProperties //(Def)
	{
		public CompProperties_TentBag()
		{
			this.compClass = typeof(TentBagComp);
		}
	}
}