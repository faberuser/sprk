using System;
using System.Collections.Generic;
using NShared;
using NVespa;

namespace NGame2.NAccount;

public class UserEquipItemManager : NShared.UserEquipItemManager
{
	public enum EquipStatType
	{
		None,
		Star,
		Level,
		Exp,
		OptionIndex1,
		OptionIndex2,
		OptionIndex3,
		OptionIndex4,
		OptionIndex5,
		OptionStep1,
		OptionStep2,
		OptionStep3,
		OptionStep4,
		OptionStep5,
		RuneItemIndex1,
		RuneItemIndex2,
		RuneItemIndex3,
		EquippedHeroIndex,
		UpgradeStarFailBonus,
		Locked,
		RenewOptionSlotIndex,
		RenewOptionCount,
		EnchantOptionIndex1,
		EnchantOptionIndex2,
		EnchantOptionIndex3,
		EnchantOptionStep1,
		EnchantOptionStep2,
		EnchantOptionStep3,
		RenewEnchantOptionSlotIndex,
		OptionRenewCount1,
		OptionRenewCount2,
		OptionRenewCount3,
		OptionRenewCount4,
		OptionRenewCount5,
		IsRenewedOption1,
		IsRenewedOption2,
		IsRenewedOption3,
		IsRenewedOption4,
		IsRenewedOption5,
		ChangeInventoryType,
		Add,
		Remove,
		NewIconRemove,
		ApplyRunePage,
		Identified,
		ExtraOptionIndex1,
		ExtraOptionIndex2,
		ExtraOptionStep1,
		ExtraOptionStep2,
		ExtraOptionRenewCount1,
		ExtraOptionRenewCount2,
		IsRenewedExtraOption1,
		IsRenewedExtraOption2
	}

	public class EquipStatInfo
	{
		public int SlotIndex { get; set; }

		public EquipItemInfo EquipItemInfo { get; set; }

		public EquipStatType StatType { get; set; }

		public long BeforeValue { get; set; }

		public long AfterValue { get; set; }
	}

	public struct TechnoMagic
	{
		public int UnIndentifiedCount;

		public List<int> _listSlotIndex;

		public void IncreaseUnIdentified()
		{
			UnIndentifiedCount++;
		}

		public void DecreaseUnIdentified()
		{
			UnIndentifiedCount--;
		}

		public bool HasUnIdentifiedItem()
		{
			return UnIndentifiedCount > 0;
		}

		public void AddSlotIndex(int slotIndex)
		{
			if (_listSlotIndex == null)
			{
				_listSlotIndex = new List<int>();
			}
			_listSlotIndex.Add(slotIndex);
		}

		public void AddSlotIndices(int[] slotIndices)
		{
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(slotIndices))
			{
				foreach (int slotIndex in slotIndices)
				{
					AddSlotIndex(slotIndex);
				}
			}
		}

		public void RemoveSlotIndex(int slotIndex)
		{
			if (_listSlotIndex != null)
			{
				_listSlotIndex.Remove(slotIndex);
			}
		}

		public List<int> GetSlotIndexList()
		{
			return _listSlotIndex;
		}

