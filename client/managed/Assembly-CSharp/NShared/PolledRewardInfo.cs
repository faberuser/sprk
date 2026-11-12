namespace NShared;

[JsonSerializable(Javascript = true)]
public class PolledRewardInfo
{
	public long Gold { get; set; }

	public long Gem { get; set; }

	public long PvpCoin { get; set; }

	public long GuildPoint { get; set; }

	public RewardItemInfo[] Items { get; set; }
}
