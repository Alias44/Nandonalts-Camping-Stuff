using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;

namespace Camping_Stuff
{
	public class CompTentDammage : ThingComp //(Thing)
	{
		public CompProperties_TentDammage Props => (CompProperties_TentDammage)this.props;

		public HashSet<SketchEntity> dammagedCells = new HashSet<SketchEntity>();
	}

	public class CompProperties_TentDammage : CompProperties //(Def)
	{
		public CompProperties_TentDammage()
		{
			this.compClass = typeof(CompTentDammage);
		}
	}
}
