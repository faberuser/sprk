using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ItemProbabilityData : IDataKeySelector<ProbabilityDataKey2>
{
	ProbabilityDataKey2 IDataKeySelector<ProbabilityDataKey2>.Key => new ProbabilityDataKey2(ItemIndex, SubIndex);

	[Key(0)]
	public int ItemIndex { get; set; }

	[Key(1)]
	public int SubIndex { get; set; }

	[Key(2)]
	public int InformIndex { get; set; }

	[Key(3)]
	public string ComponentTitle { get; set; }

	[Key(4)]
	public int ComponentCount { get; set; }

	[Key(5)]
	public string ComponentRate { get; set; }

	public override string ToString()
	{
		return "ProbabilityData ItemIndex=" + ItemIndex + "SubIndex = " + SubIndex;
	}
}
