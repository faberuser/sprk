using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace NVespa;

public abstract class WebRequest
{
	protected UnityWebRequest _unityWebRequest;

	public virtual bool isDone { get; protected set; }

	public virtual Dictionary<string, string> responseHeaders { get; protected set; }

	public virtual string error { get; protected set; }

	public virtual float progress
	{
		get
		{
			if (_unityWebRequest != null)
			{
				return _unityWebRequest.downloadProgress;
			}
			return 1f;
		}
	}

	public WebRequest(DownloadHandlerBase.Listener listener, string url, DownloadContextData downloadContextData, Action onComplete)
	{
	}
}
