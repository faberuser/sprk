using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ValanceFilterGroupData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public EValanceFilterGroupType GroupType { get; set; }

	[Key(2)]
	public int SortingOrder { get; set; }

	[Key(3)]
	public string LocalKey { get; set; }

	[Key(4)]
	public bool IsLongComponent { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
