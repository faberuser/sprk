using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWorldMap.NMapLayout;
using NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar;
using NShared;
using NShared.GetPushReward;
using NShared.ReceivePushReward;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NController;

public class DailyGiftBoxController : AiInteractionComponent.Listener, WebManager.IListener<NShared.GetPushReward.Response>, WebManager.IListener<NShared.ReceivePushReward.Response>
{
	private const float DefaultDelaySeconds = 30f;

	private const int MaxHour = 24;

	private List<PushRewardData> _pushRewardDataList = new List<PushRewardData>();

	private int _currentDayOfWeek = -1;

	private PushRewardData _currentPushRewardData;

	private AiInteractionComponent _dailyGiftBoxComponent;

	private NGame2.NUI.NWorldMap.NMapLayout.Avatar _avatar;

	private DailyGiftBoxWebManager _dailyGiftBoxWebManager;

	private Coroutine _requestCo;

	private string _getRewardNameKey = "";

	private bool _exceptionEnable;

	private bool CheckDailyGiftBoxState
	{
		get
		{
			if (_dailyGiftBoxComponent != null)
			{
				return _dailyGiftBoxComponent.IsRewardOn;
			}
			return false;
		}
	}

	public void Open(AiInteractionComponent component, NGame2.NUI.NWorldMap.NMapLayout.Avatar avatar)
	{
		Init(component, avatar);
	}

	private void Init(AiInteractionComponent component, NGame2.NUI.NWorldMap.NMapLayout.Avatar avatar)
	{
		_dailyGiftBoxComponent = component;
		_avatar = avatar;
		_exceptionEnable = true;
		SetPushRewardDataListByDayOfWeek();
		StartCoroutineDailyGiftBoxState();
	}

	private void StartCoroutineDailyGiftBoxState(float delaySeconds = 30f)
	{
		if (_requestCo == null)
		{
			_requestCo = VespaCoroutine.Start(CoroutineDailyGiftBoxState(delaySeconds), null, null);
		}
	}

