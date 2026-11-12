using System;
using NGame2.NClearMission;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class ClearMissionPopupManagement : BaseManagement<ClearMissionPopupManagement>, ClearMissionPopup.IListener
{
	protected override void Awake()
	{
		base.Awake();
		AddInstance<ClearMissionPopup>("Resources/Prefab2/Window/ClearMission/ClearMissionPopup");
	}

	protected override void ChainUpdate()
	{
		base.ChainUpdate();
	}

	public void OpenClearMissionPopup()
	{
		ClearMissionPopup popupWindow = GetInstance<ClearMissionPopup>();
		SceneAutoBehaviour<LobbyPopupQueue>.instance.AddPopup(popupWindow, delegate(ClearMissionPopup window)
		{
			window.Open(this);
		}, PopupQueue.PopupFlag.DefaultPopupFlag);
	}

	public void CloseClearMissionPopup()
	{
		ClearMissionPopup clearMissionPopup = GetInstance<ClearMissionPopup>();
		if (clearMissionPopup != null && clearMissionPopup.IsOpen())
		{
			clearMissionPopup.Close();
		}
	}

	public void RefreshClearMissionPopup(ClearMissionProductData productData)
	{
		ClearMissionPopup clearMissionPopup = GetInstance<ClearMissionPopup>(openedOnly: true);
		if (clearMissionPopup != null && clearMissionPopup.IsOpen())
		{
			clearMissionPopup.RefreshPopup(productData);
		}
	}

	void ClearMissionPopup.IListener.OnBuyPayShopProduct(int payShopProductIndex)
	{
		SingletonUpdater<ClearMissionManager>.instance.BuyPayShopProduct(payShopProductIndex);
	}

	void ClearMissionPopup.IListener.OnGetMissionReward(int missionIndex, string rewardTitle, Action onReceived, Action onRefreshTab)
	{
		SingletonUpdater<ClearMissionManager>.instance.GetMissionReward(missionIndex, rewardTitle, onReceived, onRefreshTab);
	}
}
