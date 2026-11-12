using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EmoticonData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string Keyword { get; set; }

	[Key(2)]
	public int TabIndex { get; set; }

	[Key(3)]
	public string AssetPath { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
