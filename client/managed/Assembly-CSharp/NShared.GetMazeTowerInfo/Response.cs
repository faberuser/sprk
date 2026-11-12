namespace NShared.GetMazeTowerInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int MazeTowerRewardStep { get; set; }

	public int MazeKey { get; set; }

	public string MazeKeyRechargeTime { get; set; }
}
