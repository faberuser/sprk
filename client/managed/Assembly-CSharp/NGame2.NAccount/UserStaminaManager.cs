using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NLogin;
using NGame2.NUI;
using NShared;
using NShared.GetStaminaInfos;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NAccount;

public class UserStaminaManager
{
	private readonly Dictionary<StaminaType, StaminaInfo> _staminaDictionary = new Dictionary<StaminaType, StaminaInfo>();

	private readonly AccountManager _accountManager;

	private readonly UserManager _userManager;

	private DateTime _lastUpdatedTime = DateTime.UtcNow;

	private List<StaminaType> _updateStaminaTypes = new List<StaminaType>();

	public UserStaminaManager(AccountManager accountManager, UserManager userManager)
	{
		_accountManager = accountManager;
		_userManager = userManager;
		RegisterStaminaInfo();
	}

	private void RegisterStaminaInfo()
	{
		if (GameRoot.Instance == null || GameRoot.Instance.StaminaDataContainer == null)
		{
			return;
		}
		foreach (StaminaData allDatum in GameRoot.Instance.StaminaDataContainer.GetAllData())
		{
			StaminaType staminaType = allDatum.StaminaType;
			if (staminaType != StaminaType.None)
			{
				StaminaData staminaDataLocal = allDatum;
				SetStaminaInfo(staminaType, () => _userManager.GetStamina(staminaDataLocal), () => _userManager.GetStaminaRechargeCount(staminaDataLocal));
			}
		}
	}

	private void SetStaminaInfo(StaminaType type, Func<long> curValueFunc, Func<int> rechargeCountFunc)
	{
		if (Helper.GameDataLoadedAndLogined())
		{
			StaminaData data = GameRoot.Instance.StaminaDataContainer.GetData(type);
			if (data == null)
			{
				VespaDebug.LogError("Stamina data not found : " + type);
				return;
			}
			StaminaInfo value = new StaminaInfo(data, curValueFunc, rechargeCountFunc);
			_staminaDictionary.Remove(type);
			_staminaDictionary.Add(type, value);
		}
	}

	public StaminaInfo GetStaminaInfo(StaminaType type)
	{
		StaminaInfo value = null;
		_staminaDictionary.TryGetValue(type, out value);
		return value;
	}

	public void ApplyExpectedRechargeTime(StaminaType type, int nextRechargeRemainTime, int fullRechargeRemainTime)
	{
		SprkNetworkPerformance.SetStaminaClock(this, (int)type, nextRechargeRemainTime);
		StaminaInfo staminaInfo = GetStaminaInfo(type);
		if (staminaInfo != null)
		{
			DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
			if (nextRechargeRemainTime >= 0)
			{
				staminaInfo.NextRechargeTime = currentServerUTCTime.AddSeconds(nextRechargeRemainTime);
			}
			if (fullRechargeRemainTime >= 0)
			{
				staminaInfo.FullRechargeTime = currentServerUTCTime.AddSeconds(fullRechargeRemainTime);
			}
		}
	}

	public bool GetCurrentStamina(StaminaType staminaType, out long currentValue, out long maxValue, int reqChapterIndex = 0)
	{
		int num = 0;
		currentValue = num;
		int num2 = 0;
		maxValue = num2;
		if (staminaType == StaminaType.HideoutKey)
		{
			GlobalAutoBehaviour<AccountManager>.instance.HideoutDungeonManager.GetChapterRemainCount(out currentValue, out maxValue, reqChapterIndex);
			return true;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.KeyTypeDungeonManager.IsKeyTypeDungeon(staminaType))
		{
			GlobalAutoBehaviour<AccountManager>.instance.KeyTypeDungeonManager.GetChapterRemainCount(staminaType, out currentValue, out maxValue, reqChapterIndex);
			return true;
		}
		StaminaInfo staminaInfo = GetStaminaInfo(staminaType);
		if (staminaInfo != null)
		{
			currentValue = staminaInfo.CurValue;
			int maxValue2 = staminaInfo.MaxValue;
			maxValue = maxValue2;
			return true;
		}
		return false;
	}

	public long GetCurrentStamina(StaminaType staminaType, int reqChapterIndex = 0)
	{
		GetCurrentStamina(staminaType, out var currentValue, out var _, reqChapterIndex);
		return currentValue;
	}

