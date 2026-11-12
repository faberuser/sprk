using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JsonBin;
using NGameAsset.NBattleFieldMap2;
using NGameAsset.NCreature;
using NGameAsset.NEffect;
using NGameAsset.NNgui;
using NGameAsset.NNguiAtlas;
using NGameAsset.NSound;
using NGameAsset.NWorldMap;
using NVespa;
using UnityEngine;

namespace NGameAsset.NBundleManager;

public class LoaderPlaying : SingletonUpdater<LoaderPlaying>, BundleManager.ILoader
{
	public enum EBundleLoadType
	{
		LoadFromFileAsync,
		LoadFromFile,
		Www,
		Thread
	}

	private const int MaxLoadingCount = 10;

	private const int MaxInsertPendingPerFrame = 100;

	private const int DestroyDelayTick = 10000;

	private IBundleLoad _shaderBundleLoad;

	private bool _isReady;

	private Action _onReady;

	private List<IBundleLoad> _pendingList = new List<IBundleLoad>();

	private List<IBundleLoad> _loadingList = new List<IBundleLoad>();

	private HashSet<IBundleLoad> _closingSet = new HashSet<IBundleLoad>();

	private List<IBundleLoad> _downLoadingList = new List<IBundleLoad>();

	private Dictionary<string, IBundleLoad> _dic = new Dictionary<string, IBundleLoad>(StringComparer.OrdinalIgnoreCase);

	public static float BundlePackLoad_ElapacedTime = 0f;

	public static LocalStorageEnum<EBundleLoadType> BundleLoadType = new LocalStorageEnum<EBundleLoadType>(EBundleLoadType.LoadFromFileAsync, typeof(LoaderPlaying).FullName + "BundleLoadType");

	int BundleManager.ILoader.LoadingCount => __Extension_Method_ICollection__.COUNT(_loadingList);

	int BundleManager.ILoader.BundleCount => _dic.Count;

	bool BundleManager.ILoader.IsReady => _isReady;

	bool BundleManager.ILoader.IsLoadShader
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	bool BundleManager.ILoader.IsLoadPack
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	public BundleInfoContainer InfoContainer { get; set; }

	private static string AdjustBundleFileName(string bundleFileName)
	{
		if (bundleFileName.EndsWith(".unity3d", StringComparison.OrdinalIgnoreCase))
		{
			return bundleFileName;
		}
		return bundleFileName + ".unity3d";
	}

	public LoaderPlaying()
	{
		InfoContainer = new BundleInfoContainer();
		VespaCoroutine.Start(coReady());
	}

	void BundleManager.ILoader.Load()
	{
	}

	private void OnMessage(string msg)
	{
		Vespa_System_Action_Extension_Method.fire(SingletonAuto<BundleManager>.instance.OnMessage, msg);
	}

	private IEnumerator coReady()
	{
		while (!Define.PatchDownloaded)
		{
			yield return null;
		}
		OnMessage("BundlePack Load");
		if (!LoadPackFromBinary())
		{
			OnMessage("BundlePack Load Failed.....");
			yield break;
		}
		OnMessage("Shader Bundle Load");
		_shaderBundleLoad = CreateBundleLoad("Unity_2018/Shader.unity3d", pending: false);
		_shaderBundleLoad.StartLoad();
		while (!_shaderBundleLoad.IsDone)
		{
			yield return null;
		}
		_shaderBundleLoad.LoadAll();
		_isReady = true;
		Vespa_System_Action_Extension_Method.fire(_onReady);
		_onReady = null;
	}

	public override void ChainUpdate()
	{
		if (!_isReady || (SingletonUpdater<AsyncManager>.instance.AsyncOperationCount == 0 && ((_closingSet.Count > 0 && CloseBundleFromClosingList()) || CloseBundleFromLastOne())))
		{
			return;
		}
		lock (_downLoadingList)
		{
			for (int i = 0; i < _downLoadingList.Count; i++)
			{
				IBundleLoad bundleLoad = _downLoadingList[i];
				if (bundleLoad.IsDownloaded())
				{
					_downLoadingList.RemoveAt(i);
					i--;
					_loadingList.Add(bundleLoad);
					bundleLoad.StartLoad();
				}
			}
		}
		lock (_loadingList)
		{
			for (int j = 0; j < _loadingList.Count; j++)
			{
				if (_loadingList[j].IsDone)
				{
					_loadingList.RemoveAt(j);
					j--;
				}
			}
		}
		int num = 0;
		while (num < 100)
		{
			lock (_pendingList)
			{
				lock (_loadingList)
				{
					if (_pendingList.Count == 0 || _loadingList.Count >= 10)
					{
						break;
					}
					IBundleLoad bundleLoad2 = _pendingList[0];
					_pendingList.RemoveAt(0);
					_loadingList.Add(bundleLoad2);
					if (bundleLoad2.Exists())
					{
						bundleLoad2.StartLoad();
					}
					else
					{
						_downLoadingList.Add(bundleLoad2);
						bundleLoad2.StartDownload();
					}
					num++;
				}
			}
		}
	}

