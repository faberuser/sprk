namespace NShared.NRaid;

[JsonSerializable(Javascript = true)]
public class MultiPlayChallengeRaidRankerInfo
{
	public int Rank { get; set; }

	public RankerInfo[] RankerInfos { get; set; }

	public long Score { get; set; }

	public int BattleTime { get; set; }

	public int RaidLevel { get; set; }

	public int[] AffixIndices { get; set; }
}
