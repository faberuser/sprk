using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ValanceIdentifiedData : IDataKeySelector<int>
{
	[Key(0)]
	public int DetailIndex { get; set; }

	[Key(1)]
	public int ReqGold { get; set; }

	int IDataKeySelector<int>.Key => DetailIndex;
}
