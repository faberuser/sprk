namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class PartyBattleStart
{
	public string BattleServerAddress { get; set; }

	public int BattleServerPort { get; set; }

	public RewardItemInfo[] RewardItems { get; set; }

	public string MasterServerAddress { get; set; }

	public int MasterServerPort { get; set; }

	public int RaidIndex { get; set; }

	public int RaidLevel { get; set; }

	public bool OnetimeBooster { get; set; }

	public int[] HeroIndices { get; set; }

	public long[] AccountIds { get; set; }
}
