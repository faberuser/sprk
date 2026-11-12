using NShared.NWorldMap;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class WorldMapEventInfo : IAccountAttrKey<WorldMapEventInfoKey>
{
	WorldMapEventInfoKey IAccountAttrKey<WorldMapEventInfoKey>.Key => new WorldMapEventInfoKey(EventIndex, ChapterIndex, DungeonIndex);

	public int EventIndex { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public EventType EventType { get; set; }

	public string SerializedSubData { get; set; }

	public string RewardItems { get; set; }

	public int RewardGold { get; set; }

	public int RewardGem { get; set; }

	public string OccuredTime { get; set; }

	public int Duration { get; set; }

	public bool Completed { get; set; }

	public bool NoExpire { get; set; }

	public bool NoExpireOnComplete { get; set; }

	public int ProgressCount { get; set; }

	public int RequireCount { get; set; }
}
