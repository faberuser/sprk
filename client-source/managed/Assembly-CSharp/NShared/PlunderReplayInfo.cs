namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlunderReplayInfo
{
	public string FieldName { get; set; }

	public ReplayTeamInfo Attacker { get; set; }

	public ReplayTeamInfo Defender { get; set; }

	public bool Defended { get; set; }

	public int StolenItemIndex { get; set; }

	public int StolenItemCount { get; set; }

	public int RewardItemIndex { get; set; }

	public int RewardItemCount { get; set; }

	public int PlayTimeMs { get; set; }

	public string RecordedTime { get; set; }
}
