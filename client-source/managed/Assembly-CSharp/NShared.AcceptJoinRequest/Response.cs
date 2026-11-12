namespace NShared.AcceptJoinRequest;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildMemberInfo MemberInfo { get; set; }
}
