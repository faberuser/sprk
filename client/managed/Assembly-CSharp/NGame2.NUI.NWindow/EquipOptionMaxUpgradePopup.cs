using System;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EquipOptionMaxUpgradePopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Equip/EquipOptionMaxUpgradePopup";

	public Action Callback_OK;

	public UISimpleGrid ItemIconGrid;

	public UILabel Label_UpgradeCount;

	public UILabel Label_CurrentUpgradePercent;

	public UILabel Label_AfterUpgradePercent;

	public UILabel Label_OriginalMaterialCount;

	public UILabel Label_CurrentMaterialCount;

	public UILabel Label_OriginalGold;

	public UILabel Label_CurrentGold;

	public GameObject CompeletUpgrade;

	public GameObject EventImage;

	private ItemIconComponent MaterialItemIconComponent;

	public static void Open(EquipItemInfo itemInfo, Action callback)
	{
		WindowPrefab<EquipOptionMaxUpgradePopup> windowPrefab = new WindowPrefab<EquipOptionMaxUpgradePopup>("Resources/Prefab2/Window/Equip/EquipOptionMaxUpgradePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(itemInfo, callback);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal(EquipItemInfo itemInfo, Action callback)
	{
		base.Open();
		Callback_OK = callback;
		MaterialItemIconComponent = UIHelper.AddGridItem(ItemIconGrid, ItemIconComponent.PrefabAuto);
		MaterialItemIconComponent.Open(itemInfo, null);
		int count = 0;
		int num = 0;
		long materialCount = num;
		int num2 = 0;
		long gold = num2;
		DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.DiscountEquipItemOptionUpgrade, null);
		int num3 = 0;
		int num4 = 0;
		for (int i = 1; i <= 5; i++)
		{
			EquipItemOptionInfo optionInfo = itemInfo.GetOptionInfo(i);
			if (optionInfo != null)
			{
				num3 += optionInfo.OptionStep;
				EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionInfo.OptionIndex);
				if (data != null)
				{
					num4 += data.OptionValueStep;
				}
			}
		}
		bool maxUpgradeCount = GetMaxUpgradeCount(itemInfo, out count, out materialCount, out gold);
		checked
		{
			if (effectiveEvent != null)
			{
				long i_value = gold;
				long num5 = materialCount;
				long num6 = gold;
				int bonusPercentage = effectiveEvent.BonusPercentage;
				long num7 = num6 * bonusPercentage;
				int num8 = 100;
				long num9 = unchecked(num7 / num8);
				long val = gold - num9;
				int num10 = 0;
				gold = System.Math.Max(val, num10);
				long num11 = materialCount;
				int bonusPercentage2 = effectiveEvent.BonusPercentage;
				long num12 = num11 * bonusPercentage2;
				int num13 = 100;
				num9 = unchecked(num12 / num13);
				long val2 = materialCount - num9;
				int num14 = 0;
				materialCount = System.Math.Max(val2, num14);
				UIHelper.EnableGameObject(Label_OriginalMaterialCount, enable: true);
				UIHelper.EnableGameObject(Label_OriginalGold, enable: true);
				UIHelper.SetLabelText(Label_OriginalMaterialCount, num5);
				UIHelper.SetLabelText(Label_OriginalGold, Vespa_Int_Extension_Method.ToCommaString(i_value));
			}
			else
			{
				UIHelper.EnableGameObject(Label_OriginalMaterialCount, enable: false);
				UIHelper.EnableGameObject(Label_OriginalGold, enable: false);
			}
			UIHelper.EnableGameObject(EventImage, effectiveEvent != null);
			UIHelper.EnableGameObject(Label_AfterUpgradePercent, !maxUpgradeCount);
			UIHelper.EnableGameObject(CompeletUpgrade, maxUpgradeCount);
			string percentString = GetPercentString(num3, num4);
			UIHelper.SetLabelText(Label_CurrentUpgradePercent, percentString);
		}
		if (!maxUpgradeCount)
		{
			string percentString2 = GetPercentString(num3 + count, num4);
			UIHelper.SetLabelText(Label_AfterUpgradePercent, percentString2);
		}
		string text = LocalizationManager.GetString(NShared.NLocalization.EquipOptionUpgrade.EQUIP_OPTION_UPGRADE_COUNT, count);
		UIHelper.SetLabelText(Label_UpgradeCount, text);
		UIHelper.SetLabelText(Label_CurrentMaterialCount, materialCount);
		UIHelper.SetLabelText(Label_CurrentGold, Vespa_Int_Extension_Method.ToCommaString(gold));
	}

	private static string GetPercentString(int step, int maxStep)
	{
		float num = (float)step / (float)maxStep * 100f;
		string text = "";
		if (NVespa.Math.Approximately(num, Mathf.Floor(num)))
		{
			return (int)num + "%";
		}
		return num.ToString("0.0") + "%";
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.SafeReleaseComponent(ref MaterialItemIconComponent);
	}

	public void OnClickMaxUpgrade()
	{
		if (Callback_OK != null)
		{
			Callback_OK();
		}
		Close();
	}

	public static bool GetMaxUpgradeCount(EquipItemInfo itemInfo, out int count, out long materialCount, out long gold)
	{
		count = 0;
		int num = 0;
		materialCount = num;
		int num2 = 0;
		gold = num2;
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(itemInfo.ItemIndex);
		if (data == null || data.SubData == null)
		{
			return false;
		}
		EquipOptionUpgradeData data2 = GameRoot.Instance.EquipOptionUpgradeDataContainer.GetData(data.SubData.Tier, data.SubData.DetailIndex);
		if (data2 == null)
		{
			return false;
		}
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(data2.MaterialItemIndex);
		if (itemCount == 0)
		{
			return false;
		}
		int reqGold = data2.ReqGold;
		int num3 = data2.ReqGold;
		int materialItemCount = data2.MaterialItemCount;
		int num4 = data2.MaterialItemCount;
		DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.DiscountEquipItemOptionUpgrade, null);
		if (effectiveEvent != null)
		{
			int num5 = reqGold * effectiveEvent.BonusPercentage / 100;
			num3 = Mathf.Max(reqGold - num5, 0);
			num5 = materialItemCount * effectiveEvent.BonusPercentage / 100;
			num4 = Mathf.Max(materialItemCount - num5, 0);
		}
		int num6 = 0;
		int num7 = 0;
		bool result = true;
		for (int i = 1; i <= 5; i++)
		{
			EquipItemOptionInfo optionInfo = itemInfo.GetOptionInfo(i);
			if (optionInfo == null)
			{
				continue;
			}
			EquipOptionData data3 = GameRoot.Instance.EquipOptionDataContainer.GetData(optionInfo.OptionIndex);
			if (data3 == null || optionInfo.OptionStep >= data3.OptionValueStep)
			{
				continue;
			}
			int num8 = data3.OptionValueStep - optionInfo.OptionStep;
			for (int j = 1; j <= num8; j++)
			{
				if (num6 + num3 > GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold)
				{
					result = false;
					break;
				}
				if (num7 + num4 > itemCount)
				{
					result = false;
					break;
				}
				num6 += num3;
				num7 += num4;
				long num9 = gold;
				int num10 = reqGold;
				checked
				{
					gold = num9 + num10;
					long num11 = materialCount;
					int num12 = materialItemCount;
					materialCount = num11 + num12;
				}
				count++;
			}
		}
		return result;
	}
}
