using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ProbabilityBaseData : IDataKeySelector<ProbabilityDataKey>
{
	[IgnoreMember]
	public virtual int Index { get; set; }

	[Key(1)]
	public int SubIndex { get; set; }

	[Key(2)]
	public string ComponentTitle { get; set; }

	[Key(3)]
	public int ComponentCount { get; set; }

	[Key(4)]
	public string ComponentRate { get; set; }

	ProbabilityDataKey IDataKeySelector<ProbabilityDataKey>.Key => new ProbabilityDataKey(Index, SubIndex);
}
