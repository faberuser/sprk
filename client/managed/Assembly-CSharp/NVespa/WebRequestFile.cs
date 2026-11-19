using System;
using System.Collections;
using System.IO;
using NGameAsset.NPatch;
using NPatchGenerator;
using NShared;
using NVespa.NUtil;
using UnityEngine;
using UnityEngine.Networking;

namespace NVespa;

/// Downloads one raw file archive straight to disk.
///
/// SPRK: the file is written to "<path>.part", checked against the archive's
/// length and MD5 on a worker thread, and only then moved over the installed
/// file and recorded. A failed or corrupt download sets <see cref="WebRequest.error"/>
/// (PatchDownloader retries it) and never replaces or records the installed copy.
public class WebRequestFile : WebRequest
{
	public interface Listener
	{
		void OnUpateExtractAsset(DownloadContextData downloadContextData, double ratio);

		void OnCompleteExtractArchive(DownloadContextData downloadContextData);
	}

	public const string PartialSuffix = ".part";

	private DownloadContextData _downloadContextData;

	private Action<bool> _onComplete;

	private Listener _listener;

	private readonly string _targetPath;

	private readonly string _partialPath;

	public WebRequestFile(Listener listener, string url, DownloadContextData downloadContextData, Action<bool> onComplete)
		: base(null, url, downloadContextData, null)
	{
		_listener = listener;
		_onComplete = onComplete;
		_downloadContextData = downloadContextData;
		string entryName = downloadContextData.ArchiveInfo.AssetInfos[0].EntryName;
		_targetPath = PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, entryName);
		_partialPath = _targetPath + PartialSuffix;
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(_targetPath));
			DeleteIfExists(_partialPath);
			_unityWebRequest = new UnityWebRequest(url, "GET");
			_unityWebRequest.downloadHandler = new DownloadHandlerFile(_partialPath)
			{
				removeFileOnAbort = true
			};
		}
		catch (Exception ex) when (ex is ArgumentException || ex is IOException || ex is UnauthorizedAccessException)
		{
			Debug.LogWarning("Cannot start download of " + _targetPath + ": " + ex.Message);
			if (_unityWebRequest != null)
			{
				_unityWebRequest.Dispose();
				_unityWebRequest = null;
			}
			Fail("Cannot write " + _partialPath + ": " + ex.Message);
			return;
		}
		VespaCoroutine.Start(coWait());
	}

	private IEnumerator coWait()
	{
		_unityWebRequest.SendWebRequest();
		while (!_unityWebRequest.isDone)
		{
			_listener.OnUpateExtractAsset(_downloadContextData, _unityWebRequest.downloadProgress);
			yield return null;
		}
		responseHeaders = _unityWebRequest.GetResponseHeaders();
		string failure = null;
		if (_unityWebRequest.result != UnityWebRequest.Result.Success)
		{
			failure = String_Extension_Method.IsNullOrEmpty(_unityWebRequest.error) ? _unityWebRequest.result.ToString() : _unityWebRequest.error;
		}
		else if (_unityWebRequest.responseCode != 200)
		{
			failure = "HTTP " + _unityWebRequest.responseCode;
		}
		// Dispose closes the partial file before it is hashed or moved.
		_unityWebRequest.Dispose();
		_unityWebRequest = null;
		if (failure != null)
		{
			DeleteIfExists(_partialPath);
			Fail(failure);
			yield break;
		}
		AssetInfo expected = _downloadContextData.ArchiveInfo.AssetInfos[0];
		SingletonUpdater<ThreadManager>.instance.Enqueue((Action)delegate
		{
			string problem = null;
			try
			{
				problem = Verify(expected);
				if (problem == null)
				{
					DeleteIfExists(_targetPath);
					File.Move(_partialPath, _targetPath);
					_listener.OnCompleteExtractArchive(_downloadContextData);
				}
			}
			catch (Exception ex)
			{
				problem = "Cannot install " + _targetPath + ": " + ex.Message;
			}
			if (problem != null)
			{
				DeleteIfExists(_partialPath);
			}
			SingletonUpdater<Executer>.instance.NextRun(delegate
			{
				if (problem == null)
				{
					Succeed();
				}
				else
				{
					Fail(problem);
				}
			});
		}, (Action)null);
	}

	private string Verify(AssetInfo expected)
	{
		long length = new FileInfo(_partialPath).Length;
		if (length != expected.Length)
		{
			return $"Length mismatch for {expected.EntryName}: got {length}, expected {expected.Length}";
		}
		string md5 = FileUtility.GetFileMd5Hash(_partialPath);
		if (!string.Equals(md5, expected.Md5, StringComparison.OrdinalIgnoreCase))
		{
			return $"MD5 mismatch for {expected.EntryName}: got {md5}, expected {expected.Md5}";
		}
		return null;
	}

	private void Succeed()
	{
		Vespa_System_Action_Extension_Method.fire(_onComplete, true);
		isDone = true;
	}

	// PatchDownloader retries on error, so a failure only reports the error;
	// it must not also fire the completion callback of a finished attempt.
	private void Fail(string message)
	{
		VespaDebug.LogError("Download failed: " + message);
		error = message;
		isDone = true;
	}

	private static void DeleteIfExists(string path)
	{
		try
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Cannot delete " + path + ": " + ex.Message);
		}
	}
}
