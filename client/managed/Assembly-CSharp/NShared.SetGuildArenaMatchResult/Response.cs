namespace NShared.SetGuildArenaMatchResult;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int Win { get; set; }

	public int MySessionScore { get; set; }

	public int AddSessionScore { get; set; }

	public ItemResultInfo[] RewardItems { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }
}
