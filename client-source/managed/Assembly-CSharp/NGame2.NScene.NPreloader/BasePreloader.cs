using System;
using System.Collections.Generic;
using System.Text;
using NGame2.NNodeTree;
using NGame2.NUI;
using NGameAsset;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NScene.NPreloader;

public class BasePreloader : IResourcePreloader
{
	public interface ILoadingTime
	{
		float BeginTime { get; }

		float EndTime { get; }
	}

	private class AssetTexInfo : ILoadingTime
	{
		public int AddIndex;

		public AssetTexture AssetTex;

		public bool IsComplete;

		public float BeginTime { get; set; }

		public float EndTime { get; set; }
	}

	private class AsyncLoadInfo : ILoadingTime
	{
		public int AddIndex;

		public IAsyncLoadBase AsyncLoad;

		public bool IsComplete;

		public float BeginTime { get; set; }

		public float EndTime { get; set; }
	}

	private class PrefabLoadInfo : ILoadingTime
	{
		public int AddIndex;

		public AsyncLoader.IPrefabLoad PrefabLoad;

		public Action OnComplete;

		public bool IsComplete;

		public float BeginTime => PrefabLoad.BeginLoad;

		public float EndTime => PrefabLoad.EndLoad;
	}

	private class NodeTreeInfo : ILoadingTime
	{
		public int AddIndex;

		public PackLoad PackLoad;

		public long TreeUid;

		public bool IsComplete;

		public float BeginTime { get; set; }

		public float EndTime { get; set; }
	}

	private class ActionInfo
	{
		public int AddIndex;

		public Action Action;

		public string Desc;

		public bool IsComplete;
	}

	private class CompleteFuncInfo : ILoadingTime
	{
		public int AddIndex;

		public Func<bool> Func;

		public string Desc;

		public bool IsComplete;

		public float BeginTime { get; set; }

		public float EndTime { get; set; }
	}

	private class LoadAssetData
	{
		public List<AssetTexInfo> AssetTexList = new List<AssetTexInfo>();

		public List<AsyncLoadInfo> AsyncLoadList = new List<AsyncLoadInfo>();

		public List<PrefabLoadInfo> PrefabLoadList2 = new List<PrefabLoadInfo>();

		public List<NodeTreeInfo> NodeTreeList = new List<NodeTreeInfo>();

		public List<ActionInfo> ActionList = new List<ActionInfo>();

		public List<CompleteFuncInfo> CompleteFuncList = new List<CompleteFuncInfo>();

		public int CompleteCount;

		public int TotalCount => AssetTexList.Count + AsyncLoadList.Count + PrefabLoadList2.Count + NodeTreeList.Count + ActionList.Count + CompleteFuncList.Count;

		public float Progress => (float)CompleteCount / Vespa_Float_Extension_Method.NoneZero(TotalCount);
	}

	public string PreloaderName = string.Empty;

	public bool DumpActionTime;

	protected RootPreloader _rootPreloader;

	private LoadAssetData _currentLoad = new LoadAssetData();

	private int _addCount;

	private int _frameCount;

	private bool _isEnd;

	private List<Action> _delayedActionList = new List<Action>();

	private List<IAsyncLoadBase> _reserveOnlyList = new List<IAsyncLoadBase>();

	public virtual object ExtraData => null;

	public virtual float Progress => _currentLoad.Progress;

	public virtual int Current => _currentLoad.CompleteCount;

	public virtual int Total => _currentLoad.TotalCount;

	public virtual bool IsEnd => _isEnd;

	EAsyncPreloadType IResourcePreloader.CreaturePreloadType => _rootPreloader.CreaturePreloadType;

	EAsyncPreloadType IResourcePreloader.EffectPreloadType => _rootPreloader.EffectPreloadType;

	EAsyncPreloadType IResourcePreloader.SoundPreloadType => _rootPreloader.SoundPreloadType;

	public BasePreloader(RootPreloader rootPreloader)
	{
		_rootPreloader = rootPreloader;
	}

	public virtual void Begin()
	{
		_isEnd = false;
		_delayedActionList.Clear();
	}

	public virtual void OnEnd()
	{
		for (int i = 0; i < _delayedActionList.Count; i++)
		{
			Vespa_System_Action_Extension_Method.fire(_delayedActionList[i]);
		}
		_delayedActionList.Clear();
	}

