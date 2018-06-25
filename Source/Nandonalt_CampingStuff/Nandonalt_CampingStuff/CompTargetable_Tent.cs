using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.Sound;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse.AI;

namespace Nandonalt_CampingStuff
{
    public class CompTargetable_Tent : CompTargetable
    {
        private Thing target;
        private LocalTargetInfo targetInfo1;
        private IntVec3 targetPos;
        public Rot4 placingRot = Rot4.South;
        public Rot4 lastRot;

        /*int[,] array2D = new int[7, 7] {
            { 0, 1, 1, 1, 1, 1, 0 },
            { 1, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 3, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 1 },
            { 0, 1, 1, 2, 1, 1, 0 } };*/

        public List<int> line1 = new List<int>();
        public List<int> line2 = new List<int>();
        public List<int> line3 = new List<int>();
        public  List<int> line4 = new List<int>();
        public  List<int> line5 = new List<int>();
        public  List<int> line6 = new List<int>();
        public List<int> line7 = new List<int>();
        public List<List<int>> lines = new List<List<int>>();
        public List<IntVec3> wallCells = new List<IntVec3>();
        public IntVec3 supportCell;
        public List<IntVec3> doorCells = new List<IntVec3>();
        public List<IntVec3> roofCells = new List<IntVec3>();

        public new CompProperties_Tent Props
        {
            get
            {
                return (CompProperties_Tent)this.props;
            }
        }


        
        public void StringToLines(CompTargetable_Tent t, Rot4 rot)
        {
           
               
                    CompProperties_Tent props = this.Props;
                    string[] sline1;
                    string[] sline2;
                    string[] sline3;
                    string[] sline4;
                    string[] sline5;
                    string[] sline6;
                    string[] sline7;
                    if (rot == Rot4.South)
                    {
                        sline1 = t.Props.line1_s.Split(',');
                        sline2 = t.Props.line2_s.Split(',');
                        sline3 = t.Props.line3_s.Split(',');
                        sline4 = t.Props.line4_s.Split(',');
                        sline5 = t.Props.line5_s.Split(',');
                        sline6 = t.Props.line6_s.Split(',');
                        sline7 = t.Props.line7_s.Split(',');
                    }
                    else if (rot == Rot4.West)
                    {
                        sline1 = t.Props.line1_w.Split(',');
                        sline2 = t.Props.line2_w.Split(',');
                        sline3 = t.Props.line3_w.Split(',');
                        sline4 = t.Props.line4_w.Split(',');
                        sline5 = t.Props.line5_w.Split(',');
                        sline6 = t.Props.line6_w.Split(',');
                        sline7 = t.Props.line7_w.Split(',');
                    }
                    else if (rot == Rot4.North)
                    {
                        sline1 = t.Props.line1_n.Split(',');
                        sline2 = t.Props.line2_n.Split(',');
                        sline3 = t.Props.line3_n.Split(',');
                        sline4 = t.Props.line4_n.Split(',');
                        sline5 = t.Props.line5_n.Split(',');
                        sline6 = t.Props.line6_n.Split(',');
                        sline7 = t.Props.line7_n.Split(',');
                    }
                    else
                    {
                        sline1 = t.Props.line1_e.Split(',');
                        sline2 = t.Props.line2_e.Split(',');
                        sline3 = t.Props.line3_e.Split(',');
                        sline4 = t.Props.line4_e.Split(',');
                        sline5 = t.Props.line5_e.Split(',');
                        sline6 = t.Props.line6_e.Split(',');
                        sline7 = t.Props.line7_e.Split(',');
                    }
                t.line1.Clear();
                t.line2.Clear();
                t.line3.Clear();
                t.line4.Clear();
                t.line5.Clear();
                t.line6.Clear();
                t.line7.Clear();
                foreach (String l in sline1)
                    {
                        t.line1.Add(Int32.Parse(l));
                    }
                    foreach (String l in sline2)
                    {
                        t.line2.Add(Int32.Parse(l));
                    }
                    foreach (String l in sline3)
                    {
                        t.line3.Add(Int32.Parse(l));
                    }
                    foreach (String l in sline4)
                    {
                        t.line4.Add(Int32.Parse(l));
                    }
                    foreach (String l in sline5)
                    {
                        t.line5.Add(Int32.Parse(l));
                    }
                    foreach (String l in sline6)
                    {
                        t.line6.Add(Int32.Parse(l));
                    }
                    foreach (String l in sline7)
                    {
                        t.line7.Add(Int32.Parse(l));
                    }
                    this.lastRot = rot;
                
            
        }

        private int tick = 0;

