using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class AutoSeparationData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public BattleType[] BattleType { get; set; }

	[Key(2)]
	public string LocalAutoKey { get; set; }

	[Key(3)]
	public int ChapterIndex { get; set; }

	[Key(4)]
	public int DungeonIndex { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
