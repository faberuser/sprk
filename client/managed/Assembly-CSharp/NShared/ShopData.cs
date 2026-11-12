using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ShopData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(2)]
	public string Code { get; set; }

	[Key(3)]
	public int HeroIndex { get; set; }

	[Key(4)]
	public ShopCostType BuyCostType { get; set; }

	[Key(5)]
	public ShopCostType[] BuyCostTypeUI { get; set; }

	[Key(6)]
	public string[] BuyCostTypeIconSprite { get; set; }

	[Key(7)]
	public int MaxStock { get; set; }

	[Key(8)]
	public string[] Timeline { get; set; }

	[Key(9)]
	public string[] Buyline { get; set; }

	[Key(10)]
	public string GroupTitleKey { get; set; }

	[Key(11)]
	public bool BeforeBuyConfirm { get; set; }

	[Key(12)]
	public int RestockSecond { get; set; }

	[Key(13)]
	public int RestockGem { get; set; }

	[Key(14)]
	public byte EventOnly { get; set; }

	[Key(15)]
	public int MiniGuideIndex { get; set; }

	[Key(16)]
	public bool UseGroupIndexForLevel { get; set; }

	[Key(17)]
	public bool MultiplePurchaseShop { get; set; }

	[Key(18)]
	public EShopItemFilterGroupType ShopFilterGroupType { get; set; }

	[IgnoreMember]
	public string Name => GameRoot.LocalizeProxy.GetCache_or_Build(this, "Name", BuildLocalize_Name);

	public override string ToString()
	{
		return "Index=" + Index + " (" + Name + ")";
	}

	private string BuildLocalize_Name()
	{
		return GameRoot.LocalizeProxy.GetString("SHOP_NAME_" + Code.ToUpper());
	}
}
