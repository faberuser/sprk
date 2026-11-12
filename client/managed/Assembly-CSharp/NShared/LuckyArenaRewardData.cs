using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class LuckyArenaRewardData : IDataKeySelector<LuckyArenaRewardDataKey>
{
	[Key(0)]
	public LuckyMatchRewardType Type { get; set; }

	[Key(1)]
	public int ShowIndex { get; set; }

	[Key(2)]
	public int CountMin { get; set; }

	[Key(3)]
	public int CountMax { get; set; }

	[Key(4)]
	public int RewardIndex { get; set; }

	LuckyArenaRewardDataKey IDataKeySelector<LuckyArenaRewardDataKey>.Key => new LuckyArenaRewardDataKey(Type, ShowIndex);

	public override string ToString()
	{
		return $"Type={Type} ShowIndex={ShowIndex}";
	}
}
