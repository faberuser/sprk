using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NGameAsset.NPatch;
using NShared;
using NVespa;

namespace NGameAsset;

public class AssetFileManager : SingletonAuto<AssetFileManager>
{
	private IncreaseDic<string, AssetInfo> _remoteAssetInfoDic = new IncreaseDic<string, AssetInfo>(StringComparer.OrdinalIgnoreCase);

	private HashSet<string> _preLoadedTableHash = new HashSet<string>();

	protected override bool ThreadSafe => true;

	public void Setup(Dictionary<string, AssetInfo> assetInfoDic)
	{
		lock (_remoteAssetInfoDic)
		{
			foreach (AssetInfo value in assetInfoDic.Values)
			{
				_remoteAssetInfoDic.Add(value.EntryName, value);
			}
		}
	}

	public void AddToPreloadTable(string path)
	{
		lock (_preLoadedTableHash)
		{
			_preLoadedTableHash.Add(path);
		}
	}

	public void SetPreloadTables(string csv)
	{
		lock (_preLoadedTableHash)
		{
			_preLoadedTableHash = new HashSet<string>(csv.Split(','));
		}
	}

	public string PrealoadTablesToCsv()
	{
		lock (_preLoadedTableHash)
		{
			return string.Join(",", _preLoadedTableHash.ToArray());
		}
	}

	public bool TableExist(string assetPath, bool showLog = true)
	{
		lock (_preLoadedTableHash)
		{
			if (_preLoadedTableHash.Contains(assetPath))
			{
				return true;
			}
		}
		if (ExistInAssetInfos(assetPath))
		{
			return true;
		}
		if (File.Exists(PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, assetPath)))
		{
			return true;
		}
		return false;
	}

	public bool ExistInAssetInfos(string assetPath)
	{
		lock (_remoteAssetInfoDic)
		{
			return _remoteAssetInfoDic.ContainsKey(assetPath);
		}
	}

	public bool DownloadedExist(string assetPath)
	{
		lock (_remoteAssetInfoDic)
		{
			return _remoteAssetInfoDic.ContainsKey(assetPath);
		}
	}
}
