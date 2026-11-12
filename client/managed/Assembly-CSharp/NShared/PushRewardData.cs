using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class PushRewardData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int DayOfTheWeek { get; set; }

	[Key(2)]
	public int OpenTime { get; set; }

	[Key(3)]
	public int EndTime { get; set; }

	[Key(4)]
	public string TouchAction { get; set; }

	[Key(5)]
	public int RewardIndex { get; set; }

	[Key(6)]
	public string NameKey { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
