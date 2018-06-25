using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;

namespace Nandonalt_CampingStuff
{
    public class CompPropTent : CompProperties
    {
        public JobDef useJob;

        public string useLabel;

        public CompPropTent()
        {
            this.compClass = typeof(CompPackTent);
        }
    }
}
