namespace NShared;

[JsonSerializable(Javascript = true)]
public class WorldMapEventTimeInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => EventIndex;

	public int EventIndex { get; set; }

	public string LastCoolCheckTime { get; set; }

	public int ReqDungeonCount { get; set; }
}
