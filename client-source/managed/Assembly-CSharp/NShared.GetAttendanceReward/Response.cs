namespace NShared.GetAttendanceReward;

[Response]
public class Response : GenericResponse<ResultType>
{
	public DbRewardResult AttendanceRewardResult { get; set; }

	public AttendanceInfo[] AttendanceInfos { get; set; }

	public AttendanceData[] AttendanceDatas { get; set; }

	public string NewbiePopUpEndTime { get; set; }
}
