using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NContentsInfo;
using NGame2.NLogin;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.IAP;

public class ApplyToAccountWrapper
{
	public CurrencyResultInfo3[] CurrencyResults { get; private set; }

	public ItemResultInfo[] ItemResults { get; private set; }

	public EquipItemInfo[] EquipItemInfos { get; private set; }

	public HeroAddResultInfo[] HeroAddResults { get; private set; }

	public AttendanceInfo AttendanceInfo { get; private set; }

	public AttendanceData[] AttendanceDatas { get; private set; }

	public PlayerItemTimeDurationInfo ItemTimeDurationInfo { get; private set; }

	public PurchaseMarketingResultInfo PurchaseMarketingResult { get; private set; }

	public PlayerProductPurchaseInfo PlayerProductPurchaseInfo { get; private set; }

	public NewbieMissionInfo[] NewbieMissionInfos { get; private set; }

	public PlayerAnyMisc[] PlayerAnyMiscs { get; private set; }

	public MissionCategoryInfo[] OpendMissionCategories { get; private set; }

	public void Set(CurrencyResultInfo3[] currencyResults, ItemResultInfo[] itemResults, EquipItemInfo[] equipItemInfos, HeroAddResultInfo[] heroAddResults, AttendanceInfo attendanceInfo, AttendanceData[] attendanceDatas, PlayerItemTimeDurationInfo itemTimeDurationInfo, PurchaseMarketingResultInfo purchaseMarketingResult, PlayerProductPurchaseInfo playerProductPurchaseInfo, NewbieMissionInfo[] newbieMissionInfos, PlayerAnyMisc[] playerAnyMiscs, MissionCategoryInfo[] opendMissionCategories)
	{
		CurrencyResults = currencyResults;
		ItemResults = itemResults;
		EquipItemInfos = equipItemInfos;
		HeroAddResults = heroAddResults;
		AttendanceInfo = attendanceInfo;
		AttendanceDatas = attendanceDatas;
		ItemTimeDurationInfo = itemTimeDurationInfo;
		PurchaseMarketingResult = purchaseMarketingResult;
		PlayerProductPurchaseInfo = playerProductPurchaseInfo;
		NewbieMissionInfos = newbieMissionInfos;
		PlayerAnyMiscs = playerAnyMiscs;
		OpendMissionCategories = opendMissionCategories;
	}

	public void Apply()
	{
		SingletonAuto<AttendanceDataContainer>.instance.SetAttendanceData(AttendanceDatas);
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(CurrencyResults);
		GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(ItemResults);
		GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(EquipItemInfos);
		GlobalAutoBehaviour<AccountManager>.instance.ApplyAddHero(HeroAddResults);
		GlobalAutoBehaviour<AccountManager>.instance.AttendanceManager.Apply(AttendanceInfo);
		GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.Apply(ItemTimeDurationInfo);
		GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager.Apply(PurchaseMarketingResult);
		GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Apply(PlayerProductPurchaseInfo);
		GlobalAutoBehaviour<AccountManager>.instance.NewbieMissionManager.Apply(NewbieMissionInfos);
		GlobalAutoBehaviour<AccountManager>.instance.UserAnyMiscManager.Set(PlayerAnyMiscs);
		if (OpendMissionCategories != null)
		{
			LoginData.MissionCategoryInfo = OpendMissionCategories;
		}
	}

	public void DoActionAfterApply()
	{
		List<ItemData<PotionItemData>> extendItems = GetExtendItems(ItemResults);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(extendItems))
		{
			ExcuteExtendItems(extendItems);
		}
		else
		{
			OpenProductResultPopup();
		}
		OpenAttendanceResultPopup(AttendanceInfo);
		RefreshProductPopup(PlayerProductPurchaseInfo);
	}

	public static List<ItemData<PotionItemData>> GetExtendItems(ItemResultInfo[] itemResults)
	{
		List<ItemData<PotionItemData>> list = new List<ItemData<PotionItemData>>();
		if (itemResults != null)
		{
			foreach (ItemResultInfo itemResultInfo in itemResults)
			{
				ItemData<PotionItemData> data = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(itemResultInfo.ItemIndex);
				if (data != null && (data.SubData.ActionType == ItemActionType.ExtendChest || data.SubData.ActionType == ItemActionType.ExtendEquip))
				{
					list.Add(data);
					break;
				}
			}
		}
		return list;
	}

	private static void ExcuteExtendItems(List<ItemData<PotionItemData>> extendItemDatas)
	{
		foreach (ItemData<PotionItemData> extendItemData in extendItemDatas)
		{
			if (extendItemData.SubData.ActionType == ItemActionType.ExtendEquip)
			{
				InventoryManagement.SendExtendEquip(extendItemData);
			}
			else if (extendItemData.SubData.ActionType == ItemActionType.ExtendChest)
			{
				InventoryManagement.SendExtendChest(extendItemData);
			}
		}
	}

	private void OpenProductResultPopup()
	{
		BaseRewardPopup.PopupOpenData popupOpen = new BaseRewardPopup.PopupOpenData().SetItemResultInfos(ItemResults).SetEquipResultInfos(EquipItemInfos).SetCurrencyResultInfos3(CurrencyResults);
		BaseRewardPopup baseRewardPopup = BaseRewardPopup.Create(isGlobal: false);
		Action<BaseRewardPopup> action = delegate(BaseRewardPopup window)
		{
			window.OpenWithData(popupOpen, isGlobal: false, delegate
			{
				if (ItemTimeDurationInfo != null)
				{
					PayShopManagement.PopupUseBoosterMsg(ItemTimeDurationInfo);
				}
			});
			window.SetTitle(LocalizationManager.GetString(PayShop.BUY_TITLE));
		};
		if (SceneAutoBehaviour<BattleManagement>.isValidInstance)
		{
			action(baseRewardPopup);
		}
		else
		{
			SceneAutoBehaviour<LobbyPopupQueue>.instance.AddPopup(baseRewardPopup, action);
		}
	}

	private static void OpenAttendanceResultPopup(AttendanceInfo attendanceInfo)
	{
		if (attendanceInfo != null)
		{
			AttendanceKeywordGroup attendanceGroup = AttendanceKeywordGroup.Helper.CreateByAnyAttendanceIndex(attendanceInfo.AttendanceIndex);
			SceneAutoBehaviour<LobbyPopupQueue>.instance.ReopenAttendancePopup(attendanceGroup);
		}
	}

	private static void RefreshProductPopup(PlayerProductPurchaseInfo playerProductPurchaseInfo)
	{
		if (playerProductPurchaseInfo != null)
		{
			ClearMissionProductData dataByPayShopProductIndex = GameRoot.Instance.ClearMissionProductDataContainer.GetDataByPayShopProductIndex(playerProductPurchaseInfo.ProductIndex);
			if (dataByPayShopProductIndex != null)
			{
				SceneAutoBehaviour<ClearMissionPopupManagement>.instance.RefreshClearMissionPopup(dataByPayShopProductIndex);
			}
		}
	}
}
