using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using RimWorld;
using Verse;

namespace Camping_Stuff
{
	public class BackCompatibilityConverter_LegacyTent : BackCompatibilityConverter
	{
		private static Dictionary<string, ThingDef> partReplacements = new Dictionary<string, ThingDef>();

		private Dictionary<int, ThingDef> oldPackedTents = new Dictionary<int, ThingDef>();

		private Dictionary<int, (ThingDef cover, Rot4 orientation, int mapId)> oldDeployedTents =
			new Dictionary<int, (ThingDef, Rot4, int)>();

		private Dictionary<int, List<NCS_Tent>> newDeployedTents = new Dictionary<int, List<NCS_Tent>>();

		private static TentSpec legacyLargeLayout = new TentSpec(new List<string> {
								"1,1,1,1,1,1,1",
								"1,4,4,1,4,4,1",
								"1,2,1,1,1,2,1",
								"1,4,4,3,4,4,1",
								"1,4,4,4,4,4,1",
								"1,4,4,4,4,4,1",
								"1,1,1,2,1,1,1"
							}, Rot4.South);

		public override bool AppliesToVersion(int majorVer, int minorVer) =>
			majorVer == 0 || (majorVer == 1 && minorVer == 0); // applies to <= 1.0

		public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
		{
			if (defType == typeof(ThingDef) && partReplacements.ContainsKey(defName))
			{
				return TentDefOf.NCS_MiniTentBag.defName;
			}
			else if (defName.Equals("TentDeployed"))
			{
				return TentDefOf.NCS_TentBag.defName;
			}
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
				int id = Thing.IDNumberFromThingID(node["id"].InnerText);

				if (partReplacements.ContainsKey(def) && providedClassName.Equals("ThingWithComps"))
				{
					oldPackedTents[id] = partReplacements[def];

					return TentDefOf.NCS_MiniTentBag.thingClass;
				}
				else if (providedClassName.Equals("Building") && def.Equals("TentDeployed"))
				{
					string cover = "DeployableTent";
					var nameNode = node["tentName"];
					if (nameNode != null)
					{
						cover = nameNode.InnerText;
					}

					Rot4 direction;
					try
					{
						direction = Rot4.FromString(node["placingRot"].InnerText);

						if (direction == Rot4.East || direction == Rot4.West)
						{
							direction.AsInt += 2;
						}
					}
					catch
					{
						direction = Rot4.South;
					}

					int.TryParse(node["map"].InnerText, out var mapId);

					oldDeployedTents[id] = (partReplacements[cover], direction, mapId);
					return TentDefOf.NCS_TentBag.thingClass;
				}
			}

			return null;
		}

		public override void PostExposeData(object obj)
		{
			if (Scribe.mode != LoadSaveMode.PostLoadInit)
				return;

			if (obj is Map map && newDeployedTents.ContainsKey(map.uniqueID))
			{
				// Convert Constructed roofs above each tent into tent roofs
				newDeployedTents[map.uniqueID]
					.ForEach(tent => tent.sketch.Entities
						.Where(e => e is SketchRoof &&
									map.roofGrid.RoofAt(tent.Position + e.pos) == RoofDefOf.RoofConstructed)
						.ToList()
						.ForEach(e => e.Spawn(tent.Position + e.pos, map, tent.Faction, wipeIfCollides: true)));
			}
			else if (obj is NCS_MiniTent miniTent && oldPackedTents.ContainsKey(miniTent.thingIDNumber))
			{
				// Create an inner tent to set in the bag (the minified thing won't have any by default)
				var tent = (NCS_Tent)ThingMaker.MakeThing(TentDefOf.NCS_TentBag, miniTent.Stuff);
				// make the user whole: fill the bag with parts equivalent to the replaced tent
				tent.SpawnParts(oldPackedTents[miniTent.thingIDNumber]);

				miniTent.Bag = tent;
				miniTent.HitPoints = miniTent.MaxHitPoints;
			}
			else if (obj is NCS_Tent tent && oldDeployedTents.ContainsKey(tent.thingIDNumber))
			{
				var (thingDef, orientation, mapId) = oldDeployedTents[tent.thingIDNumber];
				tent.SpawnParts(thingDef);

				tent.Rotation = orientation;

				if (tent.Cover.def.Equals(TentDefOf.NCS_TentPart_Cover_Large))
				{
					tent.SetDeployedSketch(legacyLargeLayout);
				}

				try
				{
					newDeployedTents[mapId].Add(tent);
				}
				catch
				{
					newDeployedTents[mapId] = new List<NCS_Tent> { tent };
				}
			}
		}

		public override void PreLoadSavegame(string loadingVersion)
		{
			oldPackedTents.Clear();
			oldDeployedTents.Clear();
			newDeployedTents.Clear();
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
			oldPackedTents.Clear();
			oldDeployedTents.Clear();
			newDeployedTents.Clear();
			partReplacements.Clear();
		}
	}
}
