namespace NShared;

[JsonSerializable(Javascript = true)]
public class EclipseDeckResult
{
	public int DeckIndex { get; set; }

	public string HeroIndices { get; set; }

	public CachedHeroInfo[] CachedHeroInfos { get; set; }

	public string UpdatedTime { get; set; }

	[NonDbSchema]
	public int ClearMaxWaveIndex { get; set; }
}
