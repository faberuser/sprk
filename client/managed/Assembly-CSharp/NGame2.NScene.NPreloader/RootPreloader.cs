using System;
using System.Collections.Generic;
using System.Text;
using NGame2.N3D;
using NGame2.NNodeTree;
using NGameAsset;
using NShared;
using UnityEngine;

namespace NGame2.NScene.NPreloader;

public class RootPreloader : BaseScene.IPreload
{
	public static float ResourcePreloadTime;

	public EAsyncPreloadType CreaturePreloadType;

	public EAsyncPreloadType EffectPreloadType;

	public EAsyncPreloadType SoundPreloadType;

	private List<BasePreloader> _preloaderList = new List<BasePreloader>();

	private bool _isBegin;

	private bool _isEnd;

	private HashSet<Action<UiPreloader>> _uiPreloadFilter = new HashSet<Action<UiPreloader>>();

	private HashSet<Tuple<int, int, bool>> _creatureFilter = new HashSet<Tuple<int, int, bool>>();

	private HashSet<string> _assetTexFilter = new HashSet<string>();

	private HashSet<IAsyncLoadBase> _asyncLoadFilter = new HashSet<IAsyncLoadBase>();

	private HashSet<IAsyncLoadBase> _reserveOnlyFilter = new HashSet<IAsyncLoadBase>();

	private HashSet<Tuple<PackLoad, long>> _nodeTreeFilter = new HashSet<Tuple<PackLoad, long>>();

	private HashSet<Action> _actionFilter = new HashSet<Action>();

	private HashSet<Func<bool>> _funcFilter = new HashSet<Func<bool>>();

	public BasePreloader Preloader { get; private set; }

	public float BeginTime { get; private set; }

	public float EndTime { get; private set; }

	float BaseScene.IPreload.Progress
	{
		get
		{
			float num = Preloader.Progress;
			for (int i = 0; i < _preloaderList.Count; i++)
			{
				num += _preloaderList[i].Progress;
			}
			return num / (float)(1 + __Extension_Method_ICollection__.COUNT(_preloaderList));
		}
	}

	int BaseScene.IPreload.Current
	{
		get
		{
			int num = Preloader.Current;
			for (int i = 0; i < _preloaderList.Count; i++)
			{
				num += _preloaderList[i].Current;
			}
			return num;
		}
	}

	int BaseScene.IPreload.Total
	{
		get
		{
			int num = Preloader.Total;
			for (int i = 0; i < _preloaderList.Count; i++)
			{
				num += _preloaderList[i].Total;
			}
			return num;
		}
	}

	public bool IsEnd
	{
		get
		{
			if (!Preloader.IsEnd)
			{
				return false;
			}
			for (int i = 0; i < _preloaderList.Count; i++)
			{
				if (!_preloaderList[i].IsEnd)
				{
					return false;
				}
			}
			return true;
		}
	}

	public RootPreloader()
	{
		Preloader = new BasePreloader(this);
		Preloader.PreloaderName = "Main";
	}

	public virtual void Begin()
	{
		_isBegin = true;
		_isEnd = false;
		BeginTime = Time.realtimeSinceStartup;
		Preloader.Begin();
		for (int i = 0; i < _preloaderList.Count; i++)
		{
			_preloaderList[i].Begin();
		}
	}

	public void Tick()
	{
		Preloader.ChainUpdate();
		for (int i = 0; i < _preloaderList.Count; i++)
		{
			_preloaderList[i].ChainUpdate();
		}
	}

	void BaseScene.IPreload.OnEnd()
	{
		_uiPreloadFilter.Clear();
		_nodeTreeFilter.Clear();
		_actionFilter.Clear();
		_funcFilter.Clear();
		EndTime = Time.realtimeSinceStartup;
		ResourcePreloadTime = EndTime - BeginTime;
		Preloader.OnEnd();
		for (int i = 0; i < _preloaderList.Count; i++)
		{
			_preloaderList[i].OnEnd();
		}
	}

