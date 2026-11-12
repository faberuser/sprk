using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NShared.SearchFriend;

namespace NGame2.NUI.NWindow;

public class FriendMenu : LobbyFullWindow, IUserObserver<FriendChangeInfo>, IUserObserver, IUserObserver<FriendshipPointActionInfo>, IUserObserver<UserManager.UserStatInfo>, LobbyTopmostMenu.IUserStatObserver
{
	public enum EToggle
	{
		List,
		InviteAndRecommend
	}

	public interface Listener
	{
		void OnSearch();

		void SearchFriend(string name);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Friend/FriendMenu";

	public UIToggle ToggleList;

	public UIToggle ToggleInvite;

	private Listener _listener;

	public FriendInvitePanel InvitePanel;

	public FriendListPanel ListPanel;

	public FriendRecommendPanel RecommendPanel;

	public EToggle _toggle { get; private set; }

	public UserManager.UserStatType StatType => UserManager.UserStatType.FriendshipPoint;

	public void Open(EToggle toggle, Listener listener)
	{
		if (!IsOpen())
		{
			Open();
		}
		else
		{
			ShowRoot(show: true);
		}
		_listener = listener;
		MakeTopmostToggleGroup(ToggleList, ToggleInvite);
		_toggle = toggle;
		if (_toggle == EToggle.List)
		{
			ToggleList.value = true;
			ToggleInvite.value = false;
		}
		else if (_toggle == EToggle.InviteAndRecommend)
		{
			ToggleList.value = false;
			ToggleInvite.value = true;
		}
		OpenComponent(toggle, listener);
	}

	public void OpenComponent(EToggle toggle, Listener listener)
	{
		switch (toggle)
		{
		case EToggle.List:
			ListPanel.Open(listener as FriendListPanel.Listener);
			InvitePanel.Close();
			RecommendPanel.Close();
			break;
		case EToggle.InviteAndRecommend:
			ListPanel.Close();
			InvitePanel.Open(listener as FriendInvitePanel.Listener);
			RecommendPanel.Open(listener as FriendRecommendPanel.Listener);
			break;
		}
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		ListPanel.Reset();
		RecommendPanel.Reset();
		InvitePanel.Reset();
	}

	public void OnToggleChanged()
	{
		EToggle eToggle = EToggle.List;
		if (ToggleList.value)
		{
			eToggle = EToggle.List;
		}
		else if (ToggleInvite.value)
		{
			eToggle = EToggle.InviteAndRecommend;
		}
		if (_toggle != eToggle)
		{
			OpenComponent(eToggle, _listener);
			_toggle = eToggle;
		}
	}

	public void OnClickSearch()
	{
		if (_listener != null)
		{
			_listener.OnSearch();
		}
	}

	public void Search()
	{
		if (_listener != null && (bool)RecommendPanel.input)
		{
			string text = RecommendPanel.input.label.text;
			if (string.IsNullOrEmpty(text))
			{
				MessagePopup.OpenError(ResultType.FriendNotFound);
			}
			else
			{
				_listener.SearchFriend(text);
			}
		}
	}

	public void SearchRandom()
	{
		_listener.SearchFriend(null);
	}

	public void SetupSearchFriendRecommend(CachedUserInfo[] friendInfos)
	{
		RecommendPanel.SetupSearchFriendSlotComponent(friendInfos);
	}

	public void RemoveFriendRecomment(long friendId)
	{
		RecommendPanel.RemoveFriendList(friendId);
	}

	public void UpdateStatusList(long friendId)
	{
		ListPanel.UpdateStatus(friendId);
	}

	public void UpdateList()
	{
		if (ListPanel != null && ListPanel.gameObject.activeSelf)
		{
			ListPanel.UpdateList();
		}
		if (InvitePanel != null && InvitePanel.gameObject.activeSelf)
		{
			InvitePanel.UpdateList();
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<FriendChangeInfo>.OnChanged(FriendChangeInfo changedInfo)
	{
		if (changedInfo.Type == FriendChangeInfo.ChangeType.RemoveFriend || changedInfo.Type == FriendChangeInfo.ChangeType.AddInvitor)
		{
			ListPanel.UpdateList();
			InvitePanel.UpdateList();
		}
		ListPanel.UpdateChangedInfo(changedInfo.FriendId);
	}

	void IUserObserver<FriendshipPointActionInfo>.OnChanged(FriendshipPointActionInfo info)
	{
		if (info.ChangeType == FriendshipPointActionInfo.EChangeType.DidAction)
		{
			ListPanel.UpdateChangedInfo(info.friendId);
		}
		if (info.ChangeType == FriendshipPointActionInfo.EChangeType.ListUpdated)
		{
			ListPanel.UpdateSendRecvFriendButton();
		}
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo statInfo)
	{
		if (statInfo.UserStatType == UserManager.UserStatType.DailyAccFriendshipPoint && ListPanel != null && ListPanel.gameObject.activeSelf)
		{
			ListPanel.SetupDailyAccFriendshipPoint();
		}
	}
}
