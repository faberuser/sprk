using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NContentsInfo;
using NGame2.NLogin;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NManager.NBooster;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class QuestListCompare : Comparer<Tuple<EQuestType, int>>
{
	public override int Compare(Tuple<EQuestType, int> l, Tuple<EQuestType, int> r)
	{
		if (l.Item1 == EQuestType.Achievement && r.Item1 == EQuestType.Achievement)
		{
			AchievementData data = GameRoot.Instance.AchievementDataContainer.GetData(l.Item2, 1);
			AchievementData data2 = GameRoot.Instance.AchievementDataContainer.GetData(r.Item2, 1);
			int num = ((data != null && data.ShowIndex != 0) ? data.ShowIndex : l.Item2);
			int value = ((data2 != null && data2.ShowIndex != 0) ? data2.ShowIndex : r.Item2);
			return num.CompareTo(value);
		}
		if (IsWebTool(l.Item1) && IsWebTool(r.Item1))
		{
			if (l.Item1 == EQuestType.WebContent || r.Item1 == EQuestType.WebContent)
			{
				WebContentInfo info = null;
				WebContentInfo info2 = null;
				WebContentInfo[] webContents = LoginData.WebEvent.WebContents;
				foreach (WebContentInfo webContentInfo in webContents)
				{
					if (l.Item2 == webContentInfo.Index)
					{
						info = webContentInfo;
					}
					if (r.Item2 == webContentInfo.Index)
					{
						info2 = webContentInfo;
					}
				}
				bool flag = IsNewMark(info);
				bool flag2 = IsNewMark(info2);
				if (flag && !flag2)
				{
					return -1;
				}
				if (!flag && flag2)
				{
					return 1;
				}
			}
			int remainTime = GetRemainTime(l.Item1, l.Item2);
			int remainTime2 = GetRemainTime(r.Item1, r.Item2);
			if (remainTime > remainTime2)
			{
				return 1;
			}
			if (remainTime < remainTime2)
			{
				return -1;
			}
		}
		return 0;
	}

	private bool IsNewMark(WebContentInfo Info)
	{
		if (Info == null)
		{
			return false;
		}
		return !SingletonAuto<PlayerPrefsOpenHistoryData>.instance.IsOpened_WebContent(Info.Index, Info.BeginTime);
	}

	private bool IsWebTool(EQuestType type)
	{
		if (type != EQuestType.WebContent && type != EQuestType.Attendance)
		{
			return type == EQuestType.InGameEvent;
		}
		return true;
	}

	private int GetRemainTime(EQuestType type, int index)
	{
		switch (type)
		{
		case EQuestType.WebContent:
		{
			WebContentInfo[] webContents = LoginData.WebEvent.WebContents;
			foreach (WebContentInfo webContentInfo in webContents)
			{
				if (webContentInfo.Index == index)
				{
					return GetRemainTime(webContentInfo);
				}
			}
			break;
		}
		case EQuestType.Attendance:
		{
			if (AttendanceKeywordGroup.Helper.CreateByAnyAttendanceIndex(index).TryGetRemainTimeSpan(out var remainTimeSpan))
			{
				return Mathf.Max((int)remainTimeSpan.TotalSeconds, 0);
			}
			break;
		}
		case EQuestType.InGameEvent:
		{
			List<BoosterInfo> allEffectiveBooster = GlobalAutoBehaviour<BoosterManager>.instance.GetAllEffectiveBooster();
			if (__Extension_Method_ICollection__.COUNT(allEffectiveBooster) <= 0)
			{
				break;
			}
			int num = 0;
			foreach (BoosterInfo item in allEffectiveBooster)
			{
				if (String_Extension_Method.IsNullOrEmpty(item.ItemCode))
				{
					continue;
				}
				if (item.BuffPosition == DropBonusEvnetBuffPosition.None)
				{
					if (NGame2.NUI.Common.IsShowBoosterInfoList(item.BonusType))
					{
						continue;
					}
				}
				else if (item.BuffPosition != DropBonusEvnetBuffPosition.Left)
				{
					continue;
				}
				if (num == index)
				{
					return GetRemainTime(item);
				}
				num++;
			}
			break;
		}
		}
		return 0;
	}

	private int GetRemainTime(WebContentInfo Info)
	{
		Vespa_String_Extension_Method2.TryParseNetworkTime2(Info.EndTime, out var o_dateTime);
		return Mathf.Max((int)(o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds, 0);
	}

	private int GetRemainTime(BoosterInfo Info)
	{
		return Mathf.Max((int)(Info.EndTimeUTC - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds, 0);
	}
}
