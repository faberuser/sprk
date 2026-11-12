namespace NShared.GetGuildAttendance;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildAttendanceInfo GuildAttendanceInfo { get; set; }

	public GuildMemberAttendanceInfo GuildMemberAttendanceInfo { get; set; }
}
