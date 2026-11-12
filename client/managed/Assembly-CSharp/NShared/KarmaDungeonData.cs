using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class KarmaDungeonData : IDataKeySelector<KarmaDungeonDataKey>
{
	[Key(0)]
	public int PunishmentGroupIndex { get; set; }

	[Key(1)]
	public int StageLevel { get; set; }

	[Key(2)]
	public int NextLevel { get; set; }

	[Key(3)]
	public int ChapterIndex { get; set; }

	[Key(4)]
	public int DungeonIndex { get; set; }

	[Key(5)]
	public int TotalWaveCount { get; set; }

	KarmaDungeonDataKey IDataKeySelector<KarmaDungeonDataKey>.Key => new KarmaDungeonDataKey(PunishmentGroupIndex, StageLevel);
}
