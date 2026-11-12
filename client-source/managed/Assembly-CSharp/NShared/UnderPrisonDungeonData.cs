using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class UnderPrisonDungeonData : IDataKeySelector<UnderPrisonDungeonKey>
{
	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int DungeonIndex { get; set; }

	[Key(2)]
	public int Diff { get; set; }

	UnderPrisonDungeonKey IDataKeySelector<UnderPrisonDungeonKey>.Key => new UnderPrisonDungeonKey(ChapterIndex, Diff);
}
