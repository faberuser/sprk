namespace NShared.GetAttendanceReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "attendance/get_attendance_reward";

	public int AttendanceIndex { get; set; }
}
