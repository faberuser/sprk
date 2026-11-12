using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NChatting;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NChatting;

public class ChattingWhisperComponent : ChattingListComponentBase, WhisperUserComponent.Listener
{
	public new interface Listener : ChattingListComponentBase.Listener
	{
		void OnSubmit(long accountId, string userName, string chat);

		void OnAdd();

		void OnRemove(long accountId, string userName);

		List<ChatInfo> GetWhisperChat(long accountId);

		int GetAlertCount(long accountId);

		void OnSelectUser(long accountId);

		List<Tuple<long, string>> GetWhisperUser();

		void OnClickGameOption();
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Chatting/ChattingWhisperPanelComponent";

	public const string BattlePrefabPath = "Resources/Prefab2/Component/BattleChatting/BattleChattingWhisperPanelComponent";

	public UIScrollGrid UserGrid;

	public Transform NoUserRoot;

	private Listener _listener;

	private List<WhisperUserComponent> _userComponentList = new List<WhisperUserComponent>();

	private long _currentAccountId;

	private static long _lastAccountId;

	private ItemLinkButtonComponent ItemLinkButtonComponent;

	public GameObject ItemLinkButtonRoot;

	private bool HasTargetUser;

	public void Open(Listener listener)
	{
		Open((ChattingListComponentBase.Listener)listener);
		_listener = listener;
		InitUser();
		ItemLinkButtonComponent.AddComponent(ref ItemLinkButtonRoot, ref ItemLinkButtonComponent, ItemLinkButtonComponent.EItemLinkButtonType.Share, ChatType.Whisper);
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		_currentAccountId = 0L;
		Input.value = string.Empty;
		ClearUser();
	}

	private void InitUser()
	{
		List<Tuple<long, string>> whisperUser = _listener.GetWhisperUser();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(whisperUser); i++)
		{
			Tuple<long, string> tuple = whisperUser[i];
			long item = tuple.Item1;
			string item2 = tuple.Item2;
			AddUser(item, item2);
		}
	}

	private void ClearUser()
	{
		FactoryBase.ReleaseAndClearList(_userComponentList);
	}

	public void Update()
	{
		UpdateEmoticonSelectButtonState();
		foreach (WhisperUserComponent userComponent in _userComponentList)
		{
			int alertCount = _listener.GetAlertCount(userComponent.AccountId);
			if (alertCount != userComponent.AlertCount)
			{
				userComponent.SetAlertCount(alertCount);
			}
		}
		if (ItemLinkButtonComponent != null)
		{
			if (HasTargetUser)
			{
				ItemLinkButtonComponent.UpdateShareButtonState(enable: true);
			}
			else
			{
				ItemLinkButtonComponent.UpdateShareButtonState(enable: false);
			}
		}
	}

	public void OnClickSubmit()
	{
		WhisperUserComponent whisperUserComponent = FindUserComponent(_currentAccountId);
		if (whisperUserComponent == null)
		{
			VespaDebug.LogError("Failed to find user : " + _currentAccountId);
		}
		else if (_listener != null)
		{
			string value = Input.value;
			Input.value = string.Empty;
			_listener.OnSubmit(whisperUserComponent.AccountId, whisperUserComponent.UserName, value);
		}
	}

	public void OnClickAdd()
	{
		if (_listener != null)
		{
			_listener.OnAdd();
		}
	}

	public void OnClickRemove()
	{
		if (_listener != null)
		{
			WhisperUserComponent whisperUserComponent = FindUserComponent(_currentAccountId);
			if (whisperUserComponent != null)
			{
				_listener.OnRemove(whisperUserComponent.AccountId, whisperUserComponent.UserName);
			}
		}
	}

	public void OnClickGameOption()
	{
		if (_listener != null)
		{
			_listener.OnClickGameOption();
		}
	}

