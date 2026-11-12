using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class RewardItemIconOrderData : IDataKeySelector<RewardItemIconTypeDataKey>
{
	[Key(0)]
	public RewardItemIconType RewardItemIconType { get; set; }

	[Key(1)]
	public CurrencyType CurrencyType { get; set; }

	[Key(2)]
	public int Priority { get; set; }

	[Key(3)]
	public bool Sum { get; set; }

	RewardItemIconTypeDataKey IDataKeySelector<RewardItemIconTypeDataKey>.Key => new RewardItemIconTypeDataKey(RewardItemIconType, CurrencyType);
}
