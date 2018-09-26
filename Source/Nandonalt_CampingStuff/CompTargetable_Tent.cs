using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace Nandonalt_CampingStuff
{
	public class CompTargetable_Tent : CompTargetable
	{
		int numRows;
		int numCols;

		int supportPosSX;
		int supportPosSY;

		private Thing target;
		private LocalTargetInfo targetInfo1;
		private IntVec3 targetPos;
		public Rot4 placingRot = Rot4.North;
		public Rot4 lastRot;

		/*int[,] array2D = new int[7, 7] {
			{ 0, 1, 1, 1, 1, 1, 0 },
			{ 1, 0, 0, 0, 0, 0, 1 },
			{ 1, 0, 0, 0, 0, 0, 1 },
			{ 1, 0, 0, 3, 0, 0, 1 },
			{ 1, 0, 0, 0, 0, 0, 1 },
			{ 1, 0, 0, 0, 0, 0, 1 },
			{ 0, 1, 1, 2, 1, 1, 0 } };*/

		int[,] lines;

		int[,] line_n;
		int[,] line_e;
		int[,] line_s;
		int[,] line_w;

		public List<IntVec3> wallCells = new List<IntVec3>();
		public List<IntVec3> doorCells = new List<IntVec3>();
		public List<IntVec3> roofCells = new List<IntVec3>();
		public IntVec3 supportCell;

		public new CompProperties_Tent Props
		{
			get
			{
				return (CompProperties_Tent)this.props;
			}
		}

		public void BuildRots (CompTargetable_Tent t)
		{
			List<List<int>> tmpSouth = new List<List<int>>();

			if (line_s == null)
			{
				//Parse tent shape loaded from xml
				for (int i = 0; i < t.Props.tentLayoutSouth.Count(); i++)
				{
					List<String> splitRow = t.Props.tentLayoutSouth[i].Split(',').ToList();

					List<int> intRow = splitRow.ConvertAll(int.Parse);
					tmpSouth.Add(intRow);
				}
			}

			//Find tent size
			numRows = tmpSouth.Count();
			numCols = 0;

			for (int i = 0; i < numRows; i++)
			{
				int rowLen = tmpSouth[i].Count();
				if (numCols < rowLen)
				{
					numCols = rowLen;
				}
			}

			//Build South orientation
			if (tmpSouth != null && line_s == null)
			{
				line_s = new int[numRows, numCols];

				for (int r = 0; r < numRows; r++)
				{
					for (int c = 0; c < numCols; c++)
					{
						line_s[r, c] = tmpSouth[r].ElementAt(c);

						if (line_s[r, c] == 3)
						{
							supportPosSX = c;
							supportPosSY = r;
						}
					}
				}
			}

			// Variables for reverse traversing arrays
			int numRowIndex = numRows - 1;
			int numColIndex = numCols - 1;

			//Build West orientation
			if (line_w == null)
			{
				line_w = new int[numCols, numRows];
				for (int c = 0; c < numCols; c++)
				{
					for (int r = numRowIndex; r >= 0; r--)
					{
						line_w[c, numRowIndex - r] = line_s[r, c];

					}
				}
			}

			//Build North orientation
			if (line_n == null)
			{
				//North
				line_n = new int[numRows, numCols];
				//either count down on both
				for (int r = numRowIndex; r >= 0; r--)
				{
					for (int c = numCols - 1; c >= 0; c--)
					{
						line_n[numRowIndex - r, numColIndex - c] = line_s[r, c];
					}
				}
			}

			//Build East orientation
			if (line_e == null) // top to bottom is flipped
			{
				//East
				line_e = new int[numCols, numRows];
				//outer column for- count down
				//inner row for
				for (int c = numColIndex; c >= 0; c--)
				{
					for (int r = 0; r < numRows; r++)
					{
						line_e[numColIndex - c, r] = line_s[r, c];
					}
				}
			}
		}

		public void StringToLines (CompTargetable_Tent t, Rot4 rot)
		{
			CompProperties_Tent props = this.Props;

			if (line_n == null || line_e == null || line_s == null || line_w == null)
			{
				BuildRots(t);
			}

			if (rot == Rot4.South)
			{
				lines = line_s;
			}
			else if (rot == Rot4.West)
			{
				lines = line_w;
			}
			else if (rot == Rot4.North)
			{
				lines = line_n;
			}
			else if(rot == Rot4.East)
			{
				lines = line_e;
			}

			this.lastRot = rot;
		}

		private int tick = 0;

		public void HandleRotation ()
		{
			RotationDirection rotationDirection = RotationDirection.None;

			if (KeyBindingDefOf.Designator_RotateRight.JustPressed)
			{
				rotationDirection = RotationDirection.Clockwise;
			}

			if (KeyBindingDefOf.Designator_RotateLeft.JustPressed)
			{
				rotationDirection = RotationDirection.Counterclockwise;
			}

			if (rotationDirection == RotationDirection.Clockwise)
			{
				if (tick >= 1)
				{
					SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
					this.placingRot.Rotate(RotationDirection.Clockwise);
					tick = 0;

				}
				else tick++;
			}
			else if (rotationDirection == RotationDirection.Counterclockwise)
			{
				if (tick >= 1)
				{
					SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
					this.placingRot.Rotate(RotationDirection.Counterclockwise);
					tick = 0;
				}
				else tick++;

			}
		}

		//Defien rectangle rotated about the center post for each orientation
		public CellRect MakeRect (IntVec3 c)
		{
			CellRect rect;

			if (this.placingRot == Rot4.North)
			{
				rect = new CellRect(c.x - (numCols - supportPosSX - 1), c.z - (numRows - supportPosSY - 1), numCols, numRows);
			}
			else if (this.placingRot == Rot4.East)
			{
				rect = new CellRect(c.x - supportPosSY, c.z - supportPosSX, numRows, numCols);
			}
			else if (this.placingRot == Rot4.South)
			{
				rect = new CellRect(c.x - supportPosSX, c.z - supportPosSY, numCols, numRows);
			}
			else if (this.placingRot == Rot4.West)
			{
				rect = new CellRect(c.x - (numRows - supportPosSY - 1), c.z - (numCols - supportPosSX - 1), numRows, numCols);
			}
			else
			{
				rect = new CellRect();
			}
			return rect;
		}

		public bool GetPlacements (IntVec3 c)
		{
			StringToLines(this, this.placingRot);

			List<int> cells = new List<int>();
			IntVec3 temp_supportCell = new IntVec3(0, 0, 0);
			List<IntVec3> temp_wallCells = new List<IntVec3>();
			List<IntVec3> temp_doorCells = new List<IntVec3>();
			List<IntVec3> temp_roofCells = new List<IntVec3>();
			CellRect cellRect = MakeRect(c);


			foreach (int val in lines)
			{
				cells.Add(val);
			}

			int i = 0;
			foreach (IntVec3 current in cellRect.Cells)
			{
				switch (cells[i])
				{
					case 1:
						temp_wallCells.Add(current);
						break;
					case 2:
						temp_doorCells.Add(current);
						break;
					case 3:
						temp_supportCell = current;
						break;
					case 4:
						temp_roofCells.Add(current);
						break;

					default:
						break;
				}

				i++;
			}

			this.wallCells = temp_wallCells;
			this.doorCells = temp_doorCells;
			this.roofCells = temp_roofCells;
			this.supportCell = temp_supportCell;

			return true;
		}

		protected override bool PlayerChoosesTarget
		{
			get
			{
				return true;
			}
		}

		protected override TargetingParameters GetTargetingParameters ()
		{
			return new TargetingParameters
			{
				canTargetLocations = true,
				canTargetSelf = false,
				canTargetPawns = false,
				canTargetFires = false,
				canTargetBuildings = false,
				canTargetItems = false,
				validator = ((TargetInfo x) => TargetValidator(x.Cell, x.Map, this)),
			};
		}
		
		public override bool SelectedUseOption (Pawn p)
		{
			if (this.PlayerChoosesTarget)
			{

				Find.Targeter.BeginTargeting(this.GetTargetingParameters(), delegate (LocalTargetInfo t)
				{
					this.target = t.Thing;
					this.targetInfo1 = t;
					this.targetPos = t.Cell;
					this.TryStartUseJob(p);
				}, p, null, null);
				return true;
			}
			this.target = null;
			return false;
		}


		private static bool TargetValidator (IntVec3 c, Map map, CompTargetable_Tent t)
		{
			if (!c.InBounds(map) || !c.Standable(map))
			{
				return false;
			}
			if (!c.Walkable(map))
			{
				return false;
			}

			t.HandleRotation();
			t.GetPlacements(c);

			Color color = new Color(1f, 1f, 1f, 0.4f);
			List<IntVec3> everything = new List<IntVec3>();
			everything.AddRange(t.wallCells);
			everything.AddRange(t.doorCells);


			foreach (IntVec3 current in everything)
			{
				if (!current.InBounds(map))
				{
					return false;
				}
				if (!GenConstruct.CanPlaceBlueprintAt(ThingDefOf.Wall, current, Rot4.North, map, false, null).Accepted)
				{
					color = new Color(1f, 0f, 0f, 0.4f);
				}

				if (current.GetFirstItem(map) != null || current.GetFirstPawn(map) != null || current.GetFirstHaulable(map) != null)
				{
					color = new Color(1f, 0f, 0f, 0.4f);
				}
				Building building = current.GetFirstBuilding(map);
				if (building != null)
				{
					if (building.def.IsEdifice())
					{
						color = new Color(1f, 0f, 0f, 0.4f);
					}
				}
				Plant plant = current.GetPlant(map);
				if (plant != null)
				{
					if (plant.def.plant.IsTree)
					{
						color = new Color(1f, 0f, 0f, 0.4f);
					}
				}
			}
			foreach (IntVec3 doorCell in t.doorCells)
			{
				GhostDrawer.DrawGhostThing(doorCell, t.placingRot, ThingDef.Named("TentDoor"), null, color, AltitudeLayer.Blueprint);
			}

			GenDraw.DrawFieldEdges(t.wallCells, color);

			return true;
		}

		public void TryStartUseJob (Pawn user)
		{
			Find.DesignatorManager.Deselect();
			if (!user.CanReserveAndReach(this.parent, PathEndMode.Touch, Danger.Deadly, 1))
			{
				return;
			}
			if (!user.CanReach(this.targetPos, PathEndMode.Touch, Danger.Deadly))
			{
				return;
			}
			if (this.parent.HitPoints <= 0)
			{

				Messages.Message("Cannot place a fully damaged tent.", MessageTypeDefOf.RejectInput);
				return;
			}

			CellRect cellRect = new CellRect(this.targetPos.x - 2, this.targetPos.z - 2, 5, 4);
			List<IntVec3> everything = new List<IntVec3>();
			everything.AddRange(this.wallCells);
			everything.AddRange(this.doorCells);


			foreach (IntVec3 current in everything)
			{

				if (!GenConstruct.CanPlaceBlueprintAt(ThingDefOf.Wall, current, Rot4.North, user.Map, false, null).Accepted)
				{
					Messages.Message("Tent placement blocked. Please deploy on a suitable space.", MessageTypeDefOf.RejectInput);
					return;
				}

				if (current.GetFirstItem(user.Map) != null || current.GetFirstPawn(user.Map) != null || current.GetFirstHaulable(user.Map) != null)
				{
					Messages.Message("Tent placement blocked by a item, pawn or a building. Please deploy on a suitable space.", MessageTypeDefOf.RejectInput);
					return;
				}

				Building building = current.GetFirstBuilding(user.Map);
				if (building != null)
				{
					if (building.def.IsEdifice())
					{
						Messages.Message("Tent placement blocked by a item, pawn or a building. Please deploy on a suitable space.", MessageTypeDefOf.RejectInput);
						return;
					}
				}
				Plant plant = current.GetPlant(user.Map);
				if (plant != null)
				{
					if (plant.def.plant.IsTree)
					{
						Messages.Message("Tent placement blocked by a tree. Please deploy on a suitable space.", MessageTypeDefOf.RejectInput);
						return;
					}
				}
			}

			Job job = new Job(DefDatabase<JobDef>.GetNamed("DeployTent", true), this.parent, this.targetInfo1);
			user.jobs.TryTakeOrderedJob(job);
		}

		public void TryStartRepairJob (Pawn user)
		{
			Thing t2 = this.FindClosestCloth(user);
			if (t2 != null)
			{
				Job job = new Job(DefDatabase<JobDef>.GetNamed("RepairTent", true), this.parent, t2);
				job.count = this.parent.MaxHitPoints - this.parent.HitPoints;
				user.jobs.TryTakeOrderedJob(job);
			}
			else
			{
				Messages.Message("There is no " + this.parent.Stuff.label.Translate() + " available to repair the tent (Need " + (this.parent.MaxHitPoints - this.parent.HitPoints).ToString() + ").", MessageTypeDefOf.RejectInput);
				return;
			}
		}

		private Thing FindClosestCloth (Pawn pawn)
		{
			Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1);
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(this.parent.Stuff), PathEndMode.InteractionCell, TraverseParms.For(pawn, pawn.NormalMaxDanger(), TraverseMode.ByPawn, false), 9999f, validator, null, -1, -1, false);
		}

		public override void DoEffect (Pawn usedBy)
		{

			if (this.target != null && !this.GetTargetingParameters().CanTarget(this.target))
			{
				return;
			}
			base.DoEffect(usedBy);
			if (this.parent.Stuff == null)
			{
				this.parent.SetStuffDirect(ThingDefOf.Cloth);
			}

			double hitpoints = (double)this.parent.HitPoints / 10;
			int wallsToPlace = (int)Math.Floor(hitpoints);
			List<IntVec3> everything = new List<IntVec3>();
			everything.AddRange(this.wallCells);
			everything.AddRange(this.doorCells);

			foreach (IntVec3 current in everything)
			{

				if (!GenConstruct.CanPlaceBlueprintAt(ThingDef.Named("TentWall"), current, Rot4.North, usedBy.Map, false, null).Accepted)
				{
					Messages.Message("Tent placement blocked. Please deploy on a suitable space.", MessageTypeDefOf.RejectInput);
					return;
				}

				if (current.GetFirstItem(usedBy.Map) != null || current.GetFirstPawn(usedBy.Map) != null || current.GetFirstHaulable(usedBy.Map) != null)
				{
					Messages.Message("Tent placement blocked. Please deploy on a suitable space.", MessageTypeDefOf.RejectInput);
					return;
				}
				Building building = current.GetFirstBuilding(usedBy.Map);
				if (building != null)
				{
					if (building.def.IsEdifice())
					{
						Messages.Message("Tent placement blocked by a item, pawn or a building. Please deploy on a suitable space.", MessageTypeDefOf.RejectInput);
						return;
					}
				}
			}

			//place roof
			foreach (IntVec3 current in this.wallCells)
			{
				if (usedBy.Map.roofGrid.RoofAt(current) == null)
				{
					usedBy.Map.roofGrid.SetRoof(current, RoofDefOf.RoofConstructed);
				}
			}
			foreach (IntVec3 current in this.doorCells)
			{
				if (usedBy.Map.roofGrid.RoofAt(current) == null)
				{
					usedBy.Map.roofGrid.SetRoof(current, RoofDefOf.RoofConstructed);
				}
			}
			foreach (IntVec3 current in this.roofCells)
			{
				if (usedBy.Map.roofGrid.RoofAt(current) == null)
				{
					usedBy.Map.roofGrid.SetRoof(current, RoofDefOf.RoofConstructed);
				}
			}

			if (usedBy.Map.roofGrid.RoofAt(supportCell) == null)
			{
				usedBy.Map.roofGrid.SetRoof(supportCell, RoofDefOf.RoofConstructed);
			}


			if (wallsToPlace < wallCells.Count + doorCells.Count)
			{
				Messages.Message("Damaged tent deployed. Some walls will be missing.", MessageTypeDefOf.NegativeEvent);
			}
			int wallsPlaced = 0;
			this.GetPlacements(this.targetPos);

			foreach (IntVec3 current in this.wallCells)
			{
				if (wallsPlaced < wallsToPlace)
				{
					TrySpawnWall(current, usedBy);
					wallsPlaced++;
				}

			}

			foreach (IntVec3 DoorPos in this.doorCells)
			{
				if (wallsPlaced < wallsToPlace)
				{

					Thing door = ThingMaker.MakeThing(ThingDef.Named("TentDoor"), this.parent.Stuff);
					door.SetFaction(usedBy.Faction, null);
					door.Rotation = Building_Door.DoorRotationAt(DoorPos, usedBy.Map);
					GenSpawn.Spawn(door, DoorPos, usedBy.Map);
					wallsPlaced++;
				}
			}

			this.parent.Destroy(DestroyMode.Vanish);
			SoundDefOf.Designate_PlaceBuilding.PlayOneShotOnCamera();
			Thing tent = ThingMaker.MakeThing(ThingDef.Named("TentDeployed"), this.parent.Stuff);
			tent.SetFaction(usedBy.Faction, null);
			tent.TryGetComp<CompPackTent>().tentName = this.parent.def.defName;
			tent.TryGetComp<CompPackTent>().placingRot = this.placingRot;
			GenSpawn.Spawn(tent, this.supportCell, usedBy.Map);

			return;
		}


		[DebuggerHidden]
		public override IEnumerable<Thing> GetTargets (Thing targetChosenByPlayer = null)
		{
			yield return targetChosenByPlayer;
			yield break;
		}

		private Thing TrySpawnWall (IntVec3 c, Pawn usedBy)
		{
			Thing thing = ThingMaker.MakeThing(ThingDef.Named("TentWall"), this.parent.Stuff);
			thing.SetFaction(usedBy.Faction, null);
			return GenSpawn.Spawn(thing, c, usedBy.Map);
		}


		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions (Pawn myPawn)
		{
			if (this.parent.Stuff == null)
			{
				this.parent.SetStuffDirect(ThingDefOf.Cloth);
			}
			if (!myPawn.CanReserve(this.parent, 1))
			{
				yield return new FloatMenuOption("Repair the tent" + " (" + "Reserved".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
			}
			else
			{
				if (this.parent.HitPoints < this.parent.MaxHitPoints)
				{
					FloatMenuOption floatMenuOptionRepair = new FloatMenuOption("Repair the tent (" + (this.parent.MaxHitPoints - this.parent.HitPoints).ToString() + " " + this.parent.Stuff.label.Translate() + " needed)", delegate
					{
						if (myPawn.CanReserveAndReach(this.parent, PathEndMode.Touch, Danger.Deadly, 1))
						{
							this.TryStartRepairJob(myPawn);
						}
					}, MenuOptionPriority.Default, null, null, 0f, null, null);
					yield return floatMenuOptionRepair;

				}
			}

			yield break;
		}
	}
}