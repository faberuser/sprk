using System;
using NGame2.NAccount;
using NGame2.NUI.NHelper.NPayShop;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GachaSelectShopMenu : BaseWindow, IUserObserver<SelectShopNoticeInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserRightAwayProductManager.ChangedRightAwayProductInfo>
{
	public class IntervalUpdater
	{
		private float _interval = 1f;

		private DateTime _lastUpdateTime;

		private Action _updateAction;

		public IntervalUpdater(Action updateAction)
		{
			_updateAction = updateAction;
		}

		public void SetInterval(float interval)
		{
			_interval = interval;
		}

		public void Update()
		{
			if (_updateAction != null)
			{
				DateTime currentTime = GetCurrentTime();
				if (!((currentTime - _lastUpdateTime).TotalSeconds < (double)_interval))
				{
					_lastUpdateTime = currentTime;
					Vespa_System_Action_Extension_Method.fire(_updateAction);
				}
			}
		}

		private DateTime GetCurrentTime()
		{
			return DateTime.Now;
		}
	}

	public interface IListener
	{
		void OnClick();
	}

	public enum EPositionType
	{
		None,
		Top,
		Bottom
	}

	[Serializable]
	public class PositionInfo
	{
		public EPositionType PositionType;

		public GameObject Root;

		public UILabel LabelRemainTime;
	}

	public const string PrefabPath = "Resources/Prefab2/Lobby/GachaSelectShopMenu";

	public PositionInfo[] PositionInfos;

	private PositionInfo _currentPositionInfo;

	private SelectShopNoticeInfo _selectNoticeInfo;

	private IntervalUpdater _intervalUpdater;

	private EPositionType _currentPositionType;

	private IListener _listener;

	public void Open(IListener listener)
	{
		_listener = listener;
		Open(delegate
		{
			AddObserver();
			Init();
		}, delegate
		{
		});
	}

	public override void OnClosing()
	{
		base.OnClosing();
		RemoveObserver();
		_selectNoticeInfo = null;
		_intervalUpdater = null;
		_listener = null;
		_currentPositionType = EPositionType.None;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_intervalUpdater != null)
		{
			_intervalUpdater.Update();
		}
	}

	private void Init()
	{
		_selectNoticeInfo = GetSelectShopNoticeInfo();
		int remainSec = GetRemainSec(_selectNoticeInfo);
		bool isValidSelectShopNoticeInfo = remainSec > 0;
		_currentPositionType = GetPositionType(isValidSelectShopNoticeInfo);
		SetPositionInfo(_currentPositionType);
		SetRemainTimeLabel(remainSec);
		SetIntervalUpdate();
	}

	private void SetIntervalUpdate()
	{
		if (_intervalUpdater == null)
		{
			_intervalUpdater = new IntervalUpdater(UpdateRemainTimeLabel);
		}
	}

	private void UpdateRemainTimeLabel()
	{
		int remainSec = GetRemainSec(_selectNoticeInfo);
		SetRemainTimeLabel(remainSec);
		if (remainSec <= 0)
		{
			Init();
		}
	}

	private void SetRemainTimeLabel(int remainSec)
	{
		GachaSelctShopHelper.SetRemainTimeLabel(GetRemainTimeLabel(_currentPositionInfo), remainSec, "RIGHTAWAY_SECOND_PRODUCT_REMAINING_TIME_TEXT");
	}

	private UILabel GetRemainTimeLabel(PositionInfo positionInfo)
	{
		return positionInfo?.LabelRemainTime;
	}

	private void SetPositionInfo(EPositionType ePositionType)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PositionInfos); i++)
		{
			PositionInfo positionInfo = PositionInfos[i];
			if (positionInfo != null && !(positionInfo.Root == null))
			{
				bool flag = ePositionType == positionInfo.PositionType;
				UIHelper.EnableGameObject(positionInfo.Root, flag);
				if (flag)
				{
					_currentPositionInfo = positionInfo;
				}
			}
		}
	}

	public bool AnyRightAway()
	{
		if (!AccountManager.IsLogin())
		{
			return false;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager.Any();
	}

	public SelectShopNoticeInfo GetSelectShopNoticeInfo()
	{
		if (!AccountManager.IsLogin())
		{
			return null;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.GachaSelectShopManager.GetSelectShopNoticeInfo();
	}

	public EPositionType GetPositionType(bool isValidSelectShopNoticeInfo)
	{
		if (!isValidSelectShopNoticeInfo)
		{
			return EPositionType.None;
		}
		if (AnyRightAway())
		{
			return EPositionType.Bottom;
		}
		return EPositionType.Top;
	}

	private int GetRemainSec(SelectShopNoticeInfo selectShopNoticeInfo)
	{
		return GachaSelctShopHelper.GetRemainTimeSec(selectShopNoticeInfo);
	}

	public void OnClickMenu()
	{
		if (_listener != null)
		{
			_listener.OnClick();
		}
	}

	private void AddObserver()
	{
		if (AccountManager.IsLogin())
		{
			GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager.ObserverManager.AddObserver(this);
			GlobalAutoBehaviour<AccountManager>.instance.GachaSelectShopManager.AddSelectShopNoticeInoObserver(this);
		}
	}

	private void RemoveObserver()
	{
		if (AccountManager.IsLogin())
		{
			GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager.ObserverManager.RemoveObserver(this);
			GlobalAutoBehaviour<AccountManager>.instance.GachaSelectShopManager.RemoveSelectShopNoticeInfoObserver(this);
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserRightAwayProductManager.ChangedRightAwayProductInfo>.OnChanged(NGame2.NAccount.UserRightAwayProductManager.ChangedRightAwayProductInfo changedInfo)
	{
		if (changedInfo != null)
		{
			Init();
		}
	}

	void IUserObserver<SelectShopNoticeInfo>.OnChanged(SelectShopNoticeInfo changedInfo)
	{
		Init();
	}
}
