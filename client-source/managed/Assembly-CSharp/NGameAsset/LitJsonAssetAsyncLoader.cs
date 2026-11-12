using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using NVespa;

namespace NGameAsset;

public class LitJsonAssetAsyncLoader : SingletonAuto<LitJsonAssetAsyncLoader>
{
	private class LoadJsonData
	{
		public string AssetPath;

		public bool IsEncrypt;

		public bool IsJsonBin;

		public Action<JsonData> OnLoad;
	}

	private Queue<LoadJsonData> _loadQueue = new Queue<LoadJsonData>();

	private bool _isLoading;

	protected override bool ThreadSafe => true;

	public static bool AbandonAllLoad { get; set; }

	public bool IsLoadQueueEmpty
	{
		get
		{
			if (!_isLoading)
			{
				return _loadQueue.Count <= 0;
			}
			return false;
		}
	}

	public void Load(PlainAssetHelper.EPathType pathType, string path, Action<JsonData> onLoad)
	{
		PlainAssetHelper.GetAssetFileInfo(pathType, path, out var assetPath, out var isEncrypt, out var isJsonBin);
		LoadJsonData item = new LoadJsonData
		{
			AssetPath = assetPath,
			IsEncrypt = isEncrypt,
			IsJsonBin = isJsonBin,
			OnLoad = onLoad
		};
		lock (_loadQueue)
		{
			_loadQueue.Enqueue(item);
			if (!_isLoading)
			{
				_isLoading = true;
				SingletonUpdater<ThreadManager>.instance.Enqueue(LoadByThread, null, ThreadManager.EEntryType.LitJson);
			}
		}
	}

	public void LoadLoadFromJsonData(PlainAssetHelper.EPathType pathType, JsonData jsonData, Action<JsonData> onLoad)
	{
	}

	private void LoadByThread()
	{
		while (true)
		{
			LoadJsonData loadJsonData;
			lock (_loadQueue)
			{
				if (AbandonAllLoad)
				{
					while (_loadQueue.Count > 0)
					{
						loadJsonData = _loadQueue.Dequeue();
						Vespa_System_Action_Extension_Method.fire(loadJsonData.OnLoad, null);
					}
				}
				if (_loadQueue.Count == 0)
				{
					_isLoading = false;
					break;
				}
				loadJsonData = _loadQueue.Dequeue();
			}
			JsonData i_argA = ReadJsonObject(loadJsonData.AssetPath, loadJsonData.IsEncrypt, loadJsonData.IsJsonBin);
			Vespa_System_Action_Extension_Method.fire(loadJsonData.OnLoad, i_argA);
			i_argA = null;
		}
	}

	private static JsonData ReadJsonObject(string path, bool encrypted, bool isJsonBin)
	{
		if (!File.Exists(path))
		{
			VespaDebug.LogError("File Not Exist : " + path);
			return null;
		}
		try
		{
			if (isJsonBin)
			{
				return PlainAssetHelper.LoadLitJsonFromJsonBin(File.ReadAllBytes(path), path, encrypted);
			}
			return ReadJsonObjectFromText(path, encrypted);
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Faild to load " + path + ". error=" + ex);
			return null;
		}
	}

	private static JsonData ReadJsonObjectFromText(string path, bool encrypted)
	{
		if (!File.Exists(path))
		{
			return null;
		}
		return PlainAssetHelper.LoadLitJsonFromJsonText(File.ReadAllText(path), path, encrypted);
	}
}
