using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class BreakConfirmPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/BreakConfirmPopup";

	public UILabel Label_Title;

	public UILabel Label_Content1;

	public UILabel Label_Count;

	public UISimpleGrid Grid_Before;

	public UISimpleGrid Grid_After;

	private Action<BreakConfirmPopup> _onYes;

	private Action<BreakConfirmPopup> _onCancel;

	private Action _onClose;

	private List<ItemIconComponent> _itemIconComponentList;

	public static BreakConfirmPopup Open(ItemData itemData, int itemCount, Action<BreakConfirmPopup> onYes, Action<BreakConfirmPopup> onCancel, Action onClose = null)
	{
		WindowPrefab<BreakConfirmPopup> windowPrefab = new WindowPrefab<BreakConfirmPopup>("Resources/Prefab2/Window/MessagePopup/BreakConfirmPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(itemData, itemCount, onYes, onCancel, onClose);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		return windowPrefab.instance;
	}

	public static BreakConfirmPopup Open(EquipItemInfo equipItemInfo, Action<BreakConfirmPopup> onYes, Action<BreakConfirmPopup> onCancel, Action onClose = null)
	{
		WindowPrefab<BreakConfirmPopup> windowPrefab = new WindowPrefab<BreakConfirmPopup>("Resources/Prefab2/Window/MessagePopup/BreakConfirmPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(equipItemInfo, onYes, onCancel, onClose);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		return windowPrefab.instance;
	}

	protected void OpenInternal(EquipItemInfo equipItemInfo, Action<BreakConfirmPopup> onYes, Action<BreakConfirmPopup> onCancel, Action onClose = null)
	{
		base.Open();
		_onYes = onYes;
		_onCancel = onCancel;
		_onClose = onClose;
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(equipItemInfo.ItemIndex);
		EquipItemData subData = data.GetSubData<EquipItemData>();
		EquipItemBreakData data2 = GameRoot.Instance.EquipItemBreakDataContainer.GetData(subData.Tier, subData.DetailIndex, data.Code, equipItemInfo.Star);
		if (data2 == null)
		{
			Debug.LogError("breakdata is null");
			return;
		}
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("UNIQUE_WEAPON_SLICE_MESSAGE", data2.MinItemCount1, data.Name);
		if (subData.PartType == EquipPartType.Treasure)
		{
			stringDirect = LocalizationManager.GetStringDirect("UNIQUE_TREASURE_SLICE_MESSAGE", data2.MinItemCount1, data.Name);
		}
		UIHelper.SetLabelText(Label_Content1, stringDirect);
		UIHelper.SetLabelText(Label_Title, text);
		_itemIconComponentList = _itemIconComponentList ?? new List<ItemIconComponent>();
		ItemIconComponent itemIconComponent = Grid_Before.AddGridItem(ItemIconComponent.PrefabAuto);
		itemIconComponent.Open(equipItemInfo, null);
		_itemIconComponentList.Add(itemIconComponent);
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(data2.ItemCode1);
		ItemIconComponent itemIconComponent2 = Grid_After.AddGridItem(ItemIconComponent.PrefabAuto);
		itemIconComponent2.Open(dataByCode);
		_itemIconComponentList.Add(itemIconComponent2);
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(dataByCode.Index);
		string stringDirect2 = LocalizationManager.GetStringDirect("ITEM.COMMON_GROWTH_REWARD_COUNT", itemCount);
		UIHelper.SetLabelText(Label_Count, stringDirect2);
	}

	protected void OpenInternal(ItemData itemData, int itemCount, Action<BreakConfirmPopup> onYes, Action<BreakConfirmPopup> onCancel, Action onClose = null)
	{
		base.Open();
		_onYes = onYes;
		_onCancel = onCancel;
		_onClose = onClose;
		ItemBreakData data = GameRoot.Instance.ItemBreakDataContainer.GetData(itemData.Index);
		if (data == null)
		{
			Debug.LogError("breakdata is null");
			return;
		}
		RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(data.RewardIndex);
		int itemMaxCount = data2.GetItemMaxCount(data2.ItemCode1);
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("SOULWEAPON_INFO_BREAKSTONE", itemCount * itemMaxCount, itemData.Name);
		UIHelper.SetLabelText(Label_Content1, stringDirect);
		UIHelper.SetLabelText(Label_Title, text);
		_itemIconComponentList = _itemIconComponentList ?? new List<ItemIconComponent>();
		ItemIconComponent itemIconComponent = Grid_Before.AddGridItem(ItemIconComponent.PrefabAuto);
		itemIconComponent.Open(itemData);
		_itemIconComponentList.Add(itemIconComponent);
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(data2.ItemCode1);
		ItemIconComponent itemIconComponent2 = Grid_After.AddGridItem(ItemIconComponent.PrefabAuto);
		itemIconComponent2.Open(dataByCode);
		_itemIconComponentList.Add(itemIconComponent2);
		int itemCount2 = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(dataByCode.Index);
		string stringDirect2 = LocalizationManager.GetStringDirect("ITEM.COMMON_GROWTH_REWARD_COUNT", itemCount2);
		UIHelper.SetLabelText(Label_Count, stringDirect2);
	}

	private void AddItemIconComponent(UISimpleGrid grid, EquipItemInfo equipItemInfo)
	{
		ItemIconComponent itemIconComponent = grid.AddGridItem(ItemIconComponent.PrefabAuto);
		itemIconComponent.Open(equipItemInfo, null);
		_itemIconComponentList.Add(itemIconComponent);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_itemIconComponentList);
		_itemIconComponentList = null;
	}

	public override void OnClosing()
	{
		Vespa_System_Action_Extension_Method.fire(_onClose);
	}

	public void SetOnClose(Action onClose)
	{
		_onClose = onClose;
	}

	public void OnClickYes()
	{
		Vespa_System_Action_Extension_Method.fire(_onYes, this);
		Close();
	}

	public void OnClickCancel()
	{
		Vespa_System_Action_Extension_Method.fire(_onCancel, this);
		Close();
	}
}
