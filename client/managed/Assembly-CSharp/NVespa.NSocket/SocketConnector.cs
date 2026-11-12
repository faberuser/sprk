using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using NGame2.NConfig;
using NShared;
using NShared.NUtil;
using SuperSocket.ClientEngine;
using UnityEngine;

namespace NVespa.NSocket;

public class SocketConnector : BehaviourUpdater, IRequester, IHandler
{
	public class ResponseActionInfo
	{
		public Action<BaseResponse> Action;

		public DateTime RequestedTime;

		public Type ResponseType;

		public Func<string, object> Deserializer;
	}

	private class ReservedAction
	{
		public Action Action { get; set; }

		public DateTime Time { get; set; }
	}

	public class OneShotActionInfo
	{
		public Action<bool> Action;

		public DateTime TimeoutAt;
	}

	private const int ResponseWaitTime = 10;

	private object _reservedActionListSync = new object();

	private object _receivedStringBuilderSync = new object();

	private object _closeSync = new object();

	private AsyncTcpSession _session;

	private List<ReservedAction> _reservedActionList;

	private SocketHandler<object> _socketHandler;

	private StringBuilder _receivedStringBuilder;

	private Dictionary<int, ResponseActionInfo> _responseDictionary;

	private Dictionary<string, OneShotActionInfo> _reservedOneshotAction;

	private int _requestId;

	public ConnectState State { get; private set; }

	public SocketHandler<object> SocketHandler => _socketHandler;

	public string Address { get; private set; }

	public int Port { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		_reservedActionList = new List<ReservedAction>();
		_socketHandler = new SocketHandler<object>();
		_receivedStringBuilder = new StringBuilder();
		_responseDictionary = new Dictionary<int, ResponseActionInfo>();
		_reservedOneshotAction = new Dictionary<string, OneShotActionInfo>();
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
		UpdateDisconnected();
		HandleReceivedData();
		FlushReservedAction();
		UpdateResponseAction();
		UpdateOneshotAction();
	}

	public void SetContext(object context)
	{
		_socketHandler.SetContext(context);
	}

	public void Connect(string address, int port, Action<bool> onConnect)
	{
		VespaCoroutine.Start(this, ConnectSocketCoroutine(address, port, onConnect, 10f));
	}

	public void Close()
	{
		if (_session != null)
		{
			lock (_closeSync)
			{
				try
				{
					AsyncTcpSession session = _session;
					_session = null;
					session?.Close();
				}
				catch (Exception obj)
				{
					VespaDebug.LogError(obj);
				}
			}
		}
		bool num = State == ConnectState.Connected;
		State = ConnectState.Disconnected;
		if (num)
		{
			OnClosed();
		}
	}

	public virtual void OnClosed()
	{
	}

	public bool Send<T>(T data)
	{
		if (_session == null)
		{
			return false;
		}
		string text = JsonMarshaler.EncodeJsonString(data);
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		string value = Base64Encoder.ToBase64String(text);
		string value2 = typeof(T).Name;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(value2);
		stringBuilder.Append(" ");
		stringBuilder.Append(value);
		stringBuilder.Append("\r\n");
		string text2 = stringBuilder.ToString();
		return SendText(text2);
	}

