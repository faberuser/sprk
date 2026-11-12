namespace NShared;

public class HeroStatsKey : Tuple<string, long>
{
	public HeroStatsKey(string ServerGroup, long AccountId)
		: base(ServerGroup, AccountId)
	{
	}
}
