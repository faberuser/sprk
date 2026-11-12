namespace NShared.OrdealInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public MatchTierType SelectedTier { get; set; }

	public int MatchScore { get; set; }

	public OrdealNodeInfo[] OrdealNodeInfos { get; set; }

	public int[] ClearNodeIndices { get; set; }

	public int[] DeadHeroIndices { get; set; }

	public int RemainRefreshCount { get; set; }

	public int[] SelectableBuffIndices { get; set; }

	public int[] SelectedBuffIndices { get; set; }
}
