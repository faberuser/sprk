namespace NShared.EndOrdealArena;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int[] DeadHeroIndices { get; set; }

	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public int[] SelectableBuffIndices { get; set; }

	public RewardResultInfo AreaRewardInfo { get; set; }

	public int[] ClearNodeIndices { get; set; }

	public int SurvivePoint { get; set; }

	public int TimePoint { get; set; }

	public int HpPoint { get; set; }

	public int PointResult { get; set; }

	public int AddMatchScore { get; set; }

	public int ResultMatchScore { get; set; }
}
