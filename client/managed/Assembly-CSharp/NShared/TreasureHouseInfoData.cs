using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class TreasureHouseInfoData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int EnterCount { get; set; }

	[Key(2)]
	public string ContentsDesc { get; set; }

	[Key(3)]
	public string ChangeCurrencyType { get; set; }

	[Key(4)]
	public int ChangeCurrencyCost { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "TreasureHouseInfoData Index=" + Index;
	}
}