	public void AddUser(long accountId, string userName)
	{
		if (!(_userComponentList.Find((WhisperUserComponent e) => e.AccountId == accountId) != null))
		{
			WhisperUserComponent whisperUserComponent = UIHelper.AddGridItem(UserGrid, WhisperUserComponent.GetPrefab());
			whisperUserComponent.Open(accountId, userName, this);
			_userComponentList.Add(whisperUserComponent);
			UIHelper.RepositionGrid(UserGrid);
		}
	}

	public void RemoveUser(long accountId)
	{
		WhisperUserComponent comp = FindUserComponent(accountId);
		if (!(comp == null))
		{
			_userComponentList.Remove(comp);
			FactoryBase.SafeReleaseComponent(ref comp);
			UIHelper.RepositionGrid(UserGrid);
			if (accountId == _currentAccountId)
			{
				long firstAccountId = GetFirstAccountId();
				SelectUser(firstAccountId);
			}
		}
	}

	public void SelectUser(long accountId)
	{
		if (_listener == null || (accountId != 0L && _currentAccountId == accountId))
		{
			return;
		}
		ClearChat();
		if (accountId == 0L && HasReceiver(_lastAccountId))
		{
			accountId = _lastAccountId;
		}
		if (accountId == 0L)
		{
			long firstAccountId = GetFirstAccountId();
			if (firstAccountId != 0L)
			{
				accountId = firstAccountId;
			}
		}
		_currentAccountId = accountId;
		_userComponentList.ForEach(delegate(WhisperUserComponent e)
		{
			e.SetSelected(accountId == e.AccountId);
		});
		bool flag = (HasTargetUser = accountId != 0);
		if (flag)
		{
			CreateDefaultChat();
			List<ChatInfo> whisperChat = _listener.GetWhisperChat(accountId);
			AddChat(whisperChat);
			_listener.OnSelectUser(accountId);
			_lastAccountId = accountId;
			if (ItemLinkButtonComponent != null)
			{
				WhisperUserComponent whisperUserComponent = FindUserComponent(_currentAccountId);
				if (whisperUserComponent == null)
				{
					return;
				}
				ItemLinkButtonComponent.SetWhisperUserComponent(whisperUserComponent);
			}
		}
		Reposition(forceReset: true);
		UIHelper.EnableGameObject(Input, flag);
		UIHelper.EnableGameObject(NoUserRoot, !flag);
	}

	public void SelectUser(string userName)
	{
		WhisperUserComponent whisperUserComponent = FindUserComponent(userName);
		if (!(whisperUserComponent == null))
		{
			SelectUser(whisperUserComponent.AccountId);
		}
	}

	private WhisperUserComponent FindUserComponent(long accountId)
	{
		return _userComponentList.Find((WhisperUserComponent e) => e.AccountId == accountId);
	}

	private WhisperUserComponent FindUserComponent(string userName)
	{
		return _userComponentList.Find((WhisperUserComponent e) => string.Equals(userName, e.UserName, StringComparison.InvariantCultureIgnoreCase));
	}

	public long GetFirstAccountId()
	{
		if (_userComponentList.Count <= 0)
		{
			return 0L;
		}
		return _userComponentList[0].AccountId;
	}

	public long GetCurrentAccountId()
	{
		return _currentAccountId;
	}

	public bool HasReceiver(long accountId)
	{
		return _userComponentList.Any((WhisperUserComponent e) => e.AccountId == accountId);
	}

	public bool HasReceiver(string userName)
	{
		return _userComponentList.Any((WhisperUserComponent e) => string.Equals(userName, e.UserName, StringComparison.InvariantCultureIgnoreCase));
	}

	void WhisperUserComponent.Listener.OnClick(WhisperUserComponent component)
	{
		SelectUser(component.AccountId);
	}

	void WhisperUserComponent.Listener.OnClose(WhisperUserComponent component)
	{
		if (_listener != null)
		{
			long accountId = component.AccountId;
			string userName = component.UserName;
			_listener.OnRemove(accountId, userName);
		}
	}
}
