using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class HonoraryChamberMenuData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int ParentIndex { get; set; }

	[Key(2)]
	public string Name { get; set; }

	[Key(3)]
	public RecordsOfHonorContentType ContentsType { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
