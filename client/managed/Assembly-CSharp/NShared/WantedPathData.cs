using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class WantedPathData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int ChapterIndex { get; set; }

	[Key(2)]
	public int[] DungeonIndices { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "WantedPathData Index=" + Index;
	}
}
