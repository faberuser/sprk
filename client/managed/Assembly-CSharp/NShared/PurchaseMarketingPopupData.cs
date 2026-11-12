using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PurchaseMarketingPopupData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int[] PurchaseIndices { get; set; }

	[Key(2)]
	public int PopupPriority { get; set; }

	[Key(3)]
	public string PopupPrefabName { get; set; }

	[Key(4)]
	public MarketingType Type { get; set; }

	[Key(5)]
	public string TitleStringKey { get; set; }

	[Key(6)]
	public string DescStringKey { get; set; }

	[Key(7)]
	public string PopupImage { get; set; }

	[Key(8)]
	public string[] Condition1 { get; set; }

	[Key(9)]
	public string[] Condition2 { get; set; }

	[Key(10)]
	public string[] Condition3 { get; set; }

	[Key(11)]
	public OpenConditionType OpenCondition { get; set; }

	[Key(12)]
	public string[] SkipCondition1 { get; set; }

	[Key(13)]
	public string[] SkipCondition2 { get; set; }

	[Key(14)]
	public string[] SkipCondition3 { get; set; }

	[Key(15)]
	public int RemindMinutes { get; set; }

	[Key(16)]
	public int PushID { get; set; }

	[Key(17)]
	public int[] PushSendMinute { get; set; }

	[Key(18)]
	public string[] PushLocalText { get; set; }

	[Key(19)]
	public string OpenDate { get; set; }

	[Key(20)]
	public string[] ServerCondition { get; set; }

	public override string ToString()
	{
		return "PurchaseMarketingPopupData Index=" + Index;
	}
}
