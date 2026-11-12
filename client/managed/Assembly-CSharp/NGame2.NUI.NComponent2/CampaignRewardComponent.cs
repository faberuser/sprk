using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NComponent2.NHelper;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class CampaignRewardComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/Icon/CampaignRewardComponent";

	public static PrefabAuto<CampaignRewardComponent> PrefabAuto = new PrefabAuto<CampaignRewardComponent>("Prefab2/Component/Icon/CampaignRewardComponent");

	public UISimpleGrid IconGrid;

	public UILabel Title;

	private List<Component> _rewardList = new List<Component>();

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		FactoryBase.ReleaseAndClearList(_rewardList);
		UIHelper.EnableGameObject(Title, enable: false);
		UIHelper.SetLabelText(Title, string.Empty);
	}

	public GetItemAnimationData Open(ItemData itemData, int addCount, int addBoosterCount, int addNPCBoosterCount, int addBonusAssignedItemPercent, ItemIconComponent.Listener listener, bool displayInvenCount, bool displayBoosterInfo, bool isFIrstClearReward, bool isBestTeam)
	{
		FactoryBase.ReleaseAndClearList(_rewardList);
		ItemIconComponent itemIconComponent = UIHelper.AddGridItem(IconGrid, ItemIconComponent.PrefabAuto);
		if (itemIconComponent == null)
		{
			return null;
		}
		itemIconComponent.Open(itemData, listener);
		_rewardList.Add(itemIconComponent);
		if (addBonusAssignedItemPercent > 0)
		{
			Color? color = Color.green;
			itemIconComponent.SetItemCount(addCount, color);
			itemIconComponent.ShowEventBonus(isShow: true);
		}
		else
		{
			itemIconComponent.SetItemCount(addCount);
		}
		if (addBoosterCount > 0)
		{
			string boosterCode = GlobalAutoBehaviour<BoosterManager>.instance.GetBoosterCode(EventBonusType.BonusItem);
			itemIconComponent.SetBoosterInReward(displayBoosterInfo, boosterCode, displayBoosterLabel: true);
			itemIconComponent.SetInvenCountInReward(displayInvenCount, itemData, isLightBackground: false);
		}
		else if (addNPCBoosterCount > 0)
		{
			List<HeroBonusData> heroBonusWithBonusType = GameRoot.Instance.HeroBonusDataContainer.GetHeroBonusWithBonusType1(EventBonusType.AddReward);
			if (heroBonusWithBonusType != null && heroBonusWithBonusType.Count > 0)
			{
				int heroIndex = heroBonusWithBonusType[0].HeroIndex;
				CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex);
				if (heroInfo != null)
				{
					HeroBonusData data = GameRoot.Instance.HeroBonusDataContainer.GetData(heroInfo.HeroIndex, heroInfo.Star, heroInfo.Transcended);
					if (data != null)
					{
						itemIconComponent.SetBoosterInReward(displayBoosterInfo, data.Icon, displayBoosterLabel: true);
						itemIconComponent.SetInvenCountInReward(displayInvenCount, itemData, isLightBackground: false);
					}
				}
			}
		}
		else
		{
			itemIconComponent.SetBoosterInReward(displayBooster: false, string.Empty, displayBoosterLabel: false);
			itemIconComponent.SetInvenCountInReward(displayInvenCount, itemData, isLightBackground: false);
		}
		itemIconComponent.SetFIrstClearLabel(isFIrstClearReward);
		itemIconComponent.SetBestTeamMark(isBestTeam);
		return itemIconComponent.CreateGetAnimation();
	}

	public GetItemAnimationData Open(EquipItemInfo equipItemInfo, bool boostApplied, ItemIconComponent.Listener listener)
	{
		FactoryBase.ReleaseAndClearList(_rewardList);
		ItemIconComponent itemIconComponent = UIHelper.AddGridItem(IconGrid, ItemIconComponent.PrefabAuto);
		if (itemIconComponent == null)
		{
			return null;
		}
		itemIconComponent.Open(equipItemInfo, listener);
		_rewardList.Add(itemIconComponent);
		if (boostApplied)
		{
			string boosterCode = GlobalAutoBehaviour<BoosterManager>.instance.GetBoosterCode(EventBonusType.BonusItem);
			itemIconComponent.SetBoosterInReward(displayBooster: true, boosterCode, displayBoosterLabel: true);
			itemIconComponent.SetInvenCountInReward(displayInvenCount: false, null, isLightBackground: false);
		}
		else
		{
			itemIconComponent.SetBoosterInReward(displayBooster: false, string.Empty, displayBoosterLabel: false);
			itemIconComponent.SetInvenCountInReward(displayInvenCount: false, null, isLightBackground: false);
		}
		return itemIconComponent.CreateGetAnimation();
	}

	public GetItemAnimationData Open(CurrencyType type, long value, string title, CurrencyIconComponent.Listener listener)
	{
		FactoryBase.ReleaseAndClearList(_rewardList);
		UIHelper.EnableGameObject(Title, !string.IsNullOrEmpty(title));
		UIHelper.SetLabelText(Title, title);
		CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(IconGrid, CurrencyIconComponent.PrefabAuto);
		if (currencyIconComponent == null)
		{
			return null;
		}
		currencyIconComponent.Open(type, value, listener);
		_rewardList.Add(currencyIconComponent);
		return currencyIconComponent.CreateGetAnimation();
	}

	public GetItemAnimationData Open(CurrencyType type, long value, string fieldName, long boostvalue, CurrencyIconComponent.Listener listener)
	{
		FactoryBase.ReleaseAndClearList(_rewardList);
		CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(IconGrid, CurrencyIconComponent.PrefabAuto);
		if (currencyIconComponent == null)
		{
			return null;
		}
		currencyIconComponent.Open(type, value, listener);
		_rewardList.Add(currencyIconComponent);
		string code = "";
		if (!(fieldName == "AddBoosterValue"))
		{
			if (fieldName == "AddBoosterCount")
			{
				if (type == CurrencyType.EventDungeonPoint || type == CurrencyType.EventDungeonPoint2)
				{
					int num = 0;
					if (boostvalue > num)
					{
						code = GlobalAutoBehaviour<BoosterManager>.instance.GetBoosterCode(EventBonusType.BonusEquipCostumeType);
						goto IL_012a;
					}
				}
				if (type == CurrencyType.EventDungeonPoint3)
				{
					code = GlobalAutoBehaviour<BoosterManager>.instance.GetBoosterCode(EventBonusType.BonusItem);
				}
				goto IL_012a;
			}
		}
		else
		{
			if (type == CurrencyType.Gold)
			{
				int num2 = 0;
				if (boostvalue > num2)
				{
					code = GlobalAutoBehaviour<BoosterManager>.instance.GetBoosterCode(EventBonusType.BonusGold);
					currencyIconComponent.ShowGoldBooster(boostApplied: true, boostvalue, code);
					goto IL_0134;
				}
			}
			if (type == CurrencyType.RaidPoint)
			{
				int num3 = 0;
				if (boostvalue > num3 && GlobalAutoBehaviour<DropBonusEventManager>.instance.IsEffectiveEvent(EventBonusType.BonusRaidPoint))
				{
					currencyIconComponent.SetAmountColor(Color.green);
					currencyIconComponent.ShowEventBonus(isShow: true);
				}
			}
		}
		goto IL_0134;
		IL_012a:
		currencyIconComponent.ShowCountBooster(boostApplied: true, boostvalue, code);
		goto IL_0134;
		IL_0134:
		return currencyIconComponent.CreateGetAnimation();
	}

	public GetItemAnimationData Open(CurrencyType type, long value, CurrencyIconComponent.Listener listener, byte firstClear, bool skipCurrencyAnimation = false)
	{
		FactoryBase.ReleaseAndClearList(_rewardList);
		CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(IconGrid, CurrencyIconComponent.PrefabAuto);
		if (currencyIconComponent == null)
		{
			return null;
		}
		currencyIconComponent.Open(type, value, listener);
		currencyIconComponent.ShowFirstClear(firstClear > 0);
		currencyIconComponent.SetSkipIncrease(skipCurrencyAnimation);
		_rewardList.Add(currencyIconComponent);
		return currencyIconComponent.CreateGetAnimation();
	}
}
