using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class PetOptionData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public OperationType OptionType { get; set; }

	[Key(2)]
	public int OptionValue0 { get; set; }

	[Key(3)]
	public int OptionValue1 { get; set; }

	[Key(4)]
	public int OptionValue2 { get; set; }

	[Key(5)]
	public int OptionValue3 { get; set; }

	[Key(6)]
	public int OptionValue4 { get; set; }

	[Key(7)]
	public int OptionValue5 { get; set; }

	[Key(8)]
	public PetOptionConditionType ConditionType { get; set; }

	[Key(9)]
	public string[] ConditionValue { get; set; }

	[Key(10)]
	public string ContentsOptionDescKey { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public int GetOptionValue(int star)
	{
		return star switch
		{
			0 => OptionValue0, 
			1 => OptionValue1, 
			2 => OptionValue2, 
			3 => OptionValue3, 
			4 => OptionValue4, 
			5 => OptionValue5, 
			_ => 0, 
		};
	}
}
