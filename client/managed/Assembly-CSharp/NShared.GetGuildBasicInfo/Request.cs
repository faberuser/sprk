namespace NShared.GetGuildBasicInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/get_guildbasicinfo";
}
