using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PetAdventureRewardData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int RewardItemIndex { get; set; }

	[Key(2)]
	public int Time { get; set; }

	[Key(3)]
	public int PenaltyTime { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