	private void SetPushRewardDataListByDayOfWeek()
	{
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		if (_currentDayOfWeek != (int)currentServerLocalTime.DayOfWeek)
		{
			_currentDayOfWeek = (int)currentServerLocalTime.DayOfWeek;
			List<PushRewardData> dataList = GameRoot.Instance.PushRewardDataContainer.GetDataList(_currentDayOfWeek);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(dataList))
			{
				_pushRewardDataList.Clear();
				_pushRewardDataList.AddRange(dataList);
			}
		}
	}

	private IEnumerator CoroutineDailyGiftBoxState(float delaySeconds)
	{
		while (true)
		{
			bool flag = IsEnableDailyGiftBox();
			PushRewardInfo dailyGiftBoxInfo = GetDailyGiftBoxInfo();
			if (flag && dailyGiftBoxInfo == null)
			{
				ReqeuestGetPushReward();
			}
			UpdateDailyGiftBoxState(flag);
			yield return new WaitForSeconds(30f);
		}
	}

	public void UpdateDailyGiftBoxState(bool enable)
	{
		if (_exceptionEnable && !(_dailyGiftBoxComponent == null))
		{
			if (CheckDailyGiftBoxState && !enable)
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.DailyGiftBoxInfo = null;
			}
			EnableDailyGiftBox(enable);
			SetPushRewardDataListByDayOfWeek();
		}
	}

	private void ReqeuestGetPushReward()
	{
		if (_dailyGiftBoxWebManager == null)
		{
			_dailyGiftBoxWebManager = new DailyGiftBoxWebManager();
		}
		if (_dailyGiftBoxWebManager != null)
		{
			_dailyGiftBoxWebManager.RequestGetPushReward(this);
		}
	}

	private void RequestReceivePushReward()
	{
		if (_dailyGiftBoxWebManager == null)
		{
			_dailyGiftBoxWebManager = new DailyGiftBoxWebManager();
		}
		if (_dailyGiftBoxWebManager != null)
		{
			_getRewardNameKey = "";
			if (_currentPushRewardData != null)
			{
				_getRewardNameKey = _currentPushRewardData.NameKey;
			}
			EnableDailyGiftBox(enable: false);
			_dailyGiftBoxWebManager.RequestReceivePushReward(this);
		}
	}

	private void EnableDailyGiftBox(bool enable)
	{
		if (_dailyGiftBoxComponent != null)
		{
			_dailyGiftBoxComponent.ShowReward(enable);
		}
	}

	public void ExceptionEnable(bool enable)
	{
		_exceptionEnable = enable;
		EnableDailyGiftBox((!enable) ? enable : (GetDailyGiftBoxInfo() != null && IsEnableDailyGiftBox()));
	}

	private PushRewardInfo GetDailyGiftBoxInfo()
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return null;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.UserManager.DailyGiftBoxInfo;
	}

	private bool IsEnableDailyGiftBox()
	{
		_currentPushRewardData = null;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_pushRewardDataList))
		{
			return false;
		}
		for (int i = 0; i < _pushRewardDataList.Count; i++)
		{
			if (CheckTimeEnableDailyGiftBox(_pushRewardDataList[i]))
			{
				_currentPushRewardData = _pushRewardDataList[i];
				return true;
			}
		}
		return false;
	}

	private bool CheckTimeEnableDailyGiftBox(PushRewardData data)
	{
		if (data == null)
		{
			return false;
		}
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		if (data.OpenTime <= currentServerLocalTime.Hour && data.EndTime > currentServerLocalTime.Hour)
		{
			return true;
		}
		return false;
	}

	public float EndRemainSeconds()
	{
		if (_currentPushRewardData == null)
		{
			return 30f;
		}
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		return (float)(new DateTime(currentServerLocalTime.Year, currentServerLocalTime.Month, currentServerLocalTime.Day, (_currentPushRewardData.EndTime >= 24) ? (_currentPushRewardData.EndTime - 24) : _currentPushRewardData.EndTime, 0, 0) - currentServerLocalTime).TotalSeconds;
	}

	private void OpenRewardPopup(NShared.ReceivePushReward.Response response)
	{
		SceneAutoBehaviour<LobbyPopupQueue>.instance.AddAction(delegate
		{
			string stringDirect = LocalizationManager.GetStringDirect(_getRewardNameKey);
			BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(stringDirect).SetCurrencyResultInfos3(response.CurrencyResultInfos).SetItemResultInfos(response.ItemResultInfos)
				.SetEquipResultInfos(response.EquipItemResultInfos)
				.SetFriendshipPointResults(response.FriendshipPointResultInfos)
				.SetStaminaResultInfos(response.StaminaResultInfos), isGlobal: false);
		}, null, PopupQueue.PopupFlag.HideOnTopmostMenuOpen, "DailyGiftBoxReward");
	}

	private void RemoveCurrentDailyGiftBoxInfo()
	{
		if (_currentPushRewardData != null)
		{
			_pushRewardDataList.Remove(_currentPushRewardData);
			StartCoroutineDailyGiftBoxState();
		}
	}

	void AiInteractionComponent.Listener.OnClickReward()
	{
		if (_avatar != null && _currentPushRewardData != null)
		{
			_avatar.PlayNamedAction(_currentPushRewardData.TouchAction);
		}
		RequestReceivePushReward();
	}

	void WebManager.IListener<NShared.GetPushReward.Response>.Success(NShared.GetPushReward.Response response)
	{
		if (GetDailyGiftBoxInfo() == null)
		{
			RemoveCurrentDailyGiftBoxInfo();
			EnableDailyGiftBox(enable: false);
		}
	}

	void WebManager.IListener<NShared.GetPushReward.Response>.Fail(NShared.GetPushReward.Response response)
	{
	}

	void WebManager.IListener<NShared.ReceivePushReward.Response>.Success(NShared.ReceivePushReward.Response response)
	{
		OpenRewardPopup(response);
		RemoveCurrentDailyGiftBoxInfo();
	}

	void WebManager.IListener<NShared.ReceivePushReward.Response>.Fail(NShared.ReceivePushReward.Response response)
	{
		EnableDailyGiftBox(enable: true);
	}
}
