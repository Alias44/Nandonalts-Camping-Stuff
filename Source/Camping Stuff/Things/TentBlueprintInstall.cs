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
		public override CellRect? CustomRectForSelector
		{
			get
			{
				if (this.ThingToInstall is NCS_Tent tent)
				{
					return tent.CandidateRect(this.Position);
				}

				return base.CustomRectForSelector;
			}
		}

		public override Graphic Graphic => (this.ThingToInstall is NCS_Tent tent) ? tent.Graphic : base.Graphic;
	}
}
