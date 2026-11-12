using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EventForgeData : IDataKeySelector<int>
{
	[Key(1)]
	public int Index { get; set; }

	[Key(2)]
	public int EnchatItemGroup { get; set; }

	[Key(3)]
	public string ItemCode { get; set; }

	[Key(4)]
	public int EnchantLevel { get; set; }

	[Key(5)]
	public int MaxEnchantLevel { get; set; }

	[Key(6)]
	public bool IsHighEnchant { get; set; }

	[Key(7)]
	public int SuccessRatio { get; set; }

	[Key(8)]
	public int FailRatio { get; set; }

	[Key(9)]
	public int BreakRatio { get; set; }

	[Key(10)]
	public string BreakItemCode { get; set; }

	[Key(11)]
	public bool IsBreakPopup { get; set; }

	[Key(12)]
	public string SuccessfulItemCode { get; set; }

	[Key(13)]
	public int SuccessfulItemCount { get; set; }

	[Key(14)]
	public int ExchangeRewardIndex { get; set; }

	[Key(15)]
	public int RequireGoldCost { get; set; }

	[Key(16)]
	public string RequireItem { get; set; }

	[Key(17)]
	public int RequireItemCount { get; set; }

	int IDataKeySelector<int>.Key => Index;

	[IgnoreMember]
	public float SuccessPercentage => GetCalcPercentage(SuccessRatio);

	[IgnoreMember]
	public float FailPercentage => GetCalcPercentage(FailRatio);

	[IgnoreMember]
	public float BreakPercentage => GetCalcPercentage(BreakRatio);

	private float GetCalcPercentage(int percentage)
	{
		return (float)percentage * 0.1f;
	}
}
