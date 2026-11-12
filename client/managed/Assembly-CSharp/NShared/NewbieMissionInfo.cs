namespace NShared;

[JsonSerializable(Javascript = true)]
public class NewbieMissionInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => MissionIndex;

	public int MissionIndex { get; set; }

	public string UpdateTime { get; set; }
}
