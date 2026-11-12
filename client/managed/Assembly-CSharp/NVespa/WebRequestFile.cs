using System;
using System.Collections;
using NGameAsset.NPatch;
using UnityEngine;
using UnityEngine.Networking;

namespace NVespa;

public class WebRequestFile : WebRequest
{
	public interface Listener
	{
		void OnUpateExtractAsset(DownloadContextData downloadContextData, double ratio);

		void OnCompleteExtractArchive(DownloadContextData downloadContextData);
	}

	private DownloadContextData _downloadContextData;

	private Action<bool> _onComplete;

	private Listener _listener;

	public WebRequestFile(Listener listener, string url, DownloadContextData downloadContextData, Action<bool> onComplete)
		: base(null, url, downloadContextData, null)
	{
		_listener = listener;
		_onComplete = onComplete;
		_downloadContextData = downloadContextData;
		string entryName = downloadContextData.ArchiveInfo.AssetInfos[0].EntryName;
		string downloadedAssetPath = PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, entryName);
		_unityWebRequest = new UnityWebRequest(url, "GET");
		try
		{
			DownloadHandlerFile downloadHandler = new DownloadHandlerFile(downloadedAssetPath)
			{
				removeFileOnAbort = true
			};
			_unityWebRequest.downloadHandler = downloadHandler;
		}
		catch (ArgumentException)
		{
			Debug.LogWarning("Already downloading file : " + downloadedAssetPath);
			OnCompleteWithException();
			return;
		}
		VespaCoroutine.Start(coWait());
	}

	private void OnCompleteInternal(bool result)
	{
		Vespa_System_Action_Extension_Method.fire(_onComplete, result);
		if (_unityWebRequest != null)
		{
			responseHeaders = _unityWebRequest.GetResponseHeaders();
			error = _unityWebRequest.error;
			isDone = true;
			_unityWebRequest.Dispose();
			_unityWebRequest = null;
		}
	}

	private void OnCompleteWithException()
	{
		OnCompleteInternal(result: false);
	}

	private void OnComplete()
	{
		OnCompleteInternal(result: true);
		_listener.OnCompleteExtractArchive(_downloadContextData);
	}

	private IEnumerator coWait()
	{
		_unityWebRequest.SendWebRequest();
		while (!_unityWebRequest.isDone)
		{
			_listener.OnUpateExtractAsset(_downloadContextData, _unityWebRequest.downloadProgress);
			yield return null;
		}
		OnComplete();
	}
}
