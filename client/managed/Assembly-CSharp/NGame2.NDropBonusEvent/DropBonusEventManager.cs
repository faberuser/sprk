using System;
using System.Collections.Generic;
using LitJson;
using NGame2.NAccount;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NDropBonusEvent;

public class DropBonusEventManager : GlobalAutoBehaviour<DropBonusEventManager>
{
	public enum DropBonusEventChangeType
	{
		EventAdded,
		EventRemoved
	}

	public class DropBonusEventChangeInfo
	{
		public DropBonusEventChangeType changeType;

		public DropBonusEvent bonusEvent;
	}

	private static List<DropBonusEvent> _allEffectCache = new List<DropBonusEvent>(50);

	private static DateTime allEffectCacheTime;

	private readonly UserObserverManager<DropBonusEventChangeInfo> _observerManager = new UserObserverManager<DropBonusEventChangeInfo>();

	private List<DropBonusEvent> _list = new List<DropBonusEvent>();

	public IUserObserverManager ObserverManager => _observerManager;

	public void Add(DropBonusEvent eventInfo)
	{
		if (eventInfo != null && _list.Find((DropBonusEvent info) => info.Id == eventInfo.Id) == null)
		{
			_list.Add(eventInfo);
			_observerManager.AnnounceToObservers(new DropBonusEventChangeInfo
			{
				changeType = DropBonusEventChangeType.EventAdded,
				bonusEvent = eventInfo
			});
		}
	}

	public void Remove(long Id)
	{
		DropBonusEvent dropBonusEvent = _list.Find((DropBonusEvent info) => info.Id == Id);
		if (dropBonusEvent != null)
		{
			_list.Remove(dropBonusEvent);
			_observerManager.AnnounceToObservers(new DropBonusEventChangeInfo
			{
				changeType = DropBonusEventChangeType.EventRemoved,
				bonusEvent = dropBonusEvent
			});
		}
	}

	public void Set(DropBonusEvent[] infos)
	{
		if (infos != null)
		{
			_list.Clear();
			for (int i = 0; i < infos.Length; i++)
			{
				Add(infos[i]);
			}
		}
	}

	public void ClearAllEffectCacheTime()
	{
		allEffectCacheTime = DateTime.UtcNow.AddSeconds(-100.0);
	}

	public List<DropBonusEvent> GetAllEffectiveEvent()
	{
		DateTime utcNow = DateTime.UtcNow;
		if ((utcNow - allEffectCacheTime).Seconds < 1)
		{
			return _allEffectCache;
		}
		allEffectCacheTime = utcNow;
		_allEffectCache.Clear();
		for (int i = 0; i < _list.Count; i++)
		{
			DropBonusEvent dropBonusEvent = _list[i];
			if (IsEffectiveEvent(dropBonusEvent))
			{
				_allEffectCache.Add(dropBonusEvent);
			}
		}
		return _allEffectCache;
	}

	public List<DropBonusEvent> GetAllEffectiveEvent(BattleType battleType)
	{
		List<DropBonusEvent> list = new List<DropBonusEvent>();
		List<int> list2 = new List<int>();
		for (int i = 0; i < _list.Count; i++)
		{
			DropBonusEvent dropBonusEvent = _list[i];
			if (!IsEffectiveEvent(dropBonusEvent, battleType))
			{
				continue;
			}
			int gachaEventConditionPropValueInt = GetGachaEventConditionPropValueInt(dropBonusEvent, "TextGroup");
			if (gachaEventConditionPropValueInt != -1)
			{
				if (list2.Contains(gachaEventConditionPropValueInt))
				{
					continue;
				}
				list2.Add(gachaEventConditionPropValueInt);
			}
			list.Add(dropBonusEvent);
		}
		return list;
	}

	public bool AnyEffectiveEventByEventBonusType(EventBonusType eventBonusType)
	{
		List<DropBonusEvent> allEffectiveEvent = GetAllEffectiveEvent();
		for (int i = 0; i < allEffectiveEvent.Count; i++)
		{
			if (allEffectiveEvent[i].BonusType == eventBonusType)
			{
				return true;
			}
		}
		return false;
	}

