namespace NShared.GetAllConditionalAttendanceReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "attendance/get_all_conditional_attendance_reward";

	public int[] AttendanceIndex { get; set; }
}
