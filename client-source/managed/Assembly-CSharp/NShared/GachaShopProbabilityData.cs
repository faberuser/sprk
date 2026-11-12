using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class GachaShopProbabilityData : IDataKeySelector<GachaShopProbabilityDataKey>
{
	[Key(0)]
	public int GachaIndex { get; set; }

	[Key(1)]
	public HighGachaCategoryType GachaType { get; set; }

	[Key(2)]
	public int[] ProbabilityIndices { get; set; }

	[Key(3)]
	public string[] GachaNames { get; set; }

	GachaShopProbabilityDataKey IDataKeySelector<GachaShopProbabilityDataKey>.Key => new GachaShopProbabilityDataKey(GachaIndex, GachaType);

	public override string ToString()
	{
		return "GachaShopProbabilityData GachaIndex=" + GachaIndex;
	}
}
