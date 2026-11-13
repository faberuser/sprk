using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NContentsInfo;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NShared.RegisterDeviceToken;
using NShared.SetPushNotification;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NNotification;

public class NotificationManager : SingletonAuto<NotificationManager>, IUserObserver<UserManager.UserStatInfo>, IUserObserver
{
	[Flags]
	public enum AlertFlag
	{
		None = 0,
		Sound = 1,
		Vibrate = 2
	}

	public const int LocalNotificationId_Stamina = 1001;

	public const int LocalNotificationId_ChallengeTower = 1002;

	public const int LocalNotificationId_ArenaTicket = 1003;

	public const int LocalNotificationId_ItemCraft = 2000;

	public const int LocalNotificationId_CreationNickGift = 3000;

	public const int LocalNotificationId_RetentionCare = 4000;

	private Dictionary<int, DateTime> lastNotifications = new Dictionary<int, DateTime>();

	private bool _isChangedRemote;

	private bool _isChangedLocal;

	private bool _isEnableAllNotification;

	private bool _isEnableSystemNotification;

	private bool _isEnableStaminaNotification;

	private bool _isEnableChallengeTowerNotification;

	private bool _isEnableArenaTicketNotification;

	private bool _isEnableItemCompleteNotification;

	private bool _isEnableGuildRaidOpenNotification;

	private bool _isEnableEventNotification;

	private bool _isEnableNightNotification;

	private long LocalNotificationCheckTime = 120000L;

	private float CheckTerm = 20f;

	public string DeviceToken { get; private set; }

	public bool RemoteNotificationRegistered { get; private set; }

	public NotificationManager()
	{
		SingletonAuto<RemoteRegister>.CreateInstance();
		SingletonAuto<LocalRegister>.CreateInstance();
		VespaCoroutine.StartGlobal(coDeviceToken(), "(coDeviceToken)");
	}

	public void StartManager()
	{
		VespaCoroutine.StartGlobal(coUpdateRemoteNotification(), "(coUpdateRemoteNotification)");
		VespaCoroutine.StartGlobal(coUpdateLocalNotification(), "(coUpdateLocalNotification)");
	}

	public void AddObserver()
	{
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.ObserverManager.AddObserver(this);
	}

	public void SetEnabled(bool isEnableAllNotification = false, bool isEnableSystemNotification = false, bool isEnableStaminaNotification = false, bool isEnableChallengeTowerNotification = false, bool isEnableArenaTicketNotification = false, bool isEnableItemCompleteNotification = false, bool isEnableGuildRaidOpenNotification = false, bool isEnableEventNotification = false, bool isEnableNightNotification = false, bool isChangedRemote = false)
	{
		if (isChangedRemote)
		{
			_isChangedRemote = true;
		}
		_isEnableAllNotification = isEnableAllNotification;
		_isEnableSystemNotification = isEnableSystemNotification;
		_isEnableStaminaNotification = isEnableStaminaNotification;
		_isEnableChallengeTowerNotification = isEnableChallengeTowerNotification;
		_isEnableArenaTicketNotification = isEnableArenaTicketNotification;
		_isEnableItemCompleteNotification = isEnableItemCompleteNotification;
		_isEnableGuildRaidOpenNotification = isEnableGuildRaidOpenNotification;
		_isEnableEventNotification = isEnableEventNotification;
		_isEnableNightNotification = isEnableNightNotification;
	}

	public void SetNotificationDefaultValuesOnce()
	{
		if (!GameOptionData._noti_isDefaultValueSet.Value)
		{
			SingletonAuto<TableDataManager>.instance.LoadConstantTableInResource();
			GameOptionData.Noti_SystemNoti = GameRoot.Instance.ConstantDataContainer.GetValueBool("GameOptionNotification_System");
			GameOptionData.Noti_StaminaFullNoti = GameRoot.Instance.ConstantDataContainer.GetValueBool("GameOptionNotification_StaminaFull");
			GameOptionData.Noti_ChallengeTowerNoti = GameRoot.Instance.ConstantDataContainer.GetValueBool("GameOptionNotification_ChallengeTower");
			GameOptionData.Noti_ArenaTicketNoti = GameRoot.Instance.ConstantDataContainer.GetValueBool("GameOptionNotification_ArenaTicket");
			GameOptionData.Noti_ItemCompleteNoti = GameRoot.Instance.ConstantDataContainer.GetValueBool("GameOptionNotification_ItemComplete");
			GameOptionData.Noti_GuildRaidOpenNoti = GameRoot.Instance.ConstantDataContainer.GetValueBool("GameOptionNotification_GuildRaidOpen");
			GameOptionData.Noti_EventNoti = GameRoot.Instance.ConstantDataContainer.GetValueBool("GameOptionNotification_Event");
			GameOptionData.Noti_NightNoti = GameRoot.Instance.ConstantDataContainer.GetValueBool("GameOptionNotification_Night");
			GameOptionData.Noti_GuildSuppressOpenNoti = GameRoot.Instance.ConstantDataContainer.GetValueBool("GameOptionNotification_GuildSuppress");
			GameOptionData.Noti_AllNoti = GameRoot.Instance.ConstantDataContainer.GetValueBool("GameOptionNotification_All");
			GameOptionData._noti_isDefaultValueSet.Value = true;
		}
	}

