using System;
using NGame2.NAccount;
using NGame2.NParty;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUtil;
using NShared;
using NShared.InspectUser;
using NShared.NLocalization;
using NShared.NParty;
using NShared.RecvFriendshipPoint;
using NShared.RejectFriend;
using NShared.RequestFriend;
using NShared.SearchFriend;
using NShared.SendFriendshipPoint;
using NShared.SendRecvFriendshipPoint;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class MyFriendManagement : BaseManagement<MyFriendManagement>, FriendMenu.Listener, FriendListPanel.Listener, FriendInvitePanel.Listener, FriendRecommendPanel.Listener, FriendSearch.Listener
{
	private DateTime _RemainDailyRemoveFriendResetTime;

	private int _QueryRemainSecondPeriod = 36000;

	private DateTime _QueryTime;

	public FriendMenu.EToggle friendMenuToggle
	{
		get
		{
			if (GetInstance<FriendMenu>() != null)
			{
				return GetInstance<FriendMenu>()._toggle;
			}
			return FriendMenu.EToggle.List;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<FriendMenu>("Resources/Prefab2/Window/Friend/FriendMenu");
		AddInstance<FriendSearch>("Resources/Prefab2/Window/Friend/FriendSearch");
		ReserveIconCache();
	}

	private void ReserveIconCache()
	{
	}

	public void OpenFriendMenu(FriendMenu.EToggle toggleType)
	{
		GetInstance<FriendMenu>().Open(toggleType, this);
	}

	public void OpenFriendSearch()
	{
		AddTask(base.TaskFactory.Run((FriendSearch window) => window.OpenAsync(immediately: true)), base.TaskFactory.Run(delegate(FriendSearch window)
		{
			window.Init(this);
		}));
	}

	public void UpdateOnlineStatus(long friendId)
	{
		AddTask(base.TaskFactory.Run(delegate(FriendMenu window)
		{
			window.UpdateStatusList(friendId);
		}));
	}

	void FriendMenu.Listener.OnSearch()
	{
		OpenFriendSearch();
	}

	void FriendMenu.Listener.SearchFriend(string keyword)
	{
		if (string.Equals(GlobalAutoBehaviour<AccountManager>.instance.UserManager.Nick, keyword))
		{
			MessagePopup.OpenError(NShared.SearchFriend.ResultType.CannotSearchMyself);
			return;
		}
		if (Array.Exists(GlobalAutoBehaviour<AccountManager>.instance.FriendManager.GetFriendInfos(), (FriendUserInfo info) => string.Equals(info.Nick, keyword)))
		{
			MessagePopup.OpenError(NShared.SearchFriend.ResultType.CannotSearchMyFriend);
			return;
		}
		SendSearchFriend(keyword, delegate(CachedUserInfo[] friendInfos)
		{
			if (friendInfos != null)
			{
				AddTask(base.TaskFactory.Run(delegate(FriendMenu window)
				{
					window.SetupSearchFriendRecommend(friendInfos);
				}));
			}
		});
	}

	void FriendListPanel.Listener.Whisper(long friendId, string message)
	{
	}

	void FriendListPanel.Listener.ShowHeroes(long friendId)
	{
	}

	void FriendListPanel.Listener.Delete(long friendId, string friendNick)
	{
		ProcessDeleteFriend(friendId, friendNick);
	}

	void FriendListPanel.Listener.InviteParty(long friendId)
	{
		if (GlobalAutoBehaviour<PartyManager>.isValidInstance && GlobalAutoBehaviour<PartyManager>.instance.PartyInfo != null && GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.IsValid())
		{
			MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
			if (myPartyMemberInfo != null && !myPartyMemberInfo.IsMaster)
			{
				MessagePopup.OpenError(LocalizationManager.GetString(Friend.NOT_PARTY_MASTER));
				return;
			}
		}
		FriendUserInfo friendInfo = GlobalAutoBehaviour<AccountManager>.instance.FriendManager.GetFriendInfo(friendId);
		if (friendInfo != null)
		{
			GlobalAutoBehaviour<PartyManager>.instance.SendInvitePartyRequest(friendId, friendInfo.Nick);
		}
	}

	void FriendListPanel.Listener.Duel(long friendId)
	{
	}

	void FriendListPanel.Listener.SendFriendshipPoint(long friendId)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.FriendManager.GetFriendInfo(friendId) != null)
		{
			RequestSendFriendshipPoint(new long[1] { friendId });
		}
	}

	void FriendListPanel.Listener.RecvFriendshipPoint(long friendId)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.FriendManager.GetFriendInfo(friendId) != null)
		{
			RequestRecvFriendshipPoint(new long[1] { friendId });
		}
	}

	void FriendListPanel.Listener.SendRecvFriendshipPoint()
	{
		RequestSendRecvFriendshipPoint();
	}

	void FriendInvitePanel.Listener.InviteAccept(long friendId, string nickName)
	{
		SendRequestFriend(friendId, nickName, FriendInviteType.Accept);
	}

	void FriendInvitePanel.Listener.InviteDeny(long friendId, string rejectNickName)
	{
		SendRejectFriend(friendId, rejectNickName);
	}

	void FriendRecommendPanel.Listener.GetRecommendFriends(Action<CachedUserInfo[]> onFriends)
	{
		SendSearchFriend(null, onFriends);
	}

	void FriendRecommendPanel.Listener.InviteRequest(CachedUserInfo userInfo)
	{
		ProcessInviteFriend(userInfo);
	}

	void FriendSearch.Listener.SearchFriend(string keyword, Action<CachedUserInfo[]> onFriends)
	{
		if (string.IsNullOrEmpty(keyword))
		{
			MessagePopup.OpenError(NShared.SearchFriend.ResultType.FriendNotFound);
		}
		else if (string.Equals(GlobalAutoBehaviour<AccountManager>.instance.UserManager.Nick, keyword))
		{
			MessagePopup.OpenError(NShared.SearchFriend.ResultType.CannotSearchMyself);
		}
		else
		{
			SendSearchFriend(keyword, onFriends);
		}
	}

	void FriendSearch.Listener.InviteRequest(CachedUserInfo userInfo)
	{
		ProcessInviteFriend(userInfo);
		GetInstance<FriendSearch>().Close();
	}

	private void Start()
	{
	}

	public void ProcessDeleteFriend(long accountId, string nick)
	{
		if (GlobalAutoBehaviour<PartyManager>.isValidInstance && GlobalAutoBehaviour<PartyManager>.instance.GetPartyMemberInfo(accountId) != null)
		{
			MessagePopup.OpenError(LocalizationManager.GetString(Friend.FRIEND_IS_IN_PARTY));
			return;
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(Friend.FRIEND_ASK_DELETE_FRIEND, nick);
		MessagePopup.Open(title, message, delegate
		{
			SendRejectFriend(accountId);
		}, null);
	}

	private void ProcessInviteFriend(CachedUserInfo userInfo)
	{
		if (userInfo != null)
		{
			ProcessInviteFriend(userInfo.AccountId, userInfo.Nick);
		}
	}

	public void ProcessInviteFriend(long accountId, string nickname)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.FriendManager.GetFriendInfo(accountId) != null)
		{
			MessagePopup.OpenError(LocalizationManager.GetString(Friend.ALREADY_MY_FRIEND, nickname));
		}
		else if (GlobalAutoBehaviour<AccountManager>.instance.FriendManager.Get(accountId) != null)
		{
			MessagePopup.OpenError(LocalizationManager.GetString(Friend.ALREADY_INVITED, nickname));
		}
		else if (__Extension_Method_ICollection__.COUNT(GlobalAutoBehaviour<AccountManager>.instance.FriendManager.GetFriendInfos()) == 50)
		{
			MessagePopup.OpenError(LocalizationManager.GetStringDirect("RESULT_TYPE.MY_FRIEND_FULL"));
		}
		else
		{
			SendRequestFriend(accountId, nickname, FriendInviteType.Request);
		}
	}

	public static void SendRequestFriend(long friendId, string nickName, FriendInviteType inviteType, Action<FriendInfo> onResponse)
	{
		SceneAutoBehaviour<MyFriendManagement>.instance.AddTask(new WebServiceTask<NShared.RequestFriend.Request, NShared.RequestFriend.Response>(new NShared.RequestFriend.Request
		{
			FriendId = friendId,
			InviteType = inviteType
		}, delegate(NShared.RequestFriend.Request request, NShared.RequestFriend.Response response)
		{
			if (response.Result != NShared.RequestFriend.ResultType.Success)
			{
				string msg = string.Empty;
				bool flag = true;
				switch (response.Result)
				{
				case NShared.RequestFriend.ResultType.TargetFriendFull:
					msg = LocalizationManager.GetStringDirect("NOTY_FRIEND_ADD_FAIL_OTHER_FULL", nickName);
					break;
				case NShared.RequestFriend.ResultType.TargetFriendWaitFull:
					msg = LocalizationManager.GetStringDirect("NOTY_FRIEND_ADD_FAIL_REQUEST_FULL", nickName);
					break;
				default:
					msg = LocalizationManager.GetString(response.Result);
					flag = false;
					break;
				}
				if (flag)
				{
					return SceneAutoBehaviour<MyFriendManagement>.instance.TaskFactory.Run(() => CenterMessage.Open(msg));
				}
				return SceneAutoBehaviour<MyFriendManagement>.instance.TaskFactory.Run(delegate
				{
					MessagePopup.OpenError(msg);
				});
			}
			return SceneAutoBehaviour<MyFriendManagement>.instance.TaskFactory.Run(delegate
			{
				GlobalAutoBehaviour<AccountManager>.instance.FriendManager.AddInvitee(response.FriendInfo);
				Vespa_System_Action_Extension_Method.fire(onResponse, response.FriendInfo);
			});
		}));
	}

	private void SendRequestFriend(long friendId, string nickName, FriendInviteType inviteType)
	{
		SendRequestFriend(friendId, nickName, inviteType, delegate
		{
			AddTask(base.TaskFactory.Run(delegate(FriendMenu window)
			{
				window.RemoveFriendRecomment(friendId);
			}), base.TaskFactory.Run(delegate
			{
				if (GlobalAutoBehaviour<AccountManager>.instance.FriendManager.IsFriend(friendId))
				{
					FriendUserInfo invitorInfo = GlobalAutoBehaviour<AccountManager>.instance.FriendManager.GetInvitorInfo(friendId);
					if (invitorInfo != null)
					{
						CenterMessage.Open(LocalizationManager.GetStringDirect("NOTY_FRIEND_ADD_SUCCESS", invitorInfo.Nick));
					}
					GlobalAutoBehaviour<AccountManager>.instance.FriendManager.OnFriendChanged();
					_ = (object)null;
					_ = 41;
					_ = 1;
					_ = 0;
					Array.Empty<object>();
				}
				else if (!String_Extension_Method.IsNullOrEmpty(nickName))
				{
					CenterMessage.Open(LocalizationManager.GetStringDirect("NOTY_FRIEND_ADD_APPLY", nickName));
				}
			}), base.TaskFactory.Run(delegate(FriendMenu window)
			{
				window.UpdateList();
			}));
		});
	}

	private void SendRejectFriend(long friendId, string rejectNickName = "")
	{
		AddTask(new WebServiceTask<NShared.RejectFriend.Request, NShared.RejectFriend.Response>(new NShared.RejectFriend.Request
		{
			FriendId = friendId
		}, (NShared.RejectFriend.Request request, NShared.RejectFriend.Response response) => (response.Result != NShared.RejectFriend.ResultType.Success) ? base.TaskFactory.Run(delegate
		{
			MessagePopup.OpenError(response.Result);
		}) : base.TaskFactory.Sequence(base.TaskFactory.Run(delegate
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.FriendManager.IsFriend(friendId))
			{
				CenterMessage.Open(LocalizationManager.GetString(Friend.NOTY_FRIEND_DELETE));
			}
		}), base.TaskFactory.Run(delegate
		{
			if (!String_Extension_Method.IsNullOrEmpty(rejectNickName))
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("NOTY_FRIEND_REJECT_SUCCESS", rejectNickName));
			}
			GlobalAutoBehaviour<AccountManager>.instance.FriendManager.RemoveFriend(friendId);
			GlobalAutoBehaviour<AccountManager>.instance.FriendManager.OnFriendChanged();
		}), base.TaskFactory.Run(delegate(FriendMenu window)
		{
			window.UpdateList();
		}))));
	}

	private void RequestSendRecvFriendshipPoint()
	{
		GlobalAutoBehaviour<AccountManager>.instance.SendRecvFriendshipPointManager.GetSendOrRecvFriendshipPointFriendIds(out var SendFriendIds, out var RecvFriendIds);
		if (__Extension_Method_ICollection__.COUNT(SendFriendIds) == 0 && __Extension_Method_ICollection__.COUNT(RecvFriendIds) == 0)
		{
			VespaDebug.LogError("No Send or Recv FriendIds");
			return;
		}
		AddTask(new WebServiceTask<NShared.SendRecvFriendshipPoint.Request, NShared.SendRecvFriendshipPoint.Response>(new NShared.SendRecvFriendshipPoint.Request
		{
			SendFriendIds = SendFriendIds.ToArray(),
			RecvFriendIds = RecvFriendIds.ToArray()
		}, (NShared.SendRecvFriendshipPoint.Request request, NShared.SendRecvFriendshipPoint.Response response) => (response.Result != NShared.SendRecvFriendshipPoint.ResultType.Success) ? base.TaskFactory.Run(delegate
		{
			MessagePopup.OpenError(response.Result);
		}) : base.TaskFactory.Sequence(base.TaskFactory.Run(delegate
		{
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.FriendshipPointResult);
			GlobalAutoBehaviour<AccountManager>.instance.SendRecvFriendshipPointManager.Apply(response.FriendshipPointInfos);
			BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetString(Friend.GET_FRIENDSHIP_POINT)).SetFriendshipPointResult(response.FriendshipPointResult), isGlobal: false);
		}))));
	}

	private void RequestSendFriendshipPoint(long[] friendIds)
	{
		AddTask(new WebServiceTask<NShared.SendFriendshipPoint.Request, NShared.SendFriendshipPoint.Response>(new NShared.SendFriendshipPoint.Request
		{
			FriendIds = friendIds
		}, (NShared.SendFriendshipPoint.Request request, NShared.SendFriendshipPoint.Response response) => (response.Result != NShared.SendFriendshipPoint.ResultType.Success) ? base.TaskFactory.Run(delegate
		{
			MessagePopup.OpenError(response.Result);
		}) : base.TaskFactory.Sequence(base.TaskFactory.Run(delegate
		{
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.FriendshipPointResult);
			GlobalAutoBehaviour<AccountManager>.instance.SendRecvFriendshipPointManager.Apply(response.FriendshipPointInfos);
			BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetString(Friend.GET_FRIENDSHIP_POINT)).SetFriendshipPointResult(response.FriendshipPointResult), isGlobal: false);
		}))));
	}

	private void RequestRecvFriendshipPoint(long[] friendIds)
	{
		AddTask(new WebServiceTask<NShared.RecvFriendshipPoint.Request, NShared.RecvFriendshipPoint.Response>(new NShared.RecvFriendshipPoint.Request
		{
			FriendIds = friendIds
		}, (NShared.RecvFriendshipPoint.Request request, NShared.RecvFriendshipPoint.Response response) => (response.Result != NShared.RecvFriendshipPoint.ResultType.Success) ? base.TaskFactory.Run(delegate
		{
			MessagePopup.OpenError(response.Result);
		}) : base.TaskFactory.Sequence(base.TaskFactory.Run(delegate
		{
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.FriendshipPointResult);
			GlobalAutoBehaviour<AccountManager>.instance.SendRecvFriendshipPointManager.Apply(response.FriendshipPointInfos);
			BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetString(Friend.GET_FRIENDSHIP_POINT)).SetFriendshipPointResult(response.FriendshipPointResult), isGlobal: false);
		}))));
	}

	private void SendSearchFriend(string keyword, Action<CachedUserInfo[]> onFriends)
	{
		AddTask(new WebServiceTask<NShared.SearchFriend.Request, NShared.SearchFriend.Response>(new NShared.SearchFriend.Request
		{
			Keyword = keyword
		}, (NShared.SearchFriend.Request request, NShared.SearchFriend.Response response) => (response.Result != NShared.SearchFriend.ResultType.Success) ? base.TaskFactory.Run(delegate
		{
			MessagePopup.OpenError(response.Result);
			onFriends(null);
		}) : base.TaskFactory.Branch(base.TaskFactory.Run(() => string.IsNullOrEmpty(keyword) && (response.FriendInfos == null || response.FriendInfos.Length == 0)), new WebServiceTask<NShared.SearchFriend.Request, NShared.SearchFriend.Response>(request, (NShared.SearchFriend.Request secondrequest, NShared.SearchFriend.Response secondResponse) => (response != null && secondResponse.FriendInfos != null) ? base.TaskFactory.Run(delegate
		{
			onFriends(secondResponse.FriendInfos);
		}) : base.TaskFactory.Run(delegate
		{
			onFriends(null);
		})), base.TaskFactory.Run(delegate
		{
			onFriends(response.FriendInfos);
		}))));
	}

	public static void SendInspectUser(long accountId, Action<NShared.InspectUser.Response> onCachedAccountInfo)
	{
		if (accountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId)
		{
			Vespa_System_Action_Extension_Method.fire(onCachedAccountInfo, null);
			return;
		}
		SceneAutoBehaviour<MyFriendManagement>.instance.AddTask(new WebServiceTask<NShared.InspectUser.Request, NShared.InspectUser.Response>(new NShared.InspectUser.Request
		{
			TargetAccountId = accountId
		}, (NShared.InspectUser.Request request, NShared.InspectUser.Response response) => (response.Result != NShared.InspectUser.ResultType.Success) ? SceneAutoBehaviour<MyFriendManagement>.instance.TaskFactory.Run(delegate
		{
			MessagePopup.OpenError(response.Result);
			onCachedAccountInfo(null);
		}) : SceneAutoBehaviour<MyFriendManagement>.instance.TaskFactory.Run(delegate
		{
			onCachedAccountInfo(response);
		})));
	}
}
