using System;
using NGame2.NAccount;
using NGame2.NClearMission;
using NGame2.NLobby;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class LeftMenuEventIconComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public interface IListener
	{
		void OnClickEventButton(LeftMenuEventIconComponent icon);
	}

	public const string LimitedPrefabPath = "Resources/Prefab2/Lobby/LeftMenuEventIconComponent";

	public const string NewEventPrefabPath = "Resources/Prefab2/Lobby/LeftMenuEventIconComponent02";

	public const string KingsPassPrefabPath = "Resources/Prefab2/Lobby/LeftMenuEventIconComponent03";

	public static PrefabAuto<LeftMenuEventIconComponent> LimitedPrefabAuto = new PrefabAuto<LeftMenuEventIconComponent>("Resources/Prefab2/Lobby/LeftMenuEventIconComponent");

	public static PrefabAuto<LeftMenuEventIconComponent> NewEventPrefabAuto = new PrefabAuto<LeftMenuEventIconComponent>("Resources/Prefab2/Lobby/LeftMenuEventIconComponent02");

	public static PrefabAuto<LeftMenuEventIconComponent> KingsPassPrefabAuto = new PrefabAuto<LeftMenuEventIconComponent>("Resources/Prefab2/Lobby/LeftMenuEventIconComponent03");

	public UISprite SpriteIcon;

	public UISpriteSelector SpriteSelector;

	public UITexture IconEffectRoot;

	public UIWidget TimeRoot;

	public UILabel LabelRemainTime;

	public GameObject NewEffectRoot;

	public UISprite SpriteReward;

	public TweenScale TweenScale;

	private LeftMenuButtonData _leftMenuButtonData;

	public LeftMenuButtonData LeftMenuButtonData => _leftMenuButtonData;

	public static LeftMenuEventIconComponent CreateEventIcon(LeftMenuButtonData data, UITable table, UIScrollView scrollView)
	{
		LeftMenuButtonType leftMenuButtonType = data.LeftMenuButtonType;
		if (leftMenuButtonType == LeftMenuButtonType.None)
		{
			return null;
		}
		LeftMenuEventIconComponent leftMenuEventIconComponent = null;
		switch (leftMenuButtonType)
		{
		case LeftMenuButtonType.ClearMission:
			leftMenuEventIconComponent = NewEventPrefabAuto.Factory.Create(table.transform);
			break;
		case LeftMenuButtonType.PurchaseMarketing:
			leftMenuEventIconComponent = LimitedPrefabAuto.Factory.Create(table.transform);
			break;
		case LeftMenuButtonType.KingsPass:
			leftMenuEventIconComponent = KingsPassPrefabAuto.Factory.Create(table.transform);
			break;
		}
		if (leftMenuEventIconComponent == null)
		{
			return null;
		}
		leftMenuEventIconComponent.Open(data);
		leftMenuEventIconComponent.AddUIDragScrollView(scrollView);
		UIHelper.EnableGameObject(leftMenuEventIconComponent, enable: false);
		return leftMenuEventIconComponent;
	}

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
		Init();
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		Init();
	}

	public void Open(LeftMenuButtonData leftMenuButtonData)
	{
		_leftMenuButtonData = leftMenuButtonData;
		string text = "";
		if (SpriteSelector != null)
		{
			text = _leftMenuButtonData.LeftMenuButtonType.ToString();
			if (!String_Extension_Method.IsNullOrEmpty(_leftMenuButtonData.SubType))
			{
				text = text + "_" + _leftMenuButtonData.SubType;
			}
			SpriteSelector.Select(text);
		}
		bool enable = text == "PurchaseMarketing_FirstClearChapter_2020";
		UIHelper.EnableGameObject(IconEffectRoot, enable);
	}

	public void AddUIDragScrollView(UIScrollView uIScrollView)
	{
		if (SpriteIcon != null)
		{
			UIDragScrollView uIDragScrollView = SpriteIcon.GetComponent<UIDragScrollView>();
			if (uIDragScrollView == null)
			{
				uIDragScrollView = SpriteIcon.gameObject.AddComponent<UIDragScrollView>();
			}
			uIDragScrollView.scrollView = uIScrollView;
		}
	}

	public void UpdateEvent()
	{
		switch (LeftMenuButtonData.LeftMenuButtonType)
		{
		case LeftMenuButtonType.PurchaseMarketing:
			UpdatePurchaseMarketing();
			break;
		case LeftMenuButtonType.ClearMission:
			UpdateClearMission();
			break;
		case LeftMenuButtonType.KingsPass:
			UpdateKingsPass();
			break;
		}
	}

	private void UpdatePurchaseMarketing()
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || !AccountManager.IsLogined())
		{
			return;
		}
		MarketingType type = Vespa_String_Extension_Method.TOENUM(LeftMenuButtonData.SubType, MarketingType.None);
		PurchaseMarketingPopupData[] purchaseMarketingPopupDatas = GameRoot.Instance.PurchaseMarketingPopupDataContainer.GetPurchaseMarketingPopupDatas(type);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(purchaseMarketingPopupDatas))
		{
			return;
		}
		bool flag = false;
		DateTime o_dateTime = DateTime.MinValue;
		TimeSpan timeSpan = TimeSpan.MaxValue;
		PurchaseMarketingPopupData[] array = purchaseMarketingPopupDatas;
		foreach (PurchaseMarketingPopupData purchaseMarketingPopupData in array)
		{
			if (purchaseMarketingPopupData == null || __Extension_Method_ICollection__.IsNullOrEmpty(purchaseMarketingPopupData.PurchaseIndices))
			{
				continue;
			}
			PurchaseMarketingInfo purchaseMarketingInfo = null;
			int[] purchaseIndices = purchaseMarketingPopupData.PurchaseIndices;
			foreach (int key in purchaseIndices)
			{
				PurchaseMarketingInfo purchaseMarketingInfo2 = ((GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager == null) ? null : GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager.Get(key));
				if (PurchaseMarketingManager.IsPurchaseMarketing(purchaseMarketingInfo2))
				{
					purchaseMarketingInfo = purchaseMarketingInfo2;
					break;
				}
			}
			if (purchaseMarketingInfo == null || purchaseMarketingInfo.Active == 0)
			{
				continue;
			}
			if (Vespa_String_Extension_Method2.TryParseNetworkTime2(purchaseMarketingInfo.EndTime, out o_dateTime))
			{
				TimeSpan timeSpan2 = o_dateTime - NGame2.NUI.NWindow.Common.GetUTCNow();
				if (timeSpan > timeSpan2 && timeSpan2.TotalSeconds > 0.0)
				{
					timeSpan = timeSpan2;
				}
			}
			flag = true;
		}
		if (flag && timeSpan != TimeSpan.MaxValue)
		{
			SetRemainTime(timeSpan);
		}
		UIHelper.EnableGameObject(this, flag);
	}

	private void UpdateClearMission()
	{
		if (AccountManager.IsLogined())
		{
			if (SingletonUpdater<ClearMissionManager>.instance.IsShowClearMissionIcon())
			{
				UIHelper.EnableGameObject(this, enable: true);
				bool show = SingletonUpdater<ClearMissionManager>.instance.IsNewPromotion();
				ShowNew(show);
				bool flag = SingletonUpdater<ClearMissionManager>.instance.CanReceiveReward(checkAllMission: true);
				ShowReward(flag);
				PlayEffect(flag);
			}
			else
			{
				UIHelper.EnableGameObject(this, enable: false);
			}
		}
	}

	private void UpdateKingsPass()
	{
		if (!AccountManager.IsLogined())
		{
			return;
		}
		bool enable = false;
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance && GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager != null)
		{
			KingsPassSeaonInfo kingsPassSeaonInfo = GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.KingsPassSeaonInfo;
			if (kingsPassSeaonInfo == null || !Vespa_String_Extension_Method2.TryParseNetworkTime2(kingsPassSeaonInfo.EndTime, out var o_dateTime))
			{
				return;
			}
			if ((o_dateTime - NGame2.NUI.NWindow.Common.GetUTCNow()).TotalSeconds > 0.0)
			{
				enable = true;
				bool show = SceneAutoBehaviour<KingsPassManagement>.instance.CanAllReceiveReward();
				ShowReward(show);
				SetRemainTime(o_dateTime);
			}
		}
		UIHelper.EnableGameObject(this, enable);
	}

	public void SetRemainTime(DateTime endTime)
	{
		UIHelper.EnableGameObject(TimeRoot, enable: true);
		TimeSpan timeSpan = endTime - NGame2.NUI.NWindow.Common.GetUTCNow();
		string text = "";
		if (timeSpan.TotalSeconds > 0.0)
		{
			text = UIHelper.GetRemainTimeLabelText((long)timeSpan.TotalSeconds);
		}
		UIHelper.SetLabelText(LabelRemainTime, text);
	}

	public void SetRemainTime(TimeSpan ts)
	{
		double totalSeconds = ts.TotalSeconds;
		if (totalSeconds <= 0.0)
		{
			UIHelper.EnableGameObject(TimeRoot, enable: false);
			return;
		}
		UIHelper.EnableGameObject(TimeRoot, enable: true);
		string remainTimeLabelText = UIHelper.GetRemainTimeLabelText((long)totalSeconds);
		UIHelper.SetLabelText(LabelRemainTime, remainTimeLabelText);
	}

	public void ShowReward(bool show)
	{
		UIHelper.EnableGameObject(SpriteReward, show);
	}

	public void ShowNew(bool show)
	{
		UIHelper.EnableGameObject(NewEffectRoot, show);
	}

	public void PlayEffect(bool play)
	{
		if (play)
		{
			PlayScaleEffect();
		}
		else
		{
			StopScaleEffect();
		}
	}

	public void OnButtonClick()
	{
		if (_leftMenuButtonData != null)
		{
			string taskName = _leftMenuButtonData.TaskName;
			string[] taskValue = _leftMenuButtonData.TaskValue;
			SceneAutoBehaviour<ActivityManager>.instance.Run(taskName, taskValue);
		}
	}

	private void PlayScaleEffect()
	{
		if (TweenScale != null && !TweenScale.enabled)
		{
			TweenScale.enabled = true;
			TweenScale.ResetToBeginning();
			TweenScale.PlayForward();
		}
	}

	private void StopScaleEffect()
	{
		if (TweenScale != null && TweenScale.enabled)
		{
			TweenScale.enabled = false;
			TweenScale.ResetToBeginning();
		}
	}

	private void Init()
	{
		UIHelper.EnableGameObject(TimeRoot, enable: false);
		UIHelper.EnableGameObject(NewEffectRoot, enable: false);
		StopScaleEffect();
	}
}
