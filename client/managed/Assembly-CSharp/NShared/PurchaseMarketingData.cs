using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PurchaseMarketingData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int RepeatCooltime { get; set; }

	[Key(2)]
	public int DurationTime { get; set; }

	[Key(3)]
	public bool IsOpen { get; set; }

	[Key(4)]
	public int ProductIndex { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "PurchaseMarketingData Index=" + Index;
	}
}
