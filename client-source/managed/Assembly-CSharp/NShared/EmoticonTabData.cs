using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EmoticonTabData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string Tab { get; set; }

	[Key(2)]
	public string AssetPath { get; set; }

	[Key(3)]
	public int ShowIndex { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
