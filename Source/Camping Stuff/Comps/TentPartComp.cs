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
	public class CompUsable_TentPart : CompUsable //(Thing)
	{
		public CompProperties_TentPart Props
		{
			get
			{
				return (CompProperties_TentPart)this.props;
			}
		}

		protected TargetingParameters GetTargetingParameters()
		{
			return new TargetingParameters()
			{
				canTargetPawns = false,
				canTargetBuildings = false,
				canTargetAnimals = false,
				canTargetHumans = false,
				canTargetMechs = false,
				canTargetItems = true,
				mapObjectTargetsMustBeAutoAttackable = false,
				validator = (Predicate<TargetInfo>)(t => t.Thing is NCS_MiniTent)
			};
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			if (selPawn.CanReserveAndReach(this.parent, PathEndMode.Touch, Danger.Deadly))
			{
				yield return new FloatMenuOption("Pack " + parent.def.label+ " into bag", delegate
				 {
					 Find.Targeter.BeginTargeting(this.GetTargetingParameters(), delegate (LocalTargetInfo t)
					 {
						 TryStartUseJob(selPawn, t);
					 });
				 });
			}
			else
			{
				yield return new FloatMenuOption("Unable to reach or reserve target", null);
			}
		}
	}

	public class CompProperties_TentPart : CompProperties_Usable //(Def)
	{
		public TentPart partType;

		public CompProperties_TentPart()
		{
			this.compClass = typeof(CompUsable_TentPart);
		}
	}
}
