using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EventRouletteData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public EventRouletteType Type { get; set; }

	[Key(2)]
	public CurrencyType CurrencyType { get; set; }

	[Key(3)]
	public int CurrencyValue { get; set; }

	[Key(4)]
	public string ItemCode { get; set; }

	[Key(5)]
	public int ItemCount { get; set; }

	[Key(6)]
	public string DropItemGroupCode { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "EventRouletteData Index=" + Index;
	}
}
