namespace NShared.LevelUpGuildSkill;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildActivityPointResultInfo GuildActivityPointResult { get; set; }

	public GuildSuppressPointResultInfo GuildSuppressPointResult { get; set; }
}
