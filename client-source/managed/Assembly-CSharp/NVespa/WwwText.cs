using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace NVespa;

public class WwwText : WwwAbstract
{
	public string text { get; private set; }

	public WwwText(string url, Dictionary<string, string> headers = null)
		: base(url, headers)
	{
	}

	public WwwText(string url, WWWForm formData, Dictionary<string, string> headers = null)
		: base(url, formData, headers)
	{
	}

	protected override void OnComplete(UnityWebRequest webRequest)
	{
		SprkNetworkPerformance.TransportComplete(webRequest);
		if (webRequest != null && webRequest.isDone && String_Extension_Method.IsNullOrEmpty(webRequest.error))
		{
			text = webRequest.downloadHandler.text;
		}
	}
}