	public List<DropBonusEvent> GetAllEffectiveEvent(EventBonusType eventBonusType)
	{
		List<DropBonusEvent> list = new List<DropBonusEvent>();
		List<DropBonusEvent> allEffectiveEvent = GetAllEffectiveEvent();
		for (int i = 0; i < allEffectiveEvent.Count; i++)
		{
			DropBonusEvent dropBonusEvent = allEffectiveEvent[i];
			if (dropBonusEvent.BonusType == eventBonusType)
			{
				list.Add(dropBonusEvent);
			}
		}
		return list;
	}

	public DropBonusEvent GetPayshopDiscountEffectiveEvent(int productIndex)
	{
		List<DropBonusEvent> allEffectiveEvent = GetAllEffectiveEvent();
		if (__Extension_Method_ICollection__.COUNT(allEffectiveEvent) == 0)
		{
			return null;
		}
		for (int i = 0; i < allEffectiveEvent.Count; i++)
		{
			if (allEffectiveEvent[i].BonusType == EventBonusType.PayShopDiscount)
			{
				int result = 0;
				if (int.TryParse(allEffectiveEvent[i].Condition, out result) && result != 0 && result == productIndex)
				{
					return allEffectiveEvent[i];
				}
			}
		}
		return null;
	}

	public DropBonusEvent[] GetItemGachaEffectiveEvent(int gachaIndex = -1)
	{
		EventBonusType[] obj = new EventBonusType[2]
		{
			EventBonusType.MulItemGachaRate,
			EventBonusType.MulItemGachaRateTreasure
		};
		List<DropBonusEvent> list = new List<DropBonusEvent>();
		EventBonusType[] array = obj;
		foreach (EventBonusType eventBonusType in array)
		{
			List<DropBonusEvent> effectiveEvents = GetEffectiveEvents(eventBonusType);
			if (__Extension_Method_ICollection__.IsNullOrEmpty(effectiveEvents))
			{
				continue;
			}
			foreach (DropBonusEvent item in effectiveEvents)
			{
				if (gachaIndex == -1 || GetGachaEventConditionPropValueInt(item, "GachaIndex") == gachaIndex)
				{
					list.Add(item);
				}
			}
		}
		return list.ToArray();
	}

	public List<DropBonusEvent> GetEffectiveEvents(EventBonusType eventBonusType)
	{
		List<DropBonusEvent> allEffectiveEvent = GetAllEffectiveEvent();
		if (__Extension_Method_ICollection__.COUNT(allEffectiveEvent) == 0)
		{
			return null;
		}
		List<DropBonusEvent> list = new List<DropBonusEvent>();
		for (int i = 0; i < allEffectiveEvent.Count; i++)
		{
			if (allEffectiveEvent[i].BonusType == eventBonusType)
			{
				list.Add(allEffectiveEvent[i]);
			}
		}
		return list;
	}

	public DropBonusEvent GetEffectiveEvent(BattleType evnetBattleType, EventBonusType eventBonusType)
	{
		foreach (DropBonusEvent item in GlobalAutoBehaviour<DropBonusEventManager>.instance.GetAllEffectiveEvent(evnetBattleType))
		{
			if (item.BonusType == eventBonusType)
			{
				return item;
			}
		}
		return null;
	}

	public DropBonusEvent GetEffectiveEvent(EventBonusType eventBonusType, string condition)
	{
		List<DropBonusEvent> allEffectiveEvent = GetAllEffectiveEvent();
		if (__Extension_Method_ICollection__.COUNT(allEffectiveEvent) == 0)
		{
			return null;
		}
		for (int i = 0; i < allEffectiveEvent.Count; i++)
		{
			DropBonusEvent dropBonusEvent = allEffectiveEvent[i];
			if (dropBonusEvent.BonusType != eventBonusType)
			{
				continue;
			}
			if (IsGachaEventType(eventBonusType))
			{
				int num = Vespa_String_Extension_Method.TOINT(condition, -1);
				if (GetGachaEventConditionPropValueInt(dropBonusEvent, "GachaIndex") != num)
				{
					continue;
				}
			}
			else if (!String_Extension_Method.IsNullOrEmpty(condition) && dropBonusEvent.Condition != null && !dropBonusEvent.Condition.Contains(condition))
			{
				continue;
			}
			return dropBonusEvent;
		}
		return null;
	}