	public long GetMaxStamina(StaminaType staminaType)
	{
		GetCurrentStamina(staminaType, out var _, out var maxValue);
		return maxValue;
	}

	public long GetRemainStamina(StaminaType staminaType)
	{
		GetCurrentStamina(staminaType, out var currentValue, out var maxValue);
		long val = checked(maxValue - currentValue);
		int num = 0;
		return System.Math.Max(val, num);
	}

	public int GetRechargeStamina(StaminaType staminaType)
	{
		StaminaInfo staminaInfo = GetStaminaInfo(staminaType);
		if (staminaInfo == null || staminaInfo.StaminaData == null)
		{
			return 0;
		}
		if (__Extension_Method_ICollection__.COUNT(staminaInfo.StaminaData.ResetCount) <= 0)
		{
			return 0;
		}
		return staminaInfo.StaminaData.ResetCount[0];
	}

	public int GetRechargeGem(StaminaType staminaType)
	{
		return GetStaminaInfo(staminaType)?.RechargeCostGem ?? 0;
	}

	public int GetRechargeGold(StaminaType staminaType)
	{
		return GetStaminaInfo(staminaType)?.RechargeCostGold ?? 0;
	}

	public int GetRechargeCount(StaminaType staminaType)
	{
		return GetStaminaInfo(staminaType)?.RechargeCount ?? 0;
	}

	public List<ItemData> GetStaminaPotion(StaminaType staminaType, IList<int> forcedItemIndices = null)
	{
		IList<ItemData> itemDataListByType = GameRoot.Instance.ItemDataContainer.GetItemDataListByType(ItemType.Potion);
		List<ItemData> list = new List<ItemData>();
		foreach (ItemData item in itemDataListByType)
		{
			if (forcedItemIndices != null && !forcedItemIndices.Contains(item.Index))
			{
				continue;
			}
			ItemData<PotionItemData> data = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(item.Index);
			if (data != null)
			{
				if (!EnumTools.TryParse<StaminaType>(data.SubData.ActionSubValue, out var o_value))
				{
					o_value = StaminaType.Chicken;
				}
				if (o_value == staminaType)
				{
					list.Add(data);
				}
			}
		}
		return list;
	}

	public bool IsRechargeable(StaminaType staminaType)
	{
		StaminaInfo staminaInfo = GetStaminaInfo(staminaType);
		if (staminaInfo == null)
		{
			return false;
		}
		if (staminaInfo.StaminaData == null)
		{
			return false;
		}
		if (staminaInfo.StaminaData.RechargeCostType == StaminaValueType.None)
		{
			return false;
		}
		return true;
	}

	public bool IsRegenType(StaminaType staminaType)
	{
		StaminaInfo staminaInfo = GetStaminaInfo(staminaType);
		if (staminaInfo == null)
		{
			return false;
		}
		if (staminaInfo.StaminaData == null)
		{
			return false;
		}
		return staminaInfo.StaminaData.UpdateType == StaminaUpdateType.Regen;
	}

	public StaminaUpdateType GetStaminaUpdateType(StaminaType staminaType)
	{
		StaminaInfo staminaInfo = GetStaminaInfo(staminaType);
		if (staminaInfo == null)
		{
			return StaminaUpdateType.None;
		}
		if (staminaInfo.StaminaData == null)
		{
			return StaminaUpdateType.None;
		}
		return staminaInfo.StaminaData.UpdateType;
	}

	public DateTime GetFullRechargeTime(StaminaType staminaType)
	{
		return GetStaminaInfo(staminaType).FullRechargeTime;
	}

	public DateTime? GetNextRechargeTime(StaminaType staminaType)
	{
		return GetStaminaInfo(staminaType)?.NextRechargeTime;
	}

	public List<StaminaInfo> GetStaminaList()
	{
		return _staminaDictionary.Values.ToList();
	}

	public void Update()
	{
		if (Helper.GameDataLoadedAndLogined())
		{
			UpdateStamina();
		}
	}

