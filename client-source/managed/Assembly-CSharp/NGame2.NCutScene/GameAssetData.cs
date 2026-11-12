using System;
using System.Collections.Generic;
using LitJson;
using NGame2.NCutScene.NScript;
using NGame2.NScene.NPreloader;
using NGameAsset;
using NGameAsset.NCreature;
using NGameAsset.NNgui;
using NGameAsset.NPlainAsset;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene;

public class GameAssetData : IGenericVisitor<CreatureEnter>, IVisitor, IGenericVisitor<CreatureAction>, IGenericVisitor<PlayAssetSound>, IGenericVisitor<SceneChange>, IGenericVisitor<NodeTreeControl>
{
	private class LoadCreatureData
	{
		public int Slot;

		public CreatureStock Stock;
	}

	public PlainAssetJson ScriptAsset;

	private List<INguiPrefab> _nguiPrefabList = new List<INguiPrefab>();

	private SimplePreloader _preloader = new SimplePreloader(AnimationKeyData.EmptyPreloadSet);

	private List<Tuple<int, CreatureStock>> _loadCreatureList = new List<Tuple<int, CreatureStock>>();

	private Dictionary<int, HashSet<AnimationKeyData>> _preloadSetDic = new Dictionary<int, HashSet<AnimationKeyData>>();

	public bool IsCompleteAsyncLoad()
	{
		_preloader.Tick();
		if (!_preloader.IsEnd)
		{
			return false;
		}
		for (int i = 0; i < _nguiPrefabList.Count; i++)
		{
			if (!_nguiPrefabList[i].ISDONE())
			{
				return false;
			}
		}
		return true;
	}

	public void Close()
	{
		if (ScriptAsset != null)
		{
			JsonContainer.ReleaseAsset(ScriptAsset);
			ScriptAsset = null;
		}
		_preloader.ReleaseAsyncLoadAll();
		for (int i = 0; i < _nguiPrefabList.Count; i++)
		{
			_nguiPrefabList[i]?.Close();
		}
		_nguiPrefabList.Clear();
		_loadCreatureList.Clear();
		_preloadSetDic.Clear();
	}

	public void LoadScript(string scriptPath, Action<JsonData> onLoadComplete)
	{
		ScriptAsset = SingletonAuto<JsonContainer>.instance.Lock(scriptPath);
		AsyncManager.SingleEntry singleEntry = new AsyncManager.SingleEntry();
		singleEntry.AsyncLoad = ScriptAsset;
		singleEntry.OnComplete = delegate
		{
			Vespa_System_Action_Extension_Method.fire(onLoadComplete, ScriptAsset.Json);
		};
		SingletonUpdater<AsyncManager>.instance.Add(singleEntry);
	}

	public void LoadEntryMenuBackground(string entryMenuCode)
	{
		EntryMenuSettingData dataByMenuType = GameRoot.Instance.EntryMenuSettingDataContainer.GetDataByMenuType(entryMenuCode);
		if (dataByMenuType != null && !String_Extension_Method.IsNullOrEmpty(dataByMenuType.AssetNguiCode))
		{
			INguiPrefab nguiPrefab = SingletonAuto<Container>.instance.LoadPrefab(dataByMenuType.AssetNguiCode);
			if (nguiPrefab != null)
			{
				_nguiPrefabList.Add(nguiPrefab);
			}
		}
	}

	public void BeginLoadCreature()
	{
		for (int i = 0; i < _loadCreatureList.Count; i++)
		{
			Tuple<int, CreatureStock> tuple = _loadCreatureList[i];
			int item = tuple.Item1;
			CreatureStock item2 = tuple.Item2;
			Builder builder = SingletonAuto<BuilderContainer>.instance.Get(item2.Code);
			if (builder != null)
			{
				HashSet<AnimationKeyData> hashSet = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_preloadSetDic, item, null);
				if (hashSet == null)
				{
					hashSet = AnimationKeyData.EmptyPreloadSet;
				}
				item2.PreloadSet = hashSet;
				OpenData asyncLoad = builder.CreateOpenData(item2);
				_preloader.AddAsyncLoad(asyncLoad);
			}
		}
	}

	void IGenericVisitor<CreatureEnter>.Visit(CreatureEnter script)
	{
		if (SingletonAuto<BuilderContainer>.instance.Get(script.CreatureCode) != null)
		{
			CreatureStock stock = script.GetBuilder().GetStock();
			_loadCreatureList.Add(Tuple.Create(script.SlotIndex, stock));
		}
	}

	void IGenericVisitor<CreatureAction>.Visit(CreatureAction script)
	{
		HashSet<AnimationKeyData> hashSet = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_preloadSetDic, script.SlotIndex, null);
		if (hashSet == null)
		{
			hashSet = new HashSet<AnimationKeyData>();
			_preloadSetDic.Add(script.SlotIndex, hashSet);
		}
		hashSet.Add(script.ActionMod.ActionStock.AnimationKey);
	}

	void IGenericVisitor<PlayAssetSound>.Visit(PlayAssetSound script)
	{
		List<IAsyncLoad<AudioClip>> asyncLoadList = SingletonAuto<SoundContainer>.instance.ReserveSound(script.Code);
		_preloader.AddAsyncLoadList(asyncLoadList);
	}

	void IGenericVisitor<SceneChange>.Visit(SceneChange script)
	{
		LoadEntryMenuBackground(script.EntryMenuCode);
	}

	void IGenericVisitor<NodeTreeControl>.Visit(NodeTreeControl script)
	{
		script.LinkNodeTree.ReserveCache(_preloader);
	}
}
