using System;
using System.Collections.Generic;

namespace NVespa;

public class WebRequestWWW : WebRequest
{
	private WwwBytes _wwwBytes;

	public override bool isDone => _wwwBytes.isDone;

	public override Dictionary<string, string> responseHeaders => _wwwBytes.responseHeaders;

	public override string error => _wwwBytes.error;

	public override float progress => _wwwBytes.progress;

	public WebRequestWWW(DownloadHandlerBase.Listener listener, string url, byte[] buffer, DownloadContextData downloadContextData, Action onComplete)
		: base(listener, url, downloadContextData, null)
	{
		_wwwBytes = new WwwBytes(url, onComplete);
	}
}
