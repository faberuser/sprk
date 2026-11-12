namespace NShared.NWorldMap.NEventSubData;

[JsonSerializable(Javascript = true)]
public class EventDungeon : EventSubData
{
	public int EventDungeonIndex { get; set; }
}
