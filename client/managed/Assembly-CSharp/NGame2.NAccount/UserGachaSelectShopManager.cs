using System;
using NGame2.NUI.NHelper.NPayShop;
using NShared;

namespace NGame2.NAccount;

public class UserGachaSelectShopManager : NShared.UserGachaSelectShopManager
{
	public enum EChangedType
	{
		None,
		New,
		Override,
		Remove
	}

	private bool _isEditor;

	private readonly UserObserverManager<SelectShopNoticeInfo> _selectShopNoticeInfoObserverManager = new UserObserverManager<SelectShopNoticeInfo>();

	protected SelectShopNoticeInfo _selectShopNoticeInfo;

	public void ApplySelectShopNoticeInfo(SelectShopNoticeInfo[] infos)
	{
		if (!GachaSelctShopHelper.UseEditor)
		{
			SelectShopNoticeInfo firstOrDefault = GetFirstOrDefault(infos);
			if (GetNoticeInfoChagnedType(firstOrDefault) != EChangedType.None)
			{
				_selectShopNoticeInfo = firstOrDefault;
				AnnounceToObserver(firstOrDefault);
			}
		}
	}

	public SelectShopNoticeInfo GetSelectShopNoticeInfo()
	{
		return _selectShopNoticeInfo;
	}

	public bool HaveGachaSelectShopNoticeInfo()
	{
		return _selectShopNoticeInfo != null;
	}

	private SelectShopNoticeInfo GetFirstOrDefault(SelectShopNoticeInfo[] infos)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(infos))
		{
			return null;
		}
		return infos[0];
	}

	private EChangedType GetNoticeInfoChagnedType(SelectShopNoticeInfo info)
	{
		if (_selectShopNoticeInfo == null && info == null)
		{
			return EChangedType.None;
		}
		if (_selectShopNoticeInfo != null && info == null)
		{
			return EChangedType.Remove;
		}
		if (_selectShopNoticeInfo == null && info != null)
		{
			return EChangedType.New;
		}
		if (GetEqualsSelectShopNoticeInfo(_selectShopNoticeInfo, info))
		{
			return EChangedType.None;
		}
		return EChangedType.Override;
	}

	public static bool GetEqualsSelectShopNoticeInfo(SelectShopNoticeInfo lhs, SelectShopNoticeInfo rhs)
	{
		if (lhs.EndTime != rhs.EndTime)
		{
			return false;
		}
		if (lhs.OpenCategory != rhs.OpenCategory)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(lhs.CategoryGroups) != __Extension_Method_ICollection__.COUNT(rhs.CategoryGroups))
		{
			return false;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(lhs.CategoryGroups); i++)
		{
			if (lhs.CategoryGroups[i] != rhs.CategoryGroups[i])
			{
				return false;
			}
		}
		return true;
	}

	public void AddSelectShopNoticeInoObserver(IUserObserver observer)
	{
		_selectShopNoticeInfoObserverManager.RemoveObserver(observer);
		_selectShopNoticeInfoObserverManager.AddObserver(observer);
	}

	public void RemoveSelectShopNoticeInfoObserver(IUserObserver observer)
	{
		_selectShopNoticeInfoObserverManager.RemoveObserver(observer);
	}

	private void AnnounceToObserver(SelectShopNoticeInfo info)
	{
		_selectShopNoticeInfoObserverManager.AnnounceToObservers(info);
	}

	public void SetCustomNoticeInfo(DateTime endTime, string[] group)
	{
		SelectShopNoticeInfo selectShopNoticeInfo = new SelectShopNoticeInfo
		{
			OpenCategory = group[0],
			CategoryGroups = group,
			EndTime = Vespa_DateTime_Extension_Method.ToNetworkString2(endTime)
		};
		_selectShopNoticeInfo = selectShopNoticeInfo;
		AnnounceToObserver(_selectShopNoticeInfo);
	}

	public void ClearSelectShopNoticeInfo()
	{
		_selectShopNoticeInfo = null;
		AnnounceToObserver(_selectShopNoticeInfo);
	}

	public string GetCategoryGroup(int index)
	{
		if (__Extension_Method_ICollection__.COUNT(_selectShopNoticeInfo.CategoryGroups) <= index)
		{
			return string.Empty;
		}
		return _selectShopNoticeInfo.CategoryGroups[index];
	}
}