	private bool LoadPackFromBinary()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		string bundlePath = BundleHelper.GetBundlePath("BundlePack.bin", DefinePlatform.PlatformName, string.Empty, checkExist: true);
		if (String_Extension_Method.IsNullOrEmpty(bundlePath))
		{
			VespaDebug.LogError("BundlePack Load Failed: PackPath is null");
			return false;
		}
		byte[] bytes = File.ReadAllBytes(bundlePath);
		if (String_Extension_Method.IsNullOrEmpty(bundlePath))
		{
			VespaDebug.LogError("BundlePack Load Failed: load bytes is empty");
			return false;
		}
		JsonRead root = default(JsonRead);
		JsonRead.CreateRoot(bytes, out root);
		InfoContainer.LoadFromJson(root["BundleFileInfo"]);
		SingletonAuto<SoundDataContainer>.instance.LoadFromJson(root["SoundTable_Json"]);
		SingletonAuto<BgmDataContainer>.instance.LoadFromJson(root["BgmTable_Json"]);
		SingletonAuto<NGameAsset.NCreature.BuilderContainer>.instance.Load(root["Creature"]);
		SingletonAuto<NGameAsset.NEffect.BuilderContainer>.instance.Load(root["Effect"]);
		SingletonAuto<NGameAsset.NBattleFieldMap2.BuilderContainer>.instance.Load(root["BattleFieldMap2"]);
		SingletonAuto<NGameAsset.NNgui.Container>.instance.Load(root["Ngui"]);
		SingletonAuto<AtlasDataContainer>.instance.Load(root["NguiAtlas"], root["NguiAsset"]);
		SingletonAuto<NGameAsset.NWorldMap.Container>.instance.Load(root["WorldMap"]);
		SingletonAuto<CameraContainer>.instance.LoadFromJson(root["CameraData_Json"]);
		BundlePackLoad_ElapacedTime = Time.realtimeSinceStartup - realtimeSinceStartup;
		Debug.Log("BundleLoad elapsedTime: " + BundlePackLoad_ElapacedTime.ToString("F3"));
		return true;
	}

	IBundleLoad BundleManager.ILoader.LockBundle(string bundleFileName)
	{
		return LockBundleInternal(bundleFileName, pending: true);
	}

	void BundleManager.ILoader.ReleaseBundle(IBundleLoad bundleLoad, bool destroyImmediately)
	{
		ReleaseBundleInternal(bundleLoad, destroyImmediately);
	}

	private void ReleaseBundleInternal(IBundleLoad bundleLoad, bool destroyImmediately)
	{
		bundleLoad.Lock.Decrease(null);
		if (bundleLoad.Lock.Count <= 0 && destroyImmediately)
		{
			DestroyUnusedThisImmediately(bundleLoad);
		}
	}

	void BundleManager.ILoader.DestroyUnused(bool unusedAll)
	{
		if (unusedAll)
		{
			DestroyUnusedAll();
		}
	}

	private bool CloseBundleFromClosingList()
	{
		List<IBundleLoad> list = null;
		IBundleLoad bundleLoad = null;
		foreach (IBundleLoad item in _closingSet)
		{
			if (!item.ISDONE() || item.Lock.Count > 0)
			{
				if (list == null)
				{
					list = new List<IBundleLoad>();
				}
				list.Add(item);
				continue;
			}
			bundleLoad = item;
			break;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			_closingSet.Remove(list[i]);
		}
		if (bundleLoad != null)
		{
			_closingSet.Remove(bundleLoad);
			string bundleFileName = bundleLoad.BundleFileName;
			_dic.Remove(bundleFileName);
			bundleLoad.Close(unloadAllLoadedObjects: false);
			return true;
		}
		return false;
	}

	private bool CloseBundleFromLastOne()
	{
		int unscaledTick = UnitySystemTimer.unscaledTick;
		IBundleLoad bundleLoad = null;
		int num = int.MaxValue;
		foreach (KeyValuePair<string, IBundleLoad> item in _dic)
		{
			IBundleLoad value = item.Value;
			if (value.IsDone && value.Lock.Count <= 0 && unscaledTick - value.Lock.UnlockTick >= 10000 && value.Lock.UnlockTick < num)
			{
				num = value.Lock.UnlockTick;
				bundleLoad = value;
			}
		}
		if (bundleLoad != null)
		{
			_dic.Remove(bundleLoad.BundleFileName);
			bundleLoad.Close(unloadAllLoadedObjects: false);
			return true;
		}
		return false;
	}

	private void DestroyUnusedThisImmediately(IBundleLoad This)
	{
		if (This.ISDONE() && This.Lock.Count <= 0 && !_closingSet.Contains(This))
		{
			_closingSet.Add(This);
		}
	}

	private void DestroyUnusedAll()
	{
		foreach (KeyValuePair<string, IBundleLoad> item in _dic)
		{
			IBundleLoad value = item.Value;
			if (value.IsDone && value.Lock.Count <= 0 && !_closingSet.Contains(value))
			{
				_closingSet.Add(value);
			}
		}
	}

	void BundleManager.ILoader.ListenOnReady(Action onReady)
	{
		if (_isReady)
		{
			Vespa_System_Action_Extension_Method.fire(onReady);
		}
		else
		{
			_onReady = (Action)Delegate.Combine(_onReady, onReady);
		}
	}

	private IBundleLoad LockBundleInternal(string bundleFileName, bool pending)
	{
		bundleFileName = AdjustBundleFileName(bundleFileName);
		IBundleLoad bundleLoad = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, bundleFileName, null);
		if (bundleLoad == null)
		{
			bundleLoad = CreateBundleLoad(bundleFileName, pending);
			_dic.Add(bundleFileName, bundleLoad);
		}
		bundleLoad.Lock.Increase();
		return bundleLoad;
	}

	private IBundleLoad CreateBundleLoad(string bundleFileName, bool pending)
	{
		IBundleLoad bundleLoad = LoadInMobile(bundleFileName);
		if (bundleLoad == null)
		{
			return null;
		}
		if (pending)
		{
			lock (_pendingList)
			{
				_pendingList.Add(bundleLoad);
			}
		}
		return bundleLoad;
	}

	private IBundleLoad LoadInMobile(string bundleFileName)
	{
		BundleLoadFromFileAsync bundleLoadFromFileAsync = new BundleLoadFromFileAsync();
		bundleLoadFromFileAsync.Open(bundleFileName);
		return bundleLoadFromFileAsync;
	}

	private IBundleLoad LoadInWindow(string bundleFileName)
	{
		BundleLoadFromFile bundleLoadFromFile = new BundleLoadFromFile();
		bundleLoadFromFile.Open(bundleFileName);
		return bundleLoadFromFile;
	}

	string BundleManager.ILoader.GetSummary()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, IBundleLoad> item in _dic)
		{
			string fileName = Path.GetFileName(item.Key);
			string text = $"Lock={item.Value.Lock.Count,4}, ";
			if (item.Value.IsDone)
			{
				text += "Done, ";
			}
			if (item.Value.IsValid)
			{
				text += "Valid, ";
			}
			stringBuilder.AppendLine(fileName + "> " + text);
		}
		lock (_pendingList)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_pendingList); i++)
			{
				string fileName2 = Path.GetFileName(_pendingList[i].BundleFileName);
				string text2 = "Pending";
				stringBuilder.AppendLine(fileName2 + "> " + text2);
			}
		}
		lock (_loadingList)
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_loadingList); j++)
			{
				string fileName3 = Path.GetFileName(_loadingList[j].BundleFileName);
				string text3 = "Loading";
				stringBuilder.AppendLine(fileName3 + "> " + text3);
			}
		}
		lock (_closingSet)
		{
			foreach (IBundleLoad item2 in _closingSet)
			{
				string fileName4 = Path.GetFileName(item2.BundleFileName);
				string text4 = "Closing";
				stringBuilder.AppendLine(fileName4 + "> " + text4);
			}
		}
		return stringBuilder.ToString();
	}
}
