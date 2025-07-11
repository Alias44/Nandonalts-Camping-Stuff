﻿using System;
using System.Xml;
using Verse;

namespace Camping_Stuff;

#if !(RELEASE_1_3 || RELEASE_1_2 || RELEASE_1_1)
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

		if (obj is NCS_Tent tent && tent.ParentHolder == null && tent.Floor != null && tent.deployedFloor == null)
		{
			tent.deployedFloor = TentDefOf.NCS_TentFloorRed;
		}

		return;
	}

}
#endif
