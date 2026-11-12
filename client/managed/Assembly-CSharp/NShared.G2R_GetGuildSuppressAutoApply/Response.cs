namespace NShared.G2R_GetGuildSuppressAutoApply;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public GuildSuppressAutoApplyInfo GuildSuppressAutoApplyInfo { get; set; }
}
