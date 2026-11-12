namespace NShared;

[JsonSerializable(Javascript = true)]
public class UserHeroStatsInfo : IAccountAttrKey<HeroStatsKey>
{
	HeroStatsKey IAccountAttrKey<HeroStatsKey>.Key => new HeroStatsKey(ServerGroup, AccountId);

	public string ServerGroup { get; set; }

	public long AccountId { get; set; }

	public int TeamLevel { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int RegTime { get; set; }

	public HeroData[] Heroes { get; set; }
}
