namespace NShared.GetGuildAttendance;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/get_guild_attendance";
}
