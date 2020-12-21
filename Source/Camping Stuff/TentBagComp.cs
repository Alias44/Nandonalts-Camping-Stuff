using System;
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
	public class TentBagComp : ThingComp //(Thing)
	{
		ThingComp_MiniTentBag Inner => this.parent.SpawnedParentOrMe.TryGetComp<ThingComp_MiniTentBag>();

		int poleCount = 0;
		public int maxPoles = DefDatabase<ThingDef>.AllDefs.Where(d => d.HasComp(typeof(TentCoverComp))).Min(cover => cover.GetCompProperties<CompProperties_TentCover>().numPoles);

		//public Dictionary<ThingDef, Thing> poles = new Dictionary<ThingDef, Thing>();
		public List<Thing> poles = new List<Thing>();
		public Thing cover;
		public Thing floor;

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

			UpdateStats();
		}

		private void Eject(Thing t)
		{
			if (t != null)
			{
				GenPlace.TryPlaceThing(t, this.parent.PositionHeld, this.parent.MapHeld, ThingPlaceMode.Near);
			}
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

		private void UpdateStats()
		{
			if (cover == null)
			{
				maxPoles = DefDatabase<ThingDef>.AllDefs.Where(d => d.HasComp(typeof(TentCoverComp))).Min(cover => cover.GetCompProperties<CompProperties_TentCover>().numPoles);
			}
			else
			{
				maxPoles = cover.TryGetComp<TentCoverComp>().Props.numPoles;
			}

			AdjustPoles();
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
			if (cover != null && poleCount >= cover.TryGetComp<TentCoverComp>().Props.numPoles)
			{
				return "Ready to deploy";
			}

			return "Not ready to deploy";
			/*
			"Cover: {0}\nPoles: {1}\nFloor: {2}".Formatted(
			cover == null ? "<color=red>None</color>" : cover.LabelShortCap,
			poles == null ? "<color=red>None</color>" : poles.Count.ToString,
			floor == null ? "<color=red>None</color>" : cover.LabelShortCap);*/
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

			if (Scribe.mode != LoadSaveMode.Saving)
			{
				if (poles == null)
				{
					poles = new List<Thing>();
				}
			}
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			JobDef jd = DefDatabase<JobDef>.GetNamed("NCS_UnpackBag");

			if (selPawn.CanReserveAndReach(this.parent, PathEndMode.Touch, Danger.Deadly))
			{
				if (cover != null)
				{
					yield return new FloatMenuOption("Unpack " + cover.LabelCap, delegate
					{
						jd.driverClass = typeof(JobDriver_UnpackBagCover);
						Job j = JobMaker.MakeJob(jd, parent);

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
							Job j = JobMaker.MakeJob(jd, parent, pole);

							selPawn.jobs.TryTakeOrderedJob(j);
						});
					}

					yield return new FloatMenuOption("Unpack all poles (x" + poleCount + ")", delegate
					{
						jd.driverClass = typeof(JobDriver_UnpackBagAllPoles);
						Job j = JobMaker.MakeJob(jd, parent);

						selPawn.jobs.TryTakeOrderedJob(j);
					});
				}

				if (floor != null)
				{
					yield return new FloatMenuOption("Unpack " + floor.LabelCap, delegate
					{
						jd.driverClass = typeof(JobDriver_UnpackBagFloor);
						Job j = JobMaker.MakeJob(jd, parent);

						selPawn.jobs.TryTakeOrderedJob(j);
					});
				}

				if (cover != null || (poles != null && poles.Count > 0) || floor != null)
				{
					yield return new FloatMenuOption("Unpack all parts", delegate
					{
						jd.driverClass = typeof(JobDriver_UnpackBagAll);
						Job j = JobMaker.MakeJob(jd, parent);

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
