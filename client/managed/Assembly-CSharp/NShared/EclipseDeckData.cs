namespace NShared;

[JsonSerializable(Javascript = true)]
public class EclipseDeckData
{
	public int DeckIndex { get; set; }

	public CachedHeroInfo[] HeroInfos { get; set; }

	public EclipseHeroConditionInfo[] HeroConditionInfos { get; set; }
}