	public DropBonusEvent GetEffectiveEvent(string condition)
	{
		List<DropBonusEvent> allEffectiveEvent = GetAllEffectiveEvent();
		if (__Extension_Method_ICollection__.COUNT(allEffectiveEvent) == 0)
		{
			return null;
		}
		return allEffectiveEvent.Find((DropBonusEvent x) => x.Condition == condition);
	}

	public static bool IsGachaEventType(EventBonusType bonusType)
	{
		if (bonusType == EventBonusType.MulItemGachaRate || bonusType == EventBonusType.MulItemGachaRateTreasure)
		{
			return true;
		}
		return false;
	}

	public DropBonusEvent GetEffectiveEvent(EventBonusType eventBonusType)
	{
		List<DropBonusEvent> allEffectiveEvent = GetAllEffectiveEvent();
		for (int i = 0; i < allEffectiveEvent.Count; i++)
		{
			DropBonusEvent dropBonusEvent = allEffectiveEvent[i];
			if (dropBonusEvent.BonusType == eventBonusType)
			{
				return dropBonusEvent;
			}
		}
		return null;
	}

	public static int GetGachaEventConditionPropValueInt(DropBonusEvent info, string Prop)
	{
		if (info == null || !IsGachaEventType(info.BonusType) || String_Extension_Method.IsNullOrEmpty(info.Condition))
		{
			return -1;
		}
		JsonData jsonData = JsonMapper.ToObject(info.Condition);
		if (jsonData == null)
		{
			return -1;
		}
		int num = 0;
		while (true)
		{
			JsonData fromArray = jsonData.GetFromArray(num);
			if (fromArray == null)
			{
				break;
			}
			int iNT = fromArray[Prop].GETINT(-1);
			if (iNT != -1)
			{
				return iNT;
			}
			num++;
		}
		return -1;
	}

	public static List<int> GetEventConditionPropValueInt(DropBonusEvent info, string Prop, List<int> listCache = null)
	{
		if (info == null || String_Extension_Method.IsNullOrEmpty(info.Condition))
		{
			return null;
		}
		JsonData jsonData = JsonMapper.ToObject(info.Condition)[Prop];
		if (jsonData == null)
		{
			return null;
		}
		listCache = ((listCache == null) ? new List<int>() : listCache);
		int num = 0;
		while (true)
		{
			JsonData fromArray = jsonData.GetFromArray(num);
			if (fromArray == null)
			{
				break;
			}
			int iNT = fromArray.GETINT(-1);
			if (iNT != -1)
			{
				listCache.Add(iNT);
			}
			num++;
		}
		if (listCache.Count == 0)
		{
			int iNT2 = jsonData.GETINT(-1);
			if (iNT2 == -1)
			{
				return null;
			}
			listCache.Add(iNT2);
		}
		return listCache;
	}

	public static string GetGachaEventConditionPropValueString(DropBonusEvent info, string Prop)
	{
		if (info == null || !IsGachaEventType(info.BonusType) || String_Extension_Method.IsNullOrEmpty(info.Condition))
		{
			return string.Empty;
		}
		JsonData jsonData = JsonMapper.ToObject(info.Condition);
		if (jsonData == null)
		{
			return string.Empty;
		}
		int num = 0;
		while (true)
		{
			JsonData fromArray = jsonData.GetFromArray(num);
			if (fromArray == null)
			{
				break;
			}
			string sTRING = fromArray[Prop].GETSTRING(string.Empty);
			if (!String_Extension_Method.IsNullOrEmpty(sTRING))
			{
				return sTRING;
			}
			num++;
		}
		return string.Empty;
	}

