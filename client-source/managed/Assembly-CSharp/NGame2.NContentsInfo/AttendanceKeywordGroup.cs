using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI3.NAttendance;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NContentsInfo;

public struct AttendanceKeywordGroup
{
	public static class Helper
	{
		public static EAttendanceKeyword GetKeywordByIndex(int attendanceIndex)
		{
			internal_Helper.VerifyAttendanceKeyword((EAttendanceKeyword)attendanceIndex, attendanceIndex);
			return (EAttendanceKeyword)attendanceIndex;
		}

		public static int GetIndexByKeyword(EAttendanceKeyword keyword)
		{
			internal_Helper.VerifyAttendanceKeyword(keyword, (int)keyword);
			return (int)keyword;
		}

		public static bool IsSubAttendanceIndex(int attendanceIndex)
		{
			return IsSubKeyword((EAttendanceKeyword)attendanceIndex);
		}

		public static bool IsSubKeyword(EAttendanceKeyword keyword)
		{
			return _map_SubKeywordToMainKeyword.ContainsKey(keyword);
		}

		public static AttendanceKeywordGroup CreateByAnyAttendanceIndex(int attendanceIndex)
		{
			return CreateByAnyKeyword((EAttendanceKeyword)attendanceIndex);
		}

		public static AttendanceKeywordGroup CreateByAnyKeyword(EAttendanceKeyword keyword)
		{
			return new AttendanceKeywordGroup(Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_map_SubKeywordToMainKeyword, keyword, keyword));
		}

