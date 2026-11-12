using System;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class FriendSearch : BaseWindow, FriendSlotComponent.Listener
{
	public interface Listener
	{
		void SearchFriend(string name, Action<CachedUserInfo[]> onFriends);

		void InviteRequest(CachedUserInfo userInfo);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Friend/FriendSearch";

	[SerializeField]
	private UIInput _input;

	[SerializeField]
	private Transform _componentParent;

	[SerializeField]
	private UILabel NoResultLabel;

	private FriendSlotComponent _component;

	private Listener _listener;

	public void Open(Listener listener)
	{
		base.Open();
		Init(listener);
	}

	public void Init(Listener listener)
	{
		_listener = listener;
	}

	public void Search()
	{
		ClearComponent();
		if (_listener == null || !_input)
		{
			return;
		}
		string text = _input.label.text;
		_listener.SearchFriend(text, delegate(CachedUserInfo[] friendInfos)
		{
			if (friendInfos != null && friendInfos.Length != 0)
			{
				_component = FriendSlotComponent.PrefabAutoRecommend.Factory.Create(_componentParent);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_component.transform);
				_component.Open(friendInfos[0], this);
				UIHelper.EnableGameObject(NoResultLabel, enable: false);
			}
		});
	}

	public override void ReserveCache(UiPreloader preloader)
	{
		base.ReserveCache(preloader);
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		ClearComponent();
	}

	private void ClearComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _component);
		UIHelper.EnableGameObject(NoResultLabel, enable: true);
	}

	void FriendSlotComponent.Listener.InviteAccept(FriendSlotComponent This)
	{
	}

	void FriendSlotComponent.Listener.InviteDeny(FriendSlotComponent This)
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

	void FriendSlotComponent.Listener.Duel(FriendSlotComponent This)
	{
	}

	void FriendSlotComponent.Listener.InviteRequest(FriendSlotComponent This)
	{
		if (_listener != null)
		{
			string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(Friend.FRIEND_ASK_INVITE_FRIEND, This.FriendInfo.Nick);
			MessagePopup.Open(text, message, delegate
			{
				_listener.InviteRequest(This.FriendInfo);
			}, null);
		}
	}

	void FriendSlotComponent.Listener.sendFriendshipPoint(FriendSlotComponent This)
	{
	}

	void FriendSlotComponent.Listener.recvFriendshipPoint(FriendSlotComponent This)
	{
	}
}
