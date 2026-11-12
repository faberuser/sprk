using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class MatchBanPickRewardData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public MatchRewardType Type { get; set; }

	[Key(2)]
	public string Name { get; set; }

	[Key(3)]
	public string Desc { get; set; }

	[Key(4)]
	public int RewardIndex { get; set; }

	[Key(5)]
	public MatchSeasonType[] Season { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index + " MatchRewardType =" + Type;
	}
}
