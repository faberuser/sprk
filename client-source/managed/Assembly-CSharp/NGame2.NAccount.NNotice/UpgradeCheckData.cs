using System.Collections.Generic;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NAccount.NNotice;

public class UpgradeCheckData
{
	public enum EConstructorType
	{
		Hero,
		UniqueItem,
		SoulStone
	}

	public int HeroIndex;

	public int UniqueItemIndex;

	public int SoulStonItemIndex;

	private bool _haveHero;

	private bool _isDispatch;

	private List<int> _listUniqueItemSlotIndex = new List<int>();

	private int _soulStoneItemCount;

	private int _soulWeaponSlotIndex;

	private int _soulWeaponGrade;

	private int _soulWeaponReinforceLevel;

	public Tuple<ESoulWeaponNoticeType, bool> _state = new Tuple<ESoulWeaponNoticeType, bool>(ESoulWeaponNoticeType.None, i_item2: false);

	public UpgradeCheckData(int index, EConstructorType constructorType)
	{
		switch (constructorType)
		{
		case EConstructorType.Hero:
			HeroIndex = index;
			UniqueItemIndex = 1000 + index;
			SoulStonItemIndex = 111000 + index;
			SetHero();
			break;
		case EConstructorType.UniqueItem:
			HeroIndex = index - 1000;
			UniqueItemIndex = index;
			SoulStonItemIndex = 110000 + index;
			break;
		case EConstructorType.SoulStone:
			HeroIndex = index - 111000;
			UniqueItemIndex = index - 110000;
			SoulStonItemIndex = index;
			break;
		}
	}

	public void SetHero()
	{
		_haveHero = true;
	}

	public void SetDispatch(bool isDispatch)
	{
		_isDispatch = isDispatch;
	}

