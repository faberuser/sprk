using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class EquipItemDetailData : IDataKeySelector<int>
{
	[Key(0)]
	public int DetailIndex { get; set; }

	[Key(1)]
	public EquipItemDetailType EquipItemDetailType { get; set; }

	[Key(2)]
	public bool IsUnique { get; set; }

	[Key(3)]
	public int MaxLevel { get; set; }

	[Key(4)]
	public bool MaxLevelByHero { get; set; }

	[Key(5)]
	public int ReqAwakeMaterialLevel { get; set; }

	[Key(6)]
	public int MaxStar { get; set; }

	[Key(7)]
	public int OptionCount { get; set; }

	[Key(8)]
	public bool AllOptionOpen { get; set; }

	[Key(9)]
	public bool RenewEnable { get; set; }

	[Key(10)]
	public int TotalOptionStepUpgradeCount { get; set; }

	[Key(11)]
	public int RenewableOptionCount { get; set; }

	[Key(12)]
	public CurrencyType RenewCurrencyType { get; set; }

	[Key(13)]
	public int[] RenewPrice { get; set; }

	[Key(14)]
	public int RenewTicketItemIndex { get; set; }

	[Key(15)]
	public bool EnableRenewSameOption { get; set; }

	[Key(16)]
	public string ForgeAwakeSuccessNodeTree { get; set; }

	[Key(17)]
	public string ForgeAwakeFailNodeTree { get; set; }

	[Key(18)]
	public string ForgeRenewSuccessNodeTree { get; set; }

	[Key(19)]
	public string ForgeEnchantSuccessNodeTree { get; set; }

	[Key(20)]
	public int RenewItemIndex1 { get; set; }

	[Key(21)]
	public int RenewItemCount1 { get; set; }

	[Key(22)]
	public int RenewItemIndex2 { get; set; }

	[Key(23)]
	public int RenewItemCount2 { get; set; }

	[Key(24)]
	public int RenewItemIndex3 { get; set; }

	[Key(25)]
	public int RenewItemCount3 { get; set; }

	[Key(26)]
	public int ExtraSkillRenewItemIndex { get; set; }

	[Key(27)]
	public int ExtraSkillRenewItemCount { get; set; }

	int IDataKeySelector<int>.Key => DetailIndex;

	public override string ToString()
	{
		return "DetailIndex=" + DetailIndex;
	}
}
