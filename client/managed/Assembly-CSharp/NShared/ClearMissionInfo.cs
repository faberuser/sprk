namespace NShared;

[JsonSerializable(Javascript = true)]
public class ClearMissionInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => MissionIndex;

	public int MissionIndex { get; set; }

	public int LastStep { get; set; }

	public long Progress { get; set; }

	public string RewardedTime { get; set; }
}
