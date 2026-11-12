using System;
using System.Collections.Generic;
using System.Linq;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LoginSelect : BaseWindow
{
	public enum LoginMethodType
	{
		Guest,
		Facebook,
		GooglePlus
	}

	public interface Listener
	{
		void OnServerList(LoginSelect caller);

		void OnSelect(LoginSelect caller, NShared.LoginMethodType methodType, string serverName, bool UserSelect = false);

		void OnLoginByAccountId(LoginSelect caller, int accountId);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Login/LoginSelect";

	public UIButton Button_AppleSignin;

	public UIButton Button_TwitterSignIn;

	public UIButton Button_LINESignIn;

	public UIButton Button_ServerSelect;

	public UILabel Label_ServerName;

	public Transform Root_Development;

	public UIInput Input_ForcedAccountId;

	public UITable LoginButtonTable;

	public UIButton Button_GuestSignIn;

	public List<UIButton> IOSOnlyButtons = new List<UIButton>();

	public List<UIButton> AndroidOnlyButtons = new List<UIButton>();

	private Listener _listener;

	private AppServerInfo[] _serverDatas;

	private string _serverName;

	public void Open(Listener listener, NShared.LoginMethodType lastMethod, AppServerInfo[] serverDatas, string lastServerName)
	{
		SprkAccounts.Open(this, listener, lastMethod, serverDatas, lastServerName);
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		_serverDatas = null;
		_serverName = string.Empty;
	}

	public void SelectServer(string serverName)
	{
		if (__Extension_Method_ICollection__.COUNT(_serverDatas) != 0)
		{
			AppServerInfo appServerInfo = _serverDatas.FirstOrDefault((AppServerInfo e) => string.Equals(e.Name, serverName, StringComparison.InvariantCultureIgnoreCase));
			if (appServerInfo == null)
			{
				appServerInfo = _serverDatas[0];
			}
			_serverName = appServerInfo.Name;
			UIHelper.SetLabelText(Label_ServerName, appServerInfo.Name);
		}
	}

	public void OnClickGuest()
	{
	}

	public void OnClickFacebook()
	{
	}

	public void OnClickGooglePlus()
	{
	}

	public void OnClickGameCenter()
	{
	}

	public void OnClickApple()
	{
	}

	public void OnClickLoginTwitter()
	{
	}

	public void OnClickLoginLine()
	{
	}

	public void OnClickServerSelect()
	{
		if (_listener != null)
		{
			_listener.OnServerList(this);
		}
	}

	public void OnClickLoginByAccountId()
	{
	}

	public void LoginByAccountId(string strAccountId)
	{
	}
}