	private IEnumerator coUpdateRemoteNotification()
	{
		SingletonAuto<RemoteRegister>.instance.Clear();
		bool isComplete = false;
		string deviceToken = string.Empty;
		string error = string.Empty;
		SingletonAuto<RemoteRegister>.instance.Register(delegate(string iDeviceToken, string iError)
		{
			deviceToken = iDeviceToken;
			error = iError;
			isComplete = true;
		});
		while (!isComplete)
		{
			yield return null;
		}
		RemoteNotificationRegistered = true;
		if (!String_Extension_Method.IsNullOrEmpty(deviceToken))
		{
			_ = (object)null;
			_ = deviceToken;
		}
		while (true)
		{
			yield return null;
			if (_isChangedRemote && AccountManager.IsLogined())
			{
				_isChangedRemote = false;
				if (_isEnableAllNotification)
				{
					yield return coEnableRemoteNotification();
				}
				else
				{
					yield return coDisableRemoteNotification();
				}
			}
		}
	}

	private IEnumerator coUpdateLocalNotification()
	{
		ResetLocalNotification();
		while (true)
		{
			yield return new WaitForSeconds(CheckTerm);
			if (AccountManager.IsLogined())
			{
				RegisteStaminaFullNotification();
				RegisteChallengeTowerNotification();
				RegisteArenaTicketNotification();
				CheckItemCraftSlots();
				RegisteCreateNickAfterNotification();
				CheckLimitedBM();
				RegisteRetentionCareNotification();
			}
		}
	}

	private IEnumerator coDeviceToken()
	{
		bool isComplete = false;
		string deviceToken = string.Empty;
		string error = string.Empty;
		SingletonAuto<RemoteRegister>.instance.Register(delegate(string iDeviceToken, string iError)
		{
			deviceToken = iDeviceToken;
			error = iError;
			isComplete = true;
		});
		while (!isComplete)
		{
			yield return null;
		}
		if (String_Extension_Method.IsNullOrEmpty(error))
		{
			DeviceToken = deviceToken;
		}
	}

	private IEnumerator coEnableRemoteNotification()
	{
		yield return null;
		bool isComplete = false;
		string deviceToken = string.Empty;
		string error = string.Empty;
		SingletonAuto<RemoteRegister>.instance.Register(delegate(string iDeviceToken, string iError)
		{
			deviceToken = iDeviceToken;
			error = iError;
			isComplete = true;
		});
		while (!isComplete)
		{
			yield return null;
		}
		RemoteNotificationRegistered = true;
		_isChangedRemote = false;
		if (String_Extension_Method.IsNullOrEmpty(error))
		{
			DeviceToken = deviceToken;
			isComplete = false;
			RequestRegisterDeviceToken(deviceToken, null);
		}
	}

	private IEnumerator coDisableRemoteNotification()
	{
		yield return null;
		SingletonAuto<RemoteRegister>.instance.Unregister(null);
		_isChangedRemote = false;
		RequestRegisterDeviceToken(null, null);
	}

