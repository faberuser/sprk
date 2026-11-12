using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CodecSupportListData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string VideoType { get; set; }

	[Key(2)]
	public string CodecType { get; set; }

	[Key(3)]
	public string OS { get; set; }

	[Key(4)]
	public string OSVersion { get; set; }

	[Key(5)]
	public string ModelManufacturer { get; set; }

	[Key(6)]
	public string TargetSpec { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