	public bool SendText(string text)
	{
		if (_session == null)
		{
			return false;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		ArraySegment<byte> array = new ArraySegment<byte>(bytes);
		ReserveOrRunByForcedDelay(delegate
		{
			if (_session != null && _session.IsConnected)
			{
				_session.TrySend(array);
			}
		});
		return true;
	}

	private void OnApplicationQuit()
	{
		Close();
	}

	private void Reserve(Action action, int delayMs)
	{
		lock (_reservedActionListSync)
		{
			_reservedActionList.Add(new ReservedAction
			{
				Action = action,
				Time = DateTime.UtcNow.AddMilliseconds(delayMs)
			});
		}
	}

	private void ReserveOrRunByForcedDelay(Action action)
	{
		int forceLatencyMs = SingletonAuto<ClientDebugNetwork>.instance.ForceLatencyMs;
		if (forceLatencyMs <= 0)
		{
			Vespa_System_Action_Extension_Method.fire(action);
		}
		else
		{
			Reserve(action, forceLatencyMs);
		}
	}

	private void OnData(byte[] data, int offset, int length)
	{
		if (data == null)
		{
			return;
		}
		string value = Encoding.UTF8.GetString(data, offset, length);
		lock (_receivedStringBuilderSync)
		{
			_receivedStringBuilder.Append(value);
		}
	}

	private void UpdateDisconnected()
	{
		if (_session != null && !_session.IsConnected)
		{
			VespaDebug.LogError("Session disconnected.");
			Close();
		}
	}

	private void HandleReceivedData()
	{
		while (true)
		{
			string text = string.Empty;
			lock (_receivedStringBuilderSync)
			{
				text = _receivedStringBuilder.ToString();
				int num = text.IndexOf("\r\n");
				if (num < 0)
				{
					break;
				}
				text = text.Substring(0, num);
				_receivedStringBuilder.Remove(0, num + 2);
			}
			string empty = string.Empty;
			string decodingText = string.Empty;
			int num2 = text.IndexOf(' ');
			if (num2 <= 0)
			{
				empty = text;
			}
			else
			{
				empty = text.Substring(0, num2);
				decodingText = text.Substring(num2 + 1);
			}
			try
			{
				HandleOneshotAction(text);
				string content = Base64Encoder.FromBase64String(decodingText);
				_socketHandler.Handle(empty, content);
				ProcessResponse(empty, content);
			}
			catch (Exception ex)
			{
				VespaDebug.LogError("Parsing received data error : " + ex);
			}
		}
	}

	private void FlushReservedAction()
	{
		using PoolingList<ReservedAction> poolingList = default(PoolingList<ReservedAction>);
		lock (_reservedActionListSync)
		{
			DateTime utcNow = DateTime.UtcNow;
			ReservedAction[] array = _reservedActionList.ToArray();
			foreach (ReservedAction reservedAction in array)
			{
				if (utcNow >= reservedAction.Time)
				{
					poolingList.Add(reservedAction);
					_reservedActionList.Remove(reservedAction);
				}
			}
		}
		for (int j = 0; j < poolingList.Count; j++)
		{
			Vespa_System_Action_Extension_Method.fire(poolingList[j].Action);
		}
	}

	private IEnumerator SprkConnectTcpCoroutine(string address, int port, Action<bool> onConnect, float timeOut = 10f)
	{
		Close();
		State = ConnectState.Connecting;
		yield return null;
		if (address == "localhost")
		{
			address = "127.0.0.1";
		}
		IPAddress address2 = null;
		if (!IPAddress.TryParse(address, out address2))
		{
			IPHostEntry hostEntry = Dns.GetHostEntry(address);
			if (hostEntry != null && hostEntry.AddressList.Length != 0)
			{
				address2 = hostEntry.AddressList[0];
			}
		}
		if (address2 == null)
		{
			Close();
			Vespa_System_Action_Extension_Method.fire(onConnect, i_argA: false);
			yield break;
		}
		IPEndPoint remoteEndPoint = new IPEndPoint(address2, port);
		AsyncTcpSession session = new AsyncTcpSession();
		session.ReceiveBufferSize = 102400;
		bool finished = false;
		bool error = false;
		string errorMessage = "";
		try
		{
			session.Connected += delegate
			{
				session.NoDelay = true;
				finished = true;
			};
			session.Error += delegate(object sender, ErrorEventArgs e)
			{
				error = true;
				errorMessage = ((e.Exception != null) ? e.Exception.ToString() : "Unknown");
			};
			session.Closed += delegate
			{
				error = true;
				errorMessage = "Closed";
				finished = true;
			};
			session.Connect(remoteEndPoint);
		}
		catch (Exception ex)
		{
			Debug.LogError("Add event error : " + ex.ToString());
			Close();
			Vespa_System_Action_Extension_Method.fire(onConnect, i_argA: false);
			yield break;
		}
		DateTime startTime = DateTime.UtcNow;
		while (!finished)
		{
			if ((DateTime.UtcNow - startTime).TotalSeconds > (double)timeOut)
			{
				error = true;
				errorMessage = "Timeout";
				break;
			}
			yield return null;
		}
		if (error)
		{
			State = ConnectState.Disconnected;
			Debug.LogError("Socket : connect failed! message=" + errorMessage);
			Close();
			Vespa_System_Action_Extension_Method.fire(onConnect, i_argA: false);
			yield break;
		}
		State = ConnectState.Connected;
		Address = address;
		Port = port;
		_session = session;
		session.DataReceived += delegate(object sender, DataEventArgs e)
		{
			OnData(e.Data, e.Offset, e.Length);
		};
		session.Closed += delegate(object sender, EventArgs e)
		{
			SprkWebSocketTransport.SessionClosed(this, sender);
		};
		Vespa_System_Action_Extension_Method.fire(onConnect, i_argA: true);
	}

	public void Request<TReq, TRes>(TReq data, Action<TRes> onResponse) where TReq : BaseRequest where TRes : BaseResponse
	{
		int key = (data.RequestId = ++_requestId);
		if (!Send(data))
		{
			if (onResponse != null)
			{
				onResponse(null);
			}
			return;
		}
		_responseDictionary.Remove(key);
		_responseDictionary.Add(key, new ResponseActionInfo
		{
			Action = delegate(BaseResponse r)
			{
				Vespa_System_Action_Extension_Method.fire(onResponse, r as TRes);
			},
			RequestedTime = DateTime.UtcNow,
			ResponseType = typeof(TRes),
			Deserializer = JsonMarshaler.Parse<TRes>
		});
	}

	private bool ProcessResponse(string typeName, string content)
	{
		if (string.IsNullOrEmpty(typeName))
		{
			return false;
		}
		int num = 0;
		foreach (KeyValuePair<int, ResponseActionInfo> item in _responseDictionary)
		{
			int key = item.Key;
			ResponseActionInfo actionInfo = item.Value;
			if (!(actionInfo.ResponseType.Name == typeName))
			{
				continue;
			}
			BaseResponse response = actionInfo.Deserializer(content) as BaseResponse;
			if (response == null || response.RequestId != key)
			{
				continue;
			}
			num = key;
			if (actionInfo != null)
			{
				ReserveOrRunByForcedDelay(delegate
				{
					actionInfo.Action(response);
				});
			}
			break;
		}
		if (num > 0)
		{
			_responseDictionary.Remove(num);
			return true;
		}
		return false;
	}

	private void UpdateResponseAction()
	{
		DateTime utcNow = DateTime.UtcNow;
		using PoolingList<int> poolingList = default(PoolingList<int>);
		foreach (KeyValuePair<int, ResponseActionInfo> item in _responseDictionary)
		{
			int key = item.Key;
			ResponseActionInfo actionInfo = item.Value;
			if (!((utcNow - actionInfo.RequestedTime).TotalSeconds > 10.0))
			{
				continue;
			}
			poolingList.Add(key);
			if (actionInfo != null)
			{
				ReserveOrRunByForcedDelay(delegate
				{
					actionInfo.Action(null);
				});
			}
		}
		for (int num = 0; num < poolingList.Count; num++)
		{
			int key2 = poolingList[num];
			_responseDictionary.Remove(key2);
		}
	}

	public void ReserveOneshotAction(string type, float timeout, Action<bool> action)
	{
		_reservedOneshotAction[type] = new OneShotActionInfo
		{
			Action = action,
			TimeoutAt = DateTime.Now.AddSeconds(timeout)
		};
	}

	private void HandleOneshotAction(string type)
	{
		OneShotActionInfo value = null;
		if (_reservedOneshotAction.TryGetValue(type, out value) && _reservedOneshotAction.Remove(type) && value != null)
		{
			Vespa_System_Action_Extension_Method.fire(value.Action, i_argA: true);
		}
	}

	private void UpdateOneshotAction()
	{
		HashSet<string> hashSet = null;
		foreach (KeyValuePair<string, OneShotActionInfo> item in _reservedOneshotAction)
		{
			string key = item.Key;
			OneShotActionInfo value = item.Value;
			if (DateTime.Now > value.TimeoutAt)
			{
				hashSet = hashSet ?? new HashSet<string>();
				hashSet.Add(key);
			}
		}
		if (hashSet == null)
		{
			return;
		}
		foreach (string item2 in hashSet)
		{
			OneShotActionInfo oneShotActionInfo = _reservedOneshotAction[item2];
			if (_reservedOneshotAction.Remove(item2))
			{
				Vespa_System_Action_Extension_Method.fire(oneShotActionInfo.Action, i_argA: false);
			}
		}
	}

	void IHandler.Register<T>(Action<T> handler)
	{
		_socketHandler.Register(handler);
	}

	void IHandler.Clear()
	{
		_socketHandler.Clear();
	}

	private IEnumerator ConnectSocketCoroutine(string address, int port, Action<bool> onConnect, [Optional] float timeOut)
	{
		return SprkWebSocketTransport.Connect(this, address, port, onConnect, timeOut);
	}
}
