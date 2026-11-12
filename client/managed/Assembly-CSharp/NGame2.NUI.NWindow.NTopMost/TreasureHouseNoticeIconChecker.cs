using System;
using NGame2.NAccount;
using NGame2.NContents;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NTopMost;

public class TreasureHouseNoticeIconChecker : INoticeIconChcker
{
	private TreasureHouseNoticeComponent _comp;

	private bool _compActiveSelf;

	private bool _contentsStatus;

	private DateTime _resetTime;

	private TreasureHouseDungeonData _cachedDungonData;

	private CurrencyTypeData _cachedCurrencyTypeData;

	private long _maxPoint;

	private long _enterCount;

	void INoticeIconChcker.Init(IGrid grid)
	{
		if (grid != null)
		{
			_comp = UIHelper.AddGridItem(grid, TreasureHouseNoticeComponent.PrefabAuto);
			if (!(_comp == null))
			{
				_compActiveSelf = _comp.gameObject.activeSelf;
				SetContentsStatus();
			}
		}
	}

	bool INoticeIconChcker.Check(bool show)
	{
		show = show && _contentsStatus;
		if (show)
		{
			bool show2 = CanShow();
			return SetShow(show2);
		}
		return SetShow(show: false);
	}

	private bool CanShow()
	{
		if (!AccountManager.IsLogin())
		{
			return false;
		}
		PlayerTreasureHouseInfo treasureHouseInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TreasureHouseInfo;
		if (treasureHouseInfo == null)
		{
			return false;
		}
		int clearCount = treasureHouseInfo.ClearCount;
		if (_cachedDungonData == null || _cachedDungonData.Index != treasureHouseInfo.Index)
		{
			_cachedDungonData = GameRoot.Instance.TreasureHouseDungeonDataContainer.GetData(treasureHouseInfo.Index);
			if (_cachedDungonData == null)
			{
				return false;
			}
			TreasureHouseInfoData data = GameRoot.Instance.TreasureHouseInfoDataContainer.GetData(_cachedDungonData.InfoIndex);
			if (data == null)
			{
				return false;
			}
			_enterCount = data.EnterCount;
		}
		if (_enterCount <= clearCount)
		{
			return false;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(treasureHouseInfo.NextResetTime, out _resetTime))
		{
			return false;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if (_resetTime.CompareTo(currentServerUTCTime) < 0)
		{
			return false;
		}
		if (_cachedCurrencyTypeData == null)
		{
			_cachedCurrencyTypeData = GameRoot.Instance.CurrencyTypeDataContainer.GetCurrencyData(CurrencyType.TreasureHousePoint);
			if (_cachedCurrencyTypeData != null)
			{
				_maxPoint = _cachedCurrencyTypeData.MaxValue;
			}
		}
		long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.TreasureHousePoint);
		return _maxPoint <= userStat;
	}

	private bool SetShow(bool show)
	{
		if (_compActiveSelf != show)
		{
			_compActiveSelf = show;
			UIHelper.EnableGameObject(_comp, show);
			return true;
		}
		return false;
	}

	void INoticeIconChcker.Close()
	{
		FactoryBase.SafeReleaseComponent(ref _comp);
		_cachedDungonData = null;
		_cachedCurrencyTypeData = null;
	}

	void INoticeIconChcker.CheckContentsStatus()
	{
		SetContentsStatus();
	}

	private void SetContentsStatus()
	{
		_contentsStatus = SingletonAuto<ContentsStatusManager>.instance.GetContentsStatus(ContentsStatusType.TreasureHouse) == ContentsStatus.On;
	}
}
