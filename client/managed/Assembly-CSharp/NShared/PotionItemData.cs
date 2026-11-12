using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PotionItemData : IItemSubData, IDataKeySelector<string>
{
	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public ItemActionType ActionType { get; set; }

	[Key(2)]
	public int ActionValue { get; set; }

	[Key(3)]
	public int ActionValue2 { get; set; }

	[Key(4)]
	public string ActionSubValue { get; set; }

	[Key(5)]
	public string[] BoosterCodes { get; set; }

	[Key(6)]
	public string UIEffectPrefabName { get; set; }

	[Key(7)]
	public string[] UIEffectValue { get; set; }

	[Key(8)]
	public bool AutoUse { get; set; }

	[Key(9)]
	public ResetType StaminaResetWarnningType { get; set; }

	[Key(10)]
	public int StaminaResetWarnningValue { get; set; }

	string IDataKeySelector<string>.Key => ItemCode;

	public override string ToString()
	{
		return "ItemCode=" + ItemCode + " (" + ActionType.ToString() + " " + ActionValue + ")";
	}
}
