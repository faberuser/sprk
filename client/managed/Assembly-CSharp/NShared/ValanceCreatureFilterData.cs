using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ValanceCreatureFilterData : IDataKeySelector<int>, IValanceCreatureFilterInfo, IValanceFilterInfo
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int GroupIndex { get; set; }

	[Key(2)]
	public int SortingOrder { get; set; }

	[Key(3)]
	public string LocalKey { get; set; }

	[Key(4)]
	public SkillAttrType AttrType { get; set; }

	[Key(5)]
	public CreatureTagType CreatureTagType { get; set; }

	int IDataKeySelector<int>.Key => Index;

	SkillAttrType IValanceCreatureFilterInfo.AttrType => AttrType;

	CreatureTagType IValanceCreatureFilterInfo.CreatureTagType => CreatureTagType;
}
