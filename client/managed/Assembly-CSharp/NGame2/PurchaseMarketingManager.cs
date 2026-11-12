using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NCutScene;
using NGame2.NTutorial;
using NGame2.NUI.NWindow;
using NGame2.NUtil;
using NShared;
using NShared.SetPurchaseMarketing;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2;

public class PurchaseMarketingManager : GlobalAutoBehaviour<PurchaseMarketingManager>
{
	private static Dictionary<MarketingType, Func<PurchaseMarketingPopupData, bool>> OpenPopupFunction;

	static PurchaseMarketingManager()
	{
		OpenPopupFunction = new Dictionary<MarketingType, Func<PurchaseMarketingPopupData, bool>>();
		OpenPopupFunction.Add(MarketingType.BuyHeroAndWeapon, OpenRecommendHeroPopup);
		OpenPopupFunction.Add(MarketingType.RecommendDealer_2019, OpenRecommendHeroPopup);
		OpenPopupFunction.Add(MarketingType.RecommendTanker_2019, OpenRecommendHeroPopup);
		OpenPopupFunction.Add(MarketingType.RecommendSupporter_2019, OpenRecommendHeroPopup);
		OpenPopupFunction.Add(MarketingType.RecommendArtifact_2019, OpenRecommendHeroPopup);
		OpenPopupFunction.Add(MarketingType.RecommendDealer_2020, OpenRecommendHeroPopup);
		OpenPopupFunction.Add(MarketingType.RecommendSupporter_2020, OpenRecommendHeroPopup);
		OpenPopupFunction.Add(MarketingType.RecommendTanker_2020, OpenRecommendHeroPopup);
		OpenPopupFunction.Add(MarketingType.RecommendUniqueGear_2020, OpenRecommendHeroPopup);
		OpenPopupFunction.Add(MarketingType.NovicePackage, OpenNovicePackage);
		OpenPopupFunction.Add(MarketingType.NovicePackage1_2019, OpenNovicePackage);
		OpenPopupFunction.Add(MarketingType.NovicePackage2_2019, OpenNovicePackage);
		OpenPopupFunction.Add(MarketingType.FirstClearChapter_2020, OpenFirstClearChapterPopup);
		OpenPopupFunction.Add(MarketingType.Halloween_2021, OpenFirstClearChapterPopup);
		OpenPopupFunction.Add(MarketingType.EventWorldTree2021, OpenServerDependence);
	}

	public static bool IsPurchaseMarketingByProduct(int productIndex)
	{
		PurchaseMarketingData purchaseMarketingData = GameRoot.Instance.PurchaseMarketingDataContainer.GetAllData().FirstOrDefault((PurchaseMarketingData v) => v.ProductIndex == productIndex);
		if (purchaseMarketingData == null)
		{
			return false;
		}
		return IsPurchaseMarketing(purchaseMarketingData.Index);
	}

	public static bool IsPurchaseMarketing(int purchaseIndex)
	{
		PurchaseMarketingInfo purchaseMarketingInfo = GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager.Get(purchaseIndex);
		if (purchaseMarketingInfo == null)
		{
			return false;
		}
		return IsPurchaseMarketing(purchaseMarketingInfo);
	}

	public static bool IsPurchaseMarketing(PurchaseMarketingInfo info)
	{
		if (info == null)
		{
			return false;
		}
		if (info.Active == 0)
		{
			return false;
		}
		if (!IsValidTime(info))
		{
			return false;
		}
		return true;
	}

