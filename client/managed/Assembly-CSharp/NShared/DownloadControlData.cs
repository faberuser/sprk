using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class DownloadControlData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int ChapterIndex { get; set; }

	[Key(2)]
	public string[] Categories { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "DownloadControlData Index=" + Index;
	}
}
