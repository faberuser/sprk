using System;
using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NAccount;

public class UserSendRecvFriendshipPointManager : NShared.UserSendRecvFriendshipPointManager
{
	public enum FriendshipPointSendRecvType
	{
		CanSend,
		SentNotReceived,
		SentCanReceive,
		SentReceived
	}

	private readonly UserObserverManager<FriendshipPointActionInfo> _observerManager = new UserObserverManager<FriendshipPointActionInfo>();

	private readonly List<FriendshipPointInfo> _RecvFriendshipPoints;

	public IUserObserverManager ObserverManager => _observerManager;

	public UserSendRecvFriendshipPointManager()
	{
		_RecvFriendshipPoints = new List<FriendshipPointInfo>();
	}

	public void Set(FriendshipPointInfo[] friendshipPointInfos, FriendshipPointInfo[] recvFriendshipPointInfos)
	{
		Set(friendshipPointInfos);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(recvFriendshipPointInfos))
		{
			_RecvFriendshipPoints.AddRange(recvFriendshipPointInfos);
		}
	}

	public void Apply(FriendshipPointInfo[] friendshipPointInfos)
	{
		Set(friendshipPointInfos);
		if (friendshipPointInfos != null)
		{
			for (int i = 0; i < friendshipPointInfos.Length; i++)
			{
				_observerManager.AnnounceToObservers(new FriendshipPointActionInfo
				{
					ChangeType = FriendshipPointActionInfo.EChangeType.DidAction,
					friendId = friendshipPointInfos[i].FriendId
				});
			}
			_observerManager.AnnounceToObservers(new FriendshipPointActionInfo
			{
				ChangeType = FriendshipPointActionInfo.EChangeType.ListUpdated
			});
		}
	}

	public void AddRecvFriendshipPointInfo(FriendshipPointInfo friendshipPointInfo)
	{
		if (friendshipPointInfo != null)
		{
			_RecvFriendshipPoints.RemoveAll((FriendshipPointInfo e) => e.FriendId == friendshipPointInfo.FriendId);
			_RecvFriendshipPoints.Add(friendshipPointInfo);
			_observerManager.AnnounceToObservers(new FriendshipPointActionInfo
			{
				ChangeType = FriendshipPointActionInfo.EChangeType.DidAction,
				friendId = friendshipPointInfo.FriendId
			});
			_observerManager.AnnounceToObservers(new FriendshipPointActionInfo
			{
				ChangeType = FriendshipPointActionInfo.EChangeType.ListUpdated
			});
		}
	}

	public bool CanSendFriendshipPoint(long friendId)
	{
		FriendshipPointInfo friendshipPointInfo = Get(friendId);
		if (friendshipPointInfo == null)
		{
			return true;
		}
		return !IsValidTime(friendshipPointInfo.LastSendTime);
	}

	public bool FriendSentFriendshipPoint(long friendId)
	{
		FriendshipPointInfo friendshipPointInfo = _RecvFriendshipPoints.Find((FriendshipPointInfo info) => info.FriendId == friendId);
		if (friendshipPointInfo == null)
		{
			return false;
		}
		return IsValidTime(friendshipPointInfo.LastSendTime);
	}

	public bool CanRecvFriendshipPoint(long friendId)
	{
		if (CanSendFriendshipPoint(friendId))
		{
			return false;
		}
		if (!FriendSentFriendshipPoint(friendId))
		{
			return false;
		}
		FriendshipPointInfo friendshipPointInfo = Get(friendId);
		return !IsValidTime(friendshipPointInfo.LastRecvTime);
	}

	public bool IsValidTime(string time)
	{
		if (String_Extension_Method.IsNullOrEmpty(time))
		{
			return false;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(time, out var o_dateTime))
		{
			return false;
		}
		DateTime dateTime = GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(o_dateTime);
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		DateTime value = NVespa.Math.CreateDateTime(currentServerLocalTime.Year, currentServerLocalTime.Month, currentServerLocalTime.Day);
		if (dateTime.CompareTo(value) < 0 || dateTime.CompareTo(value.AddDays(1.0)) > 0)
		{
			return false;
		}
		return true;
	}

	public bool SendOrRecvFriendshipPointFriendExist()
	{
		long[] friendIds = GlobalAutoBehaviour<AccountManager>.instance.FriendManager.GetFriendIds();
		foreach (long friendId in friendIds)
		{
			if (CanSendFriendshipPoint(friendId))
			{
				return true;
			}
			if (CanRecvFriendshipPoint(friendId))
			{
				return true;
			}
		}
		return false;
	}

	public void GetSendOrRecvFriendshipPointFriendIds(out List<long> SendFriendIds, out List<long> RecvFriendIds)
	{
		SendFriendIds = new List<long>();
		RecvFriendIds = new List<long>();
		long[] friendIds = GlobalAutoBehaviour<AccountManager>.instance.FriendManager.GetFriendIds();
		foreach (long num in friendIds)
		{
			if (CanSendFriendshipPoint(num))
			{
				SendFriendIds.Add(num);
			}
			else if (CanRecvFriendshipPoint(num))
			{
				RecvFriendIds.Add(num);
			}
		}
	}

	public FriendshipPointSendRecvType GetSendRecvType(long friendId)
	{
		Get(friendId);
		if (CanSendFriendshipPoint(friendId))
		{
			return FriendshipPointSendRecvType.CanSend;
		}
		if (!FriendSentFriendshipPoint(friendId))
		{
			return FriendshipPointSendRecvType.SentNotReceived;
		}
		if (CanRecvFriendshipPoint(friendId))
		{
			return FriendshipPointSendRecvType.SentCanReceive;
		}
		return FriendshipPointSendRecvType.SentReceived;
	}
}
