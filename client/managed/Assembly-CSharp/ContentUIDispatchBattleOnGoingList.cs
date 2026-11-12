using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDispatchBattle;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NWrappedInfo;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

public class ContentUIDispatchBattleOnGoingList : BaseWindow, IUserObserver<UserDispatchBattleInfoManager.Observer.BroadCastInfo>, IUserObserver, ContentUIDispatchBattleOnGoingComponent.IListener
{
	public interface IListener
	{
		void EnableList();
	}

	public const string PATH_PREFAB = "Resources/Prefab2/Window/DispatchBattle/ContentUIDispatchBattleOnGoingList";

	public UIScrollGrid GridScrollOnGoing;

	private PrefabAuto<ContentUIDispatchBattleOnGoingComponent> _prefabAutoComp = new PrefabAuto<ContentUIDispatchBattleOnGoingComponent>("Resources/Prefab2/Window/DispatchBattle/ContentUIDispatchBattleOnGoingComponent");

	private List<WCL_DispatchBattleInfo> _dataList = new List<WCL_DispatchBattleInfo>();

	private List<ContentUIDispatchBattleOnGoingComponent> _compList = new List<ContentUIDispatchBattleOnGoingComponent>();

	private IListener _iListener;

	public void Open(IListener iListener)
	{
		base.Open();
		_iListener = iListener;
		UpdateUI();
	}

	public override void OnClosing()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.ObserverManager.RemoveObserver(this);
		}
		DispatchInfoListUpdater.Inst.ClearTempTime();
		DispatchInfoListUpdater.Inst.SetLazyUpdate(useLazyUpdate: true);
		base.OnClosing();
	}

	private void OnEnable()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.ObserverManager.AddObserver(this);
		}
		EnableList();
	}

	private void OnDisable()
	{
		EnableList();
	}

	private void UpdateUI()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager != null)
		{
			WCL_DispatchBattleInfo[] values = GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.GetValues();
			Array.Sort(values, SortFunc);
			FactoryBase.ReleaseAndClearList(_compList);
			int i = 0;
			for (int num = values.Length; i < num; i++)
			{
				WCL_DispatchBattleInfo info = values[i];
				AddComp(info);
			}
			GridScrollOnGoing.RepositionGrid();
			UpdateInfoListUpdaterTimer();
		}
	}

	private void UpdateInfoListUpdaterTimer()
	{
		double num = double.MaxValue;
		WCL_DispatchBattleInfo[] values = GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.GetValues();
		int i = 0;
		for (int num2 = values.Length; i < num2; i++)
		{
			WCL_DispatchBattleInfo wCL_DispatchBattleInfo = values[i];
			if (wCL_DispatchBattleInfo != null)
			{
				double totalSeconds = wCL_DispatchBattleInfo.TimeForEach.TotalSeconds;
				if (num > totalSeconds)
				{
					num = wCL_DispatchBattleInfo.TimeForEach.TotalSeconds;
				}
			}
		}
		DispatchInfoListUpdater.Inst.SetTempTime((float)num);
		DispatchInfoListUpdater.Inst.SetLazyUpdate(useLazyUpdate: false);
	}

	private int SortFunc(WCL_DispatchBattleInfo lf, WCL_DispatchBattleInfo rt)
	{
		int dispatchState = (int)DispatchHelper.GetDispatchState(lf);
		int dispatchState2 = (int)DispatchHelper.GetDispatchState(rt);
		int num = -dispatchState.CompareTo(dispatchState2);
		if (num != 0)
		{
			return num;
		}
		return lf.ConvCompleteTime.Ticks.CompareTo(rt.ConvCompleteTime.Ticks);
	}

	private void AddComp(WCL_DispatchBattleInfo info)
	{
		if (info != null)
		{
			ContentUIDispatchBattleOnGoingComponent contentUIDispatchBattleOnGoingComponent = GridScrollOnGoing.AddGridItem(_prefabAutoComp);
			if (!(contentUIDispatchBattleOnGoingComponent == null))
			{
				contentUIDispatchBattleOnGoingComponent.Open(info, this);
				_compList.Add(contentUIDispatchBattleOnGoingComponent);
			}
		}
	}

	private void RemoveComp(WCL_DispatchBattleInfo info)
	{
		if (info == null)
		{
			return;
		}
		int i = 0;
		for (int count = _compList.Count; i < count; i++)
		{
			ContentUIDispatchBattleOnGoingComponent contentUIDispatchBattleOnGoingComponent = _compList[i];
			if (!(contentUIDispatchBattleOnGoingComponent == null) && DispatchHelper.IsSameInfo(info, contentUIDispatchBattleOnGoingComponent.Info))
			{
				FactoryBase.ReleaseComponent(contentUIDispatchBattleOnGoingComponent);
			}
		}
	}

	private void EnableList()
	{
		if (_iListener != null)
		{
			_iListener.EnableList();
		}
	}

	void IUserObserver<UserDispatchBattleInfoManager.Observer.BroadCastInfo>.OnChanged(UserDispatchBattleInfoManager.Observer.BroadCastInfo noti)
	{
		if (!(GridScrollOnGoing == null))
		{
			switch (noti.Reason)
			{
			case UserDispatchBattleInfoManager.Observer.BroadCastReason.OnAdd:
				AddComp(noti.Info);
				GridScrollOnGoing.RepositionGridWithFixableScroll();
				break;
			case UserDispatchBattleInfoManager.Observer.BroadCastReason.OnRemove:
				RemoveComp(noti.Info);
				GridScrollOnGoing.RepositionGridWithFixableScroll();
				break;
			case UserDispatchBattleInfoManager.Observer.BroadCastReason.OnUpdate:
			case UserDispatchBattleInfoManager.Observer.BroadCastReason.OnChangeState:
				break;
			}
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return true;
	}

	void ContentUIDispatchBattleOnGoingComponent.IListener.OnClick(ContentUIDispatchBattleOnGoingComponent comp)
	{
		Close();
	}
}
