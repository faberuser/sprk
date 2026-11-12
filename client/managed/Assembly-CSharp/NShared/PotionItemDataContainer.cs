using System;
using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class PotionItemDataContainer : SubItemDataContainer<PotionItemData>
{
	public const string TABLENAME = "PotionItemTable";

	private JitDictionary<ItemActionType, List<PotionItemData>> _dicByActionType = new JitDictionary<ItemActionType, List<PotionItemData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("PotionItemTable");

	public PotionItemDataContainer()
	{
		base.Packer.RegisterAndBind(_dicByActionType);
	}

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[1] { ItemType.Potion };
	}

	protected override void OnEndLoadFromJsonData()
	{
		base.OnEndLoadFromJsonData();
		foreach (KeyValuePair<string, PotionItemData> item in base.BaseDic.JsonLoad_Get())
		{
			_dicByActionType.JsonLoad_AddToListValue(item.Value.ActionType, item.Value);
		}
	}

	private IList<PotionItemData> GetPotionItemDataOfActionType(ItemActionType type)
	{
		return _dicByActionType.GetOrDefault(type, null);
	}

	public bool CanRecoverStaminaWithItem(StaminaType staminaType)
	{
		if (staminaType == StaminaType.Chicken)
		{
			return true;
		}
		IList<PotionItemData> potionItemDataOfActionType = GetPotionItemDataOfActionType(ItemActionType.RecoverStamina);
		if (potionItemDataOfActionType == null)
		{
			return false;
		}
		for (int i = 0; i < potionItemDataOfActionType.Count; i++)
		{
			if (string.Equals(potionItemDataOfActionType[i].ActionSubValue, staminaType.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}
}
