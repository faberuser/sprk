using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NPayShop;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

internal class RightAwayProductMenu : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate, RightAwayProductRollingBanner.IListener, LobbyRightGridWindow.IRightGridSort, IUserObserver<NGame2.NAccount.UserRightAwayProductManager.ChangedRightAwayProductInfo>, IUserObserver
{
	public interface Listener
	{
		void InitRightAwayProductInfo(Action onSuccess, Action onFail);

		void OnClickBanner(int productIndex, Action onSuccess);
	}

	public const string PrefabPath = "Resources/Prefab2/Lobby/RightAwayProductMenu";

	public static PrefabAuto<NGame2.NUI.NWindow.RightAwayProductMenu> PrefabAuto = new PrefabAuto<NGame2.NUI.NWindow.RightAwayProductMenu>("Resources/Prefab2/Lobby/RightAwayProductMenu");

	private Listener _listener;

	public RightAwayProductRollingBanner RollingBanner;

	private bool isInit = true;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
		GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager.ObserverManager.AddObserver(this);
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager.ObserverManager.RemoveObserver(this);
		}
		_listener = null;
	}

	public void Open(Listener listener)
	{
		_listener = listener;
		Init();
	}

	private void Init()
	{
		if (_listener != null)
		{
			RollingBanner.Open(this);
			InitBanner();
		}
	}

	private void InitBanner()
	{
		RollingBanner.Clear();
		RollingBanner.Stop();
		RightAwayProductInfo[] values = GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager.GetValues();
		List<RightAwayProductInfo> list = new List<RightAwayProductInfo>();
		list.AddRange(values);
		list.Sort(delegate(RightAwayProductInfo a, RightAwayProductInfo b)
		{
			int num2 = 0;
			int num3 = ((a.ShowIndex != 0) ? a.ShowIndex : int.MaxValue);
			int value = ((b.ShowIndex != 0) ? b.ShowIndex : int.MaxValue);
			num2 = num3.CompareTo(value);
			return (num2 != 0) ? num2 : a.Index.CompareTo(b.Index);
		});
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(list); num++)
		{
			RollingBanner.AddBannerInfo(list[num]);
		}
		RollingBanner.StartPlay();
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			if (GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager != null)
			{
				GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager.ObserverManager.RemoveObserver(this);
			}
			_listener = null;
		}
	}

	public void Reset()
	{
		RollingBanner.Clear();
		RollingBanner.Stop();
	}

	void RightAwayProductRollingBanner.IListener.OnClick(int productIndex)
	{
		if (_listener != null)
		{
			_listener.OnClickBanner(productIndex, delegate
			{
				isInit = true;
			});
		}
	}

	bool LobbyRightGridWindow.IRightGridSort.IsDestroy()
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return false;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager.Count() == 0;
	}

	void LobbyRightGridWindow.IRightGridSort.ChainUpdate()
	{
		RollingBanner.UpdateDerived();
	}

	int LobbyRightGridWindow.IRightGridSort.GetSortIndex()
	{
		return 1;
	}

	void IUserObserver<NGame2.NAccount.UserRightAwayProductManager.ChangedRightAwayProductInfo>.OnChanged(NGame2.NAccount.UserRightAwayProductManager.ChangedRightAwayProductInfo changedInfo)
	{
		if (RollingBanner.CheckBannerReset(changedInfo.RightAwayProductInfos) || isInit)
		{
			InitBanner();
			isInit = false;
		}
		else
		{
			RollingBanner.RefreshBannerInfo();
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return true;
	}
}
