namespace NShared.SendGuildAttendanceReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/send_guild_attendance_reward";
}
