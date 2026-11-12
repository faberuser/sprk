using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PetSelectItemData : IItemSubData, IDataKeySelector<string>
{
	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public int PetStar { get; set; }

	[Key(2)]
	public int PetLevel { get; set; }

	[Key(3)]
	public int[] PetIndices { get; set; }

	string IDataKeySelector<string>.Key => ItemCode;

	public override string ToString()
	{
		return "ItemCode=" + ItemCode;
	}
}
