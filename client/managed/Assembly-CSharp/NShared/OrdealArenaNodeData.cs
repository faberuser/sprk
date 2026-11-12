using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class OrdealArenaNodeData : IDataKeySelector<ChapterDungeonKey2>
{
	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int DungeonIndex { get; set; }

	[Key(2)]
	public int Index { get; set; }

	[Key(3)]
	public int Floor { get; set; }

	[Key(4)]
	public EOrdealArenaNodeType NodeType { get; set; }

	[Key(5)]
	public int RewardIndex { get; set; }

	[Key(6)]
	public string DescKey { get; set; }

	ChapterDungeonKey2 IDataKeySelector<ChapterDungeonKey2>.Key => new ChapterDungeonKey2(ChapterIndex, DungeonIndex);
}
