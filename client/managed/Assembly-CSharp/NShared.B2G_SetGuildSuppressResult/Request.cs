namespace NShared.B2G_SetGuildSuppressResult;

[BattlePacket]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/b2g_set_guild_suppress_result";

	public long AccountId { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int RaidIndex { get; set; }

	public int RaidLevel { get; set; }

	public int PlayTime { get; set; }

	public bool Win { get; set; }

	public bool IsMaster { get; set; }

	public bool IsLogOn { get; set; }

	public int AvgHeroLevel { get; set; }

	public int[] AliveHeroIndices { get; set; }

	public string PartyInfos { get; set; }

	public long BossTotalDamaged { get; set; }

	public int TotalHeroCount { get; set; }

	public long MaxHp { get; set; }

	public long CurrentHp { get; set; }

	public string BattleLogString { get; set; }

	public long[] OfflineAccountIds { get; set; }
}
