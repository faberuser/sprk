namespace NShared.G2R_SyncHeroStats;

[Request]
public class Request
{
	public string Host { get; set; }

	public HeroStatsDataInfo[] HeroStatsDataInfos { get; set; }
}
