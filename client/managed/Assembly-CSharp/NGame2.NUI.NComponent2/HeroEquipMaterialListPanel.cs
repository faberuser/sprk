using System;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NHelper.NItem;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

[Serializable]
public class HeroEquipMaterialListPanel : MonoBehaviour, IHeroPanel, EquipListManager.Listener, IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>, IUserObserver, IUserObserver<ItemInfo>
{
	public interface Listener
	{
		void OnClick(ItemIconComponent component);

		void UpdateItemIcon(ItemIconComponent component);
	}

	public enum ESortType
	{
		Up,
		Down,
		Max
	}

	[Serializable]
	public class UpDownSpriteInfo
	{
		public UISprite SpriteUp;

		public UISprite SpriteDown;

		public void SetSprite(ESortType sortType)
		{
			UIHelper.EnableGameObject(SpriteUp, sortType == ESortType.Up);
			UIHelper.EnableGameObject(SpriteDown, sortType == ESortType.Down);
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Hero/HeroEquipMaterialListPanel";

	public static PrefabAuto<HeroEquipMaterialListPanel> prefabAuto = new PrefabAuto<HeroEquipMaterialListPanel>("Resources/Prefab2/Component/Hero/HeroEquipMaterialListPanel");

	public EquipPartType[] PartTypes;

	public UpDownSpriteInfo SortReqLevelSprites;

	public UpDownSpriteInfo SortGradeSprites;

	public UpDownSpriteInfo SortStarSprites;

	private ESortType _TierSortType;

	private ESortType _GradeSortType;

	private ESortType _StarSortType;

	private Listener _listener;

	private bool _isDirty;

	public EquipListManager _equipListManager;

	private EquipmentItemFilterManager _equipmentItemFilterManager = new EquipmentItemFilterManager();

	public void Open(Listener listener)
	{
		_listener = listener;
		_equipListManager.Init(this);
		_equipListManager.InitEquipmentItemFilterManager(ref _equipmentItemFilterManager);
		_equipmentItemFilterManager.Init(EquipmentItemFilter.Mode.HeroList);
		_equipListManager.SetSortFunc(SortItem);
		GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
	}

	public void Reset()
	{
		_equipListManager.ClearItemList();
		_TierSortType = ESortType.Down;
		_GradeSortType = ESortType.Down;
		_StarSortType = ESortType.Down;
		UpdateSortButtonState();
	}

	public void Close()
	{
		GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this);
		Reset();
	}

	private void Reposition()
	{
		if (base.gameObject.activeSelf)
		{
			_equipListManager.UpdateAllItem(_listener.UpdateItemIcon);
			UIHelper.RepositionGrid(_equipListManager.EquipIconGrid);
		}
	}

	public void Refresh(bool immediate = true)
	{
		if (base.gameObject.activeSelf)
		{
			if (!immediate)
			{
				_isDirty = true;
				return;
			}
			_equipListManager.InitEquipList(Selector, null, bInitMinSlot: true);
			Reposition();
			_isDirty = false;
		}
	}

	public void OnSelectDeSelect()
	{
		if (base.gameObject.activeSelf)
		{
			_equipListManager.UpdateAllItem(_listener.UpdateItemIcon);
		}
	}

	public void Update()
	{
		if (_isDirty)
		{
			_isDirty = false;
			Refresh();
		}
	}

	private void SortItemList(ref ESortType sortType, UpDownSpriteInfo SpriteInfo)
	{
		sortType++;
		if (sortType == ESortType.Max)
		{
			sortType = ESortType.Up;
		}
		Reposition();
		UpdateSortButtonState();
	}

	public void OnClickSortByGrade()
	{
		SortItemList(ref _GradeSortType, SortGradeSprites);
	}

	public void OnClickSortByLevel()
	{
		SortItemList(ref _TierSortType, SortReqLevelSprites);
	}

	public void OnClickSortByStar()
	{
		SortItemList(ref _StarSortType, SortStarSprites);
	}

	private void UpdateSortButtonState()
	{
		SortReqLevelSprites.SetSprite(_TierSortType);
		SortGradeSprites.SetSprite(_GradeSortType);
		SortStarSprites.SetSprite(_StarSortType);
	}

	private int SortItem(ItemIconComponent comp1, ItemIconComponent comp2)
	{
		int num = ItemComparer.NullCheck(comp1, comp2);
		if (num != 0)
		{
			return num;
		}
		ItemData itemData = comp1.ItemData;
		ItemData itemData2 = comp2.ItemData;
		EquipItemInfo equipItemInfo = comp1.EquipItemInfo;
		EquipItemInfo equipItemInfo2 = comp2.EquipItemInfo;
		EquipItemData equipItemData = comp1.EquipItemData;
		EquipItemData equipItemData2 = comp2.EquipItemData;
		ItemComparer.SortClass sortClass = new ItemComparer.SortClass();
		sortClass.Add(ItemComparer.Compare, equipItemData.Tier, equipItemData2.Tier, _TierSortType == ESortType.Up).Add(ItemComparer.Compare_ItemGrade, itemData.Grade, itemData2.Grade, _GradeSortType == ESortType.Up).Add(ItemComparer.Compare_WeaponType, equipItemData.PartType, equipItemData2.PartType, isAscending: true)
			.Add(ItemComparer.Compare, equipItemData.SubType, equipItemData2.SubType, isAscending: true)
			.Add(ItemComparer.Compare, itemData.Index, itemData2.Index, isAscending: true)
			.Add(ItemComparer.Compare, equipItemInfo.Star, equipItemInfo2.Star, _StarSortType == ESortType.Up)
			.Add(ItemComparer.Compare, equipItemInfo.Level, equipItemInfo2.Level, isAscending: false)
			.Add(ItemComparer.Compare_CreateTime, equipItemInfo.CreateDateTime, equipItemInfo2.CreateDateTime, isAscending: true);
		return sortClass.Sort();
	}

	private bool Selector(IItemInfo iItemInfo)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(iItemInfo.ItemIndex);
		if (data == null || data.Type != ItemType.Equip)
		{
			return false;
		}
		EquipItemData subData = data.GetSubData<EquipItemData>();
		if (subData == null)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(PartTypes) > 0 && !PartTypes.Contains(subData.PartType))
		{
			return false;
		}
		if ((iItemInfo as EquipItemInfo).Locked != 0)
		{
			return false;
		}
		return true;
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>.OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo equipStatInfo)
	{
		if (_equipListManager.UpdateUserChanged(equipStatInfo, Selector))
		{
			Refresh(immediate: false);
		}
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo itemInfo)
	{
		if (_equipListManager.UpdateUserChanged(itemInfo, Selector))
		{
			Refresh(immediate: false);
		}
	}

	void EquipListManager.Listener.OnClickItemIcon(ItemIconComponent This)
	{
		_listener.OnClick(This);
	}
}
