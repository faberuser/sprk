using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipItemStarData : IDataKeySelector<int>
{
	[Key(0)]
	public int Star { get; set; }

	[Key(1)]
	public int RuneSlotOpen { get; set; }

	[Key(2)]
	public int BuyPriceFactor { get; set; }

	[Key(3)]
	public int SellPriceFactor { get; set; }

	[Key(4)]
	public int StatFactor { get; set; }

	int IDataKeySelector<int>.Key => Star;

	public override string ToString()
	{
		return "Star=" + Star;
	}
}
