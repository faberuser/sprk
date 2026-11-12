using System;
using NGame2.NAccount;
using NGame2.NContents;
using NGame2.NDev;
using NGame2.NLogin;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NShared.EnterLobby;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NScene.NLobbyScenePrerequisites;

public class RequestEnterLobbyWrapper
{
	public static bool ForceTimeout;

	public const float Timeout = 20f;

	private float _requestTime;

	private NShared.EnterLobby.Response _response;

	private static int _remainDaySecond;

	private static int _remainWeekSecond;

	private static int _remainMonthSecond;

	public static int DayOfYear { get; private set; }

	public static int WeekOfYear { get; private set; }

	public static int MonthOfYear { get; private set; }

	public static int MonthFromMillennium { get; private set; }

	public static int WoyRandomSeed { get; private set; }

	public static int DoyRandomSeed { get; private set; }

	public static int SecRandomSeed { get; private set; }

	public bool IsDoneOrTimeout
	{
		get
		{
			if (!IsDone)
			{
				return IsTimeout;
			}
			return true;
		}
	}

	public bool IsDone { get; private set; }

	public bool IsTimeout
	{
		get
		{
			if (IsDone)
			{
				return false;
			}
			return Time.realtimeSinceStartup - _requestTime > 20f;
		}
	}

	public void Request()
	{
		try
		{
			RequestInternal();
		}
		catch (Exception i_object)
		{
			Debug.LogError("RequestEnterLobby.Request exception: " + Vespa_Common_Class_Extension_Method.TOSTRING(i_object));
			IsDone = true;
		}
	}

