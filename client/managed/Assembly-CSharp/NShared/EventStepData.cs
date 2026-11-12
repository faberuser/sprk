using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EventStepData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public CurrencyType CurrencyType { get; set; }

	[Key(2)]
	public EventStepType Type { get; set; }

	[Key(3)]
	public int Step { get; set; }

	[Key(4)]
	public int TargetScore { get; set; }

	[Key(5)]
	public int RewardIndex { get; set; }

	[Key(6)]
	public string WorldTreeIdleName { get; set; }

	[Key(7)]
	public string WorldTreeGrowName { get; set; }

	[Key(8)]
	public string GetRewardName { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
