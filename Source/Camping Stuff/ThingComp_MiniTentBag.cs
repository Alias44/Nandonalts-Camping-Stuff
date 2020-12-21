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
	public enum TentPart
	{
		other = -1,
		bag,
		pole,
		cover,
		floor
	}

	public class ThingComp_MiniTentBag : ThingComp //(Thing)
	{
		TentBagComp Bag {
			get {
				var f = this.parent.GetInnerIfMinified().TryGetComp<TentBagComp>();
 				return f;
			}
		}

		public int MaxPoles => Bag.maxPoles;


		public void PackPart(Thing part)
		{
			Bag.PackPart(part);
		}

		public void EjectCover()
		{
			Bag.EjectCover();
		}

		public void EjectPole(Thing pole)
		{
			Bag.EjectPole(pole);
		}

		public void EjectPole(int poleIndex)
		{
			Bag.EjectPole(poleIndex);
		}

		public void EjectAllPoles()
		{
			Bag.EjectAllPoles();
		}

		public void EjectFloor()
		{
			Bag.EjectFloor();
		}

		public void EjectAll()
		{
			Bag.EjectAll();
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			return Bag.CompFloatMenuOptions(selPawn);
		}
	}

	public class CompProperties_MiniTentBag : CompProperties //(Def)
	{
		public CompProperties_MiniTentBag()
		{
			this.compClass = typeof(ThingComp_MiniTentBag);
		}
	}
}
