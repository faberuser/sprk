using System;
using System.Collections.Generic;
using NShared;

namespace NGame2.NAccount;

public class UserUnderPrisonManager : NShared.UserUnderPrisonManager
{
	private Dictionary<UnderPrisonType, int> _newDic = new Dictionary<UnderPrisonType, int>();

	public int SelectedDiff { get; set; }

	public void SetDiff(int chapterIndex, int Diff)
	{
		UnderPrisonInfo underPrisonInfo = Get(chapterIndex);
		if (underPrisonInfo != null)
		{
			underPrisonInfo.Diff = Diff;
		}
	}

	public void Set(UnderPrisonInfo info)
	{
		Remove(info.ChapterIndex);
		Add(info);
	}

	public void InitNew()
	{
		_newDic.Clear();
		UnderPrisonData underPrisonData = null;
		foreach (UnderPrisonType value in Enum.GetValues(typeof(UnderPrisonType)))
		{
			underPrisonData = GameRoot.Instance.UnderPrisonDataContainer.GetUnderPrisonDataByUnderPrisonType(value);
			if (underPrisonData != null)
			{
				_newDic.Add(value, GetOpenDiff(underPrisonData.ChapterIndex));
			}
		}
	}

	public bool CheckIsNewDiff(UnderPrisonType type, int diff)
	{
		if (!_newDic.ContainsKey(type))
		{
			return false;
		}
		UnderPrisonData underPrisonDataByUnderPrisonType = GameRoot.Instance.UnderPrisonDataContainer.GetUnderPrisonDataByUnderPrisonType(type);
		if (underPrisonDataByUnderPrisonType == null)
		{
			return false;
		}
		if (diff < GetOpenDiff(underPrisonDataByUnderPrisonType.ChapterIndex))
		{
			return false;
		}
		if (_newDic[type] < diff && GetOpenDiff(underPrisonDataByUnderPrisonType.ChapterIndex) >= diff)
		{
			return true;
		}
		return false;
	}

	public void SetIsNewState(UnderPrisonType type, int diff)
	{
		if (_newDic.ContainsKey(type))
		{
			_newDic[type] = diff;
		}
	}

	public int GetOpenDiff(int chapterIndex)
	{
		UnderPrisonInfo underPrisonInfo = Get(chapterIndex);
		if (underPrisonInfo == null)
		{
			return 0;
		}
		return underPrisonInfo.Diff + 1;
	}

	public int GetRemainEnterCountByChapterIndex(int chapterIndex)
	{
		UnderPrisonInfo underPrisonInfo = Get(chapterIndex);
		if (underPrisonInfo == null)
		{
			return 0;
		}
		int num = underPrisonInfo.MaxTryCount - underPrisonInfo.CompletedCount;
		if (num <= 0)
		{
			return 0;
		}
		return num;
	}
}
