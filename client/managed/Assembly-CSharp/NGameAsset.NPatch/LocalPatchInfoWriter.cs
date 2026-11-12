using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiniJSON;
using NPatchGenerator;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGameAsset.NPatch;

public class LocalPatchInfoWriter : SceneAutoBehaviour<LocalPatchInfoWriter>
{
	public static object SyncObj = new object();

	private string _persistentDataPath;

	private bool _requestEndWrite;

	private bool _dirty;

	private ThreadSafeDic<string, AssetInfo> _assetInfoDic;

	private int _lastSaveAssetInfoCount;

	public bool IsProcessing { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		_persistentDataPath = UnityCache.persistentDataPath;
	}

	public void BeginWrite(ThreadSafeDic<string, AssetInfo> assetInfoDic)
	{
		if (IsProcessing)
		{
			VespaDebug.LogError("Invalid BeginWrite " + assetInfoDic.COUNT());
			return;
		}
		lock (SyncObj)
		{
			_assetInfoDic = assetInfoDic;
		}
		_requestEndWrite = false;
		VespaCoroutine.Start(GlobalAutoBehaviour<PatchDownloader>.instance, coLoop());
	}

	public void SetDirty()
	{
		lock (SyncObj)
		{
			_dirty = true;
		}
	}

	public void RequestEnd()
	{
		_requestEndWrite = true;
	}

	private IEnumerator coLoop()
	{
		IsProcessing = true;
		while (true)
		{
			yield return null;
			float beginWait = Time.realtimeSinceStartup;
			while (!_requestEndWrite)
			{
				yield return null;
				if (Time.realtimeSinceStartup - beginWait > 3f)
				{
					break;
				}
			}
			lock (SyncObj)
			{
				if (_assetInfoDic == null)
				{
					break;
				}
			}
			if (NeedWriteLocalVersion())
			{
				yield return VespaCoroutine.Start(GlobalAutoBehaviour<PatchDownloader>.instance, coWrite());
			}
			else if (_requestEndWrite)
			{
				break;
			}
		}
		IsProcessing = false;
		_assetInfoDic = null;
	}

	private bool NeedWriteLocalVersion()
	{
		lock (SyncObj)
		{
			lock (_assetInfoDic)
			{
				return _assetInfoDic.COUNT() != _lastSaveAssetInfoCount || _dirty;
			}
		}
	}

	private IEnumerator coWrite()
	{
		bool isComplete = false;
		int remoteVersion = GlobalAutoBehaviour<PatchDownloader>.instance.GetPatchVersion();
		SingletonUpdater<ThreadManager>.instance.Enqueue((Action)delegate
		{
			List<AssetInfo> list = new List<AssetInfo>();
			lock (SyncObj)
			{
				_dirty = false;
				if (_assetInfoDic == null)
				{
					return;
				}
				lock (_assetInfoDic.LockObj)
				{
					_assetInfoDic.CopyValueTo(list);
					_lastSaveAssetInfoCount = __Extension_Method_ICollection__.COUNT(list);
				}
			}
			_ = DateTime.UtcNow;
			list.Sort((AssetInfo l, AssetInfo r) => l.EntryName.CompareTo(r.EntryName));
			ArchiveInfo archiveInfo = new ArchiveInfo
			{
				SrcVersion = remoteVersion,
				AssetInfos = list
			};
			string patchInfoFileName = PatchGenerator.GetPatchInfoFileName(isBinary: true);
			string downloadedAssetPath = PatchDownloader.GetDownloadedAssetPath(_persistentDataPath, patchInfoFileName);
			Dictionary<string, object> dictionary = JsonMarshaler.EncodeJsonObject(new PatchInfo
			{
				Archives = new List<ArchiveInfo>(new ArchiveInfo[1] { archiveInfo })
			});
			byte[] bytes = MiniJSON.JsonBin.Serialize(dictionary);
			JsonPool.instance.Release(dictionary);
			dictionary = null;
			File.WriteAllBytes(downloadedAssetPath, bytes);
		}, (Action)delegate
		{
			isComplete = true;
		});
		while (!isComplete)
		{
			yield return null;
		}
	}

	public PatchInfo GetLocalPatchInfo()
	{
		try
		{
			string patchInfoFileName = PatchGenerator.GetPatchInfoFileName(isBinary: true);
			string downloadedAssetPath = PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, patchInfoFileName);
			if (File.Exists(downloadedAssetPath))
			{
				IDictionary dictionary = MiniJSON.JsonBin.Deserialize(File.ReadAllBytes(downloadedAssetPath)) as IDictionary;
				PatchInfo result = JsonMarshaler.Parse<PatchInfo>(dictionary);
				JsonPool.instance.Release(dictionary);
				dictionary = null;
				return result;
			}
			patchInfoFileName = PatchGenerator.GetPatchInfoFileName();
			downloadedAssetPath = PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, patchInfoFileName);
			if (File.Exists(downloadedAssetPath))
			{
				return JsonMarshaler.Parse<PatchInfo>(File.ReadAllText(downloadedAssetPath));
			}
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("GetLocalPatchInfo() error : " + ex);
			return null;
		}
		return null;
	}
}
