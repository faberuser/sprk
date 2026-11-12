namespace NShared;

[JsonSerializable(Javascript = true)]
public class RaidResultInfo
{
	public string LastRaidResetTime { get; set; }

	public RaidInfo[] RaidInfos { get; set; }
}
