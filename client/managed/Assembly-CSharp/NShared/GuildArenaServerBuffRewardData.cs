using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class GuildArenaServerBuffRewardData : IDataKeySelector<int>
{
	[Key(0)]
	public int MinRank { get; set; }

	[Key(1)]
	public int MaxRank { get; set; }

	[Key(2)]
	public int ItemIndex { get; set; }

	int IDataKeySelector<int>.Key => MinRank;
}
