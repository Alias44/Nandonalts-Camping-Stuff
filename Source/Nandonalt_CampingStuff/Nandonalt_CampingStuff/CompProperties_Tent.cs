using System;
using RimWorld;
using Verse;

namespace Nandonalt_CampingStuff
{
    public class CompProperties_Tent : CompProperties_Targetable
    {
        public string line1_s;
        public string line2_s;
        public string line3_s;
        public string line4_s;
        public string line5_s;
        public string line6_s;
        public string line7_s;

        public string line1_w;
        public string line2_w;
        public string line3_w;
        public string line4_w;
        public string line5_w;
        public string line6_w;
        public string line7_w;

        public string line1_e;
        public string line2_e;
        public string line3_e;
        public string line4_e;
        public string line5_e;
        public string line6_e;
        public string line7_e;

        public string line1_n;
        public string line2_n;
        public string line3_n;
        public string line4_n;
        public string line5_n;
        public string line6_n;
        public string line7_n;


        public CompProperties_Tent()
        {
            this.compClass = typeof(CompTargetable_Tent);
        }
    }
}
