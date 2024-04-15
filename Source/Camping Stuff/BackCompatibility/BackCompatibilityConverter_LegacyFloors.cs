using System;
using System.Xml;
using Verse;

namespace Camping_Stuff
{
	public class BackCompatibilityConverter_LegacyFloors : BackCompatibilityConverter
	{
		public override bool AppliesToVersion(int majorVer, int minorVer) => majorVer == 1 && minorVer <= 4;

		public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
		{
			return null;
		}

		public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
		{
			return null;
		}

		public override void PostExposeData(object obj)
		{
			if (Scribe.mode != LoadSaveMode.PostLoadInit)
				return;

			if (obj is NCS_Tent tent && tent.Floor != null)
			{
				tent.floorCache = TentDefOf.NCS_TentFloorRed;
			}

			return;
		}

	}
}
