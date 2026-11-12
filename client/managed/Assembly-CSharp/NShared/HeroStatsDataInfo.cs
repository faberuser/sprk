namespace NShared;

[JsonSerializable(Javascript = true)]
public class HeroStatsDataInfo
{
	public UserHeroStatsInfo[] UserHeroStatsInfos { get; set; }
}
