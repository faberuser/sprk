namespace NShared.GetAllConditionalAttendanceReward;

[Response]
public class Response : GenericResponse<ResultType>
{
	public DbRewardResult AttendanceRewardResult { get; set; }

	public AttendanceInfo[] AttendanceInfos { get; set; }
}
