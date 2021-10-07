using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Camping_Stuff
{
	public class CompTentBagDamageComp : CompTentPartDamage
	{
		public CompProperties_TentPartDamage Props => (CompProperties_TentPartDamage)this.props;
		public override int RepairCost => (int)Math.Ceiling(this.parent.GetInnerIfMinified().def.costStuffCount * DamageCost);
		protected override ThingDef RepairStuff => this.parent.GetInnerIfMinified().Stuff;
	}

	public class CompProperties_TentBagDamageComp : CompProperties //(Def)
	{
		public CompProperties_TentBagDamageComp()
		{
			this.compClass = typeof(CompTentBagDamageComp);
		}
	}
}