	public void ExecuteAction(EAsyncPreloadType preloadType, Action action)
	{
		switch (preloadType)
		{
		case EAsyncPreloadType.Normal:
			Vespa_System_Action_Extension_Method.fire(action);
			break;
		case EAsyncPreloadType.Delay:
			_delayedActionList.Add(action);
			break;
		}
	}

	public void AddAsyncLoadList<T>(List<T> asyncLoadList) where T : IAsyncLoadBase
	{
		if (asyncLoadList != null)
		{
			for (int i = 0; i < asyncLoadList.Count; i++)
			{
				AddAsyncLoad(asyncLoadList[i]);
			}
		}
	}

	public void AddBundleTexPath(string bundleTexPath)
	{
		if (!String_Extension_Method.IsNullOrEmpty(bundleTexPath) && !_rootPreloader.FilterAssetTexture(bundleTexPath))
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(bundleTexPath);
			AssetTexInfo assetTexInfo = new AssetTexInfo();
			assetTexInfo.AssetTex = plainTexture;
			assetTexInfo.IsComplete = false;
			assetTexInfo.AddIndex = _addCount++;
			assetTexInfo.BeginTime = Time.realtimeSinceStartup;
			_currentLoad.AssetTexList.Add(assetTexInfo);
		}
	}

	public void AddAsyncLoad(IAsyncLoadBase asyncLoad)
	{
		if (_isEnd)
		{
			ReserveAsyncLoad(asyncLoad);
		}
		else
		{
			AddAsyncLoadReal(asyncLoad);
		}
	}

	public void ReserveAsyncLoad(IAsyncLoadBase asyncLoad)
	{
		if (asyncLoad != null)
		{
			if (_rootPreloader.FilterReserveOnly(asyncLoad))
			{
				Tools.ReleaseAsyncLoad(asyncLoad);
			}
			else
			{
				_reserveOnlyList.Add(asyncLoad);
			}
		}
	}

	private void AddAsyncLoadReal(IAsyncLoadBase asyncLoad)
	{
		if (asyncLoad != null)
		{
			if (_rootPreloader.FilterAsyncLoad(asyncLoad))
			{
				Tools.ReleaseAsyncLoad(asyncLoad);
				return;
			}
			AsyncLoadInfo asyncLoadInfo = new AsyncLoadInfo();
			asyncLoadInfo.AsyncLoad = asyncLoad;
			asyncLoadInfo.IsComplete = false;
			asyncLoadInfo.AddIndex = _addCount++;
			asyncLoadInfo.BeginTime = Time.realtimeSinceStartup;
			_currentLoad.AsyncLoadList.Add(asyncLoadInfo);
		}
	}

	public void AddPrefabLoad(AsyncLoader.IPrefabLoad prefabLoad, Action onComplete)
	{
		if (prefabLoad == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		PrefabLoadInfo prefabLoadInfo = new PrefabLoadInfo();
		prefabLoadInfo.PrefabLoad = prefabLoad;
		prefabLoadInfo.OnComplete = onComplete;
		prefabLoadInfo.IsComplete = false;
		prefabLoadInfo.AddIndex = _addCount++;
		_currentLoad.PrefabLoadList2.Add(prefabLoadInfo);
	}

	public void AddNodeTree(string packPathName, long treeUid = -1L)
	{
		if (!String_Extension_Method.IsNullOrEmpty(packPathName))
		{
			PackLoad pack = SingletonAuto<PackContainer>.instance.GetPack(packPathName);
			if (pack != null && !_rootPreloader.FilterNodeTree(pack, treeUid))
			{
				NodeTreeInfo nodeTreeInfo = new NodeTreeInfo();
				nodeTreeInfo.PackLoad = pack;
				nodeTreeInfo.TreeUid = treeUid;
				nodeTreeInfo.IsComplete = false;
				nodeTreeInfo.AddIndex = _addCount++;
				nodeTreeInfo.BeginTime = Time.realtimeSinceStartup;
				_currentLoad.NodeTreeList.Add(nodeTreeInfo);
			}
		}
	}

	public void AddAction(Action action, string desc)
	{
		if (action != null && !_rootPreloader.FilterAction(action))
		{
			ActionInfo actionInfo = new ActionInfo();
			actionInfo.Action = action;
			actionInfo.Desc = desc;
			actionInfo.IsComplete = false;
			actionInfo.AddIndex = _addCount++;
			_currentLoad.ActionList.Add(actionInfo);
		}
	}

	public void AddCompleteFunc(Func<bool> func, string desc)
	{
		if (func != null && !_rootPreloader.FilterFunc(func))
		{
			CompleteFuncInfo completeFuncInfo = new CompleteFuncInfo();
			completeFuncInfo.Func = func;
			completeFuncInfo.Desc = desc;
			completeFuncInfo.IsComplete = false;
			completeFuncInfo.AddIndex = _addCount++;
			completeFuncInfo.BeginTime = Time.realtimeSinceStartup;
			_currentLoad.CompleteFuncList.Add(completeFuncInfo);
		}
	}

	public void ReserveComponent<T>(PrefabAuto<T> prefabAuto, int count = 0) where T : Component
	{
		prefabAuto.ReservePrefab();
	}

	public void ChainUpdate()
	{
		if (_frameCount == Time.frameCount)
		{
			return;
		}
		_frameCount = Time.frameCount;
		try
		{
			UpdateLoadingInternal();
			_currentLoad.CompleteCount = 0;
			_isEnd = true;
			for (int i = 0; i < _currentLoad.AssetTexList.Count; i++)
			{
				if (_currentLoad.AssetTexList[i].IsComplete)
				{
					_currentLoad.CompleteCount++;
				}
				else
				{
					_isEnd = false;
				}
			}
			for (int j = 0; j < _currentLoad.AsyncLoadList.Count; j++)
			{
				if (_currentLoad.AsyncLoadList[j].IsComplete)
				{
					_currentLoad.CompleteCount++;
				}
				else
				{
					_isEnd = false;
				}
			}
			for (int k = 0; k < _currentLoad.PrefabLoadList2.Count; k++)
			{
				if (_currentLoad.PrefabLoadList2[k].IsComplete)
				{
					_currentLoad.CompleteCount++;
				}
				else
				{
					_isEnd = false;
				}
			}
			for (int l = 0; l < _currentLoad.NodeTreeList.Count; l++)
			{
				if (_currentLoad.NodeTreeList[l].IsComplete)
				{
					_currentLoad.CompleteCount++;
				}
				else
				{
					_isEnd = false;
				}
			}
			for (int m = 0; m < _currentLoad.ActionList.Count; m++)
			{
				if (_currentLoad.ActionList[m].IsComplete)
				{
					_currentLoad.CompleteCount++;
				}
				else
				{
					_isEnd = false;
				}
			}
			for (int n = 0; n < _currentLoad.CompleteFuncList.Count; n++)
			{
				if (_currentLoad.CompleteFuncList[n].IsComplete)
				{
					_currentLoad.CompleteCount++;
				}
				else
				{
					_isEnd = false;
				}
			}
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Preload Exception: " + ex.ToString());
			_isEnd = true;
		}
	}

	private void UpdateLoadingInternal()
	{
		for (int i = 0; i < _currentLoad.AssetTexList.Count; i++)
		{
			AssetTexInfo assetTexInfo = _currentLoad.AssetTexList[i];
			if (!assetTexInfo.IsComplete && assetTexInfo.AssetTex.IsDone && !assetTexInfo.IsComplete)
			{
				assetTexInfo.EndTime = Time.realtimeSinceStartup;
				assetTexInfo.IsComplete = true;
			}
		}
		for (int j = 0; j < _currentLoad.AsyncLoadList.Count; j++)
		{
			AsyncLoadInfo asyncLoadInfo = _currentLoad.AsyncLoadList[j];
			if (!asyncLoadInfo.IsComplete && asyncLoadInfo.AsyncLoad.IsDone && !asyncLoadInfo.IsComplete)
			{
				asyncLoadInfo.EndTime = Time.realtimeSinceStartup;
				asyncLoadInfo.IsComplete = true;
			}
		}
		for (int k = 0; k < _currentLoad.PrefabLoadList2.Count; k++)
		{
			PrefabLoadInfo prefabLoadInfo = _currentLoad.PrefabLoadList2[k];
			if (!prefabLoadInfo.IsComplete && prefabLoadInfo.PrefabLoad.IsDone && !prefabLoadInfo.IsComplete)
			{
				prefabLoadInfo.IsComplete = true;
				Vespa_System_Action_Extension_Method.fire(prefabLoadInfo.OnComplete);
				prefabLoadInfo.OnComplete = null;
			}
		}
		for (int l = 0; l < _currentLoad.NodeTreeList.Count; l++)
		{
			NodeTreeInfo nodeTreeInfo = _currentLoad.NodeTreeList[l];
			if (nodeTreeInfo.IsComplete || !nodeTreeInfo.PackLoad.IsDone)
			{
				continue;
			}
			if (!nodeTreeInfo.PackLoad.ISVALID())
			{
				nodeTreeInfo.IsComplete = true;
				continue;
			}
			if (nodeTreeInfo.TreeUid == -1)
			{
				nodeTreeInfo.PackLoad.Value.ReserveCache(this);
			}
			else
			{
				nodeTreeInfo.PackLoad.Value.GetNodeTree(nodeTreeInfo.TreeUid)?.ReserveCache(this);
			}
			nodeTreeInfo.EndTime = Time.realtimeSinceStartup;
			nodeTreeInfo.IsComplete = true;
			break;
		}
		for (int m = 0; m < _currentLoad.ActionList.Count; m++)
		{
			ActionInfo actionInfo = _currentLoad.ActionList[m];
			if (!actionInfo.IsComplete)
			{
				using (new VespaProfiler.Block(actionInfo.Desc))
				{
					Vespa_System_Action_Extension_Method.fire(actionInfo.Action);
					actionInfo.Action = null;
				}
				actionInfo.IsComplete = true;
				break;
			}
		}
		for (int n = 0; n < _currentLoad.CompleteFuncList.Count; n++)
		{
			CompleteFuncInfo completeFuncInfo = _currentLoad.CompleteFuncList[n];
			if (!completeFuncInfo.IsComplete && Vespa_System_Action_Extension_Method.fire(completeFuncInfo.Func))
			{
				completeFuncInfo.Func = null;
				completeFuncInfo.EndTime = Time.realtimeSinceStartup;
				completeFuncInfo.IsComplete = true;
				break;
			}
		}
	}

	public void ReleaseAsyncLoadAll()
	{
		for (int i = 0; i < _currentLoad.AssetTexList.Count; i++)
		{
			_currentLoad.AssetTexList[i].AssetTex.Close();
		}
		_currentLoad.AssetTexList.Clear();
		for (int j = 0; j < _currentLoad.AsyncLoadList.Count; j++)
		{
			Tools.ReleaseAsyncLoad(_currentLoad.AsyncLoadList[j].AsyncLoad);
		}
		_currentLoad.AsyncLoadList.Clear();
		for (int k = 0; k < _currentLoad.PrefabLoadList2.Count; k++)
		{
			Tools.ReleaseAsyncLoad(_currentLoad.PrefabLoadList2[k].PrefabLoad);
		}
		_currentLoad.PrefabLoadList2.Clear();
		for (int l = 0; l < _reserveOnlyList.Count; l++)
		{
			Tools.ReleaseAsyncLoad(_reserveOnlyList[l]);
		}
		_reserveOnlyList.Clear();
		_delayedActionList.Clear();
	}

	public void GetLoadingTime(StringBuilder sb)
	{
		List<Tuple<int, string, ILoadingTime>> loadingSummary = GetLoadingSummary(_currentLoad);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(loadingSummary); i++)
		{
			Tuple<int, string, ILoadingTime> tuple = loadingSummary[i];
			if (tuple.Item3 != null)
			{
				sb.AppendLine(tuple.Item2);
				sb.Append("> ");
				sb.AppendLine(tuple.Item3.CalcRelativeTime(_rootPreloader.BeginTime, _rootPreloader.EndTime));
				sb.AppendLine();
			}
		}
	}

	private List<Tuple<int, string, ILoadingTime>> GetLoadingSummary(LoadAssetData loadAsset)
	{
		List<Tuple<int, string, ILoadingTime>> list = new List<Tuple<int, string, ILoadingTime>>();
		for (int i = 0; i < loadAsset.AssetTexList.Count; i++)
		{
			AssetTexInfo assetTexInfo = loadAsset.AssetTexList[i];
			string text = $"AssetTex: {assetTexInfo.AssetTex.Path} - ";
			if (assetTexInfo.IsComplete)
			{
				text += "Complete, ";
			}
			list.Add(Tuple.Create(assetTexInfo.AddIndex, text, Vespa_Common_Class_Extension_Method.Interface((ILoadingTime)assetTexInfo)));
		}
		for (int j = 0; j < loadAsset.AsyncLoadList.Count; j++)
		{
			AsyncLoadInfo asyncLoadInfo = loadAsset.AsyncLoadList[j];
			string text2 = $"Async: {asyncLoadInfo.AsyncLoad.KeyDesc}({asyncLoadInfo.AsyncLoad.GetType().Name}) - ";
			if (asyncLoadInfo.IsComplete)
			{
				text2 += "Complete, ";
			}
			list.Add(Tuple.Create(asyncLoadInfo.AddIndex, text2, Vespa_Common_Class_Extension_Method.Interface((ILoadingTime)asyncLoadInfo)));
		}
		for (int k = 0; k < loadAsset.PrefabLoadList2.Count; k++)
		{
			PrefabLoadInfo prefabLoadInfo = loadAsset.PrefabLoadList2[k];
			string text3 = $"Async: {prefabLoadInfo.PrefabLoad.KeyDesc}({prefabLoadInfo.PrefabLoad.GetType().Name}) - ";
			if (prefabLoadInfo.IsComplete)
			{
				text3 += "Complete, ";
			}
			list.Add(Tuple.Create(prefabLoadInfo.AddIndex, text3, Vespa_Common_Class_Extension_Method.Interface((ILoadingTime)prefabLoadInfo)));
		}
		for (int l = 0; l < loadAsset.NodeTreeList.Count; l++)
		{
			NodeTreeInfo nodeTreeInfo = loadAsset.NodeTreeList[l];
			string text4 = Vespa_String_Extension_Method.ColorTag("NodeTree", Color.yellow);
			if (!nodeTreeInfo.IsComplete)
			{
				text4 += $": {nodeTreeInfo.PackLoad.PathName}({nodeTreeInfo.TreeUid})";
			}
			else
			{
				if (nodeTreeInfo.PackLoad == null)
				{
					VespaDebug.LogError("treeInfo.PackLoad == null");
				}
				if (nodeTreeInfo.PackLoad.Value == null)
				{
					VespaDebug.LogError("treeInfo.PackLoad.Value == null");
				}
				TreeData nodeTree = nodeTreeInfo.PackLoad.Value.GetNodeTree(nodeTreeInfo.TreeUid);
				text4 = ((nodeTree != null) ? (text4 + $": {nodeTreeInfo.PackLoad.PathName}({nodeTree.Code}) - Complete") : (text4 + $": {nodeTreeInfo.PackLoad.PathName}({nodeTreeInfo.TreeUid}) - Complete"));
			}
			list.Add(Tuple.Create(nodeTreeInfo.AddIndex, text4, Vespa_Common_Class_Extension_Method.Interface((ILoadingTime)nodeTreeInfo)));
		}
		for (int m = 0; m < loadAsset.ActionList.Count; m++)
		{
			ActionInfo actionInfo = loadAsset.ActionList[m];
			string text5 = Vespa_String_Extension_Method.ColorTag("Action", Color.blue) + $": {actionInfo.Desc} - ";
			if (actionInfo.IsComplete)
			{
				text5 += "Complete";
			}
			list.Add(Tuple.Create<int, string, ILoadingTime>(actionInfo.AddIndex, text5, null));
		}
		for (int n = 0; n < loadAsset.CompleteFuncList.Count; n++)
		{
			CompleteFuncInfo completeFuncInfo = loadAsset.CompleteFuncList[n];
			string text6 = Vespa_String_Extension_Method.ColorTag("Func", Color.green) + $": {completeFuncInfo.Desc} - ";
			if (completeFuncInfo.IsComplete)
			{
				text6 += "Complete";
			}
			list.Add(Tuple.Create(completeFuncInfo.AddIndex, text6, Vespa_Common_Class_Extension_Method.Interface((ILoadingTime)completeFuncInfo)));
		}
		list.Sort((Tuple<int, string, ILoadingTime> lhs, Tuple<int, string, ILoadingTime> rhs) => lhs.Item1.CompareTo(rhs.Item1));
		return list;
	}
}
