namespace NShared;

[JsonSerializable(Javascript = true)]
public class RaidInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => RaidIndex;

	public int RaidIndex { get; set; }

	public int RaidLevel { get; set; }

	public int BattleScore { get; set; }

	public string CreatedTime { get; set; }

	public string CompletedTime { get; set; }
}
