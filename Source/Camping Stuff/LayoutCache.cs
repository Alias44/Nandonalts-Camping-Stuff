using System.Collections.Generic;
using System.Linq;

using Verse;

namespace Camping_Stuff
{
	public class LayoutCache : GameComponent
	{
		/// <summary>
		/// Container class to hold the layout configuration and any tents actively using it. Custom class allows this information to be written to the save 
		/// (as opposed to a struct/ tuple).
		/// </summary>
		private class LayoutUsage : IExposable
		{
			public TentSpec spec;
			public HashSet<NCS_Tent> usages;

			public LayoutUsage() { } // empty constructor for postexpose

			public LayoutUsage(TentSpec spec, NCS_Tent use)
			{
				this.spec = spec;
				usages = new HashSet<NCS_Tent> { use };
			}

			public void ExposeData()
			{
				if ((Scribe.mode == LoadSaveMode.Saving && usages.Count > 0) || Scribe.mode != LoadSaveMode.Saving)
				{
					Scribe_Deep.Look(ref spec, "tentLayouts");
					Scribe_Collections.Look(ref usages, false, "usage", LookMode.Reference);
				}
			}
		}

		private Dictionary<int, LayoutUsage> layoutUsages = new Dictionary<int, LayoutUsage>();

		public LayoutCache(Game theGame) : base()
		{

		}

		public void Add(TentSpec spec, NCS_Tent use)
		{
			int hash = spec.GetHashCode();

			if (layoutUsages.ContainsKey(hash))
			{
				layoutUsages[hash].usages.Add(use);
			}
			else
			{
				layoutUsages[hash] = new LayoutUsage(spec, use);
			}
		}

		public bool Remove(int hash, NCS_Tent use)
		{
			if (layoutUsages.ContainsKey(hash))
			{
				layoutUsages[hash].usages.Remove(use);
			}

			return false;
		}

		public TentSpec GetSpec(int hash)
		{
			return layoutUsages[hash].spec;
		}

		private void Cleanup()
		{
			layoutUsages
				.Where(kv => kv.Value.usages.Count == 0)
				.ToList()
				.ForEach(kv => layoutUsages.Remove(kv.Key));
		}

		public override void ExposeData()
		{
			base.ExposeData();

			if (Scribe.mode == LoadSaveMode.Saving && layoutUsages != null)
			{
				Cleanup();
			}

			Scribe_Collections.Look(ref layoutUsages, "layoutUsage", LookMode.Value, LookMode.Deep);

			if (Scribe.mode == LoadSaveMode.PostLoadInit && layoutUsages == null)
			{
				layoutUsages = new Dictionary<int, LayoutUsage>();
			}
		}

	}
}
