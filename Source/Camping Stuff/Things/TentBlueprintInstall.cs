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
	public class TentBlueprint_Install : Blueprint_Install
	{
		protected Graphic cachedGraphic;

		public override CellRect? CustomRectForSelector => (this.ThingToInstall is NCS_Tent tent) ? tent.CandidateRect(this.Position) : base.CustomRectForSelector;

		public override Graphic Graphic
		{
			get
			{
				if (this.cachedGraphic == null)
				{
					Graphic graphic = this.ThingToInstall.def.installBlueprintDef.graphic;
					cachedGraphic = (this.ThingToInstall is NCS_Tent tent) ? tent.Graphic.GetColoredVersion(graphic.Shader, graphic.Color, graphic.ColorTwo) : base.Graphic;
				}
				return this.cachedGraphic;
			}
		}

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			base.DrawAt(drawLoc, flip);

			if(this.ThingToInstall is NCS_Tent tent)
			{
				tent.DrawGhost(drawLoc.ToIntVec3(), false, this.Rotation);
			}
		}
	}
}
