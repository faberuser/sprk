using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class BoosterItemData : IItemSubData, IDataKeySelector<string>
{
	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public int BoosterItemIndex { get; set; }

	[Key(2)]
	public string Name { get; set; }

	[Key(3)]
	public string Desc { get; set; }

	[Key(4)]
	public EventBonusType Type { get; set; }

	[Key(5)]
	public int Value { get; set; }

	[Key(6)]
	public int Duration { get; set; }

	[Key(7)]
	public BattleType[] BattleTypes { get; set; }

	[Key(8)]
	public bool IsOnetime { get; set; }

	[Key(9)]
	public string OnetimeBoosterTitle { get; set; }

	[Key(10)]
	public string OnetimeBoosterDesc { get; set; }

	[Key(11)]
	public bool IsHideAtBattle { get; set; }

	string IDataKeySelector<string>.Key => ItemCode;

	public override string ToString()
	{
		return "ItemCode=" + ItemCode + " (" + Value + " " + Value + ")";
	}
}
