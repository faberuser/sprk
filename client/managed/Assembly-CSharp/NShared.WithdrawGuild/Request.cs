namespace NShared.WithdrawGuild;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/withdraw_guild";
}
