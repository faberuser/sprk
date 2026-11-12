using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class OptionSellData : IDataKeySelector<OptionSellKey>
{
	[Key(0)]
	public OptionSellCategory Category { get; set; }

	[Key(1)]
	public int Value { get; set; }

	[Key(2)]
	public int BackGroundIndex { get; set; }

	[Key(3)]
	public string GradeStringKey { get; set; }

	[Key(4)]
	public string ReplaceKey { get; set; }

	[Key(5)]
	public OptionSellSelectType SelectType { get; set; }

	[Key(6)]
	public int[] GradeDependency { get; set; }

	OptionSellKey IDataKeySelector<OptionSellKey>.Key => new OptionSellKey(Category, Value);

	public override string ToString()
	{
		return "OptionSellData = " + Category.ToString() + "," + Value;
	}

	public OptionSellData GetClone()
	{
		return MemberwiseClone() as OptionSellData;
	}
}
