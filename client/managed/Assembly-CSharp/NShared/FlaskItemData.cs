using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class FlaskItemData : IItemSubData, IDataKeySelector<string>
{
	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public FlaskType Type { get; set; }

	[Key(2)]
	public FlaskSourceType SourceType { get; set; }

	[Key(3)]
	public FlaskRewardType RewardType { get; set; }

	[Key(4)]
	public int FlaskExp { get; set; }

	[Key(5)]
	public int PotionItemIndex { get; set; }

	[Key(6)]
	public string UseDescLocal { get; set; }

	[Key(7)]
	public int[] EffectiveSkillIndex { get; set; }

	[Key(8)]
	public bool IsUnEquip { get; set; }

	[Key(9)]
	public int ShowIndex { get; set; }

	string IDataKeySelector<string>.Key => ItemCode;

	public override string ToString()
	{
		return "ItemCode=" + ItemCode;
	}
}
