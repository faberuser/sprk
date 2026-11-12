using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NBattleView;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NSceneContainer;
using NGame2.NNodeTree.NUtil;
using NGame2.NRender;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NRender3D.NRenderTargetCinema;
using NGameAsset;
using NGameAsset.NBattleFieldMap2;
using NGameAsset.NCreature;
using NGameAsset.NEffect;
using NShader;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NGame2.NUI.NRender3D;

public class HeroRenderCinema : LinkFactoryComponent, ISceneContainer, IInspectorGuiDrawer, IUpdater, ILateUpdater
{
	public struct StockData
	{
		public string NodeTreePackCode;

		public string NodeTreeName;

		public HashSet<AnimationKeyData> PreloadAnimationSet;
	}

	public const string PrefabPath = "Prefab2/Render3D/HeroRenderCinema";

	public static PrefabAuto<HeroRenderCinema> PrefabAuto = new PrefabAuto<HeroRenderCinema>("Prefab2/Render3D/HeroRenderCinema");

	public static Transform _cinemaRoot;

	public Camera MainCamera;

	public GameObject ContentsRoot;

	private List<Coroutine> _coroutineList = new List<Coroutine>();

	private int _layerLock = -1;

	private SimplePreloader _preloader;

	private bool _nodeTreeLoadComplete;

	private PackLoad _packLoad;

	private TreeData _nodeTree;

	private IRuntime _runtime;

	private Action _onEndCallback;

	private IncreaseDic<StringKey, long> _uidKeywordDic = new IncreaseDic<StringKey, long>(StringKey.Comparer);

	private Dictionary<StringKey, long> _keywordDic = new Dictionary<StringKey, long>();

	private IncreaseDic<int, RenderFacade> _creatureDic = new IncreaseDic<int, RenderFacade>();

	private List<IAsyncLoad<IEffect>> _effectAsyncList = new List<IAsyncLoad<IEffect>>();

	private GameObject _battleFieldGo;

	private IAsyncLoad<IBattleFieldMap2> _battleFieldLoad;

	private List<UICamera.MouseOrTouch> _touchList = new List<UICamera.MouseOrTouch>();

	private float _prevScaleAmount;

	public static Transform CinemaRoot
	{
		get
		{
			if (_cinemaRoot == null)
			{
				_cinemaRoot = new GameObject("HeroCinemaRoot").transform;
			}
			return _cinemaRoot;
		}
	}

	public bool IsCameraUpdate { get; set; }

	public bool IsReady { get; private set; }

	public bool IsPlaying { get; private set; }

	public bool IsEnd
	{
		get
		{
			if (_runtime == null)
			{
				return true;
			}
			if (_runtime.IsEnd())
			{
				return true;
			}
			return false;
		}
	}

	public StockData Stock { get; private set; }

	public CinemaCamera CinemaCamera { get; private set; }

	public static void OnSceneChanged()
	{
		if (Vespa_Common_Class_Extension_Method.IsUnityNull(_cinemaRoot))
		{
			_cinemaRoot = null;
		}
	}

	public static HeroRenderCinema CreateInstance(StockData stock, Transform cinemaRoot = null)
	{
		if (stock.NodeTreePackCode == null)
		{
			stock.NodeTreePackCode = string.Empty;
		}
		if (stock.NodeTreeName == null)
		{
			stock.NodeTreeName = string.Empty;
		}
		if (cinemaRoot == null)
		{
			cinemaRoot = CinemaRoot;
		}
		HeroRenderCinema heroRenderCinema = PrefabAuto.Factory.Create(cinemaRoot, -1);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(heroRenderCinema.transform);
		if (!heroRenderCinema.Open(stock))
		{
			ReleaseInstance(heroRenderCinema);
			return null;
		}
		return heroRenderCinema;
	}

	public static void ReleaseInstance(HeroRenderCinema cinema)
	{
		if (!(cinema == null))
		{
			cinema.Close();
			FactoryBase.ReleaseComponent(cinema);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		GlobalUpdateBehaviour.AddUpdater(this);
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			Close();
			base.OnDestroy();
		}
	}

