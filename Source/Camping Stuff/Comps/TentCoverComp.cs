using System;
using System.Collections.Generic;
using System.Linq;

using Verse;

namespace Camping_Stuff
{
	/// <summary>
	/// Refines damage repair to work with tent covers
	/// </summary>
	/// <remarks>
	/// This child is needed since the cell repair costs are relative to the number of cells in the layout
	/// </remarks>
	public class TentCoverComp : CompTentPartWithCellsDamage //(Thing)
	{
		public CompProperties_TentCover Props => (CompProperties_TentCover)this.props;

		protected override int DamageUnit => Math.Max((int)Math.Floor((1.0 / this.Props.tentSpec.layoutParts) * this.parent.MaxHitPoints), 1);
		protected override double DamageCost => (double)this.parent.def.costStuffCount / this.Props.tentSpec.layoutParts;
	}

	public class CompProperties_TentCover : CompProperties_TentPartDamage //(Def)
	{
		public int numPoles;
		public string layoutName;
		private List<string> tentLayoutSouth = new List<string>();
		public TentSpec tentSpec;
		public List<LayoutSpawn> layoutSpawns = new List<LayoutSpawn>();

		public int LayoutHash => tentSpec.GetHashCode();

		public CompProperties_TentCover()
		{
			this.compClass = typeof(TentCoverComp);
		}

		public CompProperties_TentCover(Type compClass)
		{
			this.compClass = compClass;
		}

		public override void PostLoadSpecial(ThingDef parentDef)
		{
			tentSpec = new TentSpec(tentLayoutSouth, Rot4.South);
		}

		public override void ResolveReferences(ThingDef parentDef)
		{
			tentSpec.AssignSpawns(layoutSpawns.ToDictionary(spawn => spawn.part, spawn => spawn.def));
		}
	}
}