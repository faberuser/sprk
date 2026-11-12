using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class StoreCurrencyUnitData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string From { get; set; }

	[Key(2)]
	public string To { get; set; }

	[Key(3)]
	public CurrencyChangeType ChangeType { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
