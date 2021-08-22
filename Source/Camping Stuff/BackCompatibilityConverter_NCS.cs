using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using RimWorld;
using Verse;

namespace Camping_Stuff
{
	class BackCompatibilityConverter_NCS : BackCompatibilityConverter
	{
		private static Dictionary<string, ThingDef> partReplacements;
		private Dictionary<int, ThingDef> oldTents = new Dictionary<int, ThingDef>();

		public override bool AppliesToVersion(int majorVer, int minorVer) =>
			majorVer == 0 || (majorVer == 1 && minorVer == 0); // applies to <= 1.0

		public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
		{
			if (defType == typeof(ThingDef) && partReplacements.ContainsKey(defName))
			{
				return TentDefOf.NCS_MiniTentBag.defName;
			}
			/*else if (defName.Equals("TentDeployed"))
			{
				return "WoodLog";
			}*/
			else
			{
				var def = GenDefDatabase.GetDefSilentFail(defType, "NCS_" + defName, false);

				if (def != null)
				{
					return def.defName;
				}
			}

			return null;
		}

		public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
		{
			if (baseType == typeof(Thing))
			{
				string def = node["def"].InnerText;
				if (providedClassName.Equals("ThingWithComps") && partReplacements.ContainsKey(def))
				{
					int id = Thing.IDNumberFromThingID(node["id"].InnerText);
					oldTents[id] = partReplacements[def];

					return TentDefOf.NCS_MiniTentBag.thingClass;
				}
			}

			return null;
		}

		public override void PostExposeData(object obj)
		{
			if (Scribe.mode != LoadSaveMode.PostLoadInit)
				return;
			if (obj is NCS_MiniTent miniTent && oldTents.ContainsKey(miniTent.thingIDNumber))
			{
				// Create an inner tent to set in the bag (the minified thing won't have any by default)
				var tent = (NCS_Tent) ThingMaker.MakeThing(TentDefOf.NCS_TentBag, miniTent.Stuff);

				// make the user whole: fill the bag with parts equivalent to the replaced tent
				var cover = ThingMaker.MakeThing(oldTents[miniTent.thingIDNumber], miniTent.Stuff);
				tent.Cover = cover;
				var poleStack = ThingMaker.MakeThing(TentDefOf.NCS_TentPart_Pole, ThingDefOf.WoodLog);
				poleStack.stackCount = tent.maxPoles;
				tent.PackPart(poleStack);

				miniTent.Bag = tent;
				miniTent.HitPoints = miniTent.MaxHitPoints;
			}

			return;
		}

		public override void PreLoadSavegame(string loadingVersion)
		{
			oldTents.Clear();
			partReplacements = new Dictionary<string, ThingDef>
			{
				{"DeployableTent", TentDefOf.NCS_TentPart_Cover_Small},
				{"DeployableTentMedium", TentDefOf.NCS_TentPart_Cover_Med},
				{"DeployableTentBig", TentDefOf.NCS_TentPart_Cover_Large},
				{"DeployableTentLong", TentDefOf.NCS_TentPart_Cover_Long}
			};
		}

		public override void PostLoadSavegame(string loadingVersion)
		{
			oldTents.Clear();
			partReplacements.Clear();
		}
	}
}
