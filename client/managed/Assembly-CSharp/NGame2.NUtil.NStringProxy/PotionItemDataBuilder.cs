using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NStringProxy;

public class PotionItemDataBuilder : IBuilder
{
	private ItemData _itemData;

	private bool _appendLine;

	bool IBuilder.AppendLine => _appendLine;

	public PotionItemDataBuilder(ItemData itemData, bool appendLine)
	{
		_itemData = itemData;
		_appendLine = appendLine;
	}

	public string Build()
	{
		PotionItemData subData = _itemData.GetSubData<PotionItemData>();
		if (subData != null)
		{
			if (subData.ActionType == ItemActionType.RecoverStaminaPercent)
			{
				int actionValue = subData.ActionValue;
				long num = actionValue;
				long maxStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetMaxStamina(StaminaType.Chicken);
				long num2 = checked(num * maxStamina);
				int num3 = 100;
				num = num2 / num3;
				return string.Format(_itemData.Description2, num);
			}
			return _itemData.Description2;
		}
		return string.Empty;
	}
}
