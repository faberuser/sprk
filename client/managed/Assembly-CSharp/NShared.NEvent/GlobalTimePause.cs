namespace NShared.NEvent;

[JsonSerializable]
public class GlobalTimePause : BaseGlobalTime
{
	public float BattleStartTime { get; set; }

	public float GolbalStartTime { get; set; }
}
