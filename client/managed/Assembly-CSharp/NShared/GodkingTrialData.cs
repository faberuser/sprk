using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class GodkingTrialData : IDataKeySelector<ChapterDungeonKey2>
{
	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int DungeonIndex { get; set; }

	[Key(2)]
	public int DifficultyNameKey { get; set; }

	[Key(3)]
	public int RewardIndex { get; set; }

	ChapterDungeonKey2 IDataKeySelector<ChapterDungeonKey2>.Key => new ChapterDungeonKey2(ChapterIndex, DungeonIndex);
}
