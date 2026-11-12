using System;
using System.Collections;
using NVespa;
using UnityEngine;

namespace NGame2.NNotification;

public class RemoteRegister : SingletonAuto<RemoteRegister>, IRemoteNotificationReceiver
{
	private IRemoteNotificationManager _remoteNotificationManager;

	private Action<string, string> _onRegister;

	private Action<string> _onUnregister;

	public RemoteRegister()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			_ = Application.platform;
			_ = 8;
		}
		_remoteNotificationManager = _remoteNotificationManager ?? new DummyManager();
		VespaCoroutine.StartGlobal(coUpdate(), "(RemoteRegister.coUpdate)");
	}

	private IEnumerator coUpdate()
	{
		while (true)
		{
			yield return null;
			try
			{
				if (_remoteNotificationManager != null)
				{
					_remoteNotificationManager.ChainUpdate();
				}
			}
			catch (Exception obj)
			{
				VespaDebug.LogError(obj);
			}
		}
	}

	public void Register(Action<string, string> onRegister)
	{
		Debug.Log("[Notification] RemoteRegister.Register");
		if (_remoteNotificationManager == null)
		{
			Debug.LogError("[Notification] manager is null.");
			Vespa_System_Action_Extension_Method.fire(onRegister, string.Empty, string.Empty);
			return;
		}
		_onRegister = onRegister;
		try
		{
			_remoteNotificationManager.Register(this);
		}
		catch (Exception ex)
		{
			VespaDebug.LogError(ex);
			Vespa_System_Action_Extension_Method.fire(onRegister, string.Empty, ex.ToString());
		}
	}

	public void Unregister(Action<string> onUnregister)
	{
		Debug.Log("[Notification] RemoteRegister.Unregister");
		_onUnregister = onUnregister;
		try
		{
			_remoteNotificationManager.Unregister();
		}
		catch (Exception ex)
		{
			VespaDebug.LogError(ex);
			Vespa_System_Action_Extension_Method.fire(onUnregister, ex.ToString());
		}
	}

	public void Clear()
	{
		Debug.Log("[Notification] RemoteRegister.Clear");
		try
		{
			_remoteNotificationManager.Clear();
		}
		catch (Exception obj)
		{
			VespaDebug.LogError(obj);
		}
	}

	void IRemoteNotificationReceiver.OnNotificationReceived(IDictionary values)
	{
		if (values == null)
		{
			return;
		}
		ICollection keys = values.Keys;
		if (keys == null)
		{
			return;
		}
		string text = "";
		foreach (object item in keys)
		{
			object obj = values[item];
			text = text + "\n[KEY=" + item?.ToString() + "  VALUE=" + obj?.ToString() + "]";
		}
		Debug.Log("[Notification] RemoteRegister.OnNotificationReceived success. : " + text);
	}

	void IRemoteNotificationReceiver.OnRegistration(string registerId, string error)
	{
		if (String_Extension_Method.IsNullOrEmpty(error))
		{
			Debug.Log($"[Notification] RemoteRegister.OnRegistration success: registerId={registerId}");
		}
		else
		{
			Debug.LogError($"[Notification] RemoteRegister.OnRegistration failed: error={error}");
		}
		Vespa_System_Action_Extension_Method.fire(_onRegister, registerId, error);
	}

	void IRemoteNotificationReceiver.OnUnregistration(string error)
	{
		if (String_Extension_Method.IsNullOrEmpty(error))
		{
			Debug.Log("[Notification] RemoteRegister.OnUnregistration success.");
		}
		else
		{
			Debug.LogError("[Notification] RemoteRegister.OnUnregistration failed: error=" + error);
		}
		Vespa_System_Action_Extension_Method.fire(_onUnregister, error);
	}
}
