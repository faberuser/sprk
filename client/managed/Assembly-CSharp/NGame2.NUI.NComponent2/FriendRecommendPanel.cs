using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class FriendRecommendPanel : MonoBehaviour, FriendSlotComponent.Listener
{
	public interface Listener
	{
		void InviteRequest(CachedUserInfo friendInfo);

		void GetRecommendFriends(Action<CachedUserInfo[]> onFriends);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Friend/FriendRecommendPanel";

	public static PrefabAuto<FriendRecommendPanel> PrefabAuto = new PrefabAuto<FriendRecommendPanel>("Resources/Prefab2/Component/Friend/FriendRecommendPanel");

	public UIScrollGrid FriendRecommendGrid;

	public UIInput input;

	private List<FriendSlotComponent> _componentList = new List<FriendSlotComponent>();

	private Listener _listener;

	private bool _isRequesting;

	private CachedUserInfo[] _friendInfos;

	public void Open(Listener listener)
	{
		_friendInfos = null;
		Reset();
		base.gameObject.SetActive(value: true);
		if (input != null)
		{
			input.value = string.Empty;
			input.label.text = string.Empty;
		}
		_listener = listener;
		if (_friendInfos == null || _friendInfos.Length == 0)
		{
			RefreshRecommendFriendList();
		}
		else
		{
			SetupFriendSlotComponent(_friendInfos);
		}
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		Reset();
	}

	public void Reset()
	{
		_listener = null;
		ClearFriendList();
	}

	public bool RemoveFriendList(long friendId)
	{
		foreach (FriendSlotComponent component in _componentList)
		{
			if (component.FriendInfo != null && component.FriendInfo.AccountId == friendId)
			{
				_componentList.Remove(component);
				FactoryBase.ReleaseComponent(component);
				UIHelper.RepositionGrid(FriendRecommendGrid);
				return true;
			}
		}
		return false;
	}

	private void ClearFriendList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	private bool RemoveCondition(CachedUserInfo userInfo)
	{
		if (userInfo != null && userInfo.TeamLevel != 0)
		{
			return string.IsNullOrEmpty(userInfo.Nick);
		}
		return true;
	}

	public void RefreshRecommendFriendList()
	{
		if (_isRequesting)
		{
			return;
		}
		_isRequesting = true;
		_listener.GetRecommendFriends(delegate(CachedUserInfo[] friendInfos)
		{
			List<CachedUserInfo> list = new List<CachedUserInfo>();
			if (friendInfos != null)
			{
				list.AddRange(friendInfos);
				list.RemoveAll(RemoveCondition);
			}
			_isRequesting = false;
			_friendInfos = list.ToArray();
			ClearFriendList();
			_ = _friendInfos;
			_ = (Comparison<CachedUserInfo>)delegate(CachedUserInfo l, CachedUserInfo r)
			{
				int num = r.TeamLevel.CompareTo(l.TeamLevel);
				if (num == 0)
				{
					num = l.AccountId.CompareTo(r.AccountId);
				}
				return num;
			};
			SetupFriendSlotComponent(_friendInfos);
		});
	}

	public void SetupSearchFriendSlotComponent(CachedUserInfo[] friendInfos)
	{
		ClearFriendList();
		if (friendInfos != null && friendInfos.Length != 0)
		{
			SetupFriendSlotComponent(friendInfos);
		}
	}

	private void SetupFriendSlotComponent(CachedUserInfo[] friendInfos)
	{
		if (friendInfos == null)
		{
			return;
		}
		foreach (CachedUserInfo cachedUserInfo in friendInfos)
		{
			if (!GlobalAutoBehaviour<AccountManager>.instance.FriendManager.IsInvitee(cachedUserInfo.AccountId) && !GlobalAutoBehaviour<AccountManager>.instance.FriendManager.IsInvitor(cachedUserInfo.AccountId))
			{
				FriendSlotComponent friendSlotComponent = UIHelper.AddGridItem(FriendRecommendGrid, FriendSlotComponent.PrefabAutoRecommend);
				friendSlotComponent.Open(cachedUserInfo, this);
				_componentList.Add(friendSlotComponent);
			}
		}
		UIHelper.RepositionGrid(FriendRecommendGrid);
	}

	void FriendSlotComponent.Listener.InviteRequest(FriendSlotComponent This)
	{
		if (_listener != null)
		{
			_listener.InviteRequest(This.FriendInfo);
		}
	}

	void FriendSlotComponent.Listener.Duel(FriendSlotComponent This)
	{
	}

	void FriendSlotComponent.Listener.Whisper(FriendSlotComponent This)
	{
	}

	void FriendSlotComponent.Listener.ShowHeroes(FriendSlotComponent This)
	{
	}

	void FriendSlotComponent.Listener.Delete(FriendSlotComponent This)
	{
	}

	void FriendSlotComponent.Listener.InviteParty(FriendSlotComponent This)
	{
	}

	void FriendSlotComponent.Listener.InviteAccept(FriendSlotComponent This)
	{
	}

	void FriendSlotComponent.Listener.InviteDeny(FriendSlotComponent This)
	{
	}

	void FriendSlotComponent.Listener.sendFriendshipPoint(FriendSlotComponent This)
	{
	}

	void FriendSlotComponent.Listener.recvFriendshipPoint(FriendSlotComponent This)
	{
	}
}
