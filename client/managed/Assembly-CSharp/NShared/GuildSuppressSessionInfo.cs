namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildSuppressSessionInfo
{
	public long SessionIndex { get; set; }

	public string ApplyEndTime { get; set; }

	public string BattleEndTime { get; set; }

	public string SessionStartedTime { get; set; }

	public string BattleStartedTime { get; set; }

	public GuildSuppressSessionState State { get; set; }
}
