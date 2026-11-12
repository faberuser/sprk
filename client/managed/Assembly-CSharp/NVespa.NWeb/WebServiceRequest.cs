using System;
using System.Collections.Generic;
using NShared;
using UnityEngine;

namespace NVespa.NWeb;

public class WebServiceRequest
{
	private const float ExpireSecond = 30f;

	public WwwText Www { get; private set; }

	public bool Started { get; private set; }

	public bool Ended => Www == null;

	public int RequestSequence { get; set; }

	public WWWForm Form { get; set; }

	public Dictionary<string, string> Headers { get; set; }

	public string ServiceName { get; set; }

	public string Host { get; set; }

	public string Argument { get; set; }

	public WebRequestFlag Flag { get; set; }

	public DateTime RequestedTime { get; set; }

	public void Dispose()
	{
		try
		{
			if (Www != null)
			{
				Www = null;
			}
		}
		catch (Exception)
		{
		}
	}

	public virtual void Start()
	{
		Started = true;
	}

	public void CreateRequest()
	{
		SprkNetworkPerformance.Started(this);
		string url = Host + ServiceName + "?Seq=" + RequestSequence;
		Www = new WwwText(url, Form, Headers);
		RequestedTime = DateTime.UtcNow;
	}

	public static bool IsProgressing(WebServiceRequest webService)
	{
		if (webService != null && webService.Www != null)
		{
			return !((DateTime.UtcNow - webService.RequestedTime).TotalSeconds > 30.0);
		}
		return false;
	}

	public static float GetTimeout()
	{
		return 30f;
	}
}
public class WebServiceRequest<TRequest, TResponse> : WebServiceRequest where TRequest : Request, IServiceRequest where TResponse : Response
{
	public TRequest Request { get; set; }

	public Action<TResponse> OnResponse { get; set; }

	public Action<WebServiceRequest<TRequest, TResponse>> Runner { get; set; }

	public override void Start()
	{
		base.Start();
		Vespa_System_Action_Extension_Method.fire(Runner, this);
		Runner = null;
	}
}
