namespace NShared;

[JsonSerializable(Javascript = true)]
public class HeroRunePageInfo : IAccountAttrKey<HeroRunePageInfoKey>
{
	HeroRunePageInfoKey IAccountAttrKey<HeroRunePageInfoKey>.Key => new HeroRunePageInfoKey(HeroIndex, RunePage, SlotNum);

	public int HeroIndex { get; set; }

	public int RunePage { get; set; }

	public int SlotNum { get; set; }

	public int EquipItemSlotIndex { get; set; }

	public int ItemIndex { get; set; }
}
