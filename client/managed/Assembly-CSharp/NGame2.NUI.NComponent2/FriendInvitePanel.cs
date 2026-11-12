using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class FriendInvitePanel : MonoBehaviour, FriendSlotComponent.Listener
{
	public interface Listener
	{
		void InviteAccept(long friendId, string nickName);

		void InviteDeny(long friendId, string rejectNickName);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Friend/FriendInvitePanel";

	public static PrefabAuto<FriendInvitePanel> PrefabAuto = new PrefabAuto<FriendInvitePanel>("Resources/Prefab2/Component/Friend/FriendInvitePanel");

	public UIWidget EmptyInviteWidget;

	public UIScrollGrid FriendInviteGrid;

	private List<FriendSlotComponent> _componentList = new List<FriendSlotComponent>();

	private Listener _listener;

	public void Open(Listener listener)
	{
		Reset();
		base.gameObject.SetActive(value: true);
		_listener = listener;
		SetupFriendSlotComponent();
	}

	public void UpdateList()
	{
		SetupFriendSlotComponent();
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		Reset();
	}

	public void Reset()
	{
		_listener = null;
		ClearList();
	}

	private void ClearList()
	{
		UIHelper.EnableGameObject(EmptyInviteWidget, enable: false);
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	private void SetupFriendSlotComponent()
	{
		ClearList();
		FriendUserInfo[] invitorInfos = GlobalAutoBehaviour<AccountManager>.instance.FriendManager.GetInvitorInfos();
		if (__Extension_Method_ICollection__.COUNT(invitorInfos) == 0)
		{
			UIHelper.EnableGameObject(EmptyInviteWidget, enable: true);
			return;
		}
		List<FriendUserInfo> list = new List<FriendUserInfo>();
		list.AddRange(invitorInfos);
		list.Sort(delegate(FriendUserInfo l, FriendUserInfo r)
		{
			DateTime o_dateTime = NGame2.NUI.NWindow.Common.GetUTCNow();
			DateTime o_dateTime2 = NGame2.NUI.NWindow.Common.GetUTCNow();
			Vespa_String_Extension_Method2.TryParseNetworkTime2(l.InvitedTime, out o_dateTime);
			Vespa_String_Extension_Method2.TryParseNetworkTime2(r.InvitedTime, out o_dateTime2);
			return (o_dateTime < o_dateTime2) ? 1 : ((o_dateTime > o_dateTime2) ? (-1) : 0);
		});
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(list); num++)
		{
			FriendUserInfo friendInfo = list[num];
			FriendSlotComponent friendSlotComponent = UIHelper.AddGridItem(FriendInviteGrid, FriendSlotComponent.PrefabAutoInvite);
			friendSlotComponent.Open(friendInfo, this);
			_componentList.Add(friendSlotComponent);
		}
		UIHelper.RepositionGrid(FriendInviteGrid);
		UIHelper.EnableGameObject(EmptyInviteWidget, __Extension_Method_ICollection__.IsNullOrEmpty(_componentList));
	}

	void FriendSlotComponent.Listener.InviteAccept(FriendSlotComponent This)
	{
		if (_listener != null)
		{
			_listener.InviteAccept(This.FriendInfo.AccountId, This.FriendInfo.Nick);
		}
	}

	void FriendSlotComponent.Listener.InviteDeny(FriendSlotComponent This)
	{
		if (_listener != null)
		{
			_listener.InviteDeny(This.FriendInfo.AccountId, This.FriendInfo.Nick);
		}
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

	void FriendSlotComponent.Listener.InviteRequest(FriendSlotComponent This)
	{
	}

	void FriendSlotComponent.Listener.Duel(FriendSlotComponent This)
	{
	}

	void FriendSlotComponent.Listener.sendFriendshipPoint(FriendSlotComponent This)
	{
	}

	void FriendSlotComponent.Listener.recvFriendshipPoint(FriendSlotComponent This)
	{
	}
}
