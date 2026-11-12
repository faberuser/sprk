using NShared;
using NVespa;
using NVespa.NWeb;
using UnityEngine;

public class DummyClient : MonoBehaviour
{
	private IAction _action;

	public string LoginId { get; private set; }

	public string Host { get; private set; }

	public int ClientId { get; private set; }

	public WebServiceRequester WebServiceRequester { get; private set; }

	public UserInfo UserInfo { get; set; }

	public string SessionId { get; set; }

	public string SessionKey => UserInfo.SessionKey;

	private void Awake()
	{
		SetAction(new LoginAction());
	}

	private void OnApplicationQuit()
	{
	}

	public void Init(string host, int id)
	{
		Host = host;
		ClientId = id;
		LoginId = "test" + id.ToString("000");
		WebServiceRequester = SingletonUpdater<WebServiceRequester>.instance;
		WebServiceRequester.SetHost(host);
	}

	public void SetAction(IAction action)
	{
		if (_action != null)
		{
			_action.OnEnd();
			_action = null;
		}
		_action = action;
		if (_action != null)
		{
			_action.SetDummyClient(this);
			_action.OnBegin();
		}
	}

	public void DrawGui()
	{
		if (_action != null)
		{
			GUILayout.BeginVertical();
			GUILayout.Label("Current action : " + _action.GetType().Name);
			GUILayout.EndVertical();
			_action.OnGUI();
		}
	}
}
