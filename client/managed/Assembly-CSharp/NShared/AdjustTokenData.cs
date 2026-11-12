using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class AdjustTokenData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string EventName { get; set; }

	[Key(2)]
	public string Token { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
