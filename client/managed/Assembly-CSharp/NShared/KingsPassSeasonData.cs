using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class KingsPassSeasonData : IDataKeySelector<int>
{
	[Key(0)]
	public int SeasonIndex { get; set; }

	[Key(1)]
	public string BeginTime { get; set; }

	[Key(2)]
	public string EndTime { get; set; }

	[Key(3)]
	public string TitleKey { get; set; }

	[Key(4)]
	public int ExpIndex { get; set; }

	[Key(5)]
	public int LevelIndex { get; set; }

	[Key(6)]
	public int ProductIndex { get; set; }

	int IDataKeySelector<int>.Key => SeasonIndex;

	public override string ToString()
	{
		return "Index=" + SeasonIndex;
	}
}