		public static int GetAttendanceRewardEndDay(EAttendanceKeyword keyword)
		{
			AttendanceData dataByKeyword = SingletonAuto<AttendanceDataContainer>.instance.GetDataByKeyword(keyword);
			if (dataByKeyword == null)
			{
				return 0;
			}
			if (__Extension_Method_ICollection__.IsNullOrEmpty(dataByKeyword.Reward))
			{
				return 0;
			}
			int num = dataByKeyword.Reward.Length;
			if (Is_Correct_Rewards_by_Day(dataByKeyword.Reward[num - 1], num))
			{
				return num;
			}
			num = 0;
			AttendanceRewards[] reward = dataByKeyword.Reward;
			foreach (AttendanceRewards attendanceRewards in reward)
			{
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(attendanceRewards.Reward))
				{
					num = Mathf.Max(num, attendanceRewards.Reward[0].Day);
				}
			}
			return num;
		}

		public static IList<AttendanceReward> GetAttendanceRewards(EAttendanceKeyword keyword, int day)
		{
			AttendanceData dataByKeyword = SingletonAuto<AttendanceDataContainer>.instance.GetDataByKeyword(keyword);
			if (dataByKeyword == null)
			{
				return null;
			}
			if (__Extension_Method_ICollection__.IsNullOrEmpty(dataByKeyword.Reward))
			{
				return null;
			}
			if (day < 1)
			{
				return null;
			}
			int num = day - 1;
			if (num < dataByKeyword.Reward.Length)
			{
				AttendanceRewards attendanceRewards = dataByKeyword.Reward[num];
				if (Is_Correct_Rewards_by_Day(attendanceRewards, day))
				{
					return attendanceRewards.Reward;
				}
			}
			AttendanceRewards[] reward = dataByKeyword.Reward;
			foreach (AttendanceRewards attendanceRewards2 in reward)
			{
				if (Is_Correct_Rewards_by_Day(attendanceRewards2, day))
				{
					return attendanceRewards2.Reward;
				}
			}
			return null;
		}

		private static bool Is_Correct_Rewards_by_Day(AttendanceRewards attendanceRewards, int day)
		{
			if (attendanceRewards == null)
			{
				return false;
			}
			if (attendanceRewards.Reward == null)
			{
				return false;
			}
			if (attendanceRewards.Reward.Length == 0)
			{
				return false;
			}
			return attendanceRewards.Reward[0].Day == day;
		}
	}

	private static class internal_Helper
	{
		public static AttendanceInfo GetAttendanceInfo(EAttendanceKeyword keyword)
		{
			if (!AccountManager.IsLogined())
			{
				return null;
			}
			return GlobalAutoBehaviour<AccountManager>.instance.AttendanceManager.GetInfoByKeyword(keyword);
		}

		public static AttendanceInfoData GetAttendanceInfoData(EAttendanceKeyword keyword)
		{
			return GameRoot.Instance.AttendanceInfoDataContainer.GetData((int)keyword);
		}

		public static int GetRewardableDays(EAttendanceKeyword keyword)
		{
			return SingletonAuto<AttendanceDataContainer>.instance.GetDataByKeyword(keyword)?.AttendanceRewardableDays ?? 0;
		}

		public static int GetCompletedDay(EAttendanceKeyword keyword)
		{
			return GetAttendanceInfo(keyword)?.CompletedDay ?? 0;
		}

		public static void VerifyAttendanceKeyword(EAttendanceKeyword keyword, int attendanceIndex)
		{
		}

		public static bool CanDo_ReservePopup(EAttendanceKeyword keyword)
		{
			AttendanceData dataByKeyword = SingletonAuto<AttendanceDataContainer>.instance.GetDataByKeyword(keyword);
			if (dataByKeyword == null)
			{
				return false;
			}
			if (dataByKeyword.AttendanceRewardableDays == 0)
			{
				return false;
			}
			if (dataByKeyword.ManualStart > 0)
			{
				AttendanceInfo attendanceInfo = GetAttendanceInfo(keyword);
				if (attendanceInfo == null)
				{
					return false;
				}
				if (attendanceInfo.CompletedDay >= __Extension_Method_ICollection__.COUNT(dataByKeyword.Reward))
				{
					return false;
				}
			}
			return true;
		}
	}

	private static Dictionary<EAttendanceKeyword, EAttendanceKeyword> _map_SubKeywordToMainKeyword;

	private static Dictionary<EAttendanceKeyword, EAttendanceKeyword> _map_MainKeywordToSubKeyword;

	public EAttendanceKeyword KeywordMain { get; private set; }

	public EAttendanceKeyword KeywordSub { get; private set; }

	public int IndexMain => (int)KeywordMain;

	public int IndexSub => (int)KeywordSub;

	public AttendanceData AttendanceData => SingletonAuto<AttendanceDataContainer>.instance.GetDataByKeyword(KeywordMain);

	public AttendanceInfo AttendanceInfo => internal_Helper.GetAttendanceInfo(KeywordMain);

	public AttendanceInfoData AttendanceInfoData => internal_Helper.GetAttendanceInfoData(KeywordMain);

	public int RewardableDays => internal_Helper.GetRewardableDays(KeywordMain);

	public int CompletedDay => internal_Helper.GetCompletedDay(KeywordMain);

	public AttendanceData SubAttendanceData => SingletonAuto<AttendanceDataContainer>.instance.GetDataByKeyword(KeywordSub);

	public AttendanceInfo SubAttendanceInfo => internal_Helper.GetAttendanceInfo(KeywordSub);

	public AttendanceInfoData SubAttendanceInfoData => internal_Helper.GetAttendanceInfoData(KeywordSub);

	public int ShowIndex
	{
		get
		{
			int num = GameRoot.Instance.AttendanceInfoDataContainer.GetData(IndexMain)?.ShowIndex ?? 0;
			return (num > 0) ? num : (10000000 + IndexMain);
		}
	}

	public override string ToString()
	{
		return KeywordMain.ToString();
	}

	public AttendanceKeywordGroup(EAttendanceKeyword keywordMain)
	{
		KeywordMain = keywordMain;
		KeywordSub = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_map_MainKeywordToSubKeyword, KeywordMain, EAttendanceKeyword.None);
		internal_Helper.VerifyAttendanceKeyword(keywordMain, (int)keywordMain);
	}

	public AttendanceKeywordGroup(int index_MainAttendance)
	{
		KeywordMain = Helper.GetKeywordByIndex(index_MainAttendance);
		KeywordSub = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_map_MainKeywordToSubKeyword, KeywordMain, EAttendanceKeyword.None);
		internal_Helper.VerifyAttendanceKeyword((EAttendanceKeyword)index_MainAttendance, index_MainAttendance);
	}

	public bool IsValid_AttendanceInfo()
	{
		if (AttendanceData == null || AttendanceInfo == null || AttendanceInfoData == null)
		{
			return false;
		}
		return true;
	}

	public bool IsValid_AttendanceData()
	{
		if (AttendanceData == null || AttendanceInfoData == null)
		{
			return false;
		}
		return true;
	}

	public bool IsAny_AttendanceIndex(int attendanceIndex)
	{
		if (IndexMain != attendanceIndex)
		{
			return IndexSub == attendanceIndex;
		}
		return true;
	}

	public AttendanceData[] GetAllAttendanceDatas()
	{
		AttendanceData attendanceData = AttendanceData;
		AttendanceData dataByKeyword = SingletonAuto<AttendanceDataContainer>.instance.GetDataByKeyword(KeywordSub);
		if (attendanceData != null && dataByKeyword != null)
		{
			return new AttendanceData[2] { attendanceData, dataByKeyword };
		}
		if (attendanceData != null && dataByKeyword == null)
		{
			return new AttendanceData[1] { attendanceData };
		}
		return new AttendanceData[0];
	}

	public bool CanDo_ReservePopup()
	{
		if (internal_Helper.CanDo_ReservePopup(KeywordMain) || internal_Helper.CanDo_ReservePopup(KeywordSub))
		{
			return true;
		}
		return false;
	}

	public bool TryGetRemainTimeSpan(out TimeSpan remainTimeSpan)
	{
		AttendanceInfo attendanceInfo = AttendanceInfo;
		if (attendanceInfo == null)
		{
			remainTimeSpan = TimeSpan.Zero;
			return false;
		}
		if (!attendanceInfo.TryGetEndTime_ServerLocal(out var endTime_Local))
		{
			remainTimeSpan = default(TimeSpan);
			return false;
		}
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		remainTimeSpan = endTime_Local - currentServerLocalTime;
		return true;
	}

	public string GetTitleString()
	{
		AttendanceInfoData data = GameRoot.Instance.AttendanceInfoDataContainer.GetData(IndexMain);
		if (data == null)
		{
			return string.Empty;
		}
		int currentPage = GetCurrentPage();
		string text = data.GetMenuTitleKey(currentPage);
		if (String_Extension_Method.IsNullOrEmpty(text))
		{
			text = data.TitleKey;
		}
		return LocalizationManager.GetStringDirect(text);
	}

	public int GetCurrentPage()
	{
		AttendanceData attendanceData = AttendanceData;
		if (attendanceData == null)
		{
			return 0;
		}
		if (attendanceData.PageSize == 0)
		{
			return 0;
		}
		AttendanceInfo attendanceInfo = AttendanceInfo;
		if (attendanceInfo == null)
		{
			return 0;
		}
		int num = ((attendanceData.AttendanceRewardableDays > 0) ? attendanceData.AttendanceRewardableDays : attendanceInfo.CompletedDay);
		return System.Math.Max(0, (num - 1) / attendanceData.PageSize);
	}

	public bool IsPurchasable_v1()
	{
		AttendanceData attendanceData = AttendanceData;
		AttendanceInfoData attendanceInfoData = AttendanceInfoData;
		if (attendanceData == null || attendanceInfoData == null)
		{
			return false;
		}
		AttendanceInfo attendanceInfo = AttendanceInfo;
		bool num = attendanceInfoData.IsShowIcon(AttendanceInfoData.AttendanceIconStatus.Sell);
		bool flag = AttendanceHelper.IsPayShopProductSelling(IndexMain);
		bool flag2 = AttendanceHelper.IsAttendanceProgress(attendanceInfo, attendanceData);
		if (num && flag)
		{
			return !flag2;
		}
		return false;
	}

	public bool IsInProgress_v1()
	{
		AttendanceData attendanceData = AttendanceData;
		AttendanceInfoData attendanceInfoData = AttendanceInfoData;
		if (attendanceData == null)
		{
			return false;
		}
		if (attendanceInfoData == null)
		{
			return false;
		}
		if (!attendanceInfoData.IsShowIcon(AttendanceInfoData.AttendanceIconStatus.Progress))
		{
			return false;
		}
		if (!AttendanceHelper.IsAttendanceProgress(AttendanceInfo, attendanceData))
		{
			return false;
		}
		return true;
	}

	static AttendanceKeywordGroup()
	{
		_map_SubKeywordToMainKeyword = new Dictionary<EAttendanceKeyword, EAttendanceKeyword> { 
		{
			EAttendanceKeyword.BonusAttendance20221001_2,
			EAttendanceKeyword.BonusAttendance20221001_1
		} };
		_map_MainKeywordToSubKeyword = new Dictionary<EAttendanceKeyword, EAttendanceKeyword>();
		foreach (KeyValuePair<EAttendanceKeyword, EAttendanceKeyword> item in _map_SubKeywordToMainKeyword)
		{
			_map_MainKeywordToSubKeyword.Add(item.Value, item.Key);
		}
	}
}
