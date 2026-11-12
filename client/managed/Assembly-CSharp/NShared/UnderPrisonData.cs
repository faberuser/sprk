using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class UnderPrisonData : IDataKeySelector<int>
{
	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public string Name { get; set; }

	[Key(2)]
	public string Desc { get; set; }

	[Key(3)]
	public UnderPrisonType Type { get; set; }

	[Key(4)]
	public int DailyEnterMaxCount { get; set; }

	[Key(5)]
	public bool IsSelectReward { get; set; }

	int IDataKeySelector<int>.Key => ChapterIndex;
}
