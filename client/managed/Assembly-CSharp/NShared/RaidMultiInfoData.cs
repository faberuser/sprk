using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class RaidMultiInfoData : IDataKeySelector<int>
{
	[Key(0)]
	public int RaidIndex { get; set; }

	[Key(1)]
	public int Index { get; set; }

	[Key(2)]
	public int ChapterIndex { get; set; }

	[Key(3)]
	public int DungeonIndex { get; set; }

	[Key(4)]
	public int Level { get; set; }

	[Key(5)]
	public int WaveIndex { get; set; }

	[Key(6)]
	public int GroupIndex { get; set; }

	[Key(7)]
	public string Tactic { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
