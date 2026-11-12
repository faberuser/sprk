using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class TeamLevelBuffOptionData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public OperationType OptionType { get; set; }

	[Key(2)]
	public int[] OptionValue { get; set; }

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

	[Key(8)]
	public TeamLevelGrade Grade { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public int GetOptionValue(int index)
	{
		int num = OptionValue.Length;
		if (index >= num)
		{
			return OptionValue[num - 1];
		}
		return OptionValue[index];
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
		if (OptionValue == null || OptionValue.Length == 0)
		{
			return 0;
		}
		return OptionValue.Length - 1;
	}
}
