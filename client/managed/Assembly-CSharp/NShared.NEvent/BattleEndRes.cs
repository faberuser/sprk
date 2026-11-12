namespace NShared.NEvent;

[JsonSerializable]
public class BattleEndRes : BaseNodeEvent
{
	public PlayerBattleInfo BattleInfo { get; set; }

	public MatchResultInfo MatchResult { get; set; }
}
