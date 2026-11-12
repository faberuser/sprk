using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class NPCFriendlyPointItemData : IItemSubData, IDataKeySelector<string>
{
	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public int[] HeroIndices { get; set; }

	[Key(2)]
	public int FriendlyPoint { get; set; }

	[Key(3)]
	public int EventGiftPoint { get; set; }

	string IDataKeySelector<string>.Key => ItemCode;

	public override string ToString()
	{
		return "ItemCode=" + ItemCode + " (" + FriendlyPoint + " " + FriendlyPoint + ")";
	}
}
