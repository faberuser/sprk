namespace NShared.GetPhaseStepInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int DailyAchievementStep { get; set; }

	public int TotalAchievementStep { get; set; }

	public int TotalCount { get; set; }
}
