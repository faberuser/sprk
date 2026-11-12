namespace NShared.NRaid;

[JsonSerializable(Javascript = true)]
public class RankerInfo
{
	public string Nick { get; set; }

	public int AvatarHeroIndex { get; set; }

	public int AccountId { get; set; }
}
