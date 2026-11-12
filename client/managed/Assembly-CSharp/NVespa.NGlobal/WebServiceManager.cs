using System;
using NShared;
using NShared.Ping;
using NVespa.NWeb;

namespace NVespa.NGlobal;

public class WebServiceManager : SingletonUpdater<WebServiceManager>
{
	public delegate void OnResponse<in TResponse>(TResponse response) where TResponse : NShared.Response;

	public const float PingTimeInterval = 600f;

	private string _forwardedHost;

	public WebServiceRequester Requester { get; private set; }

	public DateTime LastPingTime { get; private set; }

	public static WebServiceRequest Request<TRequest, TResponse>(TRequest request, Action<TResponse> onResponse = null) where TRequest : NShared.Request, IServiceRequest where TResponse : NShared.Response, new()
	{
		if (!SingletonUpdater<WebServiceManager>.isValidInstance)
		{
			return null;
		}
		return SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, onResponse);
	}

	public WebServiceManager()
	{
		Requester = SingletonUpdater<WebServiceRequester>.instance;
		LastPingTime = DateTime.UtcNow;
	}

	public override void ChainUpdate()
	{
		UpdatePing();
	}

	public void SetForwardedHost(string host)
	{
		_forwardedHost = host;
	}

	public string PopForwardedHost()
	{
		if (string.IsNullOrEmpty(_forwardedHost))
		{
			return null;
		}
		string forwardedHost = _forwardedHost;
		_forwardedHost = null;
		return forwardedHost;
	}

	private void UpdatePing()
	{
		if (string.IsNullOrEmpty(Requester.SessionKey))
		{
			return;
		}
		DateTime utcNow = DateTime.UtcNow;
		if (!((utcNow - LastPingTime).TotalSeconds > 600.0))
		{
			return;
		}
		LastPingTime = utcNow;
		Requester.Request(new NShared.Ping.Request(), WebRequestFlag.DisableRetry | WebRequestFlag.Ping, delegate(NShared.Ping.Response response)
		{
			if (response != null && !string.IsNullOrEmpty(response.ForwardHost))
			{
				SetForwardedHost(response.ForwardHost);
			}
		});
	}
}
