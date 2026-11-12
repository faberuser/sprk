using System.Collections.Generic;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NAccount;

public class UserSoulWeaponManager : NShared.UserSoulWeaponManager
{
	public enum ESoulWeaponChangedType
	{
		None,
		EquipItemSlotIndex,
		Grade,
		ReinforceLevel,
		ReinforceRatio,
		Exp,
		ChangeRatio,
		OptionRatio1,
		OptionRatio2,
		RenewOptionCount,
		Remove,
		Add
	}

	public class SoulWeaponChangedInfo
	{
		public int SlotIndex;

		public SoulWeaponInfo SoulWeaponInfo;

		public ESoulWeaponChangedType ChangedType;

		public long BeforeValue;

		public long Aftervalue;

		public int ItemIndex;
	}

	private readonly UserObserverManager<SoulWeaponChangedInfo> _observerManager;

	public IUserObserverManager ObserverManager => _observerManager;

	public UserSoulWeaponManager()
	{
		_observerManager = new UserObserverManager<SoulWeaponChangedInfo>();
	}

	public void ApplyTransition(int beforeEquipItemSlotIndex, SoulWeaponInfo soulWeaponInfo)
	{
		Remove(beforeEquipItemSlotIndex);
		if (soulWeaponInfo != null)
		{
			Apply(soulWeaponInfo.EquipItemSlotIndex, soulWeaponInfo);
		}
		_observerManager.AnnounceToObservers(new SoulWeaponChangedInfo
		{
			SlotIndex = soulWeaponInfo.EquipItemSlotIndex,
			SoulWeaponInfo = soulWeaponInfo,
			ChangedType = ESoulWeaponChangedType.EquipItemSlotIndex,
			BeforeValue = beforeEquipItemSlotIndex,
			Aftervalue = soulWeaponInfo.EquipItemSlotIndex
		});
	}

