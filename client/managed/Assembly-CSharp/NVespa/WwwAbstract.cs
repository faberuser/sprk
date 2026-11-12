using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace NVespa;

public abstract class WwwAbstract
{
	protected UnityWebRequest _webRequset;

	public bool isDone { get; private set; }

	public Dictionary<string, string> responseHeaders { get; private set; }

	public string error { get; private set; }

	public float progress
	{
		get
		{
			if (_webRequset != null)
			{
				return _webRequset.downloadProgress;
			}
			return 1f;
		}
	}

	public long responseCode { get; private set; }

	public WwwAbstract(string url, Dictionary<string, string> headers = null)
	{
		_webRequset = UnityWebRequest.Get(url);
		if (headers != null)
		{
			foreach (KeyValuePair<string, string> header in headers)
			{
				_webRequset.SetRequestHeader(header.Key, header.Value);
			}
		}
		VespaCoroutine.Start(coWait());
	}

	public WwwAbstract(string url, WWWForm formData, Dictionary<string, string> headers = null)
	{
		_webRequset = UnityWebRequest.Post(url, formData);
		if (headers != null)
		{
			foreach (KeyValuePair<string, string> header in headers)
			{
				_webRequset.SetRequestHeader(header.Key, header.Value);
			}
		}
		VespaCoroutine.Start(coWait());
	}

	protected abstract void OnComplete(UnityWebRequest webRequest);

	private IEnumerator coWait()
	{
		yield return _webRequset.SendWebRequest();
		responseHeaders = _webRequset.GetResponseHeaders();
		error = _webRequset.error;
		isDone = true;
		responseCode = _webRequset.responseCode;
		OnComplete(_webRequset);
		if (_webRequset != null)
		{
			_webRequset.Dispose();
			_webRequset = null;
		}
	}
}
