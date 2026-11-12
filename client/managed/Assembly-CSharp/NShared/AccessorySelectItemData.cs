using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class AccessorySelectItemData : IItemSubData, IDataKeySelector<string>
{
	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public int AccessoryIndex { get; set; }

	[Key(2)]
	public int AllOwnedRewardIndex { get; set; }

	[Key(3)]
	public int[] TargetHero { get; set; }

	string IDataKeySelector<string>.Key => ItemCode;
}
