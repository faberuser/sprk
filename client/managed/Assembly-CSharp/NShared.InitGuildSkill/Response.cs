namespace NShared.InitGuildSkill;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildActivityPointResultInfo GuildActivityPointResult { get; set; }

	public GuildSuppressPointResultInfo GuildSuppressPointResult { get; set; }
}
