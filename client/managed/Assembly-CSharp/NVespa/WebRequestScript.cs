using System;
using System.Collections;
using NShared;
using UnityEngine.Networking;

namespace NVespa;

public class WebRequestScript : WebRequest
{
	public WebRequestScript(DownloadHandlerBase.Listener listener, string url, byte[] buffer, DownloadContextData downloadContextData, Action<bool> onComplete)
		: base(listener, url, downloadContextData, null)
	{
		ArchiveInfo archiveInfo = downloadContextData.ArchiveInfo;
		_unityWebRequest = new UnityWebRequest(url, "GET", (archiveInfo.Zip || archiveInfo.Name.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase)) ? ((DownloadHandlerBase)new DownloadHandlerZip2(listener, buffer, downloadContextData, onComplete)) : ((DownloadHandlerBase)new DownloadHandlerPack2(listener, buffer, downloadContextData, onComplete)), null);
		VespaCoroutine.Start(coWait());
	}

	private IEnumerator coWait()
	{
		_unityWebRequest.SendWebRequest();
		int frameCount = 1;
		while (!_unityWebRequest.isDone)
		{
			frameCount++;
			yield return null;
		}
		responseHeaders = _unityWebRequest.GetResponseHeaders();
		error = _unityWebRequest.error;
		isDone = true;
		if (_unityWebRequest != null)
		{
			_unityWebRequest.Dispose();
			_unityWebRequest = null;
		}
	}
}