	private void RequestRegisterDeviceToken(string deviceToken, Action onComplete)
	{
		if (!AccountManager.IsLogined())
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		NShared.RegisterDeviceToken.Request request = new NShared.RegisterDeviceToken.Request
		{
			DeviceToken = deviceToken
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RegisterDeviceToken.Response response)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			if (response == null)
			{
				GlobalMessagePopup.OpenGlobalError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				GlobalMessagePopup.OpenGlobalError(response.BaseResult);
			}
			else if (response.Result != NShared.RegisterDeviceToken.ResultType.Success)
			{
				GlobalMessagePopup.OpenGlobalError(response.Result);
			}
		});
	}

	public void RequestNotificationValue(PushNotificationType type, bool value)
	{
		// Login settings also run these callbacks while initializing option rows.
		// Preferences are saved locally; the account endpoint needs a live session.
		if (!AccountManager.IsLogined())
		{
			return;
		}
		NShared.SetPushNotification.Request request = new NShared.SetPushNotification.Request
		{
			PushType = type,
			Status = value
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request<NShared.SetPushNotification.Request, NShared.SetPushNotification.Response>(request, delegate
		{
		});
	}

	public void ResetLocalNotification()
	{
		SingletonAuto<LocalRegister>.instance.Clear();
		SingletonAuto<LocalRegister>.instance.Cancel();
	}

	public void RegisteStaminaFullNotification()
	{
		if (!_isEnableAllNotification || !_isEnableSystemNotification || !_isEnableStaminaNotification)
		{
			return;
		}
		DateTime fullRechargeTime = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetFullRechargeTime(StaminaType.Chicken);
		long currentStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(StaminaType.Chicken);
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetMaxStamina(StaminaType.Chicken) > currentStamina)
		{
			int num = 100;
			if (currentStamina <= num)
			{
				TimeSpan timeSpan = fullRechargeTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
				RegisteLocalNotification(1001, LocalizationManager.GetStringDirect("PUSH_TITLE_MESSAGE"), LocalizationManager.GetStringDirect("PUSH_STAMINA_ALL_CHARGE_MESSAGE"), LocalizationManager.GetStringDirect("PUSH_TITLE_MESSAGE"), (long)timeSpan.TotalMilliseconds, everyday: false);
			}
		}
	}

	public void RegisteChallengeTowerNotification()
	{
		if (!_isEnableAllNotification || !_isEnableSystemNotification || !_isEnableChallengeTowerNotification)
		{
			return;
		}
		DateTime fullRechargeTime = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetFullRechargeTime(StaminaType.ChallengeTowerKey);
		long currentStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(StaminaType.ChallengeTowerKey);
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetMaxStamina(StaminaType.ChallengeTowerKey) > currentStamina)
		{
			int num = 4;
			if (currentStamina <= num)
			{
				TimeSpan timeSpan = fullRechargeTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
				RegisteLocalNotification(1002, LocalizationManager.GetStringDirect("PUSH_TITLE_MESSAGE"), LocalizationManager.GetStringDirect("PUSH_CHALLENGE_ALL_CHARGE_MESSAGE"), LocalizationManager.GetStringDirect("PUSH_TITLE_MESSAGE"), (long)timeSpan.TotalMilliseconds, everyday: false);
			}
		}
	}

	public void RegisteArenaTicketNotification()
	{
		if (!_isEnableAllNotification || !_isEnableSystemNotification || !_isEnableArenaTicketNotification)
		{
			return;
		}
		DateTime fullRechargeTime = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetFullRechargeTime(StaminaType.Sword);
		long currentStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(StaminaType.Sword);
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetMaxStamina(StaminaType.Sword) > currentStamina)
		{
			int num = 4;
			if (currentStamina <= num)
			{
				TimeSpan timeSpan = fullRechargeTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
				RegisteLocalNotification(1003, LocalizationManager.GetStringDirect("PUSH_TITLE_MESSAGE"), LocalizationManager.GetStringDirect("PUSH_ARENA_ALL_CHARGE_MESSAGE"), LocalizationManager.GetStringDirect("PUSH_TITLE_MESSAGE"), (long)timeSpan.TotalMilliseconds, everyday: false);
			}
		}
	}

	public void CheckItemCraftSlots()
	{
		if (!AccountManager.IsLogined() || GlobalAutoBehaviour<AccountManager>.instance.CraftSlotManager == null)
		{
			return;
		}
		for (int i = 0; i < 8; i++)
		{
			CraftSlotInfo craftSlotInfo = GlobalAutoBehaviour<AccountManager>.instance.CraftSlotManager.Get(i + 1);
			if (craftSlotInfo == null)
			{
				continue;
			}
			if (craftSlotInfo.CraftIndex == 0 || craftSlotInfo.ItemIndex == 0)
			{
				break;
			}
			if (Vespa_String_Extension_Method2.TryParseNetworkTime2(craftSlotInfo.CompleteTime, out var o_dateTime))
			{
				int slotIndex = craftSlotInfo.SlotIndex;
				TimeSpan timeSpan = o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
				if (timeSpan.TotalMilliseconds > 0.0)
				{
					SingletonAuto<NotificationManager>.instance.RegisteItemCraftCompletelNotification(slotIndex, (long)timeSpan.TotalMilliseconds);
				}
			}
		}
	}

	public void RegisteItemCraftCompletelNotification(int slotNum, long remainTime)
	{
		if (_isEnableAllNotification && _isEnableSystemNotification && _isEnableItemCompleteNotification)
		{
			RegisteLocalNotification(2000 + slotNum, LocalizationManager.GetStringDirect("PUSH_TITLE_MESSAGE"), LocalizationManager.GetStringDirect("PUSH_ITEM_CREATE_COMPLETE_MESSAGE"), LocalizationManager.GetStringDirect("PUSH_TITLE_MESSAGE"), remainTime, everyday: false);
		}
	}

	public void RegisteRetentionCareNotification()
	{
		if (!_isEnableAllNotification || !_isEnableEventNotification)
		{
			return;
		}
		float[] array = null;
		array = GameRoot.Instance.ConstantDataContainer.GetValueArray<float>("RetentionCareNotiTimes");
		if (array == null)
		{
			array = new float[2] { 24f, 70f };
		}
		int num = 1;
		int num2 = 0;
		DateTime now = DateTime.Now;
		float[] array2 = array;
		foreach (float num3 in array2)
		{
			DateTime dateTime = now.AddHours(num3);
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("GameOptionNotification_NightBegin", 8);
			int num4 = GameRoot.Instance.ConstantDataContainer.GetValueInt("GameOptionNotification_NightEnd", 21) - 1;
			bool flag = false;
			if (valueInt > dateTime.Hour || dateTime.Hour > num4)
			{
				flag = true;
			}
			if (!_isEnableNightNotification && flag)
			{
				int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("RetentionCareNotiReserveTime", 10);
				if (dateTime.Hour > num4)
				{
					dateTime = dateTime.AddDays(1.0);
				}
				dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, valueInt2, 0, 0, dateTime.Kind);
			}
			int num5;
			for (num5 = UnityEngine.Random.Range(1, 4); num5 == num2; num5 = UnityEngine.Random.Range(1, 4))
			{
			}
			string key = $"PUSH_RETENTION_MESSAGE_{num5}";
			double totalMilliseconds = (dateTime - now).TotalMilliseconds;
			RegisteLocalNotification(4000 + num, LocalizationManager.GetStringDirect("PUSH_TITLE_MESSAGE"), LocalizationManager.GetStringDirect(key), LocalizationManager.GetStringDirect("PUSH_TITLE_MESSAGE"), (long)totalMilliseconds, everyday: false, ignoreNIghtPushLogic: true);
			num++;
			num2 = num5;
		}
	}

	public void UnRegisteItemCraftCompletelNotification(int slotNum)
	{
		UnRegisteLocalNotification(2000 + slotNum, "", "", "", 0L);
	}

	public void RegisteCreateNickAfterNotification()
	{
		if (!_isEnableAllNotification || !_isEnableEventNotification || String_Extension_Method.IsNullOrEmpty(GlobalAutoBehaviour<AccountManager>.instance.UserManager.Nick) || !Vespa_String_Extension_Method2.TryParseNetworkTime2(GlobalAutoBehaviour<AccountManager>.instance.UserManager.UserInfo.CreatedTime, out var o_dateTime))
		{
			return;
		}
		int[] valueArray = GameRoot.Instance.ConstantDataContainer.GetValueArray<int>("ID_Creation_AfterMinute");
		string[] valueArray2 = GameRoot.Instance.ConstantDataContainer.GetValueArray<string>("ID_Creation_AfterPushLocalText");
		if ((o_dateTime.AddMinutes(valueArray.LastOrDefault()) - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalMinutes <= 0.0)
		{
			return;
		}
		for (int i = 0; i < valueArray.Length; i++)
		{
			DateTime dateTime = o_dateTime.AddMinutes(valueArray[i]);
			TimeSpan timeSpan = dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
			if (!(timeSpan.TotalMinutes <= 0.0) && (dateTime.Day != GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime().Day || !AttendanceFacade.AlreadyReceived_Reward_Attendance_BeginUser()))
			{
				string key = (Vespa_Generic_IList_Extension_Method.IsValidIndex(valueArray2, i) ? valueArray2[i] : valueArray2.FirstOrDefault());
				RegisteLocalNotification(3000 + i, LocalizationManager.GetStringDirect("PUSH_TITLE_MESSAGE"), LocalizationManager.GetStringDirect(key), LocalizationManager.GetStringDirect("PUSH_TITLE_MESSAGE"), (long)timeSpan.TotalMilliseconds, everyday: false);
			}
		}
	}

	public void CheckLimitedBM()
	{
		if (!_isEnableAllNotification || !_isEnableEventNotification)
		{
			return;
		}
		IList<PurchaseMarketingPopupData> allData = GameRoot.Instance.PurchaseMarketingPopupDataContainer.GetAllData();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(allData))
		{
			return;
		}
		foreach (PurchaseMarketingPopupData item in allData)
		{
			if (item == null || __Extension_Method_ICollection__.IsNullOrEmpty(item.PurchaseIndices) || item.PushID == 0)
			{
				continue;
			}
			PurchaseMarketingInfo purchaseMarketingInfo = null;
			int[] purchaseIndices = item.PurchaseIndices;
			foreach (int key in purchaseIndices)
			{
				PurchaseMarketingInfo purchaseMarketingInfo2 = GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager.Get(key);
				if (PurchaseMarketingManager.IsPurchaseMarketing(purchaseMarketingInfo2))
				{
					purchaseMarketingInfo = purchaseMarketingInfo2;
					break;
				}
			}
			if (purchaseMarketingInfo == null)
			{
				continue;
			}
			int[] pushSendMinute = item.PushSendMinute;
			string[] pushLocalText = item.PushLocalText;
			if (pushSendMinute == null || pushLocalText == null || !Vespa_String_Extension_Method2.TryParseNetworkTime2(purchaseMarketingInfo.BeginTime, out var o_dateTime))
			{
				continue;
			}
			for (int j = 0; j < pushSendMinute.Length; j++)
			{
				TimeSpan timeSpan = o_dateTime.AddMinutes(pushSendMinute[j]) - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
				if (!(timeSpan.TotalMinutes <= 0.0))
				{
					string key2 = (Vespa_Generic_IList_Extension_Method.IsValidIndex(pushLocalText, j) ? pushLocalText[j] : pushLocalText.FirstOrDefault());
					RegisteLocalNotification(item.PushID, LocalizationManager.GetStringDirect("PUSH_TITLE_MESSAGE"), LocalizationManager.GetStringDirect(key2), LocalizationManager.GetStringDirect("PUSH_TITLE_MESSAGE"), (long)timeSpan.TotalMilliseconds, everyday: false);
				}
			}
		}
	}

	public bool CheckLastNotification(int id, long remainTimeMs)
	{
		DateTime dateTime = DateTime.Now.AddMilliseconds(remainTimeMs);
		if (lastNotifications.TryGetValue(id, out var value) && System.Math.Abs((dateTime - value).TotalMilliseconds) < (double)LocalNotificationCheckTime)
		{
			return false;
		}
		return true;
	}

	public void RegisteLocalNotification(int id, string title, string body, string ticker, long remainTimeMs, bool everyday, bool ignoreNIghtPushLogic = false)
	{
		if (!CheckLastNotification(id, remainTimeMs))
		{
			return;
		}
		UnRegisteLocalNotification(id, "", "", "", 0L);
		if (remainTimeMs <= LocalNotificationCheckTime)
		{
			return;
		}
		SingletonAuto<LocalRegister>.instance.Register(id, title, body, ticker, remainTimeMs, everyday);
		if (lastNotifications.TryGetValue(id, out var _))
		{
			lastNotifications[id] = DateTime.Now.AddMilliseconds(remainTimeMs);
		}
		else
		{
			lastNotifications.Add(id, DateTime.Now.AddMilliseconds(remainTimeMs));
		}
		if (!ignoreNIghtPushLogic)
		{
			DateTime dateTime = DateTime.Now.AddMilliseconds(remainTimeMs);
			bool flag = false;
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("GameOptionNotification_NightBegin", 8);
			int num = GameRoot.Instance.ConstantDataContainer.GetValueInt("GameOptionNotification_NightEnd", 21) - 1;
			if (valueInt > dateTime.Hour || dateTime.Hour > num)
			{
				flag = true;
			}
			if (!_isEnableNightNotification && flag)
			{
				UnRegisteLocalNotification(id, "", "", "", 0L);
			}
		}
	}

	public void UnRegisteLocalNotification(int id, string title = "", string body = "", string ticker = "", long remainTimeMs = 0L, bool everyday = false)
	{
		SingletonAuto<LocalRegister>.instance.UnRegister(id, title, body, ticker, remainTimeMs, everyday);
		if (lastNotifications.TryGetValue(id, out var _))
		{
			lastNotifications.Remove(id);
		}
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (changedInfo.UserStatType == UserManager.UserStatType.Stamina)
		{
			if (changedInfo.StaminaType == StaminaType.Chicken)
			{
				RegisteStaminaFullNotification();
			}
			if (changedInfo.StaminaType == StaminaType.ChallengeTowerKey)
			{
				RegisteChallengeTowerNotification();
			}
			if (changedInfo.StaminaType == StaminaType.Sword)
			{
				RegisteArenaTicketNotification();
			}
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return true;
	}
}
