using System;
using NShared;
using NVespa;
using NVespa.NSocket;

namespace NGame2.NSocket;

public class BattleSocketConnector : SocketConnector
{
	public const int PingIntarval = 10;

	private DateTime _lastPingTime;

	public Action OnDisconnected { get; set; }

	protected override void Awake()
	{
		base.Awake();
		_lastPingTime = DateTime.UtcNow;
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
		if (base.State == ConnectState.Connected && (DateTime.UtcNow - _lastPingTime).TotalSeconds > 10.0)
		{
			SendPing();
		}
	}

	private void SendPing()
	{
		_lastPingTime = DateTime.UtcNow;
		PingIdleReq data = new PingIdleReq();
		Request<PingIdleReq, PingIdleRes>(data, null);
	}

	public override void OnClosed()
	{
		base.OnClosed();
		if (!SystemStatus.IsApplicationQuit)
		{
			Vespa_System_Action_Extension_Method.fire(OnDisconnected);
		}
	}
}
