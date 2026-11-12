using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class PetBonusData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public EventBonusType BonusType { get; set; }

	[Key(2)]
	public int BonusValue0 { get; set; }

	[Key(3)]
	public int BonusValue1 { get; set; }

	[Key(4)]
	public int BonusValue2 { get; set; }

	[Key(5)]
	public int BonusValue3 { get; set; }

	[Key(6)]
	public int BonusValue4 { get; set; }

	[Key(7)]
	public int BonusValue5 { get; set; }

	[Key(8)]
	public string ConditionType { get; set; }

	[Key(9)]
	public string[] ConditionValue { get; set; }

	[Key(10)]
	public string BonusNameKey { get; set; }

	[Key(11)]
	public string BonusDescKey { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public int GetBonusValue(int star)
	{
		return star switch
		{
			0 => BonusValue0, 
			1 => BonusValue1, 
			2 => BonusValue2, 
			3 => BonusValue3, 
			4 => BonusValue4, 
			5 => BonusValue5, 
			_ => 0, 
		};
	}
}
