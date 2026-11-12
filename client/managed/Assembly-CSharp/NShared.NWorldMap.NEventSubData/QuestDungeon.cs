namespace NShared.NWorldMap.NEventSubData;

[JsonSerializable(Javascript = true)]
public class QuestDungeon : EventSubData
{
	public int MonsterIndex { get; set; }
}
