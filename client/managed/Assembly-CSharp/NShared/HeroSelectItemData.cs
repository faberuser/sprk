using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class HeroSelectItemData : IItemSubData, IDataKeySelector<string>
{
	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public int HeroStar { get; set; }

	[Key(2)]
	public int HeroLevel { get; set; }

	[Key(3)]
	public int HeroTranscend { get; set; }

	[Key(4)]
	public int[] HeroIndices { get; set; }

	[Key(5)]
	public int AllOwnedRewardIndex { get; set; }

	[Key(6)]
	public string AllOwnedRewardDesc { get; set; }

	[Key(7)]
	public HeroSelectItemType HeroSelectItemType { get; set; }

	[Key(8)]
	public bool NpcType { get; set; }

	string IDataKeySelector<string>.Key => ItemCode;

	public override string ToString()
	{
		return "ItemCode=" + ItemCode;
	}
}
