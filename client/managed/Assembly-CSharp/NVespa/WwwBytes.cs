using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace NVespa;

public class WwwBytes : WwwAbstract
{
	private Action _onComplete;

	public byte[] bytes { get; private set; }

	public WwwBytes(string url, Dictionary<string, string> headers = null)
		: base(url, headers)
	{
	}

	public WwwBytes(string url, WWWForm formData, Dictionary<string, string> headers = null)
		: base(url, formData, headers)
	{
	}

	public WwwBytes(string url, Action onComplete)
		: base(url)
	{
		_onComplete = onComplete;
	}

	protected override void OnComplete(UnityWebRequest webRequest)
	{
		if (webRequest != null && webRequest.isDone && String_Extension_Method.IsNullOrEmpty(webRequest.error))
		{
			bytes = webRequest.downloadHandler.data;
			Vespa_System_Action_Extension_Method.fire(_onComplete);
		}
	}
}
