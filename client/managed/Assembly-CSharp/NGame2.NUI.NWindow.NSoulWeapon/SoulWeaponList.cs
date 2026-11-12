using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDispatchBattle;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NItem;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NSoulWeapon;

[Serializable]
public class SoulWeaponList : ItemIconComponent.Listener
{
	public interface IListener
	{
		void SelectSoulWeapon(EquipItemInfo equipItemInfo, ListType listType);
	}

	public enum ListType
	{
		None,
		SpecialWeapon,
		EqualSpecialWeapon,
		SoulWeapon,
		SoulWeaponAll,
		Liberation,
		LiberationAll
	}

	public UIScrollGrid SoulWeaponListScrollGrid;

	private List<ItemIconComponent> _componentList = new List<ItemIconComponent>();

	private SoulWeaponMaterialInfo _materialInfo = new SoulWeaponMaterialInfo();

	private ItemIconComponent _selectedItemIconComponent;

	private IListener _iListener;

	private EquipItemInfo _equipItemInfo;

	private ListType _listType = ListType.SoulWeapon;

	public ItemIconComponent SelectedSoulWeaponItemIconComponent => _selectedItemIconComponent;

	private bool IsAllList
	{
		get
		{
			if (_listType == ListType.LiberationAll || _listType == ListType.SoulWeaponAll)
			{
				return true;
			}
			return false;
		}
	}

