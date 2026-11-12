using System.Collections.Generic;
using NGame2.NUI.NManager;
using NShared;
using NVespa;

namespace NGame2.NContents;

public class ContentsNoticePopupManager : SingletonAuto<ContentsNoticePopupManager>
{
	private List<ContentsNoticePopupInfo> _popupInfoList = new List<ContentsNoticePopupInfo>();

	private void Add(ContentsNoticePopupInfo info)
	{
		if (info != null && _popupInfoList != null)
		{
			int num = _popupInfoList.FindIndex((ContentsNoticePopupInfo item) => item.ContentsType == info.ContentsType);
			if (num == -1)
			{
				_popupInfoList.Add(info);
			}
			else
			{
				_popupInfoList[num] = info;
			}
		}
	}

	public void Add(ContentsNoticePopupInfo[] infos)
	{
		if (_popupInfoList != null)
		{
			_popupInfoList.Clear();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
			{
				Add(infos[i]);
			}
			_popupInfoList.Sort((ContentsNoticePopupInfo l, ContentsNoticePopupInfo r) => l.ContentsType.CompareTo(r.ContentsType));
		}
	}

	public List<ContentsNoticePopupInfo> GetAllInfos()
	{
		return _popupInfoList;
	}

	public ContentsNoticePopupInfo GetInfo(ContentsNoticeType contentsType)
	{
		return _popupInfoList.Find((ContentsNoticePopupInfo info) => info.ContentsType == contentsType);
	}

	public int GetPreviousSeasonIndex(ContentsNoticeType contentsType)
	{
		ContentsNoticePopupInfo info = GetInfo(contentsType);
		if (info == null)
		{
			return 0;
		}
		int num = 0;
		num = ((contentsType != ContentsNoticeType.BanPick) ? ((info.SeasonIndex > 0) ? (info.SeasonIndex - 1) : 0) : GetBanPickPreviousRegularSeasonIndex(info.SeasonIndex));
		if (CheckContentsNoticePopupOpen(contentsType, ContentsNoticePopupType.GlobalEnd, num))
		{
			return 0;
		}
		return num;
	}

	private int GetBanPickPreviousRegularSeasonIndex(int currentSeasonIndex)
	{
		int result = 0;
		IList<MatchSeasonData> allData = GameRoot.Instance.MatchBanPickSeasonDataContainer.GetAllData();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(allData))
		{
			for (int num = allData.Count - 1; num >= 0; num--)
			{
				MatchSeasonData matchSeasonData = allData[num];
				if (matchSeasonData != null && matchSeasonData.MatchSeasonType == MatchSeasonType.Regular && matchSeasonData.SeasonIndex < currentSeasonIndex)
				{
					result = matchSeasonData.SeasonIndex;
					break;
				}
			}
		}
		return result;
	}

	public bool CheckContentsNoticePopupOpen(ContentsNoticeType contentsType, ContentsNoticePopupType contentsNoticePopupType, int currentSeasonIndex)
	{
		if (ClientValueManager.IsForceOpenContentsNoticePopup)
		{
			return false;
		}
		return GetContentsNoticePopupOpenSeasonIndex(contentsType, contentsNoticePopupType) == currentSeasonIndex;
	}

	public int GetContentsNoticePopupOpenSeasonIndex(ContentsNoticeType contentsType, ContentsNoticePopupType contentsNoticePopupType)
	{
		return new LocalAccountInt(0, contentsType.ToString() + "_ContentsNoticePopup_" + contentsNoticePopupType).Value;
	}

	public void SetContentsNoticePopupOpenSeasonIndex(ContentsNoticeType contentsType, ContentsNoticePopupType contentsNoticePopupType, int currentSeasonIndex)
	{
		new LocalAccountInt(0, contentsType.ToString() + "_ContentsNoticePopup_" + contentsNoticePopupType).Value = currentSeasonIndex;
	}
}
