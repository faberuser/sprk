using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class GachaProbabilityData : IDataKeySelector<ProbabilityDataKey2>
{
	[Key(0)]
	public int GachaIndex { get; set; }

	[Key(1)]
	public int SubIndex { get; set; }

	[Key(2)]
	public string ComponentTitle { get; set; }

	[Key(3)]
	public int ComponentCount { get; set; }

	[Key(4)]
	public string ComponentRate { get; set; }

	[Key(5)]
	public int InformIndex { get; set; }

	ProbabilityDataKey2 IDataKeySelector<ProbabilityDataKey2>.Key => new ProbabilityDataKey2(GachaIndex, SubIndex);

	public override string ToString()
	{
		return "ProbabilityData Index=" + GachaIndex;
	}
}
