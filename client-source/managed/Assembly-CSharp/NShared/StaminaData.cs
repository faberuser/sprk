using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class StaminaData : IDataKeySelector<StaminaType>
{
	[Key(0)]
	public StaminaType StaminaType { get; set; }

	[Key(1)]
	public StaminaUpdateType UpdateType { get; set; }

	[Key(2)]
	public string AttributeName { get; set; }

	[Key(3)]
	public ResetType ResetType { get; set; }

	[Key(4)]
	public bool IsResetLimit { get; set; }

	[Key(5)]
	public int ResetLimitCount { get; set; }

	[Key(6)]
	public StaminaValueType MaxCountType { get; set; }

	[Key(7)]
	public string MaxCountValue { get; set; }

	[Key(8)]
	public int[] ResetCount { get; set; }

	[Key(9)]
	public bool AllowOverflow { get; set; }

	[Key(10)]
	public StaminaValueType RechargeCostType { get; set; }

	[Key(11)]
	public string[] RechargeGoldValue { get; set; }

	[Key(12)]
	public string[] RechargeGemValue { get; set; }

	[Key(13)]
	public string PopupStringKey { get; set; }

	[Key(14)]
	public string AutoRepeatStopMsgKey { get; set; }

	[Key(15)]
	public string HeroBonusTooltipDesc { get; set; }

	[Key(16)]
	public string StaminaTooltipDesc { get; set; }

	StaminaType IDataKeySelector<StaminaType>.Key => StaminaType;

	public override string ToString()
	{
		return "[" + StaminaType.ToString() + "]";
	}
}
