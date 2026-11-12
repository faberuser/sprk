using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class PurchaseDungeonData : IDataKeySelector<ChapterDungeonKey2>
{
	ChapterDungeonKey2 IDataKeySelector<ChapterDungeonKey2>.Key => new ChapterDungeonKey2(ChapterIndex, DungeonIndex);

	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int DungeonIndex { get; set; }

	[Key(2)]
	public bool Buyable { get; set; }

	[Key(3)]
	public CurrencyType CurrencyType { get; set; }

	[Key(4)]
	public int Price { get; set; }

	[Key(5)]
	public string DungeonPortraitPath { get; set; }

	[Key(7)]
	public string DungeonName { get; set; }

	[Key(8)]
	public string RewardLocalKey { get; set; }

	[Key(9)]
	public string BoosterItemCode { get; set; }

	[Key(10)]
	public string[] EnterCondition { get; set; }

	[Key(6)]
	public string DungeonRewardPath { get; set; }
}
