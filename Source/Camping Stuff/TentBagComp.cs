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

		public override string CompInspectStringExtra()
		{
			return Inner.CompInspectStringExtra();
		}

		public override string GetDescriptionPart()
		{
			return Inner.GetDescriptionPart();
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
