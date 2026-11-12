namespace NShared;

[JsonSerializable]
public class BaseBattleLog
{
	public int No { get; set; }

	public long GlobalTimeMs { get; set; }

	public BattleLogType Type { get; set; }

	public string Data { get; set; }
}
