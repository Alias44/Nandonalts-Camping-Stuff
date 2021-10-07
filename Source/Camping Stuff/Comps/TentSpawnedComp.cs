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
	public class TentSpawnedComp : ThingComp //(Thing)
	{
		public CompProperties_TentSpawnedComp Props
		{
			get
			{
				return (CompProperties_TentSpawnedComp)this.props;
			}
		}

		public NCS_Tent tent;

		public override void PostExposeData()
		{
			base.PostExposeData();

 			Scribe_References.Look(ref this.tent, "tentSpawnedBy");
		}
	}

	public class CompProperties_TentSpawnedComp : CompProperties //(Def)
	{
		public TentPart partType;

		public CompProperties_TentSpawnedComp()
		{
			this.compClass = typeof(TentSpawnedComp);
		}
	}
}
