using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PartyDungeonData : IDataKeySelector<ChapterDungeonKey2>
{
	ChapterDungeonKey2 IDataKeySelector<ChapterDungeonKey2>.Key => new ChapterDungeonKey2(ChapterIndex, DungeonIndex);

	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int DungeonIndex { get; set; }

	[Key(2)]
	public PartyDungeonType DungeonType { get; set; }

	[Key(3)]
	public PartyDungeonChapter Chapter { get; set; }

	[Key(4)]
	public bool Easy { get; set; }

	[Key(5)]
	public bool Normal { get; set; }

	[Key(6)]
	public bool Hard { get; set; }

	[Key(7)]
	public bool Hell { get; set; }

	[Key(8)]
	public int PlayerCount { get; set; }

	[Key(9)]
	public int DeckCount { get; set; }

	[Key(10)]
	public int DeckCountPerPlayer { get; set; }

	[Key(11)]
	public int IncreaseRatio { get; set; }

	[Key(12)]
	public int DecreaseRatio { get; set; }

	[Key(13)]
	public int AddtionalReward { get; set; }

	[Key(14)]
	public string PortraitPath { get; set; }

	public bool IsAbleDiff(PartyDungeonDifficultyType diff)
	{
		if (diff == PartyDungeonDifficultyType.Easy && Easy)
		{
			return true;
		}
		if (diff == PartyDungeonDifficultyType.Normal && Normal)
		{
			return true;
		}
		if (diff == PartyDungeonDifficultyType.Hard && Hard)
		{
			return true;
		}
		if (diff == PartyDungeonDifficultyType.Hell && Hell)
		{
			return true;
		}
		return false;
	}
}
