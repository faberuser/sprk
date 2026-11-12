namespace NShared;

[JsonSerializable(Javascript = true)]
public class EclipseDeckInfo : IAccountAttrKey<EclipseDeckKey>
{
	EclipseDeckKey IAccountAttrKey<EclipseDeckKey>.Key => new EclipseDeckKey(DeckIndex, HeroIndex);

	public int DeckIndex { get; set; }

	public int HeroIndex { get; set; }

	public string HeroInfo { get; set; }

	public string UpdatedTime { get; set; }
}
