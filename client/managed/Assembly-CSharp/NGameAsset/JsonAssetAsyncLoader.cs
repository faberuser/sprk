using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiniJSON;
using NGame2.NUI.NWindow;
using NGameAsset.NPatch;
using NShared;
using NVespa;

namespace NGameAsset;

public class JsonAssetAsyncLoader : IJsonLoader
{
	private class LoadJsonData
	{
		public string AssetPath;

		public bool IsEncrypt;

		public bool IsJsonBin;

		public Action<object> OnLoad;
	}

	private Queue<LoadJsonData> _loadQueue = new Queue<LoadJsonData>();

	private bool _isLoading;

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

	public int QueueCount => _loadQueue.Count;

	public string Dump()
	{
		lock (_loadQueue)
		{
			return $"JsonAssetAsyncLoader: Count={_loadQueue.Count}, isLoading={_isLoading}";
		}
	}

	void IJsonLoader.Load(string path, Action<object> onLoad)
	{
		PlainAssetHelper.GetAssetFileInfo(PlainAssetHelper.EPathType.Table, path, out var assetPath, out var isEncrypt, out var isJsonBin);
		LoadJsonData item = new LoadJsonData
		{
			AssetPath = assetPath,
			IsEncrypt = isEncrypt,
			IsJsonBin = isJsonBin,
			OnLoad = onLoad
		};
		bool value = Define.TableSingleLoad.Value;
		lock (_loadQueue)
		{
			_loadQueue.Enqueue(item);
			if (!_isLoading)
			{
				_isLoading = true;
				if (value)
				{
					VespaCoroutine.Start(coLoadLoop());
				}
				else
				{
					SingletonUpdater<ThreadManager>.instance.Enqueue(LoadByThread, null);
				}
			}
		}
	}

	private IEnumerator coLoadLoop()
	{
		while (true)
		{
			yield return null;
			LoadJsonData loadJson;
			lock (_loadQueue)
			{
				if (_loadQueue.Count == 0)
				{
					_isLoading = false;
					break;
				}
				loadJson = _loadQueue.Dequeue();
			}
			bool isComplete = false;
			bool isDetailPopup = false;
			if (Define.TableSingleLoad.Value)
			{
				GlobalMessagePopup.OpenGlobal("read table", Path.GetFileName(loadJson.AssetPath), delegate
				{
					isComplete = true;
					isDetailPopup = true;
				}, delegate
				{
					isComplete = true;
					isDetailPopup = false;
				});
				while (!isComplete)
				{
					yield return null;
				}
			}
			isComplete = false;
			object jsonData = null;
			SingletonUpdater<ThreadManager>.instance.Enqueue((Action)delegate
			{
				jsonData = ReadJsonObject(loadJson.AssetPath, loadJson.IsEncrypt, loadJson.IsJsonBin);
			}, (Action)delegate
			{
				isComplete = true;
			});
			while (!isComplete)
			{
				yield return null;
			}
			if (Define.TableSingleLoad.Value && isDetailPopup)
			{
				isComplete = false;
				GlobalMessagePopup.OpenGlobal("On Load", Path.GetFileName(loadJson.AssetPath), delegate
				{
					isComplete = true;
				}, retryButton: false);
				while (!isComplete)
				{
					yield return null;
				}
			}
			Vespa_System_Action_Extension_Method.fire(loadJson.OnLoad, jsonData);
			jsonData = null;
		}
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
			try
			{
				object i_argA = ReadJsonObject(loadJsonData.AssetPath, loadJsonData.IsEncrypt, loadJsonData.IsJsonBin);
				Vespa_System_Action_Extension_Method.fire(loadJsonData.OnLoad, i_argA);
				i_argA = null;
			}
			catch (Exception ex)
			{
				VespaDebug.LogError("Exception while run ReadJsonObject(): " + ex);
				Vespa_System_Action_Extension_Method.fire(loadJsonData.OnLoad, null);
				_isLoading = false;
			}
		}
	}

	private static List<string> GetPartialAssetPathList(string path, string assetFileRoot)
	{
		List<string> list = new List<string>();
		if (string.IsNullOrEmpty(path))
		{
			return list;
		}
		list.Add(path);
		int num = path.LastIndexOf('.');
		if (num == -1)
		{
			return list;
		}
		string text = path.Substring(0, num);
		string text2 = path.Substring(num);
		int num2 = 100;
		for (int i = 1; i <= num2; i++)
		{
			string text3 = text + i + text2;
			string assetPath = text3.Replace(assetFileRoot, "").Replace(Path.DirectorySeparatorChar, '/');
			if (!SingletonAuto<AssetFileManager>.instance.TableExist(assetPath, showLog: false))
			{
				break;
			}
			list.Add(text3);
		}
		return list;
	}

	private static object ReadJsonObject(string path, bool encrypted, bool isJsonBin)
	{
		string downloadedAssetPath = PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, "");
		List<string> partialAssetPathList = GetPartialAssetPathList(path, downloadedAssetPath);
		if (__Extension_Method_ICollection__.COUNT(partialAssetPathList) == 0)
		{
			return null;
		}
		List<object> list = JsonPool.instance.CreateList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(partialAssetPathList); i++)
		{
			string text = partialAssetPathList[i];
			List<object> list2 = null;
			try
			{
				list2 = ((!isJsonBin) ? PlainAssetHelper.LoadJsonObjectFromJsonText(File.ReadAllText(text), text, encrypted) : PlainAssetHelper.LoadJsonObjectFromJsonBin(File.ReadAllBytes(text), text, encrypted));
			}
			catch
			{
				VespaDebug.LogError("[JsonParseError]" + text + " , index : " + i);
				continue;
			}
			if (list2 == null)
			{
				VespaDebug.LogError("[JsonParseError] objList == null " + text + " , index : " + i);
				continue;
			}
			int num = text.LastIndexOf('\\') + 1;
			string path2 = text.Substring(num, text.Length - num);
			SingletonAuto<AssetFileManager>.instance.AddToPreloadTable(path2);
			list.AddRange(list2);
		}
		return list;
	}
}
