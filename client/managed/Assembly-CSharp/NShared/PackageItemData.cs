using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PackageItemData : IItemSubData, IDataKeySelector<string>
{
	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public int RewardRate1 { get; set; }

	[Key(2)]
	public int RewardIndex1 { get; set; }

	[Key(3)]
	public int RewardRate2 { get; set; }

	[Key(4)]
	public int RewardIndex2 { get; set; }

	[Key(5)]
	public int RewardRate3 { get; set; }

	[Key(6)]
	public int RewardIndex3 { get; set; }

	[Key(7)]
	public int RewardRate4 { get; set; }

	[Key(8)]
	public int RewardIndex4 { get; set; }

	[Key(9)]
	public int RewardRate5 { get; set; }

	[Key(10)]
	public int RewardIndex5 { get; set; }

	[Key(11)]
	public int MaxOpenCount { get; set; }

	[Key(12)]
	public byte OpenWithdrawal { get; set; }

	[Key(13)]
	public byte ShowContent { get; set; }

	[Key(14)]
	public string WithdrawTextKey { get; set; }

	[Key(15)]
	public bool HasEquip { get; set; }

	string IDataKeySelector<string>.Key => ItemCode;

	public override string ToString()
	{
		return "ItemCode=" + ItemCode;
	}
}
