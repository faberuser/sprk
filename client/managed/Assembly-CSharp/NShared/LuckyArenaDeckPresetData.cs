using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class LuckyArenaDeckPresetData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string DeckName { get; set; }

	[Key(2)]
	public string RouletteDeckName { get; set; }

	[Key(3)]
	public int Hero1 { get; set; }

	[Key(4)]
	public int Hero2 { get; set; }

	[Key(5)]
	public int Hero3 { get; set; }

	[Key(6)]
	public int Hero4 { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index;
	}

	public List<int> GetHeroPresetIndexList()
	{
		return new List<int> { Hero1, Hero2, Hero3, Hero4 };
	}
}