	private void UpdateStamina()
	{
		DateTime utcNow = DateTime.UtcNow;
		if ((utcNow - _lastUpdatedTime).TotalSeconds < 1.0)
		{
			return;
		}
		_lastUpdatedTime = utcNow;
		_updateStaminaTypes.Clear();
		foreach (KeyValuePair<StaminaType, StaminaInfo> item in _staminaDictionary)
		{
			StaminaType key = item.Key;
			StaminaInfo value = item.Value;
			UpdateStaminaValue(key, value);
			if (IsUpdateStamina(key, value))
			{
				_updateStaminaTypes.Add(key);
			}
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_updateStaminaTypes))
		{
			RequestGetStamina(_updateStaminaTypes.ToArray());
		}
	}

	private void UpdateStaminaValue(StaminaType staminaType, StaminaInfo staminaInfo)
	{
		StaminaData data = GameRoot.Instance.StaminaDataContainer.GetData(staminaType);
		if (data != null)
		{
			int rechargeCount = staminaInfo.RechargeCount;
			staminaInfo.MaxValue = SprkWorldBossKeyMaximum(ParseStaminaValue(data.MaxCountType, data.MaxCountValue), staminaType);
			staminaInfo.RechargeCostGold = ParseStaminaValue(data.RechargeCostType, data.RechargeGoldValue, rechargeCount);
			staminaInfo.RechargeCostGem = ParseStaminaValue(data.RechargeCostType, data.RechargeGemValue, rechargeCount);
		}
	}

	private bool IsUpdateStamina(StaminaType staminaType, StaminaInfo staminaInfo)
	{
		if (!SprkNetworkPerformance.HasStaminaClock(this, (int)staminaType))
		{
			return false;
		}
		if (staminaInfo == null)
		{
			return false;
		}
		if (staminaInfo.StaminaData.UpdateType == StaminaUpdateType.None)
		{
			return false;
		}
		if (staminaInfo.RequestedTime > 0f && Time.time - staminaInfo.RequestedTime < 10f)
		{
			return false;
		}
		int maxValue = staminaInfo.MaxValue;
		long curValue = staminaInfo.CurValue;
		int num = maxValue;
		if (curValue >= num && staminaInfo.StaminaData.UpdateType == StaminaUpdateType.Regen)
		{
			return false;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if ((staminaInfo.NextRechargeTime - currentServerUTCTime).TotalSeconds > 0.0)
		{
			return false;
		}
		if (SingletonUpdater<LoginManager>.instance.IsSessionExpired)
		{
			return false;
		}
		return true;
	}

	private void RequestGetStamina(StaminaType[] staminaTypes)
	{
		StaminaType[] array = staminaTypes;
		foreach (StaminaType type in array)
		{
			StaminaInfo staminaInfo = GetStaminaInfo(type);
			if (staminaInfo != null)
			{
				staminaInfo.RequestedTime = Time.time;
			}
		}
		NShared.GetStaminaInfos.Request request = new NShared.GetStaminaInfos.Request();
		request.StaminaTypes = staminaTypes;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.DisableRetry, delegate(NShared.GetStaminaInfos.Response response)
		{
			if (response != null)
			{
				_userManager.Apply(response.StaminaResults);
			}
			StaminaType[] array2 = staminaTypes;
			foreach (StaminaType type2 in array2)
			{
				StaminaInfo staminaInfo2 = GetStaminaInfo(type2);
				if (staminaInfo2 != null)
				{
					staminaInfo2.RequestedTime = 0f;
				}
			}
		});
	}

	private int ParseStaminaValue(StaminaValueType type, string value)
	{
		switch (type)
		{
		case StaminaValueType.Fixed:
			return BaseJsonMarshaler.ToInt(value);
		case StaminaValueType.FixedMaxCount:
			return BaseJsonMarshaler.ToInt(value);
		case StaminaValueType.OverValueRemain:
			return BaseJsonMarshaler.ToInt(value);
		case StaminaValueType.OverValueRemainMaxCount:
			return BaseJsonMarshaler.ToInt(value);
		case StaminaValueType.TeamLevelData:
		{
			int teamLevel2 = _userManager.UserInfo.TeamLevel;
			Dictionary<string, object> dictionary2 = JsonMarshaler.EncodeJsonObject(GameRoot.Instance.TeamLevelDataContainer.GetData(teamLevel2));
			if (dictionary2 != null && dictionary2.ContainsKey(value))
			{
				return BaseJsonMarshaler.ToInt(dictionary2[value].ToString());
			}
			break;
		}
		case StaminaValueType.TeamLevelDataAndHeroCount:
		{
			int num = 0;
			int teamLevel = _userManager.UserInfo.TeamLevel;
			Dictionary<string, object> dictionary = JsonMarshaler.EncodeJsonObject(GameRoot.Instance.TeamLevelDataContainer.GetData(teamLevel));
			if (dictionary != null && dictionary.ContainsKey(value))
			{
				num += BaseJsonMarshaler.ToInt(dictionary[value].ToString());
			}
			CachedHeroInfo[] heroInfos = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfos();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroInfos); i++)
			{
				CachedHeroInfo cachedHeroInfo = heroInfos[i];
				if (cachedHeroInfo != null)
				{
					CreatureData creatureData = cachedHeroInfo.CreatureData;
					if (creatureData != null)
					{
						num += creatureData.AddStamina;
					}
				}
			}
			Dictionary<StaminaType, int> activeStaminaBuffDictionary = GlobalAutoBehaviour<AccountManager>.instance.GetActiveStaminaBuffDictionary();
			int value2 = 0;
			if (activeStaminaBuffDictionary.TryGetValue(StaminaType.Chicken, out value2))
			{
				num += value2;
			}
			int petBonusBuffValue = GlobalAutoBehaviour<AccountManager>.instance.GetPetBonusBuffValue(EventBonusType.AddStamina);
			return num + petBonusBuffValue;
		}
		}
		return 0;
	}

	private int ParseStaminaValue(StaminaValueType type, string[] values, int indexOfArray)
	{
		if (type == StaminaValueType.Fixed || type == StaminaValueType.FixedMaxCount)
		{
			if (__Extension_Method_ICollection__.COUNT(values) > 0)
			{
				int num = Mathf.Min(indexOfArray, __Extension_Method_ICollection__.COUNT(values) - 1);
				return BaseJsonMarshaler.ToInt(values[num]);
			}
			return 0;
		}
		return 0;
	}

	public void GetCurStaminaTimeInfo(StaminaType staminaType, out string FullStaminaRechargeRemainTime, out string OneStaminaRechargeRemainTime)
	{
		FullStaminaRechargeRemainTime = LocalizationManager.GetStringDirect("ALLRECHARGE_TIME_ANOTHER");
		OneStaminaRechargeRemainTime = string.Empty;
		StaminaInfo staminaInfo = GetStaminaInfo(staminaType);
		if (staminaInfo == null)
		{
			return;
		}
		switch (staminaType)
		{
		case StaminaType.TreasureHouseKey:
			FullStaminaRechargeRemainTime = LocalizationManager.GetStringDirect("GOLD_CUBE_TOOLTIP");
			OneStaminaRechargeRemainTime = LocalizationManager.GetStringDirect("GOLD_CUBE_TITLE");
			return;
		case StaminaType.EventDungeonGoldKey:
		{
			string[] valueArray = GameRoot.Instance.ConstantDataContainer.GetValueArray<string>("EventDungeonGoldKeyToolTip");
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(valueArray) && valueArray.Length > 1)
			{
				FullStaminaRechargeRemainTime = LocalizationManager.GetStringDirect(valueArray[0]);
				OneStaminaRechargeRemainTime = LocalizationManager.GetStringDirect(valueArray[1]);
			}
			else
			{
				FullStaminaRechargeRemainTime = "-";
				OneStaminaRechargeRemainTime = "-";
			}
			return;
		}
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if (!String_Extension_Method.IsNullOrEmpty(staminaInfo.StaminaData.StaminaTooltipDesc))
		{
			int rechargeStamina = GetRechargeStamina(staminaType);
			FullStaminaRechargeRemainTime = LocalizationManager.GetStringDirect(staminaInfo.StaminaData.StaminaTooltipDesc, rechargeStamina);
		}
		if (staminaInfo.StaminaData.UpdateType == StaminaUpdateType.Regen)
		{
			OneStaminaRechargeRemainTime = LocalizationManager.GetString(NShared.NLocalization.System.RECHARGE_STAMINA_SECOND_DESC);
			int maxValue = staminaInfo.MaxValue;
			long curValue = staminaInfo.CurValue;
			int num = maxValue;
			if (curValue >= num)
			{
				FullStaminaRechargeRemainTime += ": -";
				OneStaminaRechargeRemainTime += ": -";
				return;
			}
			TimeSpan timeSpan = staminaInfo.FullRechargeTime - currentServerUTCTime;
			if (timeSpan.Days > 0)
			{
				string text = $": {UIHelper.GetRemainTimeLabelText((long)timeSpan.TotalSeconds)}";
				FullStaminaRechargeRemainTime = LocalizationManager.GetString(NShared.NLocalization.System.RECHARGE_STAMINA_FULL_REMAIN_TIME) + text;
			}
			else
			{
				string text2 = $": {timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
				FullStaminaRechargeRemainTime = LocalizationManager.GetString(NShared.NLocalization.System.RECHARGE_STAMINA_FULL_REMAIN_TIME) + text2;
			}
			TimeSpan timeSpan2 = staminaInfo.NextRechargeTime - currentServerUTCTime;
			string text3 = $": {timeSpan2.Hours:D2}:{timeSpan2.Minutes:D2}:{Mathf.Max(timeSpan2.Seconds, 0):D2}";
			OneStaminaRechargeRemainTime += text3;
		}
		else if (staminaInfo.StaminaData.UpdateType == StaminaUpdateType.Reset)
		{
			TimeSpan timeSpan3 = staminaInfo.NextRechargeTime - currentServerUTCTime;
			int hours = (int)timeSpan3.TotalSeconds / 3600;
			TimeSpan timeSpan4 = new TimeSpan(hours, 0, 0);
			string empty = string.Empty;
			empty = ((timeSpan4.Days <= 0) ? $": {timeSpan3.Hours:D2}:{timeSpan3.Minutes:D2}:{timeSpan3.Seconds:D2}" : $": {UIHelper.GetRemainTimeLabelText((long)timeSpan3.TotalSeconds)}");
			FullStaminaRechargeRemainTime += empty;
			int num2 = 0;
			num2 = staminaType switch
			{
				StaminaType.HideoutKey => GlobalAutoBehaviour<AccountManager>.instance.HideoutDungeonManager.GetCurrentChapterRechargeGem(), 
				StaminaType.ConquestKey => GlobalAutoBehaviour<AccountManager>.instance.KeyTypeDungeonManager.GetCurrentChapterRechargeGem(staminaType), 
				_ => staminaInfo.RechargeCostGem, 
			};
			if (num2 > 0)
			{
				OneStaminaRechargeRemainTime = LocalizationManager.GetString(NShared.NLocalization.System.FULLRECHARGE_COST_RUBY_DESC, num2);
			}
			else
			{
				OneStaminaRechargeRemainTime = LocalizationManager.GetStringDirect("DISABLE_RUBY_RECHARGE");
			}
		}
	}

	public void GetCurStaminaRemainTime(StaminaType staminaType, out string StaminaRechargeRemainTime)
	{
		StaminaRechargeRemainTime = string.Empty;
		StaminaInfo staminaInfo = GetStaminaInfo(staminaType);
		if (staminaInfo == null || staminaType == StaminaType.TreasureHouseKey || staminaType == StaminaType.EclipseKey)
		{
			return;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if (staminaInfo.StaminaData.UpdateType == StaminaUpdateType.Regen)
		{
			StaminaRechargeRemainTime = LocalizationManager.GetString(NShared.NLocalization.System.RECHARGE_STAMINA_SECOND_DESC);
			int maxValue = staminaInfo.MaxValue;
			long curValue = staminaInfo.CurValue;
			int num = maxValue;
			if (curValue >= num)
			{
				StaminaRechargeRemainTime += " -";
				return;
			}
			TimeSpan timeSpan = staminaInfo.NextRechargeTime - currentServerUTCTime;
			string text = $" {timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{Mathf.Max(timeSpan.Seconds, 0):D2}";
			StaminaRechargeRemainTime += text;
		}
		else if (staminaInfo.StaminaData.UpdateType == StaminaUpdateType.Reset)
		{
			TimeSpan timeSpan2 = staminaInfo.NextRechargeTime - currentServerUTCTime;
			int hours = (int)timeSpan2.TotalSeconds / 3600;
			TimeSpan timeSpan3 = new TimeSpan(hours, 0, 0);
			string empty = string.Empty;
			empty = ((timeSpan3.Days <= 0) ? $"{timeSpan2.Hours:D2}:{timeSpan2.Minutes:D2}:{timeSpan2.Seconds:D2}" : $"{UIHelper.GetRemainTimeLabelText((long)timeSpan2.TotalSeconds)}");
			StaminaRechargeRemainTime += empty;
		}
	}

	private static int SprkWorldBossKeyMaximum(int maximum, StaminaType type)
	{
		if (type == StaminaType.WorldBossGlobalTicket)
		{
			return 1;
		}
		return maximum;
	}
}