	public static bool IsEffectiveEvent(DropBonusEvent info)
	{
		if (info == null)
		{
			return false;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		DateTime o_dateTime;
		bool num = Vespa_String_Extension_Method2.TryParseNetworkTime2(info.BeginTime, out o_dateTime);
		DateTime o_dateTime2;
		bool flag = Vespa_String_Extension_Method2.TryParseNetworkTime2(info.EndTime, out o_dateTime2);
		if (!num || !flag)
		{
			return false;
		}
		if (DateTime.Compare(currentServerUTCTime, o_dateTime2) >= 0 || DateTime.Compare(currentServerUTCTime, o_dateTime) <= 0)
		{
			return false;
		}
		DateTime dateTime = NVespa.Math.CreateDateTime(currentServerUTCTime.Year, currentServerUTCTime.Month, currentServerUTCTime.Day, info.BeginHour, info.BeginMinute, 0);
		DateTime t = NVespa.Math.CreateDateTime(currentServerUTCTime.Year, currentServerUTCTime.Month, currentServerUTCTime.Day, info.EndHour, info.EndMinute, 0);
		if (DateTime.Compare(dateTime, t) >= 0)
		{
			return true;
		}
		if (DateTime.Compare(currentServerUTCTime, t) >= 0 || DateTime.Compare(currentServerUTCTime, dateTime) <= 0)
		{
			return false;
		}
		return true;
	}

	public static bool IsEffectiveEvent(DropBonusEvent info, BattleType battleType)
	{
		if (info == null)
		{
			return false;
		}
		if (Array.Find(info.DungeonType, (BattleType e) => e == battleType) == battleType)
		{
			return IsEffectiveEvent(info);
		}
		return false;
	}

	public static DropBonusEventLocalInfo GetLocalInfoOfCurLanguage(DropBonusEvent info)
	{
		if (info == null)
		{
			return null;
		}
		if (info.Title != null)
		{
			DropBonusEventLocalInfo dropBonusEventLocalInfo = new DropBonusEventLocalInfo();
			dropBonusEventLocalInfo.Title = LocalizationManager.LocalizeCode(info.Title);
			if (info.Message != null)
			{
				for (int i = 0; i < info.Message.Length; i++)
				{
					if (i > 0)
					{
						dropBonusEventLocalInfo.Message += "\n";
					}
					dropBonusEventLocalInfo.Message += LocalizationManager.LocalizeCode(info.Message[i]);
				}
			}
			return dropBonusEventLocalInfo;
		}
		if (info.LocalInfo == null)
		{
			return null;
		}
		string text = SingletonAuto<LocalizationManager>.instance.Language.ToString();
		for (int j = 0; j < info.LocalInfo.Length; j++)
		{
			if (info.LocalInfo[j].Language == text)
			{
				return info.LocalInfo[j];
			}
		}
		return null;
	}

	public bool IsEffectiveEvent(EventBonusType eventType, string condition = null)
	{
		return GetEffectiveEvent(eventType, condition) != null;
	}

	public int GetCheckEquipSellEventBonus(ItemData itemData)
	{
		int num = itemData.SellGold;
		if (itemData.Type == ItemType.Equip)
		{
			DropBonusEvent effectiveEvent = GetEffectiveEvent(EventBonusType.BonusEquipSellReward, null);
			if (effectiveEvent != null)
			{
				num *= effectiveEvent.BonusPercentage / 100;
			}
		}
		return num;
	}

	public int GetCheckSellEventBonus(ItemData itemData, EventBonusType eventType)
	{
		int num = itemData.SellGold;
		if (itemData.Type == ItemType.Equip)
		{
			DropBonusEvent effectiveEvent = GetEffectiveEvent(eventType, null);
			if (effectiveEvent != null)
			{
				num *= effectiveEvent.BonusPercentage / 100;
			}
		}
		return num;
	}

	public DropBonusEvent GetEquipBreakEventBonus(string itemCode)
	{
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(itemCode);
		if (dataByCode != null)
		{
			return GetEffectiveEvent(EventBonusType.BonusEquipBreakReward, dataByCode.Index.ToString());
		}
		return null;
	}

	public int GetBreakCountEquipBreakEventBonus(string itemCode, int breakCount)
	{
		DropBonusEvent equipBreakEventBonus = GetEquipBreakEventBonus(itemCode);
		if (equipBreakEventBonus != null)
		{
			return breakCount * equipBreakEventBonus.BonusPercentage / 100;
		}
		return breakCount;
	}

	public DropBonusEvent GetBonusItemEvent(int itemIndex, EventBonusType bonusType, BattleType battleType = BattleType.None)
	{
		DropBonusEvent effectiveEvent = GetEffectiveEvent(bonusType, itemIndex.ToString());
		if (battleType != BattleType.None)
		{
			if (IsEffectiveEvent(effectiveEvent, battleType))
			{
				return effectiveEvent;
			}
			return null;
		}
		return effectiveEvent;
	}

	public DropBonusEvent GetBonusAssignedItemEvent(BattleType battleType)
	{
		List<DropBonusEvent> effectiveEvents = GetEffectiveEvents(EventBonusType.BonusAssignedItem);
		if (effectiveEvents != null)
		{
			for (int i = 0; i < effectiveEvents.Count; i++)
			{
				DropBonusEvent dropBonusEvent = effectiveEvents[i];
				if (Array.Find(dropBonusEvent.DungeonType, (BattleType e) => e == battleType) == battleType)
				{
					return dropBonusEvent;
				}
			}
		}
		return null;
	}

	public long GetBonusAssignedItemCount(long baseCount, int bonusValue)
	{
		if (bonusValue != 0)
		{
			return (long)System.Math.Ceiling((double)checked(baseCount * bonusValue) / 100.0);
		}
		return baseCount;
	}

	public int GetRemainEventTime(EventBonusType eventBonusType, string condition)
	{
		DropBonusEvent effectiveEvent = GetEffectiveEvent(eventBonusType, condition);
		if (effectiveEvent == null)
		{
			return 0;
		}
		int result = 0;
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(effectiveEvent.EndTime, out var o_dateTime))
		{
			DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
			result = Mathf.Max((int)(o_dateTime - currentServerUTCTime).TotalSeconds, 0);
		}
		return result;
	}