	public bool ExistList
	{
		get
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_componentList))
			{
				return false;
			}
			return _componentList.Count > 0;
		}
	}

	public void Open(IListener iListener, EquipItemInfo equipItemInfo = null, ListType listType = ListType.SoulWeapon, bool isSelectFirst = false)
	{
		Init(iListener, equipItemInfo, listType, isSelectFirst);
	}

	public void OpenAllList(IListener listener, ListType listType = ListType.SoulWeapon)
	{
		_iListener = listener;
		_equipItemInfo = null;
		_listType = listType;
	}

	private void Init(IListener iListener, EquipItemInfo equipItemInfo, ListType listType, bool isSelectFirst)
	{
		_iListener = iListener;
		_equipItemInfo = equipItemInfo;
		_listType = listType;
		SetList(isSelectFirst);
	}

	public void Close()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
		_selectedItemIconComponent = null;
	}

	public void Refresh(EquipItemInfo equipItemInfo)
	{
		if (_iListener != null && _listType != ListType.None)
		{
			if (equipItemInfo != null)
			{
				_equipItemInfo = equipItemInfo;
			}
			SetList(isSelectFirst: false);
		}
	}

	public void SetList(bool isSelectFirst)
	{
		switch (_listType)
		{
		case ListType.SpecialWeapon:
		case ListType.EqualSpecialWeapon:
		case ListType.Liberation:
		case ListType.LiberationAll:
			SetSpecialWeaponList(isSelectFirst);
			break;
		case ListType.SoulWeapon:
		case ListType.SoulWeaponAll:
			SetSoulWeaponList(isSelectFirst);
			break;
		}
	}

	private void SetSpecialWeaponList(bool isSelectFirst)
	{
		FactoryBase.ReleaseAndClearList(_componentList);
		if (SoulWeaponListScrollGrid == null)
		{
			return;
		}
		List<EquipItemInfo> list = GlobalAutoBehaviour<AccountManager>.instance.GetOwnedEquipItemList((EquipItemInfo e) => !DispatchHelper.IsDispatchedHero(e.EquippedHeroInfo) && IsPossibleLiberationWeapon(e));
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			List<EquipItemInfo> list2 = SortLiberation(ref list);
			for (int num = 0; num < __Extension_Method_ICollection__.COUNT(list2); num++)
			{
				AddItemIconComponent(list2[num]);
			}
			if (isSelectFirst)
			{
				SelectFirstItemIconComponent();
			}
			SoulWeaponListScrollGrid.RepositionGrid();
			if (_listType != ListType.EqualSpecialWeapon)
			{
				SelectSoulWeaponItemIconComponent(_equipItemInfo);
			}
		}
	}

	private bool IsPossibleLiberationWeapon(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null)
		{
			return false;
		}
		if (equipItemInfo.EquipItemData == null)
		{
			return false;
		}
		if (!equipItemInfo.EquipItemData.IsOpenSoulWeapon)
		{
			return false;
		}
		if (equipItemInfo.EquipItemData.SubType != EquipItemSubType.SpecialWeapon)
		{
			return false;
		}
		if (equipItemInfo.IsSoulWeapon())
		{
			return false;
		}
		if (_listType != ListType.EqualSpecialWeapon)
		{
			if (equipItemInfo.GetInventoryType() == InventoryType.Chest)
			{
				return false;
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.CheckAlreadySoulWeapon(equipItemInfo))
			{
				return false;
			}
			if (!GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.HaveSoulWeaponHero(equipItemInfo))
			{
				return false;
			}
		}
		if (_listType == ListType.EqualSpecialWeapon)
		{
			if (_equipItemInfo == null)
			{
				return false;
			}
			if (_equipItemInfo.ItemIndex != equipItemInfo.ItemIndex)
			{
				return false;
			}
		}
		return true;
	}

	private void SetSoulWeaponList(bool isSelectFirst)
	{
		FactoryBase.ReleaseAndClearList(_componentList);
		if (SoulWeaponListScrollGrid == null)
		{
			return;
		}
		List<EquipItemInfo> ownedEquipItemList = GlobalAutoBehaviour<AccountManager>.instance.GetOwnedEquipItemList(delegate(EquipItemInfo e)
		{
			if (DispatchHelper.IsDispatchedHero(e.EquippedHeroInfo))
			{
				return false;
			}
			return e.IsSoulWeapon() ? true : false;
		});
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(ownedEquipItemList))
		{
			ownedEquipItemList.Sort(CompareSoulWeapon);
			for (int num = 0; num < __Extension_Method_ICollection__.COUNT(ownedEquipItemList); num++)
			{
				AddItemIconComponent(ownedEquipItemList[num]);
			}
			if (isSelectFirst)
			{
				SelectFirstItemIconComponent();
			}
			UIHelper.RepositionGrid(SoulWeaponListScrollGrid);
			SelectSoulWeaponItemIconComponent(_equipItemInfo);
		}
	}

	private void AddItemIconComponent(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null)
		{
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(SoulWeaponListScrollGrid, ItemIconComponent.PrefabAuto);
			if (!(itemIconComponent == null))
			{
				itemIconComponent.Open(equipItemInfo, this);
				_componentList.Add(itemIconComponent);
			}
		}
	}

	private void SelectFirstItemIconComponent()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_componentList) && _componentList.Count > 0)
		{
			SelectSoulWeaponItemIconComponent(_componentList[0]);
		}
	}

	private List<EquipItemInfo> SortLiberation(ref List<EquipItemInfo> list)
	{
		EquipItemInfo equipItemInfo = null;
		List<EquipItemInfo> list2 = new List<EquipItemInfo>();
		List<EquipItemInfo> list3 = new List<EquipItemInfo>();
		for (int i = 0; i < list.Count; i++)
		{
			equipItemInfo = list[i];
			if (equipItemInfo != null)
			{
				if (GetMaterialItemCount(equipItemInfo) > 0)
				{
					list2.Add(equipItemInfo);
				}
				else
				{
					list3.Add(equipItemInfo);
				}
			}
		}
		list2.Sort(CompareLiberation);
		list3.Sort(CompareLiberation);
		List<EquipItemInfo> list4 = new List<EquipItemInfo>();
		list4.Clear();
		list4.AddRange(list2);
		list4.AddRange(list3);
		return list4;
	}

	private int CompareLiberation(EquipItemInfo equipItemInfo, EquipItemInfo compareEquipItemInfo)
	{
		if (equipItemInfo == null || compareEquipItemInfo == null)
		{
			return 0;
		}
		ItemComparer.SortClass sortClass = new ItemComparer.SortClass();
		sortClass.Add(ItemComparer.Compare, equipItemInfo.Star, compareEquipItemInfo.Star, isAscending: false).Add(ItemComparer.Compare, equipItemInfo.ItemIndex, compareEquipItemInfo.ItemIndex, isAscending: true);
		return sortClass.Sort();
	}

	private int GetMaterialItemCount(EquipItemInfo equipItemInfo)
	{
		if (_materialInfo == null)
		{
			return -1;
		}
		if (equipItemInfo == null)
		{
			return 1;
		}
		equipItemInfo = UIHelper.GetFakeSoulWeaponInfo(equipItemInfo);
		if (equipItemInfo == null)
		{
			return -1;
		}
		return _materialInfo.GetMyMaterialItemCount(equipItemInfo.SoulWeaponInfo);
	}

	private int CompareSoulWeapon(EquipItemInfo equipItemInfo, EquipItemInfo compareEquipItemInfo)
	{
		if (equipItemInfo == null || compareEquipItemInfo == null)
		{
			return 0;
		}
		if (!equipItemInfo.IsSoulWeapon() || !compareEquipItemInfo.IsSoulWeapon())
		{
			return 0;
		}
		SoulWeaponInfo soulWeaponInfo = equipItemInfo.SoulWeaponInfo;
		SoulWeaponInfo soulWeaponInfo2 = compareEquipItemInfo.SoulWeaponInfo;
		ItemComparer.SortClass sortClass = new ItemComparer.SortClass();
		sortClass.Add(ItemComparer.Compare, soulWeaponInfo.Grade, soulWeaponInfo2.Grade, isAscending: false).Add(ItemComparer.Compare, soulWeaponInfo.ReinforceLevel, soulWeaponInfo2.ReinforceLevel, isAscending: false).Add(ItemComparer.Compare, equipItemInfo.ItemIndex, compareEquipItemInfo.ItemIndex, isAscending: true);
		return sortClass.Sort();
	}

	private int GetCreatureIndex(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo.EquipItemData == null || __Extension_Method_ICollection__.IsNullOrEmpty(equipItemInfo.EquipItemData.CreatureIndex) || equipItemInfo.EquipItemData.CreatureIndex.Length == 0)
		{
			return 0;
		}
		return equipItemInfo.EquipItemData.CreatureIndex[0];
	}

	public void SelectSoulWeaponItemIconComponent(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null)
		{
			return;
		}
		ItemIconComponent itemIconComponent = null;
		for (int i = 0; i < _componentList.Count; i++)
		{
			itemIconComponent = _componentList[i];
			if (!(itemIconComponent == null) && itemIconComponent.EquipItemInfo != null && itemIconComponent.EquipItemInfo.Equals(equipItemInfo))
			{
				break;
			}
		}
		MoveScrollSelectSoulWeaponItemIconComponent(itemIconComponent);
	}

	private void SelectSoulWeaponItemIconComponent(ItemIconComponent itemIconComponent)
	{
		if (!(itemIconComponent == null) && !IsAllList)
		{
			if (_selectedItemIconComponent != null)
			{
				_selectedItemIconComponent.SetSelected(selected: false);
			}
			itemIconComponent.SetSelected(selected: true);
			_selectedItemIconComponent = itemIconComponent;
			_equipItemInfo = itemIconComponent.EquipItemInfo;
		}
	}

	private void MoveScrollSelectSoulWeaponItemIconComponent(ItemIconComponent itemIconComponent)
	{
		SelectSoulWeaponItemIconComponent(itemIconComponent);
		if (!(SoulWeaponListScrollGrid == null) && SoulWeaponListScrollGrid.ClippingPanel.height < SoulWeaponListScrollGrid.Grid.cellHeight * (float)SoulWeaponListScrollGrid.Grid.GetGridChildCount())
		{
			SoulWeaponListScrollGrid.ScrollTo(itemIconComponent, 0f, 0f);
		}
	}

	public void RefreshItemIconComponent(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null)
		{
			return;
		}
		ItemIconComponent itemIconComponent = null;
		for (int i = 0; i < _componentList.Count; i++)
		{
			itemIconComponent = _componentList[i];
			if (!(itemIconComponent == null) && itemIconComponent.EquipItemInfo != null && itemIconComponent.EquipItemInfo.SlotIndex == equipItemInfo.SlotIndex)
			{
				itemIconComponent.Open(equipItemInfo, this);
				break;
			}
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (!(itemIconComponent == null) && itemIconComponent.EquipItemInfo != null && _iListener != null)
		{
			int slotIndex = itemIconComponent.EquipItemInfo.SlotIndex;
			if (!IsAlreadySelectedItem(slotIndex))
			{
				SelectSoulWeaponItemIconComponent(itemIconComponent);
				_iListener.SelectSoulWeapon(itemIconComponent.EquipItemInfo, _listType);
			}
		}
	}

	private bool IsAlreadySelectedItem(int slotIndex)
	{
		if (_equipItemInfo != null && _equipItemInfo.SlotIndex == slotIndex)
		{
			return true;
		}
		return false;
	}
}
