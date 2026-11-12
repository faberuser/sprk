namespace NShared.SetGuildAttendance;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/set_guild_attendance";
}
