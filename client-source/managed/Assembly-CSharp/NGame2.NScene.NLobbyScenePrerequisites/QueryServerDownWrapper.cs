using System;
using NGame2.NLogin;
using NGame2.NUI.NWindow;
using NGame2.NUtil;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NScene.NLobbyScenePrerequisites;

internal class QueryServerDownWrapper
{
	public static bool ForceTimeout;

	public const float Timeout = 10f;

	private QueryServerDown _queryServerDown = new QueryServerDown();

	private float _queryTime;

	public bool IsDoneOrTimeout
	{
		get
		{
			if (!IsDone)
			{
				return IsTimeout;
			}
			return true;
		}
	}

	public bool IsDone { get; private set; }

	public bool IsTimeout
	{
		get
		{
			if (IsDone)
			{
				return false;
			}
			return Time.realtimeSinceStartup - _queryTime > 10f;
		}
	}

	public string ResultMessage => _queryServerDown.ResultMessage;

	public void Query()
	{
		try
		{
			QueryInternal();
		}
		catch (Exception i_object)
		{
			Debug.LogError("QueryServerDown.Query exception: " + Vespa_Common_Class_Extension_Method.TOSTRING(i_object));
			IsDone = true;
		}
	}

	private void QueryInternal()
	{
		_queryTime = Time.realtimeSinceStartup;
		if (!ForceTimeout)
		{
			if (!SingletonUpdater<LoginManager>.isValidInstance)
			{
				IsDone = true;
			}
			if (SingletonUpdater<LoginManager>.instance.ServerInfo == null)
			{
				IsDone = true;
			}
			Debug.Log("QueryServerDown.Query: url=" + SingletonUpdater<LoginManager>.instance.ServerInfo.QueryServerStatusUrl);
			_queryServerDown.Query(SingletonUpdater<LoginManager>.instance.ServerInfo.QueryServerStatusUrl, delegate
			{
				IsDone = true;
			});
		}
	}

	public bool QuitIfServerDown()
	{
		try
		{
			if (IsTimeout)
			{
				Debug.Log("QueryServerDown Timeout");
				return false;
			}
			if (_queryServerDown == null)
			{
				return false;
			}
			if (_queryServerDown.Verify())
			{
				return false;
			}
			if (String_Extension_Method.IsNullOrEmpty(_queryServerDown.ResultMessage))
			{
				return false;
			}
			Debug.Log("QueryServerDown.QuitIfServerDown: " + _queryServerDown.ResultMessage);
			GlobalMessagePopup.OpenGlobal(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), _queryServerDown.ResultMessage, delegate
			{
				GlobalAutoBehaviour<LoadingManager>.instance.Quit();
			}, retryButton: false);
			return true;
		}
		catch (Exception i_object)
		{
			Debug.LogError("QueryServerDown.QuitIfServerDown exception: " + Vespa_Common_Class_Extension_Method.TOSTRING(i_object));
		}
		return false;
	}
}
