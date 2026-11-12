using System;
using NGame2.NAccount;
using NShared.NRoomMessage;
using NVespa;
using NVespa.NSocket;
using UnityEngine;

namespace NGame2.NSocket;

public class RoomSocketConnector : SocketConnector
{
	public const int PingIntarval = 30;

	private long _accountId;

	private bool _initialized;

	private DateTime _lastConnectTriedTime;

	private DateTime _lastPingTime;

	private string _address;

	private int _port;

	private long _roomId;

	private int _tryCount;

	private double _tryTime;

	private const int MAX_TRY_SECOND = 16;

	public Action<RoomLoginRes> OnRoomServerConnected { get; set; }

	public Action OnRoomServerDisconnected { get; set; }

	public long RoomId => _roomId;

	public void EnterRoom(long accountId, string address, int port, long roomId)
	{
		_accountId = accountId;
		_initialized = true;
		_lastConnectTriedTime = DateTime.UtcNow;
		_lastPingTime = DateTime.UtcNow;
		_address = address;
		_port = port;
		_roomId = roomId;
		Close();
		ResetTryCount();
		TryConnect();
	}

	public void LeaveRoom()
	{
		Close();
		_accountId = 0L;
		_initialized = false;
		_address = null;
		_port = 0;
		ResetTryCount();
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
		if (_initialized && base.State == ConnectState.Disconnected && (DateTime.UtcNow - _lastConnectTriedTime).TotalSeconds > _tryTime)
		{
			TryConnect();
		}
		if (base.State == ConnectState.Connected && (DateTime.UtcNow - _lastPingTime).TotalSeconds > 30.0)
		{
			SendPing();
		}
	}

	private void SendPing()
	{
		_lastPingTime = DateTime.UtcNow;
		RoomPingReq data = new RoomPingReq();
		Request<RoomPingReq, RoomPingRes>(data, null);
	}

	private void TryConnect()
	{
		IncreaseTryCount();
		_lastConnectTriedTime = DateTime.UtcNow;
		Connect();
	}

	public void Connect()
	{
		Connect(_address, _port, delegate(bool connected)
		{
			OnConnected(connected);
		});
	}

	private void OnConnected(bool connected)
	{
		_lastConnectTriedTime = DateTime.UtcNow;
		if (connected)
		{
			ResetTryCount();
			RoomLoginReq roomLoginReq = new RoomLoginReq();
			roomLoginReq.AccountId = _accountId;
			roomLoginReq.RoomId = RoomId;
			Request<RoomLoginReq, RoomLoginRes>(roomLoginReq, OnLoginRes);
		}
	}

	private void OnLoginRes(RoomLoginRes response)
	{
		if (response == null)
		{
			VespaDebug.LogError("Failed to room login : response is null.");
			LeaveRoom();
		}
		else if (response.Result != RoomLoginResType.Success)
		{
			VespaDebug.LogError("Failed to room login : " + response.Result);
			LeaveRoom();
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(OnRoomServerConnected, response);
		}
	}

	public override void OnClosed()
	{
		base.OnClosed();
		if (!SystemStatus.IsApplicationQuit)
		{
			Vespa_System_Action_Extension_Method.fire(OnRoomServerDisconnected);
		}
	}

	private void IncreaseTryCount()
	{
		if (_tryCount >= 1)
		{
			VespaDebug.LogError("MessageSocket RetryCount = " + _tryCount + " Time : " + _tryTime);
		}
		_tryCount++;
		_tryTime = Mathf.Min(Mathf.Pow(2f, _tryCount - 1), 16f);
	}

	private void ResetTryCount()
	{
		_tryCount = 0;
		_tryTime = 0.0;
	}

	public void SendRoomMessageNot<T>(T data) where T : class
	{
		if (AccountManager.IsLogined())
		{
			string text = JsonMarshaler.EncodeJsonString(data);
			string message = (string.IsNullOrEmpty(text) ? typeof(T).Name : (typeof(T).Name + " " + text));
			Send(new RoomMessageReq
			{
				Message = message
			});
		}
	}
}