	public bool SetEquipItem(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null)
		{
			return false;
		}
		int slotIndex = equipItemInfo.SlotIndex;
		if (equipItemInfo.IsSoulWeapon())
		{
			SetSoulWeaponInfo(equipItemInfo.SoulWeaponInfo);
		}
		if (!_listUniqueItemSlotIndex.Contains(slotIndex))
		{
			_listUniqueItemSlotIndex.Add(slotIndex);
			return true;
		}
		return false;
	}

	public bool RemoveUniqueItem(int slotIndex)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_listUniqueItemSlotIndex))
		{
			return false;
		}
		return _listUniqueItemSlotIndex.Remove(slotIndex);
	}

	public void SetSoulStone(ItemInfo itemInfo)
	{
		if (itemInfo != null && _soulStoneItemCount != itemInfo.Count)
		{
			_soulStoneItemCount = itemInfo.Count;
		}
	}

	public void SetSoulWeaponInfo(SoulWeaponInfo soulWeaponInfo)
	{
		if (soulWeaponInfo == null)
		{
			_soulWeaponSlotIndex = 0;
			_soulWeaponReinforceLevel = 0;
			_soulWeaponGrade = 0;
		}
		else
		{
			_soulWeaponSlotIndex = soulWeaponInfo.EquipItemSlotIndex;
			_soulWeaponReinforceLevel = soulWeaponInfo.ReinforceLevel;
			_soulWeaponGrade = soulWeaponInfo.Grade;
		}
	}

	public Tuple<ESoulWeaponNoticeType, bool> GetState()
	{
		return _state;
	}

	private bool IsStateChange(bool enable, ESoulWeaponNoticeType noticeType)
	{
		if (_state.Item1 == noticeType)
		{
			return _state.Item2 != enable;
		}
		return true;
	}

	private void SetState(bool enable, ESoulWeaponNoticeType noticeType)
	{
		_state.Item1 = noticeType;
		_state.Item2 = enable;
	}

	private SoulWeaponNoticeInfo CreateNoticeInfo(bool enable, ESoulWeaponNoticeType noticeType, int slotIndex)
	{
		if (!IsStateChange(enable, noticeType))
		{
			return null;
		}
		SetState(enable, noticeType);
		return new SoulWeaponNoticeInfo
		{
			NoticeType = noticeType,
			IsOn = enable,
			SlotIndex = slotIndex
		};
	}

	private SoulWeaponNoticeInfo NoticeLiberation(bool enable)
	{
		return CreateNoticeInfo(enable, ESoulWeaponNoticeType.Liberation, 0);
	}

	private SoulWeaponNoticeInfo NoticeUpgrade(bool enable)
	{
		return CreateNoticeInfo(enable, ESoulWeaponNoticeType.Upgrade, _soulWeaponSlotIndex);
	}

	public SoulWeaponNoticeInfo GetNoticeInfo()
	{
		if (!AccountManager.IsLogin())
		{
			return null;
		}
		if (!_haveHero)
		{
			return NoticeLiberation(enable: false);
		}
		if (HaveSoulWeapon())
		{
			return EnableUpgrade();
		}
		return EnableLiberation();
	}

	private bool HaveSoulWeapon()
	{
		return _soulWeaponSlotIndex != 0;
	}

	private SoulWeaponNoticeInfo EnableLiberation()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_listUniqueItemSlotIndex))
		{
			return NoticeLiberation(enable: false);
		}
		if (_soulStoneItemCount == 0)
		{
			return NoticeLiberation(enable: false);
		}
		SoulWeaponUpgradePriceData data = GameRoot.Instance.SoulWeaponUpgradePriceDataContainer.GetData(UniqueItemIndex, 0);
		if (data == null)
		{
			return NoticeLiberation(enable: false);
		}
		if (data.MaterialItemCount > _soulStoneItemCount)
		{
			return NoticeLiberation(enable: false);
		}
		int num = __Extension_Method_ICollection__.COUNT(_listUniqueItemSlotIndex);
		for (int i = 0; i < num; i++)
		{
			int key = _listUniqueItemSlotIndex[i];
			EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(key);
			if (equipItemInfo != null && equipItemInfo.GetInventoryType() == InventoryType.Inventory && (equipItemInfo.EquippedHeroInfo == null || !_isDispatch))
			{
				return NoticeLiberation(enable: true);
			}
		}
		return NoticeLiberation(enable: false);
	}

	private SoulWeaponNoticeInfo EnableUpgrade()
	{
		if (_soulStoneItemCount == 0)
		{
			return NoticeUpgrade(enable: false);
		}
		int maxGrade = GameRoot.Instance.SoulWeaponUpgradeDataContainer.MaxGrade;
		if (_soulWeaponGrade == maxGrade)
		{
			return NoticeUpgrade(enable: false);
		}
		int num = _soulWeaponGrade + 1;
		SoulWeaponUpgradePriceData data = GameRoot.Instance.SoulWeaponUpgradePriceDataContainer.GetData(UniqueItemIndex, num);
		if (data == null)
		{
			return NoticeUpgrade(enable: false);
		}
		if (data.MaterialItemCount > _soulStoneItemCount)
		{
			return NoticeUpgrade(enable: false);
		}
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("SoulWeaponUpgradeRatio");
		valueInt *= num;
		if (_soulWeaponReinforceLevel < valueInt)
		{
			return NoticeUpgrade(enable: false);
		}
		EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(_soulWeaponSlotIndex);
		if (equipItemInfo.EquippedHeroInfo != null && _isDispatch)
		{
			return NoticeUpgrade(enable: false);
		}
		if (equipItemInfo.GetInventoryType() != InventoryType.Inventory)
		{
			return NoticeUpgrade(enable: false);
		}
		return NoticeUpgrade(enable: true);
	}

	public override string ToString()
	{
		return $"HeroIndex : {HeroIndex} , UniqueItemIndex : {UniqueItemIndex} , SoulStoneIndex : {SoulStonItemIndex}";
	}
}
