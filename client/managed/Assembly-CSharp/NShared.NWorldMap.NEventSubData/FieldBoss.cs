namespace NShared.NWorldMap.NEventSubData;

[JsonSerializable(Javascript = true)]
public class FieldBoss : EventSubData
{
	public int BossCreatureIndex { get; set; }
}
