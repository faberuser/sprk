using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class OrdealArenaTierData : IDataKeySelector<MatchTierType>
{
	[Key(0)]
	public MatchTierType MatchTierType { get; set; }

	[Key(1)]
	public int StartRating { get; set; }

	[Key(2)]
	public string DescKey { get; set; }

	[Key(3)]
	public int SortingOrder { get; set; }

	[Key(4)]
	public int MoveChapterIndex { get; set; }

	MatchTierType IDataKeySelector<MatchTierType>.Key => MatchTierType;
}
