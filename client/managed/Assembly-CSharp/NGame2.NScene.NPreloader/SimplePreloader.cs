using System;
using System.Collections.Generic;
using NGame2.NNodeTree;
using NGameAsset;
using NGameAsset.NCreature;
using NVespa;
using UnityEngine;

namespace NGame2.NScene.NPreloader;

public class SimplePreloader : IResourcePreloader
{
	private struct ElapsedRealTime
	{
		public const float Timeout = 0.05f;

		private float _realtimeBegin;

		public void Begin()
		{
			_realtimeBegin = Time.realtimeSinceStartup;
		}

		public bool IsTimeout()
		{
			return Time.realtimeSinceStartup - _realtimeBegin > 0.05f;
		}
	}

	private class AssetTexInfo
	{
		public int AddIndex;

		public AssetTexture AssetTex;

		public bool IsComplete;
	}

	private class AsyncLoadInfo
	{
		public int AddIndex;

		public IAsyncLoadBase AsyncLoad;

		public bool IsComplete;
	}

	private class NodeTreeInfo
	{
		public int AddIndex;

		public PackLoad PackLoad;

		public long TreeUid;

		public bool IsComplete;
	}

	private class ActionInfo
	{
		public int AddIndex;

		public Action Action;

		public string Desc;

		public bool IsComplete;
	}

	private class CompleteFuncInfo
	{
		public int AddIndex;

		public Func<bool> Func;

		public string Desc;

		public bool IsComplete;
	}

	private class LoadAssetData
	{
		public List<AssetTexInfo> AssetTexList = new List<AssetTexInfo>();

		public List<AsyncLoadInfo> AsyncLoadList = new List<AsyncLoadInfo>();

		public List<NodeTreeInfo> NodeTreeList = new List<NodeTreeInfo>();

		public List<ActionInfo> ActionList = new List<ActionInfo>();

		public List<CompleteFuncInfo> CompleteFuncList = new List<CompleteFuncInfo>();

		public int CompleteCount;

		public int TotalCount => AssetTexList.Count + AsyncLoadList.Count + NodeTreeList.Count + ActionList.Count + CompleteFuncList.Count;

		public float Progress => (float)CompleteCount / Vespa_Float_Extension_Method.NoneZero(TotalCount);
	}

	public static float ResourcePreloadTime;

	public EAsyncPreloadType CreaturePreloadType;

	public EAsyncPreloadType EffectPreloadType;

	public EAsyncPreloadType SoundPreloadType;

	public bool DumpActionTime;

	private HashSet<AnimationKeyData> _preloadAnimationSet;

	private LoadAssetData _currentLoad = new LoadAssetData();

	private LoadAssetData _totalLoad = new LoadAssetData();

	private HashSet<IAsyncLoadBase> _filterReserveOnlySet = new HashSet<IAsyncLoadBase>();

	private List<IAsyncLoadBase> _reserveOnlyList = new List<IAsyncLoadBase>();

	private int _addCount;

	private int _frameCount;

	private bool _isEnd;

	private HashSet<string> _filterAssetTexSet = new HashSet<string>();

	private HashSet<IAsyncLoadBase> _filterAsyncLoadSet = new HashSet<IAsyncLoadBase>();

	private HashSet<Tuple<PackLoad, long>> _filterNodeTreeSet = new HashSet<Tuple<PackLoad, long>>();

	private HashSet<Action> _filterActionSet = new HashSet<Action>();

	private HashSet<Func<bool>> _filterFuncSet = new HashSet<Func<bool>>();

	public virtual object ExtraData => null;

	public bool IsEnd => _isEnd;

	EAsyncPreloadType IResourcePreloader.CreaturePreloadType => CreaturePreloadType;

	EAsyncPreloadType IResourcePreloader.EffectPreloadType => EffectPreloadType;

	EAsyncPreloadType IResourcePreloader.SoundPreloadType => SoundPreloadType;

	public SimplePreloader(HashSet<AnimationKeyData> preloadAnimationSet)
	{
		_preloadAnimationSet = preloadAnimationSet;
	}