	public void Apply(IEnumerable<SoulWeaponInfo> soulWeapoInfos)
	{
		if (soulWeapoInfos == null)
		{
			return;
		}
		IEnumerator<SoulWeaponInfo> enumerator = soulWeapoInfos.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SoulWeaponInfo current = enumerator.Current;
			if (current != null)
			{
				Apply(current.EquipItemSlotIndex, current);
			}
		}
	}

	public void Apply(SoulWeaponInfo soulWeaponInfo)
	{
		if (soulWeaponInfo != null)
		{
			Apply(soulWeaponInfo.EquipItemSlotIndex, soulWeaponInfo);
		}
	}

	public void Apply(int slotIndex, SoulWeaponInfo soulWeaponInfo)
	{
		if (soulWeaponInfo == null)
		{
			Remove(slotIndex);
			return;
		}
		SoulWeaponInfo soulWeaponInfo2 = Get(soulWeaponInfo.EquipItemSlotIndex);
		if (soulWeaponInfo2 == null)
		{
			Add(soulWeaponInfo);
			_observerManager.AnnounceToObservers(new SoulWeaponChangedInfo
			{
				SlotIndex = soulWeaponInfo.EquipItemSlotIndex,
				ChangedType = ESoulWeaponChangedType.Add,
				ItemIndex = soulWeaponInfo.ItemIndex,
				SoulWeaponInfo = soulWeaponInfo
			});
		}
		else
		{
			SetSoulWeaponChangeInfo(soulWeaponInfo2, ESoulWeaponChangedType.Exp, soulWeaponInfo.Exp);
			SetSoulWeaponChangeInfo(soulWeaponInfo2, ESoulWeaponChangedType.Grade, soulWeaponInfo.Grade);
			SetSoulWeaponChangeOptionRatio(soulWeaponInfo2, ESoulWeaponChangedType.ChangeRatio, soulWeaponInfo.OptionRatio1, soulWeaponInfo.OptionRatio2, soulWeaponInfo.OptionBonusRatio1, soulWeaponInfo.OptionBonusRatio2);
			SetSoulWeaponChangeInfo(soulWeaponInfo2, ESoulWeaponChangedType.ReinforceLevel, soulWeaponInfo.ReinforceLevel);
			SetSoulWeaponChangeInfo(soulWeaponInfo2, ESoulWeaponChangedType.RenewOptionCount, soulWeaponInfo.RenewOptionCount);
			SetSoulweaponEtc(soulWeaponInfo2, soulWeaponInfo);
			SetSoulWeaponChangeInfo(soulWeaponInfo2, ESoulWeaponChangedType.ReinforceRatio, soulWeaponInfo.ReinforceRatio);
		}
	}

	public void SetSoulweaponEtc(SoulWeaponInfo oldInfo, SoulWeaponInfo newInfo)
	{
		if (oldInfo.RenewOptionRatio1 != newInfo.RenewOptionRatio1)
		{
			oldInfo.RenewOptionRatio1 = newInfo.RenewOptionRatio1;
		}
		if (oldInfo.RenewOptionRatio2 != newInfo.RenewOptionRatio2)
		{
			oldInfo.RenewOptionRatio2 = newInfo.RenewOptionRatio2;
		}
		if (oldInfo.RenewOptionBonusRatio1 != newInfo.RenewOptionBonusRatio1)
		{
			oldInfo.RenewOptionBonusRatio1 = newInfo.RenewOptionBonusRatio1;
		}
		if (oldInfo.RenewOptionBonusRatio2 != newInfo.RenewOptionBonusRatio2)
		{
			oldInfo.RenewOptionBonusRatio2 = newInfo.RenewOptionBonusRatio2;
		}
	}

	public void SetSoulWeaponChangeOptionRatio(SoulWeaponInfo soulWeaponInfo, ESoulWeaponChangedType changedType, long optionRatio1, long optionRatio2, long optionBonusRatio1, long optionBonusRatio2)
	{
		if (soulWeaponInfo.OptionRatio1 != optionRatio1 || soulWeaponInfo.OptionRatio2 != optionRatio2 || soulWeaponInfo.OptionBonusRatio1 != optionBonusRatio1 || soulWeaponInfo.OptionBonusRatio2 != optionBonusRatio2)
		{
			soulWeaponInfo.OptionRatio1 = (int)optionRatio1;
			soulWeaponInfo.OptionRatio2 = (int)optionRatio2;
			soulWeaponInfo.OptionBonusRatio1 = (int)optionBonusRatio1;
			soulWeaponInfo.OptionBonusRatio2 = (int)optionBonusRatio2;
			_observerManager.AnnounceToObservers(new SoulWeaponChangedInfo
			{
				SlotIndex = soulWeaponInfo.EquipItemSlotIndex,
				SoulWeaponInfo = soulWeaponInfo,
				ChangedType = changedType
			});
		}
	}

	public void SetSoulWeaponChangeInfo(SoulWeaponInfo soulWeaponInfo, ESoulWeaponChangedType changedType, long newValue)
	{
		if (soulWeaponInfo == null)
		{
			return;
		}
		long num = 0L;
		switch (changedType)
		{
		case ESoulWeaponChangedType.EquipItemSlotIndex:
			num = soulWeaponInfo.EquipItemSlotIndex;
			if (num != newValue)
			{
				soulWeaponInfo.EquipItemSlotIndex = (int)newValue;
			}
			break;
		case ESoulWeaponChangedType.ReinforceLevel:
			num = soulWeaponInfo.ReinforceLevel;
			if (num != newValue)
			{
				soulWeaponInfo.ReinforceLevel = (int)newValue;
			}
			break;
		case ESoulWeaponChangedType.ReinforceRatio:
			num = soulWeaponInfo.ReinforceRatio;
			if (num != newValue)
			{
				soulWeaponInfo.ReinforceRatio = (int)newValue;
			}
			break;
		case ESoulWeaponChangedType.Exp:
			num = soulWeaponInfo.Exp;
			if (num != newValue)
			{
				soulWeaponInfo.Exp = (int)newValue;
			}
			break;
		case ESoulWeaponChangedType.Grade:
			num = soulWeaponInfo.Grade;
			if (num != newValue)
			{
				soulWeaponInfo.Grade = (int)newValue;
			}
			break;
		case ESoulWeaponChangedType.RenewOptionCount:
			num = soulWeaponInfo.RenewOptionCount;
			if (num != newValue)
			{
				soulWeaponInfo.RenewOptionCount = (int)newValue;
			}
			break;
		}
		if (num != newValue)
		{
			_observerManager.AnnounceToObservers(new SoulWeaponChangedInfo
			{
				SlotIndex = soulWeaponInfo.EquipItemSlotIndex,
				SoulWeaponInfo = soulWeaponInfo,
				ChangedType = changedType,
				BeforeValue = num,
				Aftervalue = newValue
			});
		}
	}

	private bool IsChangedEquipItemStat(ESoulWeaponChangedType changedType)
	{
		return changedType switch
		{
			ESoulWeaponChangedType.Grade => true, 
			ESoulWeaponChangedType.ReinforceLevel => true, 
			ESoulWeaponChangedType.OptionRatio1 => true, 
			ESoulWeaponChangedType.OptionRatio2 => true, 
			_ => false, 
		};
	}

	public List<SoulWeaponInfo> GetSoulWeaponList()
	{
		return ToList();
	}

	public bool HaveSoulWeapon()
	{
		return Count() > 0;
	}

	private void AddSoulItemInfo(SoulWeaponInfo soulWeaponInfo)
	{
		if (soulWeaponInfo != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(soulWeaponInfo.EquipItemSlotIndex)?.SetSoulWeaponInfo(soulWeaponInfo);
		}
	}

	private void RemoveSoulItemInfo(SoulWeaponInfo soulWeaponInfo)
	{
		if (soulWeaponInfo != null)
		{
			RemoveSoulItemInfo(soulWeaponInfo.EquipItemSlotIndex);
		}
	}

	private void RemoveSoulItemInfo(int slotIndex)
	{
		EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(slotIndex);
		if (equipItemInfo != null)
		{
			equipItemInfo.SetSoulWeaponInfo(null);
			_observerManager.AnnounceToObservers(new SoulWeaponChangedInfo
			{
				SlotIndex = slotIndex,
				ChangedType = ESoulWeaponChangedType.Remove,
				ItemIndex = equipItemInfo.ItemIndex
			});
		}
	}

	public SoulWeaponInfo GetNextReinforceSoulWeaponInfo(SoulWeaponInfo soulWeaponInfo)
	{
		if (soulWeaponInfo == null)
		{
			return null;
		}
		SoulWeaponInfo soulWeaponInfo2 = soulWeaponInfo.Clone();
		if (soulWeaponInfo2 == null)
		{
			return null;
		}
		SoulWeaponReinforceData data = GameRoot.Instance.SoulWeaponReinforceDataContainer.GetData(soulWeaponInfo.ReinforceLevel + 1);
		if (data != null)
		{
			soulWeaponInfo2.ReinforceLevel = data.ReinforceLevel;
		}
		return soulWeaponInfo2;
	}

	public SoulWeaponInfo GetNextUpgradeSoulWeaponInfo(SoulWeaponInfo soulWeaponInfo)
	{
		if (soulWeaponInfo == null)
		{
			return null;
		}
		SoulWeaponInfo soulWeaponInfo2 = soulWeaponInfo.Clone();
		if (soulWeaponInfo2 == null)
		{
			return null;
		}
		SoulWeaponUpgradeData data = GameRoot.Instance.SoulWeaponUpgradeDataContainer.GetData(soulWeaponInfo.Grade + 1);
		if (data != null)
		{
			soulWeaponInfo2.Grade = data.Grade;
		}
		return soulWeaponInfo2;
	}

	public bool CheckAlreadySoulWeapon(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null || equipItemInfo.EquipItemData == null)
		{
			return true;
		}
		if (equipItemInfo.EquipItemData.SubType != EquipItemSubType.SpecialWeapon)
		{
			return true;
		}
		if (equipItemInfo.IsSoulWeapon())
		{
			return true;
		}
		if (!HaveSoulWeapon())
		{
			return false;
		}
		SoulWeaponInfo[] values = GetValues();
		SoulWeaponInfo soulWeaponInfo = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(values); i++)
		{
			soulWeaponInfo = values[i];
			if (soulWeaponInfo != null && soulWeaponInfo.ItemIndex == equipItemInfo.ItemIndex)
			{
				return true;
			}
		}
		return false;
	}

	public bool HaveSoulWeaponHero(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null || equipItemInfo.EquipItemData == null)
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
		int[] creatureIndex = equipItemInfo.EquipItemData.CreatureIndex;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(creatureIndex))
		{
			return false;
		}
		int heroIndex = creatureIndex[0];
		if (GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex) == null)
		{
			return false;
		}
		return true;
	}

	protected override void OnAddData(int key, SoulWeaponInfo value)
	{
		AddSoulItemInfo(value);
		base.OnAddData(key, value);
	}

	public override bool Remove(int key)
	{
		RemoveSoulItemInfo(key);
		return base.Remove(key);
	}

	public override bool Remove(SoulWeaponInfo value)
	{
		RemoveSoulItemInfo(value);
		return base.Remove(value);
	}
}
