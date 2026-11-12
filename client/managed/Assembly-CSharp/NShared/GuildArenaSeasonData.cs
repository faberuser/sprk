using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class GuildArenaSeasonData : IDataKeySelector<int>
{
	[Key(0)]
	public int SeasonIndex { get; set; }

	[Key(1)]
	public string DisplaySeasonText { get; set; }

	[Key(2)]
	public string DisplaySeasonLocalKey { get; set; }

	[Key(3)]
	public string Begin { get; set; }

	[Key(4)]
	public string End { get; set; }

	[Key(5)]
	public int SeasonType { get; set; }

	int IDataKeySelector<int>.Key => SeasonIndex;
}
