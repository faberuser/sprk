using System.Collections.Generic;
using System.Linq;
using NGame2.NSocket;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NAccount;

public class UserFriendManager : NShared.UserFriendManager
{
	private readonly UserObserverManager<FriendChangeInfo> _observerManager = new UserObserverManager<FriendChangeInfo>();

	private readonly List<FriendUserInfo> _invitorList;

	public IUserObserverManager ObserverManager => _observerManager;

	public UserFriendManager()
	{
		_invitorList = new List<FriendUserInfo>();
	}

	public void Set(FriendInfo[] invitees, FriendUserInfo[] invitors)
	{
		Set(invitees);
		_invitorList.AddRange(invitors);
	}

	public void AddInvitee(FriendInfo invitee)
	{
		Add(invitee);
	}

	public void AddInvitor(FriendUserInfo invitor)
	{
		if (invitor != null)
		{
			_invitorList.RemoveAll((FriendUserInfo e) => e.AccountId == invitor.AccountId);
			_invitorList.Add(invitor);
			_observerManager.AnnounceToObservers(new FriendChangeInfo
			{
				FriendId = invitor.AccountId,
				Type = FriendChangeInfo.ChangeType.AddInvitor
			});
		}
	}

	public bool IsFriend(long friendId)
	{
		if (Get(friendId) == null)
		{
			return false;
		}
		if (!_invitorList.Any((FriendUserInfo e) => e.AccountId == friendId))
		{
			return false;
		}
		return true;
	}

	public bool IsInvitee(long friendId)
	{
		return Get(friendId) != null;
	}

	public bool IsInvitor(long friendId)
	{
		return GetInvitorInfo(friendId) != null;
	}

	public void RemoveFriend(long friendId)
	{
		Remove(friendId);
		_invitorList.RemoveAll((FriendUserInfo e) => e.AccountId == friendId);
		_observerManager.AnnounceToObservers(new FriendChangeInfo
		{
			FriendId = friendId,
			Type = FriendChangeInfo.ChangeType.RemoveFriend
		});
	}

	public FriendUserInfo GetInvitorInfo(long friendId)
	{
		return _invitorList.FirstOrDefault((FriendUserInfo e) => e.AccountId == friendId);
	}

	public FriendUserInfo GetFriendInfo(long friendId)
	{
		if (!IsInvitee(friendId))
		{
			return null;
		}
		FriendUserInfo invitorInfo = GetInvitorInfo(friendId);
		if (invitorInfo == null)
		{
			return null;
		}
		return invitorInfo;
	}

	public FriendUserInfo[] GetFriendInfos()
	{
		List<FriendUserInfo> list = new List<FriendUserInfo>();
		using (IEnumerator<FriendInfo> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				FriendInfo current = enumerator.Current;
				FriendUserInfo invitorInfo = GetInvitorInfo(current.FriendId);
				if (invitorInfo != null)
				{
					list.Add(invitorInfo);
				}
			}
		}
		return list.ToArray();
	}

	public long[] GetFriendIds()
	{
		List<long> list = new List<long>();
		using (IEnumerator<FriendInfo> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				FriendInfo info = enumerator.Current;
				if (_invitorList.Any((FriendUserInfo e) => e.AccountId == info.FriendId))
				{
					list.Add(info.FriendId);
				}
			}
		}
		return list.ToArray();
	}

	public FriendUserInfo[] GetInvitorInfos()
	{
		List<FriendUserInfo> list = new List<FriendUserInfo>();
		for (int i = 0; i < _invitorList.Count; i++)
		{
			FriendUserInfo friendUserInfo = _invitorList[i];
			if (Get(friendUserInfo.AccountId) == null)
			{
				list.Add(friendUserInfo);
			}
		}
		return list.ToArray();
	}

	public int GetFriendRequestCount()
	{
		int num = 0;
		using (IEnumerator<FriendInfo> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				FriendInfo info = enumerator.Current;
				if (_invitorList.Any((FriendUserInfo e) => e.AccountId == info.FriendId))
				{
					num++;
				}
			}
		}
		return _invitorList.Count - num;
	}

	public void OnFriendChanged()
	{
		long[] friendIds = GetFriendIds();
		GlobalAutoBehaviour<SocketServiceManager>.instance.MessageConnector.ChangeFriend(friendIds);
	}

	public void SetNick(long friendId, string nick)
	{
		FriendUserInfo friendInfo = GetFriendInfo(friendId);
		if (friendInfo != null && friendInfo.Nick != nick)
		{
			friendInfo.Nick = nick;
			_observerManager.AnnounceToObservers(new FriendChangeInfo
			{
				FriendId = friendId,
				Type = FriendChangeInfo.ChangeType.NickChange
			});
		}
	}

	public void SetAvatarIndex(long friendId, int avatarHeroIndex)
	{
		FriendUserInfo friendInfo = GetFriendInfo(friendId);
		if (friendInfo != null && friendInfo.AvatarHeroIndex != avatarHeroIndex)
		{
			friendInfo.AvatarHeroIndex = avatarHeroIndex;
			_observerManager.AnnounceToObservers(new FriendChangeInfo
			{
				FriendId = friendId,
				Type = FriendChangeInfo.ChangeType.AvatarHeroIndex
			});
		}
	}
}
