using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class VideoData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string Path { get; set; }

	[Key(2)]
	public string Code { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "VideoData Index=" + Index;
	}
}