	public static T[] GetEventContionValue<T>(DropBonusEvent info, string Prop)
	{
		JsonData eventContionDatas = GetEventContionDatas(info, Prop);
		if (eventContionDatas == null)
		{
			return null;
		}
		T[] array = new T[eventContionDatas.Count];
		array.LoadFromJsonEx(eventContionDatas);
		return array;
	}

	public static JsonData GetEventContionDatas(DropBonusEvent info, string prop)
	{
		if (info == null)
		{
			return null;
		}
		JsonData jsonData = JsonMapper.ToObject(info.Condition);
		if (jsonData == null)
		{
			return null;
		}
		if (!jsonData.IsObject())
		{
			return null;
		}
		return jsonData[prop];
	}

	public static string[] GetEventConditionDatas(string condition, char splitChar = ',')
	{
		if (String_Extension_Method.IsNullOrEmpty(condition))
		{
			return null;
		}
		return condition.Split(splitChar);
	}

	public static List<T> GetEventConditionStringSplit<T>(string condition, char splitChar = ',')
	{
		if (String_Extension_Method.IsNullOrEmpty(condition))
		{
			return null;
		}
		string[] eventConditionDatas = GetEventConditionDatas(condition, splitChar);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(eventConditionDatas))
		{
			return null;
		}
		List<T> list = new List<T>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(eventConditionDatas); i++)
		{
			string text = eventConditionDatas[i];
			if (!String_Extension_Method.IsNullOrEmpty(text))
			{
				T val = BaseJsonMarshaler.To<T>(text);
				if (val != null)
				{
					list.Add(val);
				}
			}
		}
		return list;
	}
}
