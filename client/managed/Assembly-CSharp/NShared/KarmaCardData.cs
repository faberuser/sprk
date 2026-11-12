using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class KarmaCardData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int GroupIndex { get; set; }

	[Key(2)]
	public int GroupChance { get; set; }

	[Key(3)]
	public int UniqueChance { get; set; }

	[Key(4)]
	public EKarmaCardType CardType { get; set; }

	[Key(5)]
	public EKarmaRarity Rarity { get; set; }

	[Key(6)]
	public int MaxStack { get; set; }

	[Key(7)]
	public int AppearChance { get; set; }

	[Key(8)]
	public int[] SkillOptions { get; set; }

	[Key(9)]
	public string BuffIconPath { get; set; }

	[Key(10)]
	public int BuffValue { get; set; }

	[Key(11)]
	public string RarityDesc { get; set; }

	[Key(12)]
	public string CardDescCode { get; set; }

	[Key(13)]
	public CreatureTagType[] ClassTypes { get; set; }

	[Key(14)]
	public int ShowIndex { get; set; }

	[Key(15)]
	public string IconPath { get; set; }

	[Key(16)]
	public int BuffIconIndex { get; set; }

	[Key(17)]
	public string BuffValueUnit { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