	public void ExecuteAction(EAsyncPreloadType preloadType, Action action)
	{
		Vespa_System_Action_Extension_Method.fire(action);
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
			if (_filterReserveOnlySet.Contains(asyncLoad))
			{
				Tools.ReleaseAsyncLoad(asyncLoad);
				return;
			}
			_filterReserveOnlySet.Add(asyncLoad);
			_reserveOnlyList.Add(asyncLoad);
		}
	}

	private void AddAsyncLoadReal(IAsyncLoadBase asyncLoad)
	{
		if (asyncLoad != null)
		{
			if (_filterAsyncLoadSet.Contains(asyncLoad))
			{
				Tools.ReleaseAsyncLoad(asyncLoad);
				return;
			}
			_filterAsyncLoadSet.Add(asyncLoad);
			AsyncLoadInfo asyncLoadInfo = new AsyncLoadInfo();
			asyncLoadInfo.AsyncLoad = asyncLoad;
			asyncLoadInfo.IsComplete = false;
			asyncLoadInfo.AddIndex = _addCount++;
			_currentLoad.AsyncLoadList.Add(asyncLoadInfo);
			_isEnd = false;
		}
	}

	public void AddNodeTree(string packPathName, long treeUid = -1L)
	{
		if (String_Extension_Method.IsNullOrEmpty(packPathName))
		{
			return;
		}
		PackLoad pack = SingletonAuto<PackContainer>.instance.GetPack(packPathName);
		if (pack != null)
		{
			Tuple<PackLoad, long> item = Tuple.Create(pack, treeUid);
			if (!_filterNodeTreeSet.Contains(item))
			{
				_filterNodeTreeSet.Add(item);
				NodeTreeInfo nodeTreeInfo = new NodeTreeInfo();
				nodeTreeInfo.PackLoad = pack;
				nodeTreeInfo.TreeUid = treeUid;
				nodeTreeInfo.IsComplete = false;
				nodeTreeInfo.AddIndex = _addCount++;
				_currentLoad.NodeTreeList.Add(nodeTreeInfo);
				_isEnd = false;
			}
		}
	}

	public void AddAction(Action action, string desc)
	{
		if (action != null && !_filterActionSet.Contains(action))
		{
			_filterActionSet.Add(action);
			ActionInfo actionInfo = new ActionInfo();
			actionInfo.Action = action;
			actionInfo.Desc = desc;
			actionInfo.IsComplete = false;
			actionInfo.AddIndex = _addCount++;
			_currentLoad.ActionList.Add(actionInfo);
			_isEnd = false;
		}
	}

	public void AddCompleteFunc(Func<bool> func, string desc)
	{
		if (func != null && !_filterFuncSet.Contains(func))
		{
			_filterFuncSet.Add(func);
			CompleteFuncInfo completeFuncInfo = new CompleteFuncInfo();
			completeFuncInfo.Func = func;
			completeFuncInfo.Desc = desc;
			completeFuncInfo.IsComplete = false;
			completeFuncInfo.AddIndex = _addCount++;
			_currentLoad.CompleteFuncList.Add(completeFuncInfo);
			_isEnd = false;
		}
	}

	public void Tick()
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
			for (int k = 0; k < _currentLoad.NodeTreeList.Count; k++)
			{
				if (_currentLoad.NodeTreeList[k].IsComplete)
				{
					_currentLoad.CompleteCount++;
				}
				else
				{
					_isEnd = false;
				}
			}
			for (int l = 0; l < _currentLoad.ActionList.Count; l++)
			{
				if (_currentLoad.ActionList[l].IsComplete)
				{
					_currentLoad.CompleteCount++;
				}
				else
				{
					_isEnd = false;
				}
			}
			for (int m = 0; m < _currentLoad.CompleteFuncList.Count; m++)
			{
				CompleteFuncInfo completeFuncInfo = _currentLoad.CompleteFuncList[m];
				if (!completeFuncInfo.IsComplete)
				{
					if (!Vespa_System_Action_Extension_Method.fire(completeFuncInfo.Func))
					{
						_isEnd = false;
						continue;
					}
					completeFuncInfo.IsComplete = true;
				}
				_currentLoad.CompleteCount++;
			}
			if (_isEnd)
			{
				MoveCurrentLoadToTotalLoad();
			}
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Preload Exception: " + ex.ToString());
			_isEnd = true;
		}
	}

	private void MoveCurrentLoadToTotalLoad()
	{
		_totalLoad.CompleteCount += _currentLoad.CompleteCount;
		_totalLoad.AssetTexList.AddRange(_currentLoad.AssetTexList);
		_totalLoad.AsyncLoadList.AddRange(_currentLoad.AsyncLoadList);
		_totalLoad.NodeTreeList.AddRange(_currentLoad.NodeTreeList);
		_totalLoad.ActionList.AddRange(_currentLoad.ActionList);
		_totalLoad.CompleteFuncList.AddRange(_currentLoad.CompleteFuncList);
		_currentLoad.CompleteCount = 0;
		_currentLoad.AssetTexList.Clear();
		_currentLoad.AsyncLoadList.Clear();
		_currentLoad.NodeTreeList.Clear();
		_currentLoad.ActionList.Clear();
		_currentLoad.CompleteFuncList.Clear();
	}

	private void UpdateLoadingInternal()
	{
		ElapsedRealTime elapsedRealTime = default(ElapsedRealTime);
		elapsedRealTime.Begin();
		for (int i = 0; i < _currentLoad.AssetTexList.Count; i++)
		{
			AssetTexInfo assetTexInfo = _currentLoad.AssetTexList[i];
			if (!assetTexInfo.IsComplete && assetTexInfo.AssetTex.IsDone && !assetTexInfo.IsComplete)
			{
				assetTexInfo.IsComplete = true;
			}
		}
		for (int j = 0; j < _currentLoad.AsyncLoadList.Count; j++)
		{
			AsyncLoadInfo asyncLoadInfo = _currentLoad.AsyncLoadList[j];
			if (!asyncLoadInfo.IsComplete && asyncLoadInfo.AsyncLoad.IsDone && !asyncLoadInfo.IsComplete)
			{
				asyncLoadInfo.IsComplete = true;
				OnComplete(asyncLoadInfo.AsyncLoad);
			}
		}
		for (int k = 0; k < _currentLoad.NodeTreeList.Count; k++)
		{
			NodeTreeInfo nodeTreeInfo = _currentLoad.NodeTreeList[k];
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
				for (int l = 0; l < nodeTreeInfo.PackLoad.Value.TreeList.Count; l++)
				{
					nodeTreeInfo.PackLoad.Value.TreeList[l].Tree?.ReserveCache(this);
				}
			}
			else
			{
				nodeTreeInfo.PackLoad.Value.GetNodeTree(nodeTreeInfo.TreeUid)?.ReserveCache(this);
			}
			nodeTreeInfo.IsComplete = true;
			if (elapsedRealTime.IsTimeout())
			{
				break;
			}
		}
		for (int m = 0; m < _currentLoad.ActionList.Count; m++)
		{
			ActionInfo actionInfo = _currentLoad.ActionList[m];
			if (!actionInfo.IsComplete)
			{
				using (new VespaProfiler.Block(actionInfo.Desc))
				{
					Vespa_System_Action_Extension_Method.fire(actionInfo.Action);
				}
				actionInfo.IsComplete = true;
				if (elapsedRealTime.IsTimeout())
				{
					break;
				}
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
		for (int j = 0; j < _totalLoad.AssetTexList.Count; j++)
		{
			_totalLoad.AssetTexList[j].AssetTex.Close();
		}
		_totalLoad.AssetTexList.Clear();
		for (int k = 0; k < _currentLoad.AsyncLoadList.Count; k++)
		{
			Tools.ReleaseAsyncLoad(_currentLoad.AsyncLoadList[k].AsyncLoad);
		}
		_currentLoad.AsyncLoadList.Clear();
		for (int l = 0; l < _totalLoad.AsyncLoadList.Count; l++)
		{
			Tools.ReleaseAsyncLoad(_totalLoad.AsyncLoadList[l].AsyncLoad);
		}
		_totalLoad.AsyncLoadList.Clear();
		for (int m = 0; m < _reserveOnlyList.Count; m++)
		{
			Tools.ReleaseAsyncLoad(_reserveOnlyList[m]);
		}
		_reserveOnlyList.Clear();
	}

	private void OnComplete(IAsyncLoadBase asyncLoadBase)
	{
	}
}