	public bool FilterAssetTexture(string assetTexPath)
	{
		return !_assetTexFilter.Add(assetTexPath);
	}

	public bool FilterReserveOnly(IAsyncLoadBase asyncLoad)
	{
		return !_reserveOnlyFilter.Add(asyncLoad);
	}

	public bool FilterAsyncLoad(IAsyncLoadBase asyncLoad)
	{
		return !_asyncLoadFilter.Add(asyncLoad);
	}

	public bool FilterNodeTree(PackLoad packLoad, long treeUid)
	{
		Tuple<PackLoad, long> item = Tuple.Create(packLoad, treeUid);
		return !_nodeTreeFilter.Add(item);
	}

	public bool FilterAction(Action action)
	{
		return !_actionFilter.Add(action);
	}

	public bool FilterFunc(Func<bool> func)
	{
		return !_funcFilter.Add(func);
	}

	public void AddCreature(HeroInfo heroInfo, ExtraSkillInfo[] addExtraSkill, bool loadCreatureImage)
	{
		_ = string.Empty;
		_ = string.Empty;
		RenderFacadeBuilder renderFacadeBuilder = ((!(heroInfo is CachedHeroInfo heroInfo2)) ? new RenderFacadeBuilder(null, TeamType.Left, heroInfo, null) : new RenderFacadeBuilder(null, TeamType.Left, heroInfo2));
		AddCreature(heroInfo, renderFacadeBuilder, addExtraSkill, loadCreatureImage);
	}

	public void AddCreature(HeroInfo heroInfo, RenderFacadeBuilder renderFacadeBuilder, ExtraSkillInfo[] addExtraSkill, bool loadCreatureImage, bool loadCirclePortrait = false)
	{
		if (!LoadingManager.IsPreloadFlag("Creature"))
		{
			return;
		}
		CreaturePreloader creaturePreloader = new CreaturePreloader(this);
		if (creaturePreloader.Open(heroInfo, renderFacadeBuilder, addExtraSkill, loadCreatureImage, loadCirclePortrait))
		{
			_preloaderList.Add(creaturePreloader);
			if (_isBegin)
			{
				creaturePreloader.Begin();
			}
		}
	}

	public void AddCreature(HeroInfo heroInfo, RenderFacadeBuilder renderFacadeBuilder, int[] addExtraSkillIndices, bool loadCreatureImage, bool loadCirclePortrait = false)
	{
		ExtraSkillInfo[] array = null;
		if (addExtraSkillIndices != null && addExtraSkillIndices.Length != 0)
		{
			array = new ExtraSkillInfo[addExtraSkillIndices.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new ExtraSkillInfo(addExtraSkillIndices[i]);
			}
		}
		AddCreature(heroInfo, renderFacadeBuilder, array, loadCreatureImage, loadCirclePortrait);
	}

	public void AddUiPreload(Action<UiPreloader> preloadAction, string preloaderName)
	{
		if (preloadAction != null && _uiPreloadFilter.Add(preloadAction))
		{
			UiPreloader uiPreloader = new UiPreloader(this);
			uiPreloader.PreloaderName = preloaderName;
			preloadAction(uiPreloader);
			_preloaderList.Add(uiPreloader);
			if (_isBegin)
			{
				uiPreloader.Begin();
			}
		}
	}

	public void ReleaseAsyncLoadAll()
	{
		Preloader.ReleaseAsyncLoadAll();
		for (int i = 0; i < _preloaderList.Count; i++)
		{
			_preloaderList[i].ReleaseAsyncLoadAll();
		}
	}

	public void GetLoadingTime(StringBuilder sb)
	{
		sb.AppendLine($"Loading Time: {EndTime - BeginTime:F3}");
		Preloader.GetLoadingTime(sb);
		for (int i = 0; i < _preloaderList.Count; i++)
		{
			_preloaderList[i].GetLoadingTime(sb);
		}
	}
}
