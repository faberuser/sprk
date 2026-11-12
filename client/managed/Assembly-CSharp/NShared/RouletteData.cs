using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class RouletteData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public RouletteType RouletteType { get; set; }

	[Key(2)]
	public string RouletteGuide { get; set; }
}
