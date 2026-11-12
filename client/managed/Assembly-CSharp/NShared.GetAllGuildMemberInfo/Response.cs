namespace NShared.GetAllGuildMemberInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildMemberInfo[] MemberInfos { get; set; }
}
