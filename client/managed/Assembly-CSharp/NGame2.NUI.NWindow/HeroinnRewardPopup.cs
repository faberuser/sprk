using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NHelper;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroinnRewardPopup : BaseRewardPopup
{
	public new const string PrefabPath = "Resources/Prefab2/Window/RewardPopup/HeroinnRewardPopup";

	public new static PrefabAuto<HeroinnRewardPopup> PrefabAuto = new PrefabAuto<HeroinnRewardPopup>("Resources/Prefab2/Window/RewardPopup/HeroinnRewardPopup");

	public new static HeroinnRewardPopup OpenPopup(PopupOpenData popupOpen, bool isGlobal, Action onClose = null)
	{
		if (popupOpen.CalcRewardCount() == 0)
		{
			Vespa_System_Action_Extension_Method.fire(onClose);
			return null;
		}
		if (String_Extension_Method.IsNullOrEmpty(popupOpen.title))
		{
			popupOpen.SetTitle(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM));
		}
		Transform parentTm = ((!isGlobal) ? GameUIRoot.instance.ContentsGroup.transform : GlobalUIRoot.instance.ContentsGroup.transform);
		HeroinnRewardPopup heroinnRewardPopup = PrefabAuto.Factory.Create(parentTm);
		if (!heroinnRewardPopup.OpenWithData(popupOpen, isGlobal, onClose))
		{
			Vespa_System_Action_Extension_Method.fire(onClose);
			return null;
		}
		heroinnRewardPopup.SetTitle(popupOpen.title);
		heroinnRewardPopup.ShowOnlyReward(popupOpen.ShowOnlyReward);
		return heroinnRewardPopup;
	}

	public new static HeroinnRewardPopup Create(bool isGlobal)
	{
		Transform parentTm = ((!isGlobal) ? GameUIRoot.instance.ContentsGroup.transform : GlobalUIRoot.instance.ContentsGroup.transform);
		return PrefabAuto.Factory.Create(parentTm, setActive: false);
	}

	protected override void SetupRewardItems()
	{
		RewardItemIconOrderData[] array = GameRoot.Instance.RewardItemIconOrderDataContainer.GetAllData().ToArray();
		Array.Sort(array, (RewardItemIconOrderData x, RewardItemIconOrderData y) => x.Priority.CompareTo(y.Priority));
		RewardItemIconOrderData[] array2 = array;
		foreach (RewardItemIconOrderData rewardItemIconOrderData in array2)
		{
			if (rewardItemIconOrderData.RewardItemIconType == RewardItemIconType.Item)
			{
				SetupReward(_popupOpen.itemResultInfos);
			}
			else if (rewardItemIconOrderData.RewardItemIconType == RewardItemIconType.Equip)
			{
				SetupRewardEquip(_popupOpen.equipResultInfos);
			}
			else if (rewardItemIconOrderData.RewardItemIconType == RewardItemIconType.GuildRaidBootyItem)
			{
				SetupRewardIndexable(_popupOpen.guildRaidBootyItemRewardInfos);
			}
			else
			{
				if (rewardItemIconOrderData.RewardItemIconType != RewardItemIconType.Currency)
				{
					continue;
				}
				CurrencyResultInfo3[] currencyResultListByType = _popupOpen.GetCurrencyResultListByType(rewardItemIconOrderData.CurrencyType);
				if (currencyResultListByType == null)
				{
					if (rewardItemIconOrderData.CurrencyType == CurrencyType.Stamina)
					{
						SetupReward(_popupOpen.staminaResultInfos);
					}
					else if (rewardItemIconOrderData.CurrencyType == CurrencyType.GuildPoint)
					{
						SetupReward(_popupOpen.guildPointResult);
					}
					else if (rewardItemIconOrderData.CurrencyType == CurrencyType.FriendshipPoint)
					{
						SetupReward(_popupOpen.friendshipPointResult);
						SetupReward(_popupOpen.friendshipPointResults);
					}
				}
				else
				{
					SetupReward(currencyResultListByType.ToArray());
				}
			}
		}
		foreach (KeyValuePair<CurrencyType, List<CurrencyResultInfo3>> info in _popupOpen.GetCurrencyResultInfos())
		{
			if (!Array.Exists(array, (RewardItemIconOrderData orderData) => orderData.CurrencyType == info.Key))
			{
				SetupReward(info.Value.ToArray());
			}
		}
		Reposition();
	}

	private void SetupRewardIndexable(IndexedRewardItemInfo[] rewardInfos)
	{
		if (__Extension_Method_ICollection__.COUNT(rewardInfos) == 0)
		{
			return;
		}
		List<ItemResultInfo> list = null;
		List<EquipItemInfo> list2 = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewardInfos); i++)
		{
			IndexedRewardItemInfo indexedRewardItemInfo = rewardInfos[i];
			int itemIndex = indexedRewardItemInfo.ItemIndex;
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
			if (data != null)
			{
				if (data.Type == ItemType.Equip)
				{
					list2 = list2 ?? new List<EquipItemInfo>();
					list2.Add(new EquipItemInfo
					{
						ItemIndex = itemIndex,
						Star = indexedRewardItemInfo.Star
					});
				}
				else
				{
					list = list ?? new List<ItemResultInfo>();
					list.Add(new ItemResultInfo
					{
						ItemIndex = itemIndex,
						AddCount = indexedRewardItemInfo.Count
					});
				}
			}
		}
		if (__Extension_Method_ICollection__.COUNT(list2) > 0)
		{
			SetupRewardEquip(list2.ToArray());
		}
		if (__Extension_Method_ICollection__.COUNT(list) > 0)
		{
			SetupReward(list.ToArray());
		}
	}

	private void SetupRewardEquip(EquipItemInfo[] equipItems, bool displayBoostInfo = true)
	{
		if (equipItems == null)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(equipItems); i++)
		{
			EquipItemInfo equipItem = equipItems[i];
			CampaignRewardComponent campaignRewardComponent = UIHelper.AddGridItem(RewardItemGrid, CampaignRewardComponent.PrefabAuto);
			if (!(campaignRewardComponent == null))
			{
				bool flag = __Extension_Method_ICollection__.COUNT(_popupOpen.boostEquipResultInfos) > 0 && Array.Find(_popupOpen.boostEquipResultInfos, (EquipItemBoostInfo e) => equipItem.SlotIndex == e.SlotIndex) != null;
				flag = flag && displayBoostInfo;
				GetItemAnimationData getItemAnimation = campaignRewardComponent.Open(equipItem, flag, this);
				_itemAnimationContainer.Add(getItemAnimation);
				AddRewardList(campaignRewardComponent);
			}
		}
	}

	public void SetupReward(ItemResultInfo[] result)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(result); i++)
		{
			SetupReward(result[i]);
		}
	}

	public void SetupReward(ItemResultInfo result)
	{
		if (result == null || result.AddCount <= 0)
		{
			return;
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(result.ItemIndex);
		if (data != null)
		{
			CampaignRewardComponent campaignRewardComponent = UIHelper.AddGridItem(RewardItemGrid, CampaignRewardComponent.PrefabAuto);
			if (!(campaignRewardComponent == null))
			{
				GetItemAnimationData getItemAnimation = campaignRewardComponent.Open(data, result.AddCount, result.AddBoosterCount, result.AddNPCBoosterCount, result.AddBonusAssignedItemPercent, this, displayInvenCount: true, displayBoosterInfo: true, isFIrstClearReward: false, isBestTeam: false);
				_itemAnimationContainer.Add(getItemAnimation);
				AddRewardList(campaignRewardComponent);
			}
		}
	}

	public void SetupReward<T>(T[] result) where T : ICurrencyResultInfo
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(result); i++)
		{
			SetupReward(result[i]);
		}
	}

	public new void SetupReward<T>(T result) where T : ICurrencyResultInfo
	{
		if (result != null)
		{
			long addValue = result.AddValue;
			int num = 0;
			if (addValue > num)
			{
				long addValue2 = result.AddValue;
				CurrencyType currencyType = result.CurrencyType;
				int num2 = 0;
				SetupRewardItemComponent(addValue2, currencyType, "", num2);
			}
		}
	}

	public void SetupReward(StaminaResultInfo[] result)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(result); i++)
		{
			SetupReward(result[i]);
		}
	}

	public void SetupReward(StaminaResultInfo result)
	{
		if (result != null)
		{
			long addValue = result.AddValue;
			int num = 0;
			if (addValue > num)
			{
				CurrencyType currencyTypeByStaminaType = BaseRewardPopup.GetCurrencyTypeByStaminaType(result.Type);
				long addValue2 = result.AddValue;
				int num2 = 0;
				SetupRewardItemComponent(addValue2, currencyTypeByStaminaType, "", num2);
			}
		}
	}

	public void SetupReward(CurrencyResultInfo2[] result)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(result); i++)
		{
			SetupReward(result[i]);
		}
	}

	public void SetupReward(CurrencyResultInfo2 result)
	{
		if (result != null)
		{
			long addValue = result.AddValue;
			int num = 0;
			if (addValue > num)
			{
				long addValue2 = result.AddValue;
				CurrencyType currencyType = result.CurrencyType;
				int num2 = 0;
				SetupRewardItemComponent(addValue2, currencyType, "", num2);
			}
		}
	}

	public void SetupReward(CurrencyResultInfo3[] result)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(result); i++)
		{
			SetupReward(result[i]);
		}
	}

	public void SetupReward(CurrencyResultInfo3 result)
	{
		if (result != null && result.AddValue > 0)
		{
			long value = result.AddValue;
			CurrencyType currencyType = result.CurrencyType;
			int num = 0;
			SetupRewardItemComponent(value, currencyType, "", num);
		}
	}
}
