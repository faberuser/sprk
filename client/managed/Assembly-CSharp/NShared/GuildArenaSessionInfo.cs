namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildArenaSessionInfo
{
	public int SessionIndex { get; set; }

	public int SeasonIndex { get; set; }

	public int SessionNumber { get; set; }

	public string BeginTime { get; set; }

	public string EndTime { get; set; }

	public string ApplyEndTime { get; set; }

	public string BattleBeginTime { get; set; }

	public string BattleEndTime { get; set; }

	public GuildArenaSessionState State { get; set; }

	public int IsFirstSession { get; set; }

	public int IsLastSession { get; set; }

	public string ApplyEndedTime { get; set; }

	public string SessionRewardedTime { get; set; }

	public string CompletedRewardedTime { get; set; }
}