		public int GetTotalCount()
		{
			return __Extension_Method_ICollection__.COUNT(_listSlotIndex);
		}
	}

	public const string KEY_NEW_EQUIP_ITEM_UID = "NewEquipItemUid_";

	private readonly UserObserverManager<EquipStatInfo> _observerManager;

	private HashSet<int> _AddedEquipItemSlots = new HashSet<int>();

	private Dictionary<string, int> _equipItemUidLinkSlotIndex = new Dictionary<string, int>();

	private TechnoMagic _technoMagicItem;

	public IUserObserverManager ObserverManager => _observerManager;

	public UserEquipItemManager()
	{
		_observerManager = new UserObserverManager<EquipStatInfo>();
	}

	public void SetEquipStat(int slotIndex, EquipStatType statType, long newValue)
	{
		EquipItemInfo equipItemInfo = Get(slotIndex);
		if (equipItemInfo != null)
		{
			SetEquipStat(equipItemInfo, statType, newValue);
		}
	}

	public void SetEquipStat(EquipItemInfo equipItemInfo, InventoryType inventoryType)
	{
		if (equipItemInfo == null)
		{
			return;
		}
		equipItemInfo.SetInventoryType(inventoryType);
		if (equipItemInfo.IsTechnoMagic && !equipItemInfo.IsIdentified)
		{
			if (inventoryType == InventoryType.Inventory)
			{
				_technoMagicItem.IncreaseUnIdentified();
			}
			else
			{
				_technoMagicItem.DecreaseUnIdentified();
			}
		}
		_observerManager.AnnounceToObservers(new EquipStatInfo
		{
			EquipItemInfo = equipItemInfo,
			SlotIndex = equipItemInfo.SlotIndex,
			StatType = EquipStatType.ChangeInventoryType
		});
	}

	public void SetEquipStat(EquipItemInfo equipItemInfo, EquipStatType statType, long newValue)
	{
		if (equipItemInfo == null)
		{
			return;
		}
		long num = newValue;
		switch (statType)
		{
		case EquipStatType.Star:
			num = equipItemInfo.Star;
			if (num != newValue)
			{
				equipItemInfo.Star = (int)newValue;
			}
			break;
		case EquipStatType.Level:
			num = equipItemInfo.Level;
			if (num != newValue)
			{
				equipItemInfo.Level = (int)newValue;
			}
			break;
		case EquipStatType.OptionIndex1:
			num = equipItemInfo.OptionIndex1;
			if (num != newValue)
			{
				equipItemInfo.OptionIndex1 = (int)newValue;
			}
			break;
		case EquipStatType.OptionIndex2:
			num = equipItemInfo.OptionIndex2;
			if (num != newValue)
			{
				equipItemInfo.OptionIndex2 = (int)newValue;
			}
			break;
		case EquipStatType.OptionIndex3:
			num = equipItemInfo.OptionIndex3;
			if (num != newValue)
			{
				equipItemInfo.OptionIndex3 = (int)newValue;
			}
			break;
		case EquipStatType.OptionIndex4:
			num = equipItemInfo.OptionIndex4;
			if (num != newValue)
			{
				equipItemInfo.OptionIndex4 = (int)newValue;
			}
			break;
		case EquipStatType.OptionIndex5:
			num = equipItemInfo.OptionIndex5;
			if (num != newValue)
			{
				equipItemInfo.OptionIndex5 = (int)newValue;
			}
			break;
		case EquipStatType.OptionStep1:
			num = equipItemInfo.OptionStep1;
			if (num != newValue)
			{
				equipItemInfo.OptionStep1 = (int)newValue;
			}
			break;
		case EquipStatType.OptionStep2:
			num = equipItemInfo.OptionStep2;
			if (num != newValue)
			{
				equipItemInfo.OptionStep2 = (int)newValue;
			}
			break;
		case EquipStatType.OptionStep3:
			num = equipItemInfo.OptionStep3;
			if (num != newValue)
			{
				equipItemInfo.OptionStep3 = (int)newValue;
			}
			break;
		case EquipStatType.OptionStep4:
			num = equipItemInfo.OptionStep4;
			if (num != newValue)
			{
				equipItemInfo.OptionStep4 = (int)newValue;
			}
			break;
		case EquipStatType.OptionStep5:
			num = equipItemInfo.OptionStep5;
			if (num != newValue)
			{
				equipItemInfo.OptionStep5 = (int)newValue;
			}
			break;
		case EquipStatType.OptionRenewCount1:
			num = equipItemInfo.OptionRenewCount1;
			if (num != newValue)
			{
				equipItemInfo.OptionRenewCount1 = (int)newValue;
			}
			break;
		case EquipStatType.OptionRenewCount2:
			num = equipItemInfo.OptionRenewCount2;
			if (num != newValue)
			{
				equipItemInfo.OptionRenewCount2 = (int)newValue;
			}
			break;
		case EquipStatType.OptionRenewCount3:
			num = equipItemInfo.OptionRenewCount3;
			if (num != newValue)
			{
				equipItemInfo.OptionRenewCount3 = (int)newValue;
			}
			break;
		case EquipStatType.OptionRenewCount4:
			num = equipItemInfo.OptionRenewCount4;
			if (num != newValue)
			{
				equipItemInfo.OptionRenewCount4 = (int)newValue;
			}
			break;
		case EquipStatType.OptionRenewCount5:
			num = equipItemInfo.OptionRenewCount5;
			if (num != newValue)
			{
				equipItemInfo.OptionRenewCount5 = (int)newValue;
			}
			break;
		case EquipStatType.IsRenewedOption1:
			num = equipItemInfo.IsRenewedOption1;
			if (num != newValue)
			{
				equipItemInfo.IsRenewedOption1 = (int)newValue;
			}
			break;
		case EquipStatType.IsRenewedOption2:
			num = equipItemInfo.IsRenewedOption2;
			if (num != newValue)
			{
				equipItemInfo.IsRenewedOption2 = (int)newValue;
			}
			break;
		case EquipStatType.IsRenewedOption3:
			num = equipItemInfo.IsRenewedOption3;
			if (num != newValue)
			{
				equipItemInfo.IsRenewedOption3 = (int)newValue;
			}
			break;
		case EquipStatType.IsRenewedOption4:
			num = equipItemInfo.IsRenewedOption4;
			if (num != newValue)
			{
				equipItemInfo.IsRenewedOption4 = (int)newValue;
			}
			break;
		case EquipStatType.IsRenewedOption5:
			num = equipItemInfo.IsRenewedOption5;
			if (num != newValue)
			{
				equipItemInfo.IsRenewedOption5 = (int)newValue;
			}
			break;
		case EquipStatType.RuneItemIndex1:
			num = equipItemInfo.RuneItemIndex1;
			if (num != newValue)
			{
				equipItemInfo.RuneItemIndex1 = (int)newValue;
			}
			break;
		case EquipStatType.RuneItemIndex2:
			num = equipItemInfo.RuneItemIndex2;
			if (num != newValue)
			{
				equipItemInfo.RuneItemIndex2 = (int)newValue;
			}
			break;
		case EquipStatType.RuneItemIndex3:
			num = equipItemInfo.RuneItemIndex3;
			if (num != newValue)
			{
				equipItemInfo.RuneItemIndex3 = (int)newValue;
			}
			break;
		case EquipStatType.ApplyRunePage:
			num = equipItemInfo.ApplyRunePage;
			if (num != newValue)
			{
				equipItemInfo.ApplyRunePage = (int)newValue;
			}
			break;
		case EquipStatType.UpgradeStarFailBonus:
			num = equipItemInfo.UpgradeStarFailBonus;
			if (num != newValue)
			{
				equipItemInfo.UpgradeStarFailBonus = (int)newValue;
			}
			break;
		case EquipStatType.Locked:
			num = equipItemInfo.Locked;
			if (num != newValue)
			{
				equipItemInfo.Locked = (byte)newValue;
			}
			break;
		case EquipStatType.EnchantOptionIndex1:
			num = equipItemInfo.EnchantOptionIndex1;
			if (num != newValue)
			{
				equipItemInfo.EnchantOptionIndex1 = (int)newValue;
			}
			break;
		case EquipStatType.EnchantOptionIndex2:
			num = equipItemInfo.EnchantOptionIndex2;
			if (num != newValue)
			{
				equipItemInfo.EnchantOptionIndex2 = (int)newValue;
			}
			break;
		case EquipStatType.EnchantOptionIndex3:
			num = equipItemInfo.EnchantOptionIndex3;
			if (num != newValue)
			{
				equipItemInfo.EnchantOptionIndex3 = (int)newValue;
			}
			break;
		case EquipStatType.EnchantOptionStep1:
			num = equipItemInfo.EnchantOptionStep1;
			if (num != newValue)
			{
				equipItemInfo.EnchantOptionStep1 = (int)newValue;
			}
			break;
		case EquipStatType.EnchantOptionStep2:
			num = equipItemInfo.EnchantOptionStep2;
			if (num != newValue)
			{
				equipItemInfo.EnchantOptionStep2 = (int)newValue;
			}
			break;
		case EquipStatType.EnchantOptionStep3:
			num = equipItemInfo.EnchantOptionStep3;
			if (num != newValue)
			{
				equipItemInfo.EnchantOptionStep3 = (int)newValue;
			}
			break;
		case EquipStatType.RenewEnchantOptionSlotIndex:
			num = equipItemInfo.RenewEnchantOptionSlotIndex;
			if (num != newValue)
			{
				equipItemInfo.RenewEnchantOptionSlotIndex = (byte)newValue;
			}
			break;
		case EquipStatType.Identified:
			num = equipItemInfo.Identified;
			if (num != newValue)
			{
				equipItemInfo.Identified = (int)newValue;
				_technoMagicItem.DecreaseUnIdentified();
			}
			break;
		case EquipStatType.ExtraOptionIndex1:
			num = equipItemInfo.ExtraOptionIndex1;
			if (num != newValue)
			{
				equipItemInfo.ExtraOptionIndex1 = (int)newValue;
			}
			break;
		case EquipStatType.ExtraOptionIndex2:
			num = equipItemInfo.ExtraOptionIndex2;
			if (num != newValue)
			{
				equipItemInfo.ExtraOptionIndex2 = (int)newValue;
			}
			break;
		case EquipStatType.ExtraOptionStep1:
			num = equipItemInfo.ExtraOptionStep1;
			if (num != newValue)
			{
				equipItemInfo.ExtraOptionStep1 = (int)newValue;
			}
			break;
		case EquipStatType.ExtraOptionStep2:
			num = equipItemInfo.ExtraOptionStep2;
			if (num != newValue)
			{
				equipItemInfo.ExtraOptionStep2 = (int)newValue;
			}
			break;
		case EquipStatType.ExtraOptionRenewCount1:
			num = equipItemInfo.ExtraOptionRenewCount1;
			if (num != newValue)
			{
				equipItemInfo.ExtraOptionRenewCount1 = (int)newValue;
			}
			break;
		case EquipStatType.ExtraOptionRenewCount2:
			num = equipItemInfo.ExtraOptionRenewCount2;
			if (num != newValue)
			{
				equipItemInfo.ExtraOptionRenewCount2 = (int)newValue;
			}
			break;
		case EquipStatType.IsRenewedExtraOption1:
			num = equipItemInfo.IsRenewedExtraOption1;
			if (num != newValue)
			{
				equipItemInfo.IsRenewedExtraOption1 = (int)newValue;
			}
			break;
		case EquipStatType.IsRenewedExtraOption2:
			num = equipItemInfo.IsRenewedExtraOption2;
			if (num != newValue)
			{
				equipItemInfo.IsRenewedExtraOption2 = (int)newValue;
			}
			break;
		}
		if (num != newValue)
		{
			equipItemInfo.ResetEquipItemScore();
			_observerManager.AnnounceToObservers(new EquipStatInfo
			{
				SlotIndex = equipItemInfo.SlotIndex,
				EquipItemInfo = equipItemInfo,
				StatType = statType,
				BeforeValue = num,
				AfterValue = newValue
			});
		}
	}

	public void Apply(IEnumerable<EquipItemInfo> equipItemInfos)
	{
		if (equipItemInfos == null)
		{
			return;
		}
		foreach (EquipItemInfo equipItemInfo in equipItemInfos)
		{
			if (equipItemInfo != null)
			{
				Apply(equipItemInfo.SlotIndex, equipItemInfo);
			}
		}
	}

	public void ApplyRemoved(IEnumerable<EquipItemInfo> equipItemInfos)
	{
		if (equipItemInfos == null)
		{
			return;
		}
		foreach (EquipItemInfo equipItemInfo in equipItemInfos)
		{
			Apply(equipItemInfo.SlotIndex, null);
		}
	}

	public void ApplyRemoved(IEnumerable<int> equipItemSlotIndices)
	{
		if (equipItemSlotIndices == null)
		{
			return;
		}
		foreach (int equipItemSlotIndex in equipItemSlotIndices)
		{
			Apply(equipItemSlotIndex, null);
		}
	}

	public void Apply(int slotIndex, EquipItemInfo equipItemInfo)
	{
		EquipItemInfo equipItemInfo2 = Get(slotIndex);
		if (equipItemInfo2 == null)
		{
			if (equipItemInfo != null)
			{
				equipItemInfo.UpdateCreateTime();
				AddNewItem(slotIndex, equipItemInfo);
			}
			return;
		}
		if (equipItemInfo == null)
		{
			Remove(slotIndex);
			return;
		}
		SetEquipStat(equipItemInfo2, EquipStatType.Star, equipItemInfo.Star);
		SetEquipStat(equipItemInfo2, EquipStatType.Level, equipItemInfo.Level);
		SetEquipStat(equipItemInfo2, EquipStatType.OptionIndex1, equipItemInfo.OptionIndex1);
		SetEquipStat(equipItemInfo2, EquipStatType.OptionIndex2, equipItemInfo.OptionIndex2);
		SetEquipStat(equipItemInfo2, EquipStatType.OptionIndex3, equipItemInfo.OptionIndex3);
		SetEquipStat(equipItemInfo2, EquipStatType.OptionIndex4, equipItemInfo.OptionIndex4);
		SetEquipStat(equipItemInfo2, EquipStatType.OptionIndex5, equipItemInfo.OptionIndex5);
		SetEquipStat(equipItemInfo2, EquipStatType.OptionStep1, equipItemInfo.OptionStep1);
		SetEquipStat(equipItemInfo2, EquipStatType.OptionStep2, equipItemInfo.OptionStep2);
		SetEquipStat(equipItemInfo2, EquipStatType.OptionStep3, equipItemInfo.OptionStep3);
		SetEquipStat(equipItemInfo2, EquipStatType.OptionStep4, equipItemInfo.OptionStep4);
		SetEquipStat(equipItemInfo2, EquipStatType.OptionStep5, equipItemInfo.OptionStep5);
		SetEquipStat(equipItemInfo2, EquipStatType.OptionRenewCount1, equipItemInfo.OptionRenewCount1);
		SetEquipStat(equipItemInfo2, EquipStatType.OptionRenewCount2, equipItemInfo.OptionRenewCount2);
		SetEquipStat(equipItemInfo2, EquipStatType.OptionRenewCount3, equipItemInfo.OptionRenewCount3);
		SetEquipStat(equipItemInfo2, EquipStatType.OptionRenewCount4, equipItemInfo.OptionRenewCount4);
		SetEquipStat(equipItemInfo2, EquipStatType.OptionRenewCount5, equipItemInfo.OptionRenewCount5);
		SetEquipStat(equipItemInfo2, EquipStatType.IsRenewedOption1, equipItemInfo.IsRenewedOption1);
		SetEquipStat(equipItemInfo2, EquipStatType.IsRenewedOption2, equipItemInfo.IsRenewedOption2);
		SetEquipStat(equipItemInfo2, EquipStatType.IsRenewedOption3, equipItemInfo.IsRenewedOption3);
		SetEquipStat(equipItemInfo2, EquipStatType.IsRenewedOption4, equipItemInfo.IsRenewedOption4);
		SetEquipStat(equipItemInfo2, EquipStatType.IsRenewedOption5, equipItemInfo.IsRenewedOption5);
		SetEquipStat(equipItemInfo2, EquipStatType.RuneItemIndex1, equipItemInfo.RuneItemIndex1);
		SetEquipStat(equipItemInfo2, EquipStatType.RuneItemIndex2, equipItemInfo.RuneItemIndex2);
		SetEquipStat(equipItemInfo2, EquipStatType.RuneItemIndex3, equipItemInfo.RuneItemIndex3);
		SetEquipStat(equipItemInfo2, EquipStatType.UpgradeStarFailBonus, equipItemInfo.UpgradeStarFailBonus);
		SetEquipStat(equipItemInfo2, EquipStatType.Locked, equipItemInfo.Locked);
		SetEquipStat(equipItemInfo2, EquipStatType.EnchantOptionIndex1, equipItemInfo.EnchantOptionIndex1);
		SetEquipStat(equipItemInfo2, EquipStatType.EnchantOptionIndex2, equipItemInfo.EnchantOptionIndex2);
		SetEquipStat(equipItemInfo2, EquipStatType.EnchantOptionIndex3, equipItemInfo.EnchantOptionIndex3);
		SetEquipStat(equipItemInfo2, EquipStatType.EnchantOptionStep1, equipItemInfo.EnchantOptionStep1);
		SetEquipStat(equipItemInfo2, EquipStatType.EnchantOptionStep2, equipItemInfo.EnchantOptionStep2);
		SetEquipStat(equipItemInfo2, EquipStatType.EnchantOptionStep3, equipItemInfo.EnchantOptionStep3);
		SetEquipStat(equipItemInfo2, EquipStatType.RenewEnchantOptionSlotIndex, equipItemInfo.RenewEnchantOptionSlotIndex);
		SetEquipStat(equipItemInfo2, EquipStatType.ApplyRunePage, equipItemInfo.ApplyRunePage);
		SetEquipStat(equipItemInfo2, EquipStatType.Identified, equipItemInfo.Identified);
		SetEquipStat(equipItemInfo2, EquipStatType.ExtraOptionIndex1, equipItemInfo.ExtraOptionIndex1);
		SetEquipStat(equipItemInfo2, EquipStatType.ExtraOptionIndex2, equipItemInfo.ExtraOptionIndex2);
		SetEquipStat(equipItemInfo2, EquipStatType.ExtraOptionStep1, equipItemInfo.ExtraOptionStep1);
		SetEquipStat(equipItemInfo2, EquipStatType.ExtraOptionStep2, equipItemInfo.ExtraOptionStep2);
		SetEquipStat(equipItemInfo2, EquipStatType.ExtraOptionRenewCount1, equipItemInfo.ExtraOptionRenewCount1);
		SetEquipStat(equipItemInfo2, EquipStatType.ExtraOptionRenewCount2, equipItemInfo.ExtraOptionRenewCount2);
		SetEquipStat(equipItemInfo2, EquipStatType.IsRenewedExtraOption1, equipItemInfo.IsRenewedExtraOption1);
		SetEquipStat(equipItemInfo2, EquipStatType.IsRenewedExtraOption2, equipItemInfo.IsRenewedExtraOption2);
	}

	public void ApplyLevel(UpdatedEquipItemInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			UpdatedEquipItemInfo info = infos[i];
			ApplyLevel(info);
		}
	}

	public void ApplyLevel(UpdatedEquipItemInfo info)
	{
		if (info != null)
		{
			EquipItemInfo equipItemInfo = Get(info.SlotIndex);
			if (equipItemInfo != null && info.ItemIndex == equipItemInfo.ItemIndex)
			{
				SetEquipStat(info.SlotIndex, EquipStatType.Level, info.Level);
			}
		}
	}

	public void ApplyInventoryType(int slotIndex, InventoryType inventoryType)
	{
		EquipItemInfo equipItemInfo = Get(slotIndex);
		if (equipItemInfo != null)
		{
			SetEquipStat(equipItemInfo, inventoryType);
		}
	}

	public void ApplyInventoryType(EquipItemInfo equipItemInfo, InventoryType inventoryType)
	{
		if (equipItemInfo != null)
		{
			SetEquipStat(equipItemInfo, inventoryType);
		}
	}

	public void Apply(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null)
		{
			Apply(equipItemInfo.SlotIndex, equipItemInfo);
		}
	}

	public void ApplyEquippedHeroIndex(EquipItemInfo equipItemInfo, int oldEquippedHeroIndex, int newEquippedHeroIndex)
	{
		if (equipItemInfo != null)
		{
			int slotIndex = equipItemInfo.SlotIndex;
			_observerManager.AnnounceToObservers(new EquipStatInfo
			{
				SlotIndex = slotIndex,
				EquipItemInfo = equipItemInfo,
				StatType = EquipStatType.EquippedHeroIndex,
				BeforeValue = oldEquippedHeroIndex,
				AfterValue = newEquippedHeroIndex
			});
		}
	}

	public bool IsNewEquipByParTypeWithInventoryMode(EquipPartType partType, bool isChestBox)
	{
		HashSet<int>.Enumerator enumerator = _AddedEquipItemSlots.GetEnumerator();
		while (enumerator.MoveNext())
		{
			EquipItemInfo equipItemInfo = Get(enumerator.Current);
			if (equipItemInfo != null)
			{
				ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfo.ItemIndex);
				if ((data == null || data.SubData.PartType == partType) && equipItemInfo.GetInventoryType() == InventoryType.Chest == isChestBox)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsNewPunishmentRune()
	{
		HashSet<int>.Enumerator enumerator = _AddedEquipItemSlots.GetEnumerator();
		while (enumerator.MoveNext())
		{
			EquipItemInfo equipItemInfo = Get(enumerator.Current);
			if (equipItemInfo != null)
			{
				ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfo.ItemIndex);
				if ((data == null || data.Type == ItemType.PunishmentRune) && equipItemInfo.GetInventoryType() == InventoryType.PunishmentRune)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool DoesExistAddedEquipItemByType(CachedHeroInfo heroInfo, EquipPartType partType)
	{
		if (heroInfo == null)
		{
			return false;
		}
		HashSet<int>.Enumerator enumerator = _AddedEquipItemSlots.GetEnumerator();
		while (enumerator.MoveNext())
		{
			EquipItemInfo equipItemInfo = Get(enumerator.Current);
			if (equipItemInfo == null)
			{
				continue;
			}
			ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfo.ItemIndex);
			if (data != null && data.SubData.PartType == partType)
			{
				CachedHeroInfo.EquipResult equipResult = heroInfo.CheckEquipResult(data.SubData);
				if (equipResult == CachedHeroInfo.EquipResult.Success || equipResult == CachedHeroInfo.EquipResult.LowLevel)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AddedEquipItemExists()
	{
		if (_AddedEquipItemSlots.Count <= 0)
		{
			return false;
		}
		return true;
	}

	public bool IsNewEquippableOnHero(CachedHeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			return false;
		}
		HashSet<int>.Enumerator enumerator = _AddedEquipItemSlots.GetEnumerator();
		while (enumerator.MoveNext())
		{
			EquipItemInfo equipItemInfo = Get(enumerator.Current);
			if (equipItemInfo == null || equipItemInfo.GetInventoryType() == InventoryType.Chest)
			{
				continue;
			}
			ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfo.ItemIndex);
			if (data != null)
			{
				CachedHeroInfo.EquipResult equipResult = heroInfo.CheckEquipResult(data.SubData);
				if (equipResult == CachedHeroInfo.EquipResult.Success || equipResult == CachedHeroInfo.EquipResult.LowLevel)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsNew(string Uid)
	{
		if (string.IsNullOrEmpty(Uid))
		{
			return false;
		}
		LocalAccountString localAccountString = new LocalAccountString("", "NewEquipItemUid_" + Uid);
		if (string.IsNullOrEmpty(localAccountString.Value))
		{
			return false;
		}
		if (Uid == localAccountString.Value)
		{
			return true;
		}
		return false;
	}

	public void AddNewItem(int slotIndex, EquipItemInfo value)
	{
		Add(value);
		if (CheckPossibleAddNew(value))
		{
			_AddedEquipItemSlots.Add(slotIndex);
			_observerManager.AnnounceToObservers(new EquipStatInfo
			{
				StatType = EquipStatType.Add,
				SlotIndex = slotIndex,
				EquipItemInfo = value
			});
			new LocalAccountString("", "NewEquipItemUid_" + value.Uid).Value = value.Uid;
		}
	}

	private bool CheckPossibleAddNew(EquipItemInfo info)
	{
		if (info != null && info.ItemData != null && info.ItemData.Type == ItemType.EventEnchantWeapon)
		{
			return false;
		}
		return true;
	}

	public override bool Remove(int slotIndex)
	{
		_AddedEquipItemSlots.Remove(slotIndex);
		EquipItemInfo equipItemInfo = Get(slotIndex);
		if (equipItemInfo != null)
		{
			if (!String_Extension_Method.IsNullOrEmpty(equipItemInfo.Uid))
			{
				_equipItemUidLinkSlotIndex.Remove(equipItemInfo.Uid);
			}
			if (equipItemInfo.IsTechnoMagic)
			{
				_technoMagicItem.RemoveSlotIndex(slotIndex);
				if (!equipItemInfo.IsIdentified)
				{
					_technoMagicItem.DecreaseUnIdentified();
				}
			}
		}
		_observerManager.AnnounceToObservers(new EquipStatInfo
		{
			StatType = EquipStatType.Remove,
			SlotIndex = slotIndex,
			EquipItemInfo = null
		});
		return SprkPresetDisposal.AfterRemove(base.Remove(slotIndex), slotIndex);
	}

	[Obsolete("", true)]
	public override bool Remove(EquipItemInfo value)
	{
		if (value == null)
		{
			return false;
		}
		return Remove(value.SlotIndex);
	}

	public void NewIconRemove(EquipItemInfo value)
	{
		if (value != null)
		{
			_AddedEquipItemSlots.Remove(value.SlotIndex);
			_observerManager.AnnounceToObservers(new EquipStatInfo
			{
				StatType = EquipStatType.NewIconRemove,
				SlotIndex = value.SlotIndex,
				EquipItemInfo = value
			});
			new LocalAccountString("", "NewEquipItemUid_" + value.Uid).Remove();
		}
	}

	public override void Set(EquipItemInfo[] values)
	{
		if (values == null)
		{
			return;
		}
		foreach (EquipItemInfo equipItemInfo in values)
		{
			equipItemInfo.UpdateCreateTime();
			if (IsNew(equipItemInfo.Uid))
			{
				AddNewItem(equipItemInfo.SlotIndex, equipItemInfo);
			}
			else
			{
				Add(equipItemInfo);
			}
		}
	}

	protected override void OnAddData(int key, EquipItemInfo value)
	{
		base.OnAddData(key, value);
		if (!String_Extension_Method.IsNullOrEmpty(value.Uid) && !_equipItemUidLinkSlotIndex.ContainsKey(value.Uid))
		{
			_equipItemUidLinkSlotIndex.Add(value.Uid, value.SlotIndex);
		}
		if (value.IsTechnoMagic)
		{
			_technoMagicItem.AddSlotIndex(value.SlotIndex);
			if (!value.IsIdentified && value.GetInventoryType() == InventoryType.Inventory)
			{
				_technoMagicItem.IncreaseUnIdentified();
			}
		}
	}

	public EquipItemInfo GetEquipItemBySlotIndex(int slotIndex)
	{
		EquipItemInfo[] values = GetValues();
		if (__Extension_Method_ICollection__.COUNT(values) == 0)
		{
			return null;
		}
		return Array.Find(values, (EquipItemInfo info) => info.SlotIndex == slotIndex);
	}

	public EquipItemInfo[] GetEquipItemsByItemType(ItemType type)
	{
		EquipItemInfo[] values = GetValues();
		if (__Extension_Method_ICollection__.COUNT(values) == 0)
		{
			return null;
		}
		return Array.FindAll(values, (EquipItemInfo info) => info.ItemData.Type == type);
	}

	public int GetSpecificSpecialWeaponCount(int itemIndex)
	{
		EquipItemInfo[] values = GetValues();
		EquipItemInfo equipItemInfo = null;
		int num = 0;
		for (int i = 0; i < values.Length; i++)
		{
			equipItemInfo = values[i];
			if (equipItemInfo != null && !equipItemInfo.IsSoulWeapon() && equipItemInfo.ItemIndex == itemIndex)
			{
				num++;
			}
		}
		return num;
	}

	public EquipItemInfo GetEquipItemInfoByUid(string uId)
	{
		if (string.IsNullOrEmpty(uId))
		{
			return null;
		}
		int value = 0;
		if (_equipItemUidLinkSlotIndex.TryGetValue(uId, out value))
		{
			return Get(value);
		}
		return null;
	}

	public bool HasUnIdentifiedItem()
	{
		return _technoMagicItem.HasUnIdentifiedItem();
	}

	public List<EquipItemInfo> GetTechnoMagicItem(Func<EquipItemInfo, bool> selctor, IComparer<EquipItemInfo> comparer)
	{
		List<int> slotIndexList = _technoMagicItem.GetSlotIndexList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(slotIndexList) || comparer == null)
		{
			return null;
		}
		int count = slotIndexList.Count;
		List<EquipItemInfo> list = new List<EquipItemInfo>(count);
		for (int i = 0; i < count; i++)
		{
			int key = slotIndexList[i];
			EquipItemInfo equipItemInfo = Get(key);
			if (equipItemInfo != null && (selctor == null || selctor(equipItemInfo)))
			{
				list.Add(equipItemInfo);
			}
		}
		if (comparer != null)
		{
			list.Sort(comparer);
		}
		return list;
	}
}
