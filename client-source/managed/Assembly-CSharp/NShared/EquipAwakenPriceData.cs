using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipAwakenPriceData : IDataKeySelector<EquipAwakenPriceDataKey>
{
	EquipAwakenPriceDataKey IDataKeySelector<EquipAwakenPriceDataKey>.Key => new EquipAwakenPriceDataKey(DetailIndex, Star, Tier);

	[Key(0)]
	public int DetailIndex { get; set; }

	[Key(1)]
	public int Star { get; set; }

	[Key(2)]
	public int Tier { get; set; }

	[Key(3)]
	public int ReqGold { get; set; }

	[Key(4)]
	public string ItemCode1 { get; set; }

	[Key(5)]
	public int ItemCount1 { get; set; }

	public override string ToString()
	{
		return "DetailIndex=" + DetailIndex + " Star=" + Star + "Tier=" + Tier;
	}
}
