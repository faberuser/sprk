namespace NShared;

[JsonSerializable(Javascript = true)]
public class MatchRankerInfo
{
	public int Rank { get; set; }

	public int MatchScore { get; set; }

	public string CountryCode { get; set; }

	public CachedUserInfo UserInfo { get; set; }

	public DeckHeroInfo[] HeroInfos { get; set; }
}
