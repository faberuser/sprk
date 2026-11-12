using System;
using NGame2.NMessageLog;
using NVespa;
using NVespa.NSocket;

namespace NGame2.NBattleContext;

public class P2pSocketRequester : IRequester
{
	private long[] _accountIds;

	public ConnectState State => ConnectState.Connected;

	public void Init(long[] accountIds)
	{
		_accountIds = accountIds;
	}

	void IRequester.Request<TReq, TRes>(TReq data, Action<TRes> onResponse)
	{
		SingletonAuto<MessageLogHandler>.instance.SendToClient(_accountIds, data);
	}

	void IRequester.Close()
	{
	}
}