	public bool Open(StockData stock)
	{
		Close();
		if (MainCamera == null)
		{
			LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI3D);
			MainCamera = layerInfo.Camera;
		}
		Stock = stock;
		_layerLock = SingletonAuto<LayerManager>.instance.LockLayer();
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(base.gameObject, _layerLock, forceRecursive: true);
		IsReady = false;
		IsPlaying = false;
		IsCameraUpdate = true;
		CinemaCamera = new CinemaCamera(MainCamera);
		_preloader = new SimplePreloader(stock.PreloadAnimationSet);
		Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<CameraFilterBehaviour>(MainCamera.gameObject).Filter = GameSharedSetting.data.Shader.CameraFilter_HeroPreview;
		MainCamera.cullingMask = 1 << _layerLock;
		PostProcessVolume componentAuto = Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<PostProcessVolume>(MainCamera.gameObject);
		componentAuto.profile = VespaResource.Load<PostProcessProfile>("Effect/PostProcessPreview");
		componentAuto.isGlobal = true;
		componentAuto.enabled = SystemOptionData.Render_EnableRimLight;
		_coroutineList.Add(VespaCoroutine.Start(this, coLoadNodeTree(Stock.NodeTreePackCode, Stock.NodeTreeName)));
		return true;
	}

	private void Clear()
	{
		VespaCoroutine.Stop(this, _coroutineList);
		_coroutineList.Clear();
		if (_preloader != null)
		{
			_preloader.ReleaseAsyncLoadAll();
		}
		_packLoad = null;
		_nodeTree = null;
		if (_runtime != null)
		{
			_runtime.RequestStop(ERequestStop.Normal);
			_runtime.ReleaseSelf();
			_runtime = null;
		}
		CloseTouch();
	}

	public void Close()
	{
		Clear();
		if (MainCamera != null)
		{
			MainCamera.targetTexture = null;
		}
		for (int i = 0; i < _effectAsyncList.Count; i++)
		{
			EffectContainer.ReleaseEffect(_effectAsyncList[i], detachParent: true, immediatelyRelease: true);
		}
		_effectAsyncList.Clear();
		for (int j = 0; j < _creatureDic.Count; j++)
		{
			RenderFacadeBuilder.ReleaseRenderFacade(_creatureDic.GetValueByIndex(j));
		}
		_creatureDic.Clear();
		if (_battleFieldLoad != null)
		{
			Factory.ReleaseMap(_battleFieldLoad);
			_battleFieldLoad = null;
		}
		if (_battleFieldGo != null)
		{
			_battleFieldGo.transform.parent = null;
			VespaObject.DestroyGo(_battleFieldGo);
			_battleFieldGo = null;
		}
		CinemaCamera = null;
		_uidKeywordDic.Clear();
		_keywordDic.Clear();
		SingletonAuto<LayerManager>.instance.ReleaseLayer(_layerLock);
		_layerLock = -1;
		SingletonUpdater<GCManager>.instance.IPhoneLowMemCollect();
	}

	void IUpdater.ChainUpdate()
	{
		if (base.isActiveAndEnabled)
		{
			UpdateDerived();
			UpdateHeroCamera();
		}
	}

	private void UpdateDerived()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_creatureDic); i++)
		{
			_creatureDic.GetValueByIndex(i).ChainUpdate();
		}
		for (int j = 0; j < _effectAsyncList.Count; j++)
		{
			IAsyncLoad<IEffect> asyncLoad = _effectAsyncList[j];
			asyncLoad.Value.Update();
			if (asyncLoad.Value.IsEnd())
			{
				EffectContainer.ReleaseEffect(asyncLoad, detachParent: true);
				_effectAsyncList.RemoveAt(j);
				j--;
			}
		}
		if (_battleFieldLoad != null)
		{
			_battleFieldLoad.Value.ChainUpdate(GlobalTimer.DefaultLocal, MainCamera);
		}
		if (IsPlaying && _runtime != null && !_runtime.IsPlaying)
		{
			IsPlaying = false;
			Vespa_System_Action_Extension_Method.fire(_onEndCallback);
			_onEndCallback = null;
		}
	}

	void ILateUpdater.ChainLateUpdate()
	{
		if (base.isActiveAndEnabled)
		{
			LateUpdateDerived();
		}
	}

	protected virtual void LateUpdateDerived()
	{
		if (IsCameraUpdate && CinemaCamera != null)
		{
			CinemaCamera.ChainUpdate();
		}
	}

	public bool IsCompleteLoad()
	{
		if (!_nodeTreeLoadComplete)
		{
			return false;
		}
		for (int i = 0; i < _creatureDic.Count; i++)
		{
			if (!_creatureDic.GetValueByIndex(i).AssetCreatureAsync.ISDONE())
			{
				return false;
			}
		}
		if (_preloader != null)
		{
			_preloader.Tick();
			if (!_preloader.IsEnd)
			{
				return false;
			}
		}
		return true;
	}

	public void LoadNodeTree(string packCode, string nodeTreeName)
	{
		Clear();
		StockData stock = Stock;
		stock.NodeTreePackCode = packCode;
		stock.NodeTreeName = nodeTreeName;
		Stock = stock;
		_coroutineList.Add(VespaCoroutine.Start(this, coLoadNodeTree(packCode, nodeTreeName)));
	}

	private IEnumerator coLoadNodeTree(string packCode, string nodeTreeName)
	{
		SingletonUpdater<GCManager>.instance.IPhoneLowMemCollect();
		_nodeTreeLoadComplete = false;
		_packLoad = SingletonAuto<PackContainer>.instance.GetPack(packCode);
		while (!_packLoad.ISDONE())
		{
			yield return null;
		}
		if (!_packLoad.ISVALID())
		{
			_nodeTreeLoadComplete = true;
			yield break;
		}
		_nodeTree = _packLoad.Value.GetNodeTree(nodeTreeName);
		if (_nodeTree == null)
		{
			_nodeTreeLoadComplete = true;
			yield break;
		}
		_nodeTree.ReserveCache(_preloader);
		_nodeTreeLoadComplete = true;
	}

	public bool StartNodeTree(Action onEnd = null)
	{
		if (!IsCompleteLoad())
		{
			return false;
		}
		if (_nodeTree == null)
		{
			return false;
		}
		BuildChildRootRuntime runtime = default(BuildChildRootRuntime);
		if (!runtime.Open(_nodeTree))
		{
			return false;
		}
		for (int i = 0; i < _uidKeywordDic.Count; i++)
		{
			KeyValuePair<StringKey, long> keyValuePairByIndex = _uidKeywordDic.GetKeyValuePairByIndex(i);
			StringKey key = keyValuePairByIndex.Key;
			long value = keyValuePairByIndex.Value;
			runtime.AddValue(key, value);
		}
		AddKeyword(runtime);
		_runtime = runtime.Build(this, GlobalTimer.DefaultLocal, null, ContentsRoot.layer);
		SceneAutoBehaviour<RuntimeGlobalManager>.instance.AddRuntime(_runtime.CaptureSelf(), immediatelyStart: false);
		_onEndCallback = onEnd;
		IsPlaying = true;
		return true;
	}

	private void AddKeyword(BuildChildRootRuntime runtime)
	{
		Dictionary<StringKey, long>.Enumerator enumerator = _keywordDic.GetEnumerator();
		while (enumerator.MoveNext())
		{
			StringKey key = enumerator.Current.Key;
			long value = enumerator.Current.Value;
			runtime.AddValue(key, value);
		}
	}

	ICamera ISceneContainer.GetCamera()
	{
		return CinemaCamera;
	}

	RenderFacade ISceneContainer.GetRenderFacade(int uid)
	{
		return _creatureDic.GETVALUE(uid, null);
	}

	Creature ISceneContainer.GetBattleCreature(int uid)
	{
		return null;
	}

	int ISceneContainer.GetCreatureCount()
	{
		return 0;
	}

	Creature ISceneContainer.GetCreatureByIndex(int index)
	{
		return null;
	}

	int ISceneContainer.SpawnRenderFacade(StringKey key, RenderFacadeBuilder renderFacadeBuilder)
	{
		int num = 1;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_creatureDic); i++)
		{
			num = Mathf.Max(_creatureDic.GetKeyByIndex(i) + 1, num);
		}
		return SpawnCreature(key, num, renderFacadeBuilder);
	}

	void ISceneContainer.ReleaseAssetEffect(string code)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_effectAsyncList); i++)
		{
			if (_effectAsyncList[i] is EffectLoad effectLoad && effectLoad.Code == code)
			{
				EffectContainer.ReleaseEffect(effectLoad, detachParent: true, immediatelyRelease: true);
				_effectAsyncList.RemoveAt(i);
				i--;
			}
		}
	}

	void ISceneContainer.SpawnAssetEffect(IAsyncLoad<IEffect> effectAsync)
	{
		_effectAsyncList.Add(effectAsync);
	}

	void ISceneContainer.LoadBattleField(string code, string prefix, Vector3 position, Quaternion rotation, float scale, float backgroundAngle)
	{
		if (_battleFieldLoad != null)
		{
			Factory.ReleaseMap(_battleFieldLoad);
			_battleFieldLoad = null;
		}
		if (!String_Extension_Method.IsNullOrEmpty(code))
		{
			if (_battleFieldGo == null)
			{
				_battleFieldGo = new GameObject();
				_battleFieldGo.transform.parent = ContentsRoot.transform;
			}
			_battleFieldGo.transform.localPosition = position;
			_battleFieldGo.transform.localRotation = rotation;
			_battleFieldGo.transform.localScale = Vector3.one * scale;
			_ = Quaternion.AngleAxis(backgroundAngle, Vector3.right) * Vector3.forward;
			BattleFieldMapStock stock = new BattleFieldMapStock(code, prefix);
			stock.Layer = _layerLock;
			_battleFieldLoad = SingletonAuto<Factory>.instance.CreateAsync(stock);
			_battleFieldLoad.Value.SetParent(_battleFieldGo.transform, MainCamera);
		}
	}

	public int SpawnCreature(StringKey key, int uid, RenderFacadeBuilder renderFacadeBuilder)
	{
		if (ContentsRoot == null)
		{
			return 0;
		}
		if (_creatureDic.ContainsKey(uid))
		{
			return 0;
		}
		renderFacadeBuilder.SetPreload(Stock.PreloadAnimationSet);
		renderFacadeBuilder.ParentTm = ContentsRoot.transform;
		RenderFacade renderFacade = renderFacadeBuilder.Build();
		if (renderFacade == null)
		{
			return 0;
		}
		renderFacade.Action.SetBattleMode(isBattleAction: false, blendMotion: false);
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(renderFacade.gameObject, ContentsRoot.layer);
		_creatureDic.Add(uid, renderFacade);
		if (!key.IsNullOrEmpty())
		{
			_uidKeywordDic.Add(key, uid);
		}
		return uid;
	}

	public void DestroyCreature(int uid)
	{
		if (!_creatureDic.ContainsKey(uid))
		{
			return;
		}
		RenderFacade vALUE = _creatureDic.GETVALUE(uid, null);
		if (vALUE == null)
		{
			return;
		}
		RenderFacadeBuilder.ReleaseRenderFacade(vALUE);
		_creatureDic.RemoveSlow(uid);
		for (int i = 0; i < _uidKeywordDic.Count; i++)
		{
			if (_uidKeywordDic.GetValueByIndex(i) == uid)
			{
				_uidKeywordDic.RemoveByIndex(i);
				break;
			}
		}
	}

	private void CloseTouch()
	{
		_touchList.Clear();
		_prevScaleAmount = 0f;
	}

	private void UpdateHeroCamera()
	{
		if (_touchList.Count != 0 && _touchList.Count == 1)
		{
			RenderFacade vALUE = _creatureDic.GETVALUE(1, null);
			if (vALUE != null)
			{
				UICamera.MouseOrTouch mouseOrTouch = _touchList[0];
				VectorXZ direction = vALUE.Action.GetDirection();
				direction = NVespa.Math.Rotate(direction, mouseOrTouch.delta.x * 1f);
				vALUE.Action.SetDirection(direction, blendMotion: false);
			}
		}
	}

	public void OnDragStart()
	{
		if (!_touchList.Contains(UICamera.currentTouch))
		{
			_touchList.Add(UICamera.currentTouch);
			_prevScaleAmount = 0f;
		}
	}

	public void OnDragEnd()
	{
		if (_touchList.Remove(UICamera.currentTouch))
		{
			_prevScaleAmount = 0f;
		}
	}
}
