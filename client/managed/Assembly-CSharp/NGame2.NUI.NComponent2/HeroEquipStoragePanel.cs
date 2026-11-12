using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDispatchBattle;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class HeroEquipStoragePanel : MonoBehaviour, IHeroPanel, EquipStorageSlotComponent.IListener, ItemIconComponent.Listener, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver, IUserObserver<EquipStoreagChangeInfo>, IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>
{
	public interface IListener
	{
		void OnClickEquip(int slotIndex);

		void OnClickSave(int slotIndex);

		void OnChagnedName(int slotIndex, string name);

		void OnClickUnlock();

		void OnClickReset(int slotIndex);

		void OnClickEquipIcon(ItemIconComponent icon);
	}

	public interface IDataProvider
	{
		int GetSelectedHeroIndex();
	}

	public UIScrollGrid Grid_StorageSlots;

	public UILabel Label_SelectSlotIndex;

	public UIInput Input_SelectSlotName;

	public Hero3DViewEquipImpl.EquipIconRootInfo[] EquipIconParents;

	public UIButton Button_Equip;

	public UIButton Button_Save;

	public UIButton Button_Reset;

	private List<EquipStorageSlotComponent> _equipStorageSlotComponentList = new List<EquipStorageSlotComponent>();

	private List<ItemIconComponent> _itemIconComponentList = new List<ItemIconComponent>();

	private IListener Listener;

	private IDataProvider DataProvider;

	private EquipStorageSlotComponent SelectedComponent;

	public EquipPresetInfo SelectedEquipPresetInfo { get; private set; }

	public void Open(IListener listener, IDataProvider dataProvider)
	{
		Listener = listener;
		DataProvider = dataProvider;
		base.gameObject.SetActive(value: true);
		GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger.ObserverManager.AddObserver(this);
		Init();
	}

	private void Init()
	{
		int equippedItemExsitSlotIndex = PresetStorageHelper.GetEquippedItemExsitSlotIndex(GetSelectedHeroIndex());
		equippedItemExsitSlotIndex = ((equippedItemExsitSlotIndex == -1) ? 1 : equippedItemExsitSlotIndex);
		CreatePresetStoargeComponents();
		SelectPresetStorage(equippedItemExsitSlotIndex);
	}

	public void Close()
	{
		ReleaseEquipItems();
		ReleasePresetStorageCompoents();
		base.gameObject.SetActive(value: false);
		SelectedEquipPresetInfo = null;
		SelectedComponent = null;
		GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger.ObserverManager.RemoveObserver(this);
	}

	private void CreatePresetStoargeComponents()
	{
		ReleasePresetStorageCompoents();
		EquipPresetInfo[] values = GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger.GetValues();
		int num = __Extension_Method_ICollection__.COUNT(values);
		int selectedHeroIndex = GetSelectedHeroIndex();
		int num2 = GameRoot.Instance.StorageSlotExtendDataContainer.MaxSlotCount();
		for (int i = 0; i < num2; i++)
		{
			EquipStorageSlotComponent equipStorageSlotComponent = UIHelper.AddGridItem(Grid_StorageSlots, EquipStorageSlotComponent.PrefabAuto);
			EquipPresetInfo equipPresetInfo = null;
			int index = i + 1;
			if (num > i)
			{
				equipPresetInfo = values[i];
				index = equipPresetInfo.EquipStorageSlotIndex;
			}
			equipStorageSlotComponent.Open(equipPresetInfo, index, this);
			equipStorageSlotComponent.UpdatePortraitTexture(selectedHeroIndex);
			_equipStorageSlotComponentList.Add(equipStorageSlotComponent);
		}
		UIHelper.RepositionGrid(Grid_StorageSlots);
	}

	private void ReleasePresetStorageCompoents()
	{
		FactoryBase.ReleaseAndClearList(_equipStorageSlotComponentList);
	}

	public void SelectPresetStorage(int storageIndex)
	{
		if (SelectedEquipPresetInfo == null || SelectedEquipPresetInfo.EquipStorageSlotIndex != storageIndex)
		{
			EquipPresetInfo presetInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger.Get(storageIndex);
			SelectPresetStorage(presetInfo);
		}
	}

	public void SelectPresetStorage(EquipPresetInfo presetInfo)
	{
		if (presetInfo != null)
		{
			SelectedEquipPresetInfo = presetInfo;
			UIHelper.SetLabelText(Label_SelectSlotIndex, presetInfo.EquipStorageSlotIndex.ToString());
			SetName(presetInfo.Name);
			CreateEquipItems();
			UpdateEquipItems();
			UpdateButtons();
			if (SelectedComponent != null)
			{
				SelectedComponent.ShowSelectImage(isShow: false);
				SelectedComponent = null;
			}
			EquipStorageSlotComponent equipStorageSlotComponent = FindSlotComponent(presetInfo.EquipStorageSlotIndex);
			if (equipStorageSlotComponent != null)
			{
				equipStorageSlotComponent.ShowSelectImage(isShow: true);
				SelectedComponent = equipStorageSlotComponent;
				Grid_StorageSlots.ScrollTo(equipStorageSlotComponent, 1f, 0f);
			}
		}
	}

	public void ReleaseEquipItems()
	{
		FactoryBase.ReleaseAndClearList(_itemIconComponentList);
	}

	public void CreateEquipItems()
	{
		ReleaseEquipItems();
		if (SelectedEquipPresetInfo == null)
		{
			return;
		}
		for (int i = 0; i < EquipIconParents.Length; i++)
		{
			Hero3DViewEquipImpl.EquipIconRootInfo equipIconRootInfo = EquipIconParents[i];
			if (equipIconRootInfo == null)
			{
				continue;
			}
			EquipPartType partType = equipIconRootInfo.PartType;
			Transform root = equipIconRootInfo.Root;
			ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(root);
			if (itemIconComponent == null)
			{
				continue;
			}
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
			_itemIconComponentList.Add(itemIconComponent);
			string equipItemUid = SelectedEquipPresetInfo.GetEquipItemUid((int)partType);
			EquipItemInfo equipItemInfoByUid = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.GetEquipItemInfoByUid(equipItemUid);
			if (equipItemInfoByUid == null || equipItemInfoByUid.ItemIndex == 0 || equipItemInfoByUid.GetInventoryType() == InventoryType.Chest)
			{
				itemIconComponent.Open(this, partType);
				continue;
			}
			int itemIndex = equipItemInfoByUid.ItemIndex;
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
			if (data == null)
			{
				itemIconComponent.Open(this);
				VespaDebug.LogError("Failed to get item data : index=" + itemIndex);
			}
			else
			{
				itemIconComponent.Open(data, equipItemInfoByUid, this, equipItemInfoByUid.EquippedHeroInfo);
				itemIconComponent.SetEquipped(isShow: true);
			}
		}
	}

	public void UpdateEquipPresetSlotComponent()
	{
		int selectedHeroIndex = GetSelectedHeroIndex();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_equipStorageSlotComponentList); i++)
		{
			EquipStorageSlotComponent equipStorageSlotComponent = _equipStorageSlotComponentList[i];
			if (!(equipStorageSlotComponent == null))
			{
				equipStorageSlotComponent.UpdatePortraitTexture(selectedHeroIndex);
			}
		}
	}

	public void UpdateEquipItems()
	{
		int selectedHeroIndex = GetSelectedHeroIndex();
		CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(selectedHeroIndex);
		if (heroInfo == null)
		{
			return;
		}
		foreach (ItemIconComponent itemIconComponent in _itemIconComponentList)
		{
			if (itemIconComponent.EquipItemInfo != null)
			{
				if (heroInfo.IsEquipSlotLocked((int)itemIconComponent.EquipItemData.PartType))
				{
					itemIconComponent.SetDisabled(disabled: true);
				}
				else if (heroInfo.CheckEquipResult(itemIconComponent.EquipItemData) != CachedHeroInfo.EquipResult.Success)
				{
					itemIconComponent.SetDisabled(disabled: true);
				}
				else
				{
					itemIconComponent.SetDisabled(disabled: false);
				}
			}
		}
	}

	public void UpdateButtons()
	{
		if (SelectedEquipPresetInfo != null)
		{
			int selectedHeroIndex = GetSelectedHeroIndex();
			CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(selectedHeroIndex);
			bool flag = DispatchHelper.IsDispatchedHero(selectedHeroIndex);
			bool num = IsEmptySlot();
			bool flag2 = !num && IsEquipable(heroInfo, SelectedEquipPresetInfo);
			bool flag3 = !num;
			bool flag4 = IsDiapatchedSlot(SelectedEquipPresetInfo);
			UIHelper.EnableButton(Button_Equip, flag2 && !flag && !flag4);
			UIHelper.EnableButton(Button_Reset, flag3 && !flag && !flag4);
			if (heroInfo != null)
			{
				bool flag5 = MyHeroManagement.IsSavableEquipItem(heroInfo);
				UIHelper.EnableButton(Button_Save, flag5 && !flag && !flag4);
			}
		}
	}

	private bool IsDiapatchedSlot(EquipPresetInfo equipPresetInfo)
	{
		EquipPartType[] saveEquipParts = EquipPresetInfo.SaveEquipParts;
		foreach (EquipPartType partIndex in saveEquipParts)
		{
			string equipItemUid = SelectedEquipPresetInfo.GetEquipItemUid((int)partIndex);
			if (!String_Extension_Method.IsNullOrEmpty(equipItemUid))
			{
				EquipItemInfo equipItemInfoByUid = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.GetEquipItemInfoByUid(equipItemUid);
				if (equipItemInfoByUid != null && equipItemInfoByUid.IsEquipped && DispatchHelper.IsDispatchedHero(equipItemInfoByUid.GetEquippedHeroIndex()))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsEquipable(CachedHeroInfo heroInfo, EquipPresetInfo equipPresetInfo)
	{
		if (heroInfo == null)
		{
			return false;
		}
		bool result = false;
		EquipPartType[] saveEquipParts = EquipPresetInfo.SaveEquipParts;
		foreach (EquipPartType partIndex in saveEquipParts)
		{
			if (heroInfo.IsEquipSlotLocked((int)partIndex))
			{
				continue;
			}
			string equipItemUid = equipPresetInfo.GetEquipItemUid((int)partIndex);
			if (String_Extension_Method.IsNullOrEmpty(equipItemUid))
			{
				continue;
			}
			EquipItemInfo equipItemInfoByUid = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.GetEquipItemInfoByUid(equipItemUid);
			if (equipItemInfoByUid != null)
			{
				ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfoByUid.ItemIndex);
				if (data != null && data.SubData != null && heroInfo.CheckEquipResult(data.SubData) == CachedHeroInfo.EquipResult.Success)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private bool IsEmptySlot()
	{
		return PresetStorageHelper.IsEmptySlotEquipPresetInfo(SelectedEquipPresetInfo);
	}

	private void SetName(string name)
	{
		if (Input_SelectSlotName != null)
		{
			Input_SelectSlotName.value = name;
		}
	}

	private int GetSelectedHeroIndex()
	{
		if (DataProvider == null)
		{
			return -1;
		}
		return DataProvider.GetSelectedHeroIndex();
	}

	private EquipStorageSlotComponent FindSlotComponent(int slotIndex)
	{
		return _equipStorageSlotComponentList.Find((EquipStorageSlotComponent comp) => comp.Index == slotIndex);
	}

	public void OnCiickEquip()
	{
		if (Listener != null && SelectedEquipPresetInfo != null)
		{
			Listener.OnClickEquip(SelectedEquipPresetInfo.EquipStorageSlotIndex);
		}
	}

	public void OnClickSave()
	{
		if (Listener != null && SelectedEquipPresetInfo != null)
		{
			Listener.OnClickSave(SelectedEquipPresetInfo.EquipStorageSlotIndex);
		}
	}

	public void OnClickNameInput()
	{
		if (Listener != null && !(Input_SelectSlotName == null) && SelectedEquipPresetInfo != null && !(Input_SelectSlotName.value == SelectedEquipPresetInfo.Name))
		{
			Listener.OnChagnedName(SelectedEquipPresetInfo.EquipStorageSlotIndex, Input_SelectSlotName.value);
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (Listener != null)
		{
			Listener.OnClickEquipIcon(itemIconComponent);
		}
	}

	void EquipStorageSlotComponent.IListener.OnClickSelect(int equipStorageIndex)
	{
		SelectPresetStorage(equipStorageIndex);
	}

	void EquipStorageSlotComponent.IListener.OnClickUnlock()
	{
		if (Listener != null)
		{
			Listener.OnClickUnlock();
		}
	}

	void IUserObserver<EquipStoreagChangeInfo>.OnChanged(EquipStoreagChangeInfo changedInfo)
	{
		if (changedInfo == null)
		{
			return;
		}
		EquipStorageSlotComponent equipStorageSlotComponent = FindSlotComponent(changedInfo.PresetInfo.EquipStorageSlotIndex);
		if (changedInfo.ChangeType == EEquipStorageChangeType.Add || changedInfo.ChangeType == EEquipStorageChangeType.Reset)
		{
			if (SelectedEquipPresetInfo == null)
			{
				SelectPresetStorage(changedInfo.PresetInfo);
			}
			else if (SelectedEquipPresetInfo.EquipStorageSlotIndex == changedInfo.PresetInfo.EquipStorageSlotIndex)
			{
				SelectPresetStorage(changedInfo.PresetInfo);
			}
			if (equipStorageSlotComponent != null)
			{
				equipStorageSlotComponent.Open(changedInfo.PresetInfo, changedInfo.PresetInfo.EquipStorageSlotIndex, this);
				equipStorageSlotComponent.ShowSelectImage(SelectedComponent == equipStorageSlotComponent);
				int selectedHeroIndex = GetSelectedHeroIndex();
				equipStorageSlotComponent.UpdatePortraitTexture(selectedHeroIndex);
			}
		}
		else if (changedInfo.ChangeType == EEquipStorageChangeType.ChangeName)
		{
			if (SelectedEquipPresetInfo.EquipStorageSlotIndex == changedInfo.PresetInfo.EquipStorageSlotIndex)
			{
				SetName(changedInfo.PresetInfo.Name);
			}
			if (equipStorageSlotComponent != null)
			{
				equipStorageSlotComponent.SetName(changedInfo.PresetInfo.Name);
			}
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	public void OnClickReset()
	{
		if (Listener != null && SelectedEquipPresetInfo != null)
		{
			Listener.OnClickReset(SelectedEquipPresetInfo.EquipStorageSlotIndex);
		}
	}

	void IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>.OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo changedInfo)
	{
		if (changedInfo == null)
		{
			return;
		}
		UpdateEquipPresetSlotComponent();
		if (changedInfo.StatType == NGame2.NAccount.UserEquipItemManager.EquipStatType.Remove)
		{
			CreateEquipItems();
			return;
		}
		ItemIconComponent itemIconComponent = _itemIconComponentList.Find((ItemIconComponent icon) => icon.EquipItemInfo != null && icon.EquipItemInfo.SlotIndex == changedInfo.SlotIndex);
		if (itemIconComponent != null)
		{
			itemIconComponent.Refresh();
			EquipItemInfo equipItemInfo = itemIconComponent.EquipItemInfo;
			if (equipItemInfo != null)
			{
				itemIconComponent.ShowEquipped(equipItemInfo.IsEquipped);
			}
		}
		if (changedInfo.StatType == NGame2.NAccount.UserEquipItemManager.EquipStatType.EquippedHeroIndex)
		{
			UpdateButtons();
			UpdateEquipPresetSlotComponent();
		}
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		if (changedInfo != null)
		{
			if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.Costume)
			{
				UpdateEquipPresetSlotComponent();
			}
			else if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.HeroLevelup || changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.OpenedEquipSlot)
			{
				UpdateEquipItems();
				UpdateButtons();
			}
		}
	}
}