	public static bool IsValidTime(PurchaseMarketingInfo info)
	{
		if (info == null)
		{
			return false;
		}
		PurchaseMarketingData data = GameRoot.Instance.PurchaseMarketingDataContainer.GetData(info.PurchaseIndex);
		if (data != null && data.DurationTime == 0 && String_Extension_Method.IsNullOrEmpty(info.PurchasedTime))
		{
			return true;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(info.BeginTime, out var o_dateTime))
		{
			return false;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if (o_dateTime >= currentServerUTCTime)
		{
			return false;
		}
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(info.EndTime, out var o_dateTime2) && o_dateTime2 < currentServerUTCTime)
		{
			return false;
		}
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(info.PurchasedTime, out var o_dateTime3) && o_dateTime3 >= o_dateTime)
		{
			return false;
		}
		return true;
	}

	public static bool AlredayOpenPopupByType(MarketingType type)
	{
		foreach (PurchaseMarketingInfo item in GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager)
		{
			if (IsPurchaseMarketing(item))
			{
				PurchaseMarketingPopupData purchaseMarketingPopupData = GameRoot.Instance.PurchaseMarketingPopupDataContainer.GetPurchaseMarketingPopupData(item.PurchaseIndex);
				if (purchaseMarketingPopupData != null && item.IsOpenPopup != 0 && purchaseMarketingPopupData.Type == type)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool AlreadyOpenPopupByProduct(PurchaseMarketingPopupData popupData)
	{
		foreach (PurchaseMarketingInfo item in GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager)
		{
			if (IsPurchaseMarketing(item) && item.IsOpenPopup != 0)
			{
				PurchaseMarketingPopupData purchaseMarketingPopupData = GameRoot.Instance.PurchaseMarketingPopupDataContainer.GetPurchaseMarketingPopupData(item.PurchaseIndex);
				if (purchaseMarketingPopupData != null && purchaseMarketingPopupData == popupData)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool OpenPurchaseMarketingPopup(OpenConditionType curScene)
	{
		if (IsTutorial())
		{
			return false;
		}
		PurchaseMarketingPopupData[] array = (from v in GameRoot.Instance.PurchaseMarketingPopupDataContainer.GetAllData()
			where v.OpenCondition == curScene
			select v).ToArray();
		Array.Sort(array, ComparePopupData);
		PurchaseMarketingPopupData[] array2 = array;
		foreach (PurchaseMarketingPopupData purchaseMarketingPopupData in array2)
		{
			if (IsOpen(purchaseMarketingPopupData) && OpenPopupFunction.ContainsKey(purchaseMarketingPopupData.Type) && OpenPopupFunction[purchaseMarketingPopupData.Type](purchaseMarketingPopupData))
			{
				return true;
			}
		}
		return false;
	}

	private static int ComparePopupData(PurchaseMarketingPopupData a, PurchaseMarketingPopupData b)
	{
		int num = a.PopupPriority.CompareTo(b.PopupPriority);
		if (num != 0)
		{
			return num;
		}
		return a.Index.CompareTo(b.Index);
	}

	private static bool CheckOpenDate(string openDateTime)
	{
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(GlobalAutoBehaviour<AccountManager>.instance.CreatedTime, out var o_dateTime) && Vespa_String_Extension_Method2.TryParseNetworkTime2(openDateTime, out var o_dateTime2))
		{
			return o_dateTime >= o_dateTime2;
		}
		return false;
	}

	private static bool OpenRecommendHeroPopup(PurchaseMarketingPopupData marketingPopupData)
	{
		if (marketingPopupData != null)
		{
			int type = (int)marketingPopupData.Type;
			if (type == 3 || type == 4 || type == 5 || type == 6 || type == 9 || type == 10 || type == 11 || type == 12)
			{
				return false;
			}
		}
		if (marketingPopupData == null)
		{
			return false;
		}
		if (!CheckOpenDate(marketingPopupData.OpenDate))
		{
			return false;
		}
		if (OnCoolTime(marketingPopupData.PurchaseIndices))
		{
			return false;
		}
		if (AlredayOpenPopupByType(marketingPopupData.Type))
		{
			return false;
		}
		if (!CheckOpenCondition(marketingPopupData))
		{
			return false;
		}
		if (CheckSkipCondition(marketingPopupData))
		{
			return false;
		}
		OpenPurchaseMarketingPopupByType(marketingPopupData.Index);
		return true;
	}

	private static bool OpenNovicePackage(PurchaseMarketingPopupData marketingPopupData)
	{
		if (marketingPopupData == null)
		{
			return false;
		}
		if (OnCoolTime(marketingPopupData.PurchaseIndices))
		{
			return false;
		}
		if (AlredayOpenPopupByType(marketingPopupData.Type))
		{
			PurchaseMarketingInfo purchaseMarketingInfo = null;
			int[] purchaseIndices = marketingPopupData.PurchaseIndices;
			foreach (int key in purchaseIndices)
			{
				PurchaseMarketingInfo purchaseMarketingInfo2 = GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager.Get(key);
				if (IsPurchaseMarketing(purchaseMarketingInfo2))
				{
					purchaseMarketingInfo = purchaseMarketingInfo2;
					break;
				}
			}
			if (purchaseMarketingInfo == null)
			{
				return false;
			}
			if (purchaseMarketingInfo.IsOpenPopup == 1)
			{
				return false;
			}
			DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
			Vespa_String_Extension_Method2.TryParseNetworkTime2(purchaseMarketingInfo.BeginTime, out var o_dateTime);
			if ((currentServerUTCTime - o_dateTime).TotalMinutes >= (double)marketingPopupData.RemindMinutes)
			{
				OpenPurchaseMarketingPopupByType(marketingPopupData.Index);
			}
			return true;
		}
		if (CheckOpenCondition(marketingPopupData))
		{
			ActivePurchaseMarketing(marketingPopupData.Index);
		}
		return false;
	}

	private static bool OpenFirstClearChapterPopup(PurchaseMarketingPopupData marketingPopupData)
	{
		if (marketingPopupData == null)
		{
			return false;
		}
		if (!CheckOpenDate(marketingPopupData.OpenDate))
		{
			return false;
		}
		if (OnCoolTime(marketingPopupData.PurchaseIndices))
		{
			return false;
		}
		if (AlreadyOpenPopupByProduct(marketingPopupData))
		{
			return false;
		}
		if (!CheckOpenCondition(marketingPopupData))
		{
			return false;
		}
		if (CheckSkipCondition(marketingPopupData))
		{
			return false;
		}
		OpenPurchaseMarketingPopupByProduct(marketingPopupData.Index);
		return true;
	}

	private static bool OpenServerDependence(PurchaseMarketingPopupData marketingPopupData)
	{
		if (marketingPopupData == null)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(marketingPopupData.ServerCondition))
		{
			return false;
		}
		PurchaseMarketingInfo purchaseMarketingInfo = GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager.Get(marketingPopupData.Index);
		if (purchaseMarketingInfo == null || purchaseMarketingInfo.IsOpenPopup == 1)
		{
			return false;
		}
		OpenPurchaseMarketingPopupByProduct(marketingPopupData.Index);
		return true;
	}

	public static PurchaseMarketingInfo GetValidPurchaseMarketingInfoByMarketingType(MarketingType type)
	{
		PurchaseMarketingPopupData[] purchaseMarketingPopupDatas = GameRoot.Instance.PurchaseMarketingPopupDataContainer.GetPurchaseMarketingPopupDatas(type);
		for (int i = 0; i < purchaseMarketingPopupDatas.Length; i++)
		{
			int[] purchaseIndices = purchaseMarketingPopupDatas[i].PurchaseIndices;
			foreach (int key in purchaseIndices)
			{
				PurchaseMarketingInfo purchaseMarketingInfo = GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager.Get(key);
				if (IsPurchaseMarketing(purchaseMarketingInfo))
				{
					return purchaseMarketingInfo;
				}
			}
		}
		return null;
	}

	public static PurchaseMarketingInfo GetValidPurchaseMarketingInfoByPopupIndex(int popupIndex)
	{
		int[] purchaseIndices = GameRoot.Instance.PurchaseMarketingPopupDataContainer.GetData(popupIndex).PurchaseIndices;
		foreach (int key in purchaseIndices)
		{
			PurchaseMarketingInfo purchaseMarketingInfo = GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager.Get(key);
			if (IsPurchaseMarketing(purchaseMarketingInfo))
			{
				return purchaseMarketingInfo;
			}
		}
		return null;
	}

	public static void ReservePurchaseMarketingPopup(int popupIndex)
	{
		RequestPurchaseMarketing(popupIndex, isActive: false, isOpenPopup: false, null);
	}

	public static void ActivePurchaseMarketing(int popupIndex)
	{
		RequestPurchaseMarketing(popupIndex, isActive: true, isOpenPopup: false, null);
	}

	public static void OpenPurchaseMarketingPopupByType(int popupIndex)
	{
		PurchaseMarketingPopupData data = GameRoot.Instance.PurchaseMarketingPopupDataContainer.GetData(popupIndex);
		PurchaseMarketingPopup.Open(data);
		PurchaseMarketingInfo validPurchaseMarketingInfoByMarketingType = GetValidPurchaseMarketingInfoByMarketingType(data.Type);
		if (validPurchaseMarketingInfoByMarketingType == null || validPurchaseMarketingInfoByMarketingType.IsOpenPopup == 0)
		{
			RequestPurchaseMarketing(popupIndex, isActive: true, isOpenPopup: true, null);
		}
	}

	public static void OpenPurchaseMarketingPopupByProduct(int popupIndex)
	{
		PurchaseMarketingPopup.Open(GameRoot.Instance.PurchaseMarketingPopupDataContainer.GetData(popupIndex));
		PurchaseMarketingInfo validPurchaseMarketingInfoByPopupIndex = GetValidPurchaseMarketingInfoByPopupIndex(popupIndex);
		if (validPurchaseMarketingInfoByPopupIndex == null || validPurchaseMarketingInfoByPopupIndex.IsOpenPopup == 0)
		{
			RequestPurchaseMarketing(popupIndex, isActive: true, isOpenPopup: true, null);
		}
	}

	public static void OpenForcePurchaseMarketingPopupByProduct(int popupIndex)
	{
		PurchaseMarketingPopup.Open(GameRoot.Instance.PurchaseMarketingPopupDataContainer.GetData(popupIndex));
	}

	public static void RequestPurchaseMarketing(int popupIndex, bool isActive, bool isOpenPopup, Action okCallback)
	{
		PurchaseMarketingPopupData data = GameRoot.Instance.PurchaseMarketingPopupDataContainer.GetData(popupIndex);
		NShared.SetPurchaseMarketing.Request request = new NShared.SetPurchaseMarketing.Request
		{
			PurchaseMarketingIndices = data.PurchaseIndices,
			IsActive = isActive,
			IsOpenPopup = isOpenPopup
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SetPurchaseMarketing.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success && response.Result == ResultType.Success)
			{
				GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager.Apply(response.PurchaseMarketingResult);
				if (okCallback != null)
				{
					okCallback();
				}
			}
		});
	}

	public static bool CheckSkipCondition(PurchaseMarketingPopupData popupData)
	{
		if (popupData == null)
		{
			return false;
		}
		if (ConditonCheck(popupData.SkipCondition1, defaultValue: false) || ConditonCheck(popupData.SkipCondition2, defaultValue: false) || ConditonCheck(popupData.SkipCondition3, defaultValue: false))
		{
			return true;
		}
		return false;
	}

	private static bool CheckOpenCondition(PurchaseMarketingPopupData popupData)
	{
		if (popupData == null)
		{
			return false;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(popupData.ServerCondition))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager.Get(popupData.Index) != null;
		}
		if (ConditonCheck(popupData.Condition1, defaultValue: true) && ConditonCheck(popupData.Condition2, defaultValue: true) && ConditonCheck(popupData.Condition3, defaultValue: true))
		{
			return true;
		}
		return false;
	}

	private static bool IsAlreadyReserve(int purchaseIndex)
	{
		PurchaseMarketingInfo purchaseMarketingInfo = GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager.Get(purchaseIndex);
		if (purchaseMarketingInfo != null && purchaseMarketingInfo.Active == 0)
		{
			return true;
		}
		return false;
	}

	private bool IsTutorial()
	{
		if (SingletonUpdater<TutorialManager>.isValidInstance)
		{
			if (SingletonUpdater<TutorialManager>.instance.IsProgressing)
			{
				return true;
			}
			if (SingletonUpdater<TutorialManager>.instance.HasNextTurorial)
			{
				return true;
			}
		}
		if (SceneAutoBehaviour<CutSceneManager>.isValidInstance && SceneAutoBehaviour<CutSceneManager>.instance.IsPlaying)
		{
			return true;
		}
		return false;
	}

	private static bool ConditonCheck(string[] condition, bool defaultValue)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(condition))
		{
			return defaultValue;
		}
		return SingletonAuto<ConditionChecker>.instance.Check(condition);
	}

	public static bool OnCoolTime(int[] purchaseIndices)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(purchaseIndices))
		{
			return false;
		}
		foreach (int num in purchaseIndices)
		{
			PurchaseMarketingData data = GameRoot.Instance.PurchaseMarketingDataContainer.GetData(num);
			if (data != null && !OnCoolTime(num, data.RepeatCooltime, data.DurationTime))
			{
				return false;
			}
		}
		return true;
	}

	public static bool OnCoolTime(int purchaseIndex, int repeatCooltimeDay, int durationTime)
	{
		PurchaseMarketingInfo purchaseMarketingInfo = GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager.Get(purchaseIndex);
		if (purchaseMarketingInfo == null)
		{
			return false;
		}
		if (durationTime > 0)
		{
			DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
			if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(purchaseMarketingInfo.BeginTime, out var o_dateTime) || !Vespa_String_Extension_Method2.TryParseNetworkTime2(purchaseMarketingInfo.EndTime, out var o_dateTime2))
			{
				return false;
			}
			DateTime dateTime;
			if (purchaseMarketingInfo.PurchasedTime != null)
			{
				Vespa_String_Extension_Method2.TryParseNetworkTime2(purchaseMarketingInfo.PurchasedTime, out var o_dateTime3);
				dateTime = ((!(o_dateTime <= o_dateTime3) || !(o_dateTime3 <= o_dateTime2)) ? o_dateTime2 : o_dateTime3);
			}
			else
			{
				dateTime = o_dateTime2;
			}
			if (dateTime > currentServerUTCTime)
			{
				return false;
			}
			if (repeatCooltimeDay > 0 && (currentServerUTCTime - dateTime).Days >= repeatCooltimeDay)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsOpen(PurchaseMarketingPopupData popupData)
	{
		if (popupData != null)
		{
			int type = (int)popupData.Type;
			if (type == 3 || type == 4 || type == 5 || type == 6 || type == 9 || type == 10 || type == 11 || type == 12)
			{
				return false;
			}
		}
		if (popupData == null || __Extension_Method_ICollection__.IsNullOrEmpty(popupData.PurchaseIndices))
		{
			return false;
		}
		int[] purchaseIndices = popupData.PurchaseIndices;
		foreach (int key in purchaseIndices)
		{
			PurchaseMarketingData data = GameRoot.Instance.PurchaseMarketingDataContainer.GetData(key);
			if (data != null && data.IsOpen)
			{
				return true;
			}
		}
		return false;
	}

	public void OpenPurchaseMarketingGroupPopup(MarketingType type)
	{
		PurchaseMarketingGroupPopup.Open(type);
	}
}
