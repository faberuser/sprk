namespace NShared;

[JsonSerializable(Javascript = true)]
public class KingsPassMission : IAccountAttrKey<KingsPassMissionKey>
{
	KingsPassMissionKey IAccountAttrKey<KingsPassMissionKey>.Key => new KingsPassMissionKey(SeasonIndex, MissionIndex);

	public int SeasonIndex { get; set; }

	public int MissionIndex { get; set; }

	public int LastStep { get; set; }

	public int Progress { get; set; }

	public string RewardedTime { get; set; }
}
