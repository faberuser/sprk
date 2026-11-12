using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class TreasureHouseDungeonData : IDataKeySelector<TreasureHouseDungeonKey>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int InfoIndex { get; set; }

	[Key(2)]
	public int ShowIndex { get; set; }

	[Key(3)]
	public int ChapterIndex { get; set; }

	[Key(4)]
	public int DungeonIndex { get; set; }

	[Key(5)]
	public int CreatureIndex { get; set; }

	[Key(6)]
	public string IconPortraitPath { get; set; }

	[Key(7)]
	public string SelectIconPortraitPath { get; set; }

	[Key(8)]
	public int RewardIndex { get; set; }

	[Key(9)]
	public int Ratio { get; set; }

	[Key(10)]
	public int SequenceIndex { get; set; }

	[Key(11)]
	public int[] CreaturePosition { get; set; }

	[Key(12)]
	public int[] CreatureRotation { get; set; }

	[Key(13)]
	public int CreatureScale { get; set; }

	TreasureHouseDungeonKey IDataKeySelector<TreasureHouseDungeonKey>.Key => new TreasureHouseDungeonKey(InfoIndex, ShowIndex);

	public override string ToString()
	{
		return "TreasureHouseData Index=" + Index;
	}
}
