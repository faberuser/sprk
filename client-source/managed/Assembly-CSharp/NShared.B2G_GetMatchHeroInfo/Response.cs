namespace NShared.B2G_GetMatchHeroInfo;

[BattlePacket]
public class Response : GenericResponse<ResultType>
{
	public CachedHeroInfo[] HeroInfos { get; set; }
}
