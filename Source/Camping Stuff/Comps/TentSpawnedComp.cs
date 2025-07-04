using Verse;

namespace Camping_Stuff;

public class TentSpawnedComp : ThingComp //(Thing)
{
	public CompProperties_TentSpawnedComp Props
	{
		get
		{
			return (CompProperties_TentSpawnedComp)this.props;
		}
	}

	public NCS_Tent tent;

	public override void PostExposeData()
	{
		base.PostExposeData();

		Scribe_References.Look(ref this.tent, "tentSpawnedBy");
	}
}

public class CompProperties_TentSpawnedComp : CompProperties //(Def)
{
	public TentPart partType;

	public CompProperties_TentSpawnedComp()
	{
		this.compClass = typeof(TentSpawnedComp);
	}
}
