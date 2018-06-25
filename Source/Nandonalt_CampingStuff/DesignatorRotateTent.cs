using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace Nandonalt_CampingStuff
{
    public class DesignatorRotateTent : Designator
    {

        public Rot4 placingRot = Rot4.South;
        
        public override string Label
        {
            get
            {
               return "TentRotator";
            }
        }


        public override bool Visible
        {
            get
            {
                return false;
            }
        }
        

        public override void DoExtraGuiControls(float leftX, float bottomY)
        {
            RotationDirection rotationDirection = RotationDirection.None;
            if (Event.current.button == 2)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    Event.current.Use();
                    Designator_Place.middleMouseDownTime = Time.realtimeSinceStartup;
                }
                if (Event.current.type == EventType.MouseUp && Time.realtimeSinceStartup - Designator_Place.middleMouseDownTime < 0.15f)
                {
                    rotationDirection = RotationDirection.Clockwise;
                }
            }
            if (KeyBindingDefOf.DesignatorRotateRight.KeyDownEvent)
            {
                rotationDirection = RotationDirection.Clockwise;
            }
            if (KeyBindingDefOf.DesignatorRotateLeft.KeyDownEvent)
            {
                rotationDirection = RotationDirection.Counterclockwise;
            }
            if (rotationDirection == RotationDirection.Clockwise)
            {
                SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                this.placingRot.Rotate(RotationDirection.Clockwise);
            }
            if (rotationDirection == RotationDirection.Counterclockwise)
            {
                SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
                this.placingRot.Rotate(RotationDirection.Counterclockwise);
            }
        
        }


        public override void SelectedUpdate()
        {
            base.SelectedUpdate();
            IntVec3 intVec = UI.MouseCell();
            ThingDef thingDef = this.entDef as ThingDef;
            if (thingDef != null && (thingDef.EverTransmitsPower || thingDef.ConnectToPower))
            {
                OverlayDrawHandler.DrawPowerGridOverlayThisFrame();
                if (thingDef.ConnectToPower)
                {
                    CompPower compPower = PowerConnectionMaker.BestTransmitterForConnector(intVec, Find.VisibleMap, null);
                    if (compPower != null)
                    {
                        PowerNetGraphics.RenderAnticipatedWirePieceConnecting(intVec, compPower.parent);
                    }
                }
            }
        }
        
    }
}
