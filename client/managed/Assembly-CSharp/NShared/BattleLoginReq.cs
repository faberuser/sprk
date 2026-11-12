namespace NShared;

[BattlePacket]
public class BattleLoginReq : BaseRequest
{
	public long MatchUid { get; set; }

	public long AccountId { get; set; }

	public long MasterId { get; set; }

	public string GameServerHost { get; set; }

	public string MatchServerHost { get; set; }

	public int[] HeroIndices { get; set; }

	public int[] AiHeroIndices { get; set; }

	public int[] GroupHeroIndices { get; set; }

	public CachedUserInfo[] UserInfos { get; set; }

	public bool IsEclipse { get; set; }

	public bool IsEclipseContinuousBattle { get; set; }

	public int GameSpeedRatio { get; set; }

	public BattleInstanceExtraData ExtraData { get; set; }

	public bool EnableTraceLogToFile { get; set; }
}
