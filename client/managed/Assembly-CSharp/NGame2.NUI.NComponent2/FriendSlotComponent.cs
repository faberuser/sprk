using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NConfig;
using NGame2.NParty;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class FriendSlotComponent : UIScrollItemBehaviour
{
	public enum ESendRecvFriendshipPoint
	{
		CanSend,
		SentNotReceived,
		SentCanReceive,
		SentReceived
	}

	public interface Listener
	{
		void Whisper(FriendSlotComponent This);

		void ShowHeroes(FriendSlotComponent This);

		void Delete(FriendSlotComponent This);

		void InviteParty(FriendSlotComponent This);

		void Duel(FriendSlotComponent This);

		void InviteAccept(FriendSlotComponent This);

		void InviteDeny(FriendSlotComponent This);

		void InviteRequest(FriendSlotComponent This);

		void sendFriendshipPoint(FriendSlotComponent This);

		void recvFriendshipPoint(FriendSlotComponent This);
	}

	[Serializable]
	public class FriendshipPointStatus
	{
		public ESendRecvFriendshipPoint Status;

		public UISprite Sprite_Status;
	}

	public const string PrefabPath = "Prefab2/Component/Friend/FriendSlotComponent";

	public const string InvitePrefabPath = "Prefab2/Component/Friend/FriendInviteSlotComponent";

	public const string RecommendPrefabPath = "Prefab2/Component/Friend/FriendRecommendSlotComponent";

	public static PrefabAuto<FriendSlotComponent> PrefabAuto = new PrefabAuto<FriendSlotComponent>("Prefab2/Component/Friend/FriendSlotComponent");

	public static PrefabAuto<FriendSlotComponent> PrefabAutoInvite = new PrefabAuto<FriendSlotComponent>("Prefab2/Component/Friend/FriendInviteSlotComponent");

	public static PrefabAuto<FriendSlotComponent> PrefabAutoRecommend = new PrefabAuto<FriendSlotComponent>("Prefab2/Component/Friend/FriendRecommendSlotComponent");

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _logoutTimeLabel;

	[SerializeField]
	private UILabel _onlineLabel;

	[SerializeField]
	private UIButton _whisperButton;

	[SerializeField]
	private UIButton _showHeroesButton;

	[SerializeField]
	private UIButton _deleteButton;

	[SerializeField]
	private UIButton _invitePartyButton;

	[SerializeField]
	private UIButton _duelButton;

	[SerializeField]
	private UIButton _acceptButton;

	[SerializeField]
	private UIButton _denyButton;

	[SerializeField]
	private UIButton _requestButton;

	public List<FriendshipPointStatus> ListFriendshipPointStatus;

	private Listener _listener;

	private float _sprkNextPresenceRefresh;

	public CachedUserInfo FriendInfo { get; private set; }

	public bool Open(CachedUserInfo friendInfo, Listener listener)
	{
		FriendInfo = friendInfo;
		_listener = listener;
		Refresh();
		return true;
	}

	public void Refresh()
	{
		SetName(FriendInfo.Nick);
		SetLevel(FriendInfo.TeamLevel);
		SetPortraitTexture(FriendInfo.AvatarHeroIndex);
		UpdateButtonStatus();
		UIHelper.EnableGameObject(_duelButton, SingletonAuto<Config>.instance.ClientConfig.ShowEditorGui);
	}

	public void UpdateButtonStatus()
	{
		if (FriendInfo != null)
		{
			UpdateOnlineButtonState();
			UpdateInvitePartyButtonState();
			UpdateFriendshipPointStatus();
		}
	}

	private void UpdateOnlineButtonState()
	{
		if (FriendInfo.IsOnline)
		{
			SetOnline();
			return;
		}
		if (FriendInfo is FriendUserInfo && Vespa_String_Extension_Method2.TryParseNetworkTime2((FriendInfo as FriendUserInfo).LastOnlineTime, out var o_dateTime))
		{
			SetOffline(o_dateTime);
			return;
		}
		DateTime o_dateTime2;
		bool num = Vespa_String_Extension_Method2.TryParseNetworkTime2(FriendInfo.LastLoginTime, out o_dateTime2);
		DateTime o_dateTime3;
		bool flag = Vespa_String_Extension_Method2.TryParseNetworkTime2(FriendInfo.LastLogoutTime, out o_dateTime3);
		if (num && flag)
		{
			DateTime offline = ((o_dateTime2 > o_dateTime3) ? o_dateTime2 : o_dateTime3);
			SetOffline(offline);
		}
		else if (flag)
		{
			SetOffline(o_dateTime3);
		}
		else
		{
			SetOffline(o_dateTime2);
		}
	}

	public void SetPortraitTexture(int avatarHeroIndex)
	{
		string iconPath = UIAvatarHelper.GetIconPath(avatarHeroIndex);
		if (!String_Extension_Method.IsNullOrEmpty(iconPath))
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(iconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, _portraitTexture, plainTexture);
		}
	}

	public void SetName(string friendName)
	{
		UIHelper.SetLabelText(_nameLabel, friendName);
	}

	public void SetLevel(int level)
	{
		UIHelper.SetLabelText(_levelLabel, level);
	}

	private void UpdateInvitePartyButtonState()
	{
		bool flag = GlobalAutoBehaviour<PartyManager>.isValidInstance && GlobalAutoBehaviour<PartyManager>.instance.CanInviteSomeone();
		bool flag2 = FriendInfo != null && FriendInfo.IsOnline;
		UIHelper.EnableButton(_invitePartyButton, flag && flag2);
	}

	private void UpdateOnlineButton()
	{
		bool flag = FriendInfo != null && FriendInfo.IsOnline;
		UIHelper.EnableGameObject(_onlineLabel, flag);
		UIHelper.EnableGameObject(_logoutTimeLabel, !flag);
		UIHelper.EnableButton(_whisperButton, flag);
	}

	private void UpdateFriendshipPointStatus()
	{
		ESendRecvFriendshipPoint sendRecvFriendshipPointStatus = GetSendRecvFriendshipPointStatus();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ListFriendshipPointStatus); i++)
		{
			UIHelper.EnableGameObject(ListFriendshipPointStatus[i].Sprite_Status, sendRecvFriendshipPointStatus == ListFriendshipPointStatus[i].Status);
		}
	}

	private ESendRecvFriendshipPoint GetSendRecvFriendshipPointStatus()
	{
		long accountId = FriendInfo.AccountId;
		return GlobalAutoBehaviour<AccountManager>.instance.SendRecvFriendshipPointManager.GetSendRecvType(accountId) switch
		{
			NGame2.NAccount.UserSendRecvFriendshipPointManager.FriendshipPointSendRecvType.CanSend => ESendRecvFriendshipPoint.CanSend, 
			NGame2.NAccount.UserSendRecvFriendshipPointManager.FriendshipPointSendRecvType.SentNotReceived => ESendRecvFriendshipPoint.SentNotReceived, 
			NGame2.NAccount.UserSendRecvFriendshipPointManager.FriendshipPointSendRecvType.SentCanReceive => ESendRecvFriendshipPoint.SentCanReceive, 
			NGame2.NAccount.UserSendRecvFriendshipPointManager.FriendshipPointSendRecvType.SentReceived => ESendRecvFriendshipPoint.SentReceived, 
			_ => ESendRecvFriendshipPoint.SentNotReceived, 
		};
	}

	private void SetOnline()
	{
		UpdateOnlineButton();
	}

	private void SetOffline(DateTime offlineTime)
	{
		UpdateOnlineButton();
		TimeSpan timeSpan = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime() - offlineTime;
		if ((int)timeSpan.TotalDays >= 1)
		{
			UIHelper.SetLabelText(_logoutTimeLabel, LocalizationManager.GetString(Match.MATCH_HISTORY_TIME_DAY, (int)timeSpan.TotalDays));
			return;
		}
		if ((int)timeSpan.TotalHours >= 1)
		{
			UIHelper.SetLabelText(_logoutTimeLabel, LocalizationManager.GetString(Match.MATCH_HISTORY_TIME_HOUR, (int)timeSpan.TotalHours));
			return;
		}
		int val = (int)timeSpan.TotalMinutes;
		val = System.Math.Max(val, 0);
		UIHelper.SetLabelText(_logoutTimeLabel, LocalizationManager.GetString(Match.MATCH_HISTORY_TIME_MINUTE, val));
	}

	public void OnClickWhisper()
	{
		if (_listener != null)
		{
			_listener.Whisper(this);
		}
	}

	public void OnClickShowHeroes()
	{
		if (_listener != null)
		{
			_listener.ShowHeroes(this);
		}
	}

	public void OnClickDelete()
	{
		if (_listener != null)
		{
			_listener.Delete(this);
		}
	}

	public void OnClickInviteParty()
	{
		if (_listener != null)
		{
			_listener.InviteParty(this);
		}
	}

	public void OnClickDuel()
	{
		if (_listener != null)
		{
			_listener.Duel(this);
		}
	}

	public void OnClickInviteAccept()
	{
		if (_listener != null)
		{
			_listener.InviteAccept(this);
		}
	}

	public void OnClickInviteDeny()
	{
		if (_listener != null)
		{
			_listener.InviteDeny(this);
		}
	}

	public void OnClickInviteRequest()
	{
		if (_listener != null)
		{
			_listener.InviteRequest(this);
		}
	}

	public void OnClickSendFriendshipPoint()
	{
		if (_listener != null)
		{
			_listener.sendFriendshipPoint(this);
		}
	}

	public void OnClickRecvFriendshipPoint()
	{
		if (_listener != null)
		{
			_listener.recvFriendshipPoint(this);
		}
	}

	public void OnClickComponent()
	{
		InspectUserManagement.InspectUserData inspectUser = new InspectUserManagement.InspectUserData(FriendInfo, InspectUserCategory.Friend);
		SceneAutoBehaviour<InspectUserManagement>.instance.OpenMenuPopup(inspectUser);
	}

	private void Update()
	{
		if (!(Time.unscaledTime < _sprkNextPresenceRefresh))
		{
			_sprkNextPresenceRefresh = Time.unscaledTime + 1f;
			if (FriendInfo != null)
			{
				UpdateOnlineButtonState();
			}
		}
	}
}
