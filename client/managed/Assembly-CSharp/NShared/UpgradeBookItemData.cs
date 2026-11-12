using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class UpgradeBookItemData : IItemSubData, IDataKeySelector<string>
{
	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public CreatureTagType TagType { get; set; }

	[Key(2)]
	public int Grade { get; set; }

	string IDataKeySelector<string>.Key => ItemCode;

	public override string ToString()
	{
		return "ItemCode=" + ItemCode + " (TagType=" + TagType.ToString() + ", Grade=" + Grade + ")";
	}
}
