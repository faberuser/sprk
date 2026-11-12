using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class NewPayShopOpenConditionData : IDataKeySelector<PayShopUserGroup>
{
	[Key(0)]
	public PayShopUserGroup Group { get; set; }

	[Key(1)]
	public string[] Condition { get; set; }

	[Key(2)]
	public string LinkCategory { get; set; }

	[Key(3)]
	public int[] LinkProductIndices { get; set; }

	PayShopUserGroup IDataKeySelector<PayShopUserGroup>.Key => Group;
}
