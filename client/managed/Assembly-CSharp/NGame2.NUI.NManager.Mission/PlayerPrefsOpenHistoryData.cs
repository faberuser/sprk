using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NManager.Mission;

public class PlayerPrefsOpenHistoryData : SingletonAuto<PlayerPrefsOpenHistoryData>
{
	[Serializable]
	private class OpenHistoryData
	{
		public int Index;

		public string type;

		public string reserved;

		public OpenHistoryData(int Index, string type, string reserved)
		{
			this.Index = Index;
			this.type = $"{type}_{GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id}";
			this.reserved = reserved;
		}

		public OpenHistoryData(OpenHistoryData other)
		{
			Index = other.Index;
			type = other.type;
			reserved = other.reserved;
		}

		public bool Equals(OpenHistoryData other)
		{
			if (Index == other.Index && type == other.type && reserved == other.reserved)
			{
				return true;
			}
			return false;
		}
	}

	[Serializable]
	private class OpenHistoryList
	{
		public List<OpenHistoryData> list;
	}

	private struct MissionNewMarkKey
	{
		public class Comparer : IEqualityComparer<MissionNewMarkKey>
		{
			public static Comparer Default = new Comparer();

			public bool Equals(MissionNewMarkKey x, MissionNewMarkKey y)
			{
				if (x.MainCategory == y.MainCategory)
				{
					return x.SubCategory == y.SubCategory;
				}
				return false;
			}

			public int GetHashCode(MissionNewMarkKey obj)
			{
				int num = 7199369;
				return obj.MainCategory * num + obj.SubCategory;
			}
		}

		public int MainCategory;

		public int SubCategory;
	}

	private struct WebContentKey
	{
		public class Comparer : IEqualityComparer<WebContentKey>
		{
			public static Comparer Default = new Comparer();

			public bool Equals(WebContentKey x, WebContentKey y)
			{
				if (x.ContentIndex == y.ContentIndex)
				{
					return x.BeginTime == y.BeginTime;
				}
				return false;
			}

			public int GetHashCode(WebContentKey obj)
			{
				int num = 7199369;
				return obj.ContentIndex * num + String_Extension_Method.EmptyIfNull(obj.BeginTime).GetHashCode();
			}
		}

		public int ContentIndex;

		public string BeginTime;
	}

	public static readonly string OldPlayerPrefsKey = typeof(MissionManagement).FullName + "OpenHistoryIndex";

	private Dictionary<MissionNewMarkKey, LocalStorageBool> _newMarkCache = new Dictionary<MissionNewMarkKey, LocalStorageBool>(MissionNewMarkKey.Comparer.Default);

	private Dictionary<WebContentKey, LocalStorageBool> _webContentCache = new Dictionary<WebContentKey, LocalStorageBool>(WebContentKey.Comparer.Default);

	public PlayerPrefsOpenHistoryData()
	{
		LoadOld();
	}

	private long GetAccountId()
	{
		if (AccountManager.IsLogin())
		{
			return GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
		}
		return 0L;
	}

	private string GetPlayerPrefsKey_NewMark(int mainCategoryIndex, int subCategoryIndex)
	{
		return $"Mission_OpenHistory_NewMark_{mainCategoryIndex}_{subCategoryIndex}";
	}

	private string GetPlayerPrefsKey_WebContent(int index, string beginTime)
	{
		return $"Mission_OpenHistory_WebContent_{index}_{beginTime}";
	}

	public bool IsOpened_NewMark(int mainCategoryIndex, int subCategoryIndex)
	{
		long accountId = GetAccountId();
		return SprkMissionHistory.Read(GetLocalStorage_NewMark(accountId, mainCategoryIndex, subCategoryIndex).Value, mainCategoryIndex, subCategoryIndex);
	}

	public void SetOpen_NewMark(int mainCategoryIndex, int subCategoryIndex)
	{
		long accountId = GetAccountId();
		GetLocalStorage_NewMark(accountId, mainCategoryIndex, subCategoryIndex).Value = true;
		SprkMissionHistory.Mark(mainCategoryIndex, subCategoryIndex);
	}

	public bool IsOpened_WebContent(int contentIndex, string beginTime)
	{
		long accountId = GetAccountId();
		return GetLocalStorage_WebContent(accountId, contentIndex, beginTime).Value;
	}

	public void SetOpen_WebContent(int contentIndex, string beginTime)
	{
		long accountId = GetAccountId();
		GetLocalStorage_WebContent(accountId, contentIndex, beginTime).Value = true;
	}

	private LocalStorageBool GetLocalStorage_NewMark(long accountId, int mainCategoryIndex, int subCategoryIndex)
	{
		MissionNewMarkKey key = new MissionNewMarkKey
		{
			MainCategory = mainCategoryIndex,
			SubCategory = subCategoryIndex
		};
		if (!_newMarkCache.TryGetValue(key, out var value))
		{
			string playerPrefsKey_NewMark = GetPlayerPrefsKey_NewMark(mainCategoryIndex, subCategoryIndex);
			string i_key = $"{accountId}_{playerPrefsKey_NewMark}";
			value = new LocalStorageBool(i_default: false, i_key);
			_newMarkCache.Add(key, value);
		}
		return value;
	}

	private LocalStorageBool GetLocalStorage_WebContent(long accountId, int contentIndex, string beginTime)
	{
		WebContentKey key = new WebContentKey
		{
			ContentIndex = contentIndex,
			BeginTime = beginTime
		};
		if (!_webContentCache.TryGetValue(key, out var value))
		{
			string playerPrefsKey_WebContent = GetPlayerPrefsKey_WebContent(contentIndex, beginTime);
			string i_key = $"{accountId}_{playerPrefsKey_WebContent}";
			value = new LocalStorageBool(i_default: false, i_key);
			_webContentCache.Add(key, value);
		}
		return value;
	}

	private void LoadOld()
	{
		string text = "CateogyNewMark_";
		string text2 = "WebContentInfo_";
		if (!SprkTestPrefs.HasKey(OldPlayerPrefsKey))
		{
			return;
		}
		OpenHistoryList openHistoryList = JsonUtility.FromJson<OpenHistoryList>(SprkTestPrefs.GetString(OldPlayerPrefsKey));
		if (openHistoryList == null)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(openHistoryList.list); i++)
		{
			OpenHistoryData openHistoryData = openHistoryList.list[i];
			if (String_Extension_Method.IsNullOrEmpty(openHistoryData.type))
			{
				continue;
			}
			long result3;
			if (openHistoryData.type.StartsWith(text))
			{
				if (long.TryParse(openHistoryData.type.Substring(text.Length), out var result) && int.TryParse(openHistoryData.reserved, out var result2))
				{
					int index = openHistoryData.Index;
					GetLocalStorage_NewMark(result, result2, index).Value = true;
				}
			}
			else if (openHistoryData.type.StartsWith(text2) && long.TryParse(openHistoryData.type.Substring(text2.Length), out result3))
			{
				GetLocalStorage_WebContent(result3, openHistoryData.Index, openHistoryData.reserved).Value = true;
			}
		}
		SprkTestPrefs.DeleteKey(OldPlayerPrefsKey);
	}
}
