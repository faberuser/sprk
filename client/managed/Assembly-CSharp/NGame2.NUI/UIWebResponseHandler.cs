using System;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NUI;

public class UIWebResponseHandler : MonoBehaviour
{
	[Serializable]
	public class KeyValueInfo
	{
		public string Key;

		public string Value;
	}

	public string ServiceName;

	public KeyValueInfo[] KeyValues;

	public List<EventDelegate> OnResponse = new List<EventDelegate>();

	private void OnEnable()
	{
		if (SingletonUpdater<WebServiceManager>.isValidInstance)
		{
			WebServiceRequester requester = SingletonUpdater<WebServiceManager>.instance.Requester;
			requester.Listen = (Action<Request, Response, string>)Delegate.Remove(requester.Listen, new Action<Request, Response, string>(OnReceive));
			WebServiceRequester requester2 = SingletonUpdater<WebServiceManager>.instance.Requester;
			requester2.Listen = (Action<Request, Response, string>)Delegate.Combine(requester2.Listen, new Action<Request, Response, string>(OnReceive));
		}
	}

	private void OnDisable()
	{
		if (SingletonUpdater<WebServiceManager>.isValidInstance)
		{
			WebServiceRequester requester = SingletonUpdater<WebServiceManager>.instance.Requester;
			requester.Listen = (Action<Request, Response, string>)Delegate.Remove(requester.Listen, new Action<Request, Response, string>(OnReceive));
		}
	}

	private void OnReceive(Request request, Response response, string responseText)
	{
		if (!(request is IServiceRequest serviceRequest) || !string.Equals(serviceRequest.ServiceName, ServiceName, StringComparison.InvariantCultureIgnoreCase) || response == null || !(Json.Deserialize(responseText) is IDictionary<string, object> dictionary))
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(KeyValues); i++)
		{
			string key = KeyValues[i].Key;
			string value = KeyValues[i].Value;
			if (!dictionary.ContainsKey(key))
			{
				return;
			}
			object obj = dictionary[key];
			if (obj == null || !string.Equals(obj.ToString(), value, StringComparison.InvariantCultureIgnoreCase))
			{
				return;
			}
		}
		EventDelegate.Execute(OnResponse);
	}
}
