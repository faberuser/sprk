using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class TeamLevelBonusBuffOptionData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public EventBonusType BonusType { get; set; }

	[Key(2)]
	public int[] BonusValue { get; set; }

	[Key(3)]
	public CurrencyType ReinforcePriceType { get; set; }

	[Key(4)]
	public int[] ReinforcePrice { get; set; }

	[Key(5)]
	public TeamLevelBuffOptionConditionType ConditionType { get; set; }

	[Key(6)]
	public string[] ConditionValue { get; set; }

	[Key(7)]
	public string ContentsOptionDescKey { get; set; }

	[Key(9)]
	public TeamLevelGrade Grade { get; set; }

	[Key(10)]
	public string BonusNameKey { get; set; }

	[Key(11)]
	public string BonusDescKey { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public int GetBonusValue(int index)
	{
		int num = BonusValue.Length;
		if (index >= num)
		{
			return BonusValue[num - 1];
		}
		return BonusValue[index];
	}

	public int GetReinforcePrice(int index)
	{
		int num = ReinforcePrice.Length;
		if (index >= num)
		{
			return ReinforcePrice[num - 1];
		}
		return ReinforcePrice[index];
	}

	public int GetMaxReinforce()
	{
		if (BonusValue == null || BonusValue.Length == 0)
		{
			return 0;
		}
		return BonusValue.Length - 1;
	}
}
