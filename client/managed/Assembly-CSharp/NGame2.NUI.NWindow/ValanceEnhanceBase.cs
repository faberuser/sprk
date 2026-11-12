using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow.NValance;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ValanceEnhanceBase : BaseCinemaWindow, ItemIconComponent.Listener, ValanceItemListPanel.IListener
{
	public Transform TargetEquipItemRoot;

	public ValanceItemListPanel ItemListPanel;

	public ValanceCreatureFilter CreatureFilter;

	public ValanceEquipFilter EquipItemFilter;

	protected ItemIconComponent TargetComponent;

	public EquipItemInfo EquipItemInfo { get; protected set; }

	public ItemData ItemData { get; protected set; }

	public EquipItemData EquipItemData { get; protected set; }

	public List<CachedHeroInfo> GetHeroItemInfos(EquipItemInfo seletedInfo)
	{
		int firstHero = seletedInfo?.GetEquippedHeroIndex() ?? (-1);
		return ValanceHelper.GetSortedHeroInfoList(HeroDefaultFilter, firstHero);
	}

	public List<EquipItemInfo> GetEquipItemInfos(EquipItemInfo seletedInfo)
	{
		return ValanceHelper.GetFiltedTechnoMagicItems(InvenDefaultFilter, seletedInfo);
	}

	public virtual bool HeroDefaultFilter(CreatureData creatureData)
	{
		return CreatureFilter.Filter(creatureData);
	}

	public virtual bool InvenDefaultFilter(EquipItemInfo info)
	{
		if (info.GetInventoryType() != InventoryType.Inventory)
		{
			return false;
		}
		if (info.GetEquippedHeroIndex() != -1)
		{
			return false;
		}
		if (!info.IsIdentified)
		{
			return false;
		}
		return EquipItemFilter.Filter(info.EquipItemData);
	}

	public virtual void ItemDisabledAction(ItemIconComponent obj)
	{
		EquipItemInfo equipItemInfo = obj.EquipItemInfo;
		if (equipItemInfo != null)
		{
			bool flag = equipItemInfo.IsTechnoMagic && equipItemInfo.IsIdentified;
			obj.SetDisabled(!flag);
		}
	}

	protected void Init(EquipItemInfo forcedSeletedItem = null)
	{
		InitFilter();
		InitItemList();
		SetItemList(forcedSeletedItem);
	}

	private void InitFilter()
	{
		CreatureFilter.SetFilterGroupComponent();
		EquipItemFilter.SetFilterGroupComponent();
	}

	public void InitItemList()
	{
		if (ItemListPanel != null)
		{
			ItemListPanel.Open(ItemDisabledAction, this);
		}
	}

	protected void SetItemList(EquipItemInfo equipItemInfo = null)
	{
		SetHeroItemList(equipItemInfo);
		SetInvenItemList(equipItemInfo);
	}

	protected void SetHeroItemList(EquipItemInfo equipItemInfo = null)
	{
		List<CachedHeroInfo> heroItemInfos = GetHeroItemInfos(equipItemInfo);
		ItemListPanel.SetHeroEquipItemList(heroItemInfos);
	}

	protected void SetInvenItemList(EquipItemInfo equipItemInfo = null)
	{
		List<EquipItemInfo> equipItemInfos = GetEquipItemInfos(equipItemInfo);
		ItemListPanel.SetInvenItemList(equipItemInfos);
	}

	public virtual void SelectedTargetItem(EquipItemInfo itemInfo)
	{
		if (itemInfo == null || itemInfo.ItemIndex == 0)
		{
			return;
		}
		ItemData = itemInfo.ItemData;
		if (ItemData == null)
		{
			return;
		}
		EquipItemData = itemInfo.EquipItemData;
		if (EquipItemData != null)
		{
			EquipItemInfo = itemInfo;
			if (TargetComponent == null)
			{
				TargetComponent = ItemIconComponent.PrefabAuto.Factory.Create(TargetEquipItemRoot);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(TargetComponent.transform);
			}
			TargetComponent.Open(ItemData, EquipItemInfo, this);
		}
	}

	public virtual void Refresh()
	{
		SetItemList();
	}

	public virtual void ClearSeletedTarget()
	{
		EquipItemInfo = null;
		ItemData = null;
		EquipItemData = null;
		ClearSeletedTargetIcon();
	}

	private void ClearSeletedTargetIcon()
	{
		if (TargetComponent != null)
		{
			TargetComponent.SetSelected(selected: false);
			FactoryBase.SafeReleaseComponent(ref TargetComponent);
		}
	}

	public void ClearFilter()
	{
		OnClickCloseFilter();
		if (CreatureFilter != null)
		{
			CreatureFilter.Close();
		}
		if (EquipItemFilter != null)
		{
			EquipItemFilter.Close();
		}
	}

	public void ClearItemList()
	{
		if (ItemListPanel != null)
		{
			ItemListPanel.Close();
		}
	}

	private void ResetCreatureFilter()
	{
		if (CreatureFilter != null)
		{
			CreatureFilter.Reset();
		}
	}

	private void ResetEquipItemFilter()
	{
		if (EquipItemFilter != null)
		{
			EquipItemFilter.Reset();
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		ClearSeletedTarget();
		ClearFilter();
		ClearItemList();
	}

	public void OnToggleChanged()
	{
		if (ItemListPanel != null)
		{
			ItemListPanel.OnToggleChanged();
		}
	}

	public void OnClickOpenFilter()
	{
		bool flag = (bool)ItemListPanel.HeroEquipToggle && ItemListPanel.HeroEquipToggle.value;
		CreatureFilter.OnOffRoot(flag);
		EquipItemFilter.OnOffRoot(!flag);
		UIHelper.EnableToggle(ItemListPanel.HeroEquipToggle, enable: false, grayscale: true);
		UIHelper.EnableToggle(ItemListPanel.InvenItemToggle, enable: false, grayscale: true);
		UIHelper.EnableGameObject(ItemListPanel.ItemListRoot, enable: false);
	}

	public void OnClickCloseFilter()
	{
		CreatureFilter.OnOffRoot(isOn: false);
		EquipItemFilter.OnOffRoot(isOn: false);
		UIHelper.EnableToggle(ItemListPanel.HeroEquipToggle, enable: true, grayscale: true);
		UIHelper.EnableToggle(ItemListPanel.InvenItemToggle, enable: true, grayscale: true);
		UIHelper.EnableGameObject(ItemListPanel.ItemListRoot, enable: true);
	}

	public void OnClickClearCreatureFilter()
	{
		ResetCreatureFilter();
		SetHeroItemList();
	}

	public void OnClickClearItemFilter()
	{
		ResetEquipItemFilter();
		SetInvenItemList();
	}

	public virtual void OnClickComfirmCreatureFilter()
	{
		SetHeroItemList();
		OnClickCloseFilter();
	}

	public virtual void OnClickComfirmItemFilter()
	{
		SetInvenItemList();
		OnClickCloseFilter();
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		OnClickTechnoMagicItem(itemIconComponent);
	}

	protected void OnClickTechnoMagicItem(ItemIconComponent itemIconComponent)
	{
		if (!(itemIconComponent == null) && itemIconComponent.EquipItemInfo != null)
		{
			ClearSeletedTarget();
			ItemListPanel.ResetItemList();
		}
	}

	void ValanceItemListPanel.IListener.OnClickItemIcon(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null)
		{
			ClearSeletedTarget();
		}
		else
		{
			SelectedTargetItem(equipItemInfo);
		}
	}
}
