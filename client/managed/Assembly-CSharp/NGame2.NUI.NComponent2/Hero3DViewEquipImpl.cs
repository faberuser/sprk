using System;
using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

[Serializable]
public class Hero3DViewEquipImpl : ItemIconComponent.Listener
{
	[Serializable]
	public class EquipIconRootInfo
	{
		public EquipPartType PartType;

		public Transform Root;
	}

	public EquipIconRootInfo[] EquipIconParents;

	public UIWidget WidgetShowUniqueWeaponButtonRoot;

	public UIButton Button_ShowUniqueWeaponOn;

	public UIButton Button_ShowUniqueWeaponOff;

	private CachedHeroInfo _cachedHeroInfo;

	private Action<ItemIconComponent> _onClickEquipIcon;

	private List<ItemIconComponent> _ItemIconComponentList = new List<ItemIconComponent>();

	public void Init(CachedHeroInfo cachedHeroInfo, Action<ItemIconComponent> onClickEquipIcon)
	{
		_cachedHeroInfo = cachedHeroInfo;
		_onClickEquipIcon = onClickEquipIcon;
	}

	public void Clear()
	{
		FactoryBase.ReleaseAndClearList(_ItemIconComponentList);
	}

	public void SetupEquipItemIcon()
	{
		Clear();
		if (_cachedHeroInfo == null)
		{
			return;
		}
		for (int i = 0; i < EquipIconParents.Length; i++)
		{
			EquipIconRootInfo equipIconRootInfo = EquipIconParents[i];
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
			_ItemIconComponentList.Add(itemIconComponent);
			EquipItemInfo equipItemInfoByPartIndex = _cachedHeroInfo.GetEquipItemInfoByPartIndex((int)partType);
			if (equipItemInfoByPartIndex == null || equipItemInfoByPartIndex.ItemIndex == 0)
			{
				itemIconComponent.Open(this, partType, _cachedHeroInfo);
				if (_cachedHeroInfo.IsEquipSlotLocked((int)partType))
				{
					itemIconComponent.SetLocked(locked: true);
				}
				continue;
			}
			int itemIndex = equipItemInfoByPartIndex.ItemIndex;
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
			if (data == null)
			{
				itemIconComponent.Open(this);
				VespaDebug.LogError("Failed to get item data : index=" + itemIndex);
			}
			else
			{
				itemIconComponent.Open(data, equipItemInfoByPartIndex, this, _cachedHeroInfo);
			}
		}
		RefreshShowUniqueWeapon();
	}

	public void RefreshShowUniqueWeapon()
	{
		if (_cachedHeroInfo == null)
		{
			UIHelper.EnableGameObject(WidgetShowUniqueWeaponButtonRoot, enable: false);
			return;
		}
		EquipItemInfo equipItemInfoByPartType = _cachedHeroInfo.GetEquipItemInfoByPartType(EquipPartType.Weapon);
		if (equipItemInfoByPartType == null)
		{
			UIHelper.EnableGameObject(WidgetShowUniqueWeaponButtonRoot, enable: false);
			return;
		}
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfoByPartType.ItemIndex);
		if (data == null)
		{
			UIHelper.EnableGameObject(WidgetShowUniqueWeaponButtonRoot, enable: false);
			return;
		}
		if (data.SubData.PartType != EquipPartType.Weapon)
		{
			UIHelper.EnableGameObject(WidgetShowUniqueWeaponButtonRoot, enable: false);
			return;
		}
		EquipItemDetailData data2 = GameRoot.Instance.EquipItemDetailDataContainer.GetData(data.SubData.DetailIndex);
		if (data2 == null || !data2.IsUnique)
		{
			UIHelper.EnableGameObject(WidgetShowUniqueWeaponButtonRoot, enable: false);
			return;
		}
		UIHelper.EnableGameObject(WidgetShowUniqueWeaponButtonRoot, enable: false);
		UIHelper.EnableGameObject(Button_ShowUniqueWeaponOff, _cachedHeroInfo.HideUniqueWeapon == 0);
		UIHelper.EnableGameObject(Button_ShowUniqueWeaponOn, _cachedHeroInfo.HideUniqueWeapon != 0);
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent This)
	{
		Vespa_System_Action_Extension_Method.fire(_onClickEquipIcon, This);
	}

	public int GetHeroPartEquipSlotIndex(ItemIconComponent itemIconComponent)
	{
		EquipIconRootInfo[] equipIconParents = EquipIconParents;
		foreach (EquipIconRootInfo equipIconRootInfo in equipIconParents)
		{
			if (equipIconRootInfo.Root == itemIconComponent.transform.parent)
			{
				return equipIconRootInfo.PartType.GetEquipSlotIndex();
			}
		}
		return 0;
	}

	public void ForEachEquip(Action<ItemIconComponent> action)
	{
		_ItemIconComponentList.ForEach(action);
	}

	public ItemIconComponent FindEquip(Predicate<ItemIconComponent> condition)
	{
		return _ItemIconComponentList.Find(condition);
	}
}
