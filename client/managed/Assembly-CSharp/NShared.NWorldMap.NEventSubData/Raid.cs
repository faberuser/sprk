namespace NShared.NWorldMap.NEventSubData;

[JsonSerializable(Javascript = true)]
public class Raid : EventSubData
{
	public RaidInfo RaidInfo { get; set; }
}