	private void RequestInternal()
	{
		_requestTime = Time.realtimeSinceStartup;
		if (!ForceTimeout)
		{
			Debug.Log("Request enter lobby.");
			NShared.EnterLobby.Request request = new NShared.EnterLobby.Request();
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.Ping, delegate(NShared.EnterLobby.Response response)
			{
				Debug.Log("Enter lobby response received.");
				IsDone = true;
				_response = response;
				DayOfYear = response?.DayOfYear ?? 0;
				WeekOfYear = response?.WeekOfYear ?? 0;
				MonthOfYear = response?.MonthOfYear ?? 0;
				MonthFromMillennium = response?.MonthFromMillennium ?? 0;
				DoyRandomSeed = response?.DoyRandomSeed ?? 0;
				WoyRandomSeed = response?.WoyRandomSeed ?? 0;
				_remainDaySecond = response?.RemainDaySecond ?? 0;
				_remainWeekSecond = response?.RemainWeekSecond ?? 0;
				_remainMonthSecond = response?.RemainMonthSecond ?? 0;
				SecRandomSeed = response?.RemainDaySecond ?? 0;
				DevParam.SetDevParam(response?.DevParam);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.DailyGiftBoxInfo = response?.PushRewardInfo;
				LoginData.MissionCategoryInfo = response?.OpendMissionCategories;
				LoginData.WebEvent = response?.WebEvent;
			});
		}
	}

	public void HandleResponse()
	{
		try
		{
			HandleResponseInternal();
		}
		catch (Exception i_object)
		{
			Debug.LogError("RequestEnterLobby.HandleResponse exception: " + Vespa_Common_Class_Extension_Method.TOSTRING(i_object));
			IsDone = true;
		}
	}

	private void HandleResponseInternal()
	{
		if (IsTimeout)
		{
			Debug.LogError("RequestEnterLobby.HandleResponse Timeout");
			return;
		}
		bool flag = false;
		if (!String_Extension_Method.IsNullOrEmpty(_response.ForwardHost))
		{
			SingletonUpdater<WebServiceManager>.instance.SetForwardedHost(_response.ForwardHost);
			flag = true;
		}
		if (_response.BaseResult != BaseResultType.Success)
		{
			GlobalMessagePopup.OpenGlobalError(_response.BaseResult);
			return;
		}
		SingletonAuto<AttendanceDataContainer>.instance.SetAttendanceData(_response.AttendanceDatas);
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(_response.EventDungeonPointResult);
		GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.Apply(_response.DungeonInfos);
		GlobalAutoBehaviour<AccountManager>.instance.HideoutDungeonManager.Apply(_response.HideoutDungeonInfos);
		GlobalAutoBehaviour<AccountManager>.instance.KeyTypeDungeonManager.Apply(_response.ConquestDungeonInfos);
		GlobalAutoBehaviour<AccountManager>.instance.TowerManager.Apply(_response.TowerInfos);
		GlobalAutoBehaviour<AccountManager>.instance.CraftSlotManager.Apply(_response.CraftSlotResults);
		GlobalAutoBehaviour<AccountManager>.instance.SetServerTime(_response.ServerLocalTime, _response.ServerUTCOffsetHour);
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.LoginDailyCount = _response.LoginDailyCount;
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetGuildArenaSessionInfo(_response.GuildArenaSessionInfo);
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetGuildSuppressSessionInfo(_response.GuildSuppressSessionInfo);
		if (_response.PayShopProductInfos != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.SetNewPayShopProductInfo(_response.PayShopProductInfos);
			SprkNetworkPerformance.CatalogApplied(_response);
		}
		if (_response.PlayerProductPurchaseInfos != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.SetPlayerProductPurchaseInfo(_response.PlayerProductPurchaseInfos);
		}
		if (_response.TopClearDungeonInfos != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.SetTopClearDungeonInfos(_response.TopClearDungeonInfos);
		}
		SingletonAuto<ContentsStatusManager>.instance.Add(_response.ContentsStatuses);
		SingletonAuto<ContentsValueManager>.instance.Add(_response.ContentsValues);
		if (!flag)
		{
			if (!ClientValueManager.IsSkipLobbyPopupQueue)
			{
				SceneAutoBehaviour<LobbyManagement>.instance.ReserveLobbyNoticePopup(_response.LobbyNotices);
				SceneAutoBehaviour<LobbyPopupQueue>.instance.ReserveAttendancePopup_on_EnterLobby();
				SceneAutoBehaviour<LobbyPopupQueue>.instance.ReserveAccumulateLoginPopup();
				SceneAutoBehaviour<LobbyPopupQueue>.instance.ReserveDungeonOpenPopUp();
				SceneAutoBehaviour<LobbyPopupQueue>.instance.ReserveUnlockCostumePopup();
				SceneAutoBehaviour<LobbyPopupQueue>.instance.ReservePurchaseMarketingPopup();
				SceneAutoBehaviour<LobbyPopupQueue>.instance.ReserveOpenClassBuffPopup();
				SceneAutoBehaviour<LobbyPopupQueue>.instance.ReserveOpenSpecialMissionPopup();
				SceneAutoBehaviour<LobbyPopupQueue>.instance.ReserveContentsNoticePopup();
			}
			SceneAutoBehaviour<LobbyPopupQueue>.instance.ReservedMainQuestClearPopup();
			SceneAutoBehaviour<LobbyPopupQueue>.instance.ReservedContentsOpenConditionPopup();
			SceneAutoBehaviour<LobbyPopupQueue>.instance.ReserveSubQuestClearPopup();
			SceneAutoBehaviour<LobbyPopupQueue>.instance.ReserveMainSubQuestOpenPopup();
			SceneAutoBehaviour<LobbyManagement>.instance.OpenGuildAttendancePopup();
		}
		LobbyRightMenu instance = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyRightMenu>();
		if (instance != null)
		{
			instance.OnEnterLobby();
		}
		LeftMenu instance2 = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LeftMenu>();
		if (instance2 != null)
		{
			instance2.OnEnterLobby();
		}
		SceneAutoBehaviour<LobbyManagement>.instance.EnableEffectFinishDispatchBattle();
		GlobalAutoBehaviour<CampaignNodeOpenEffectManager>.instance.StartEffect();
		ValueConfuserFactory.DisableConfuser(DevParam.GetDevParamBool("DisableConfuser"));
		SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<EtcMenuWindow>().OnNewEventCalendar(_response.EventCalendarVersion);
	}

	public static int GetRemainDaySecond()
	{
		int num = (int)GlobalAutoBehaviour<AccountManager>.instance.GetElapsedTimeSinceLastEnterLobby();
		int num2 = _remainDaySecond - num;
		if (num2 < 0)
		{
			int num3 = 86400;
			num2 = (num2 % num3 + num3) % num3;
		}
		return num2;
	}

	public static int GetRemainWeekSecond()
	{
		int num = (int)GlobalAutoBehaviour<AccountManager>.instance.GetElapsedTimeSinceLastEnterLobby();
		int num2 = _remainWeekSecond - num;
		if (num2 < 0)
		{
			int num3 = 604800;
			num2 = (num2 % num3 + num3) % num3;
		}
		return num2;
	}

	public static int GetThisMonthSecond()
	{
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		return DateTime.DaysInMonth(currentServerLocalTime.Year, currentServerLocalTime.Month) * 86400;
	}

	public static int GetRemainMonthSecond(int[] days)
	{
		if (days == null)
		{
			days = new int[1] { 1 };
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		int day = currentServerLocalTime.Day;
		int num = days[0];
		bool flag = true;
		int[] array = days;
		foreach (int num2 in array)
		{
			if (num2 > day)
			{
				flag = false;
				num = num2;
				break;
			}
		}
		int num3 = (int)GlobalAutoBehaviour<AccountManager>.instance.GetElapsedTimeSinceLastEnterLobby();
		int num4 = _remainMonthSecond - num3;
		TimeSpan timeSpan;
		if (num4 >= 0)
		{
			timeSpan = TimeSpan.FromSeconds(num4);
		}
		else
		{
			int num5 = num4 * -1;
			timeSpan = TimeSpan.FromSeconds(GetThisMonthSecond() - num5);
		}
		if (flag)
		{
			timeSpan += TimeSpan.FromDays(num - 1);
		}
		else
		{
			DateTime time = new DateTime(currentServerLocalTime.Year, currentServerLocalTime.Month, num) - TimeSpan.FromSeconds(1.0);
			timeSpan = GlobalAutoBehaviour<AccountManager>.instance.ServerLocalToUTC(time) - currentServerUTCTime;
		}
		return (int)timeSpan.TotalSeconds;
	}
}
