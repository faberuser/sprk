using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable]
public class StaminaConsumePointData : IDataKeySelector<CurrencyType>
{
	[Key(0)]
	public CurrencyType CurrencyType { get; set; }

	[Key(1)]
	public int DailyMaxValue { get; set; }

	[Key(2)]
	public int MaxValue { get; set; }

	[Key(3)]
	public StaminaType[] StaminaType { get; set; }

	[Key(4)]
	public int[] Multiple { get; set; }

	CurrencyType IDataKeySelector<CurrencyType>.Key => CurrencyType;

	public override string ToString()
	{
		return $"CurrencyType : {CurrencyType.ToString()} DailyMax : {DailyMaxValue.ToString()} Max : {MaxValue.ToString()}";
	}
}
