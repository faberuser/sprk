using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class GuildRaidChapterData : IDataKeySelector<int>
{
	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public string Name { get; set; }

	[Key(2)]
	public string Description { get; set; }

	[Key(3)]
	public int BonusGroup { get; set; }

	[Key(4)]
	public bool IsBonusDungeon { get; set; }

	[Key(5)]
	public bool IsDummyDungeon { get; set; }

	int IDataKeySelector<int>.Key => ChapterIndex;

	public override string ToString()
	{
		return "GuildRaidChapterIndex=" + ChapterIndex;
	}
}
