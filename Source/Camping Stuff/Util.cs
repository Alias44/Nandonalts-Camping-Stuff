﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Camping_Stuff
{
	public static class Util
	{
		public static string indent = "    "; // matches the stat indents defined in RimWorld.StatWorker

		public static string LabelCapHpFrac(this Thing t)
		{
			return $"{t.LabelCap} ({t.HitPoints} / {t.MaxHitPoints})";
		}

		public static SketchEntity Normalize(this SketchEntity se, Rot4 sketchRot)
		{
			SketchEntity entity = se.DeepCopy();

			int newRot = Rot4.North.AsInt - sketchRot.AsInt;
			if (newRot < 0)
				newRot += 4;
			Rot4 rot1 = new Rot4(newRot);

			entity.pos = entity.pos.RotatedBy(rot1);

 			return entity;
		}

		public static bool IsTentReady(Thing t)
		{
			return (t is NCS_MiniTent miniTent && miniTent.Bag.Ready) || (t is NCS_Tent tent && tent.Ready);
		}

		public static bool IsTentPart(Thing t, TentPart partType)
		{
			var partComp = t.TryGetComp<CompUsable_TentPart>();
			return partComp != null && partComp.Props.partType == partType;
		}
	}
}