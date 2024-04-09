using System;
using System.Xml;
using Verse;

namespace Camping_Stuff
{
	public class LayoutSpawn
	{
		public TentLayout part = TentLayout.other;
		public ThingDef def;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			var code = xmlRoot.Name.Replace("NCS_part_", "");
			part = (TentLayout)Enum.Parse(typeof(TentLayout), code);

			if (!xmlRoot.HasChildNodes)
				return;

			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef((object)this, "def", xmlRoot.FirstChild.Value);
		}
	}
}