        public void HandleRotation()
        {
        
                RotationDirection rotationDirection = RotationDirection.None;
          
                if (KeyBindingDefOf.DesignatorRotateRight.JustPressed)
                {
             
                    rotationDirection = RotationDirection.Clockwise;
             
                }
                if (KeyBindingDefOf.DesignatorRotateLeft.JustPressed)
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

        public bool GetPlacements(IntVec3 c)
        {

              //this.rotate();
              StringToLines(this, this.placingRot);


            lines = new List<List<int>>() { this.line7, this.line6, this.line5, this.line4, this.line3, this.line2, this.line1 };
            CellRect cellRect = new CellRect(c.x - 3, c.z - 3, 7, 7);
            List<int> cells = new List<int>();
            List<IntVec3> temp_wallCells = new List<IntVec3>();
            IntVec3 temp_supportCell = new IntVec3(0,0,0);
            List<IntVec3> temp_doorCells = new List<IntVec3>();
            List<IntVec3> temp_roofCells = new List<IntVec3>();
            for(int a = 0; a < 7; a++)
            {

            }

            foreach (List<int> line in lines)
            {
                foreach(int cell in line)
                {
                    cells.Add(cell);
                }
            }

            int i = 0;
            foreach(IntVec3 current in cellRect.Cells)
            {
                if (cells[i] == 1 )
                {
                    temp_wallCells.Add(current);
                }
                if (cells[i] == 2)
                {
                    temp_doorCells.Add(current);
                }
                if (cells[i] == 3)
                {
                    temp_supportCell = current;
                }
                if (cells[i] == 4)
                {
                    temp_roofCells.Add(current);
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

        protected override TargetingParameters GetTargetingParameters()
        {
           
            return new TargetingParameters
            {
                canTargetLocations = true,
            canTargetSelf = false,
            canTargetPawns = false,
           canTargetFires = false,
           canTargetBuildings = false,
        canTargetItems = false,
      validator = ((TargetInfo x) => TargetValidator(x.Cell, x.Map,this)),
                      };
        }

    

        public override bool SelectedUseOption(Pawn p)
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

 
        private static bool TargetValidator(IntVec3 c, Map map, CompTargetable_Tent t)
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
            CellRect cellRect = new CellRect(c.x - 2, c.z - 2, 5, 4);
            List<IntVec3> clist = new List<IntVec3>();
            Color color = new Color(1f, 1f, 1f, 0.4f);
            List<IntVec3> everything = new List<IntVec3>();
            everything.AddRange(t.wallCells);
            everything.AddRange(t.doorCells);
   

            foreach (IntVec3 current in everything)
            {
                if(!current.InBounds(map))
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
            foreach(IntVec3 doorCell in t.doorCells)
            {
                GhostDrawer.DrawGhostThing(doorCell, t.placingRot, ThingDef.Named("TentDoor"), null, color, AltitudeLayer.Blueprint);
            }
              
                   
            GenDraw.DrawFieldEdges(t.wallCells,color);

            return true;
        }

        public void TryStartUseJob(Pawn user)
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
                if ( building != null)
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
                    if(plant.def.plant.IsTree)
                    {
                        Messages.Message("Tent placement blocked by a tree. Please deploy on a suitable space.", MessageTypeDefOf.RejectInput);
                        return;
                    }
                }
            }
            Job job = new Job(DefDatabase<JobDef>.GetNamed("DeployTent", true), this.parent, this.targetInfo1);
            user.jobs.TryTakeOrderedJob(job);
        }


        public void TryStartRepairJob(Pawn user)
        {
            Thing t2 = this.FindClosestCloth(user);
            if(t2 != null) {
              
                    Job job = new Job(DefDatabase<JobDef>.GetNamed("RepairTent", true), this.parent, t2);
                job.count = this.parent.MaxHitPoints - this.parent.HitPoints;
                  user.jobs.TryTakeOrderedJob(job);
                              
            {            
            }
            }
            else
            {
                Messages.Message("There is no "+ this.parent.Stuff.label.Translate() + " available to repair the tent (Need " + (this.parent.MaxHitPoints - this.parent.HitPoints).ToString() + ").", MessageTypeDefOf.RejectInput);
                return;

            }
        }

        private Thing FindClosestCloth(Pawn pawn)
        {
            Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1);
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(this.parent.Stuff), PathEndMode.InteractionCell, TraverseParms.For(pawn, pawn.NormalMaxDanger(), TraverseMode.ByPawn, false), 9999f, validator, null, -1, -1, false);
        }

        public override void DoEffect(Pawn usedBy)
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

            CellRect cellRect = new CellRect(this.targetPos.x-2, this.targetPos.z-2, 5,4);
            double hitpoints = (double)this.parent.HitPoints / 10;
            int wallsToPlace = (int) Math.Floor(hitpoints);      
            //int wallsToPlace = 49;
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

                if (current.GetFirstItem(usedBy.Map) != null || current.GetFirstPawn(usedBy.Map) != null || current.GetFirstHaulable(usedBy.Map) != null )
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

                foreach(IntVec3 DoorPos in this.doorCells)
                {

                if (wallsPlaced < wallsToPlace)
                {
                 
                    Thing door = ThingMaker.MakeThing(ThingDef.Named("TentDoor"), this.parent.Stuff);
                    door.SetFaction(usedBy.Faction, null);
                    door.Rotation = Building_Door.DoorRotationAt(DoorPos,usedBy.Map);
                    GenSpawn.Spawn(door, DoorPos, usedBy.Map);
                    wallsPlaced++;
                }
                }
                     
   
            this.parent.Destroy(DestroyMode.Vanish);
            SoundDefOf.DesignatePlaceBuilding.PlayOneShotOnCamera();
            Thing tent = ThingMaker.MakeThing(ThingDef.Named("TentDeployed"), this.parent.Stuff);
            tent.SetFaction(usedBy.Faction, null);
            tent.TryGetComp<CompPackTent>().tentName = this.parent.def.defName;
            tent.TryGetComp<CompPackTent>().placingRot = this.placingRot;
            GenSpawn.Spawn(tent, this.supportCell, usedBy.Map);

            return;
        }



        [DebuggerHidden]
        public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
        {
            yield return targetChosenByPlayer;
            yield break;
        }
      
        private Thing TrySpawnWall(IntVec3 c, Pawn usedBy)
        {
            Thing thing = ThingMaker.MakeThing(ThingDef.Named("TentWall"), this.parent.Stuff);
            thing.SetFaction(usedBy.Faction, null);
            return GenSpawn.Spawn(thing, c, usedBy.Map);
        }


        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
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
                    FloatMenuOption floatMenuOptionRepair = new FloatMenuOption("Repair the tent (" + (this.parent.MaxHitPoints - this.parent.HitPoints).ToString() + " "+ this.parent.Stuff.label.Translate() + " needed)", delegate
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
