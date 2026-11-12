using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NBattleView;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
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
using NVespa;
using NVespa.NCamera;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NGame2.NUI.NRender3D;

public class RenderTargetCinema : LinkFactoryComponent, ISceneContainer, IInspectorGuiDrawer, IUpdater, ILateUpdater
{
	public struct StockData
	{
		public int RenderTargetWidth;

		public int RenderTargetHeight;

		public string NodeTreePackCode;

		public string NodeTreeName;

		public HashSet<AnimationKeyData> PreloadAnimationSet;
	}

	public const string Command_PlayBgm = "Common/PlayBgm";

	public const string PrefabPath = "Prefab2/Render3D/RenderTargetCinema";

	public static PrefabAuto<RenderTargetCinema> PrefabAuto = new PrefabAuto<RenderTargetCinema>("Prefab2/Render3D/RenderTargetCinema");

	public static Transform _cinemaRoot;

	public Camera MainCamera;

	public Camera SubCamera;

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

	private Dictionary<StringKey, string> _keywordStrDic = new Dictionary<StringKey, string>();

	private IncreaseDic<int, RenderFacade> _creatureDic = new IncreaseDic<int, RenderFacade>();

	private List<IAsyncLoad<IEffect>> _effectAsyncList = new List<IAsyncLoad<IEffect>>();

	private string _bgmCode = string.Empty;

	private string _beforeBgmCode = string.Empty;

	private GameObject _battleFieldGo;

	private IAsyncLoad<IBattleFieldMap2> _battleFieldLoad;

	private Holder _receiverHolder = new Holder();

	public static Transform CinemaRoot
	{
		get
		{
			if (_cinemaRoot == null)
			{
				_cinemaRoot = new GameObject("CinemaRoot").transform;
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

	public RenderTexture RenderTarget { get; private set; }

	public CinemaCamera CinemaCamera { get; private set; }

	private string _logHeader => "[RenderTargetCinema]";

	public static void OnSceneChanged()
	{
		if (Vespa_Common_Class_Extension_Method.IsUnityNull(_cinemaRoot))
		{
			_cinemaRoot = null;
		}
	}

	public static RenderTargetCinema CreateInstance(StockData stock)
	{
		if (stock.NodeTreePackCode == null)
		{
			stock.NodeTreePackCode = string.Empty;
		}
		if (stock.NodeTreeName == null)
		{
			stock.NodeTreeName = string.Empty;
		}
		RenderTargetCinema renderTargetCinema = PrefabAuto.Factory.Create(CinemaRoot, -1);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(renderTargetCinema.transform);
		if (!renderTargetCinema.Open(stock))
		{
			ReleaseInstance(renderTargetCinema);
			return null;
		}
		return renderTargetCinema;
	}

	public static void ReleaseInstance(RenderTargetCinema cinema)
	{
		if (!(cinema == null))
		{
			cinema.Close();
			FactoryBase.ReleaseComponent(cinema);
		}
	}

	public bool Open(StockData stock)
	{
		Close();
		_beforeBgmCode = SingletonAuto<BgmManager2>.instance.GetCurrentEventAssetBgmCode();
		_receiverHolder.Add<CommandMessage>(new ObjVerifier(this), OnMessageFast);
		if (MainCamera == null)
		{
			return false;
		}
		if (ContentsRoot == null)
		{
			return false;
		}
		_layerLock = SingletonAuto<LayerManager>.instance.LockLayer();
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(base.gameObject, _layerLock, forceRecursive: true);
		IsReady = false;
		IsPlaying = false;
		IsCameraUpdate = true;
		CinemaCamera = new CinemaCamera(MainCamera);
		_preloader = new SimplePreloader(stock.PreloadAnimationSet);
		Stock = stock;
		RenderTarget = SprkGraphicsOptions.CreateTarget(Stock.RenderTargetWidth, Stock.RenderTargetHeight, 24);
		MainCamera.cullingMask = 1 << _layerLock;
		MainCamera.targetTexture = RenderTarget;
		Vespa_Unity_Component_Extension_Method.GetComponentAuto<TargetTextureCache>(MainCamera);
		PostProcessVolume componentAuto = Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<PostProcessVolume>(MainCamera.gameObject);
		componentAuto.profile = VespaResource.Load<PostProcessProfile>("Effect/PostProcessPreview");
		componentAuto.isGlobal = true;
		componentAuto.enabled = SystemOptionData.Render_EnableRimLight;
		_coroutineList.Add(VespaCoroutine.Start(this, coLoadNodeTree(Stock.NodeTreePackCode, Stock.NodeTreeName)));
		return true;
	}

	public void LoadNodeTree(string packCode, string nodeTreeName)
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

	public void Close()
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
		if (MainCamera != null)
		{
			MainCamera.targetTexture = null;
		}
		if (SubCamera != null)
		{
			SubCamera.targetTexture = null;
		}
		if (RenderTarget != null)
		{
			VespaObject.DestroyObject(RenderTarget);
			RenderTarget = null;
		}
		if (!String_Extension_Method.IsNullOrEmpty(_bgmCode))
		{
			if (String_Extension_Method.IsNullOrEmpty(_beforeBgmCode))
			{
				SingletonAuto<BgmManager2>.instance.StopEvent(_bgmCode);
			}
			else if (_bgmCode != _beforeBgmCode)
			{
				SingletonAuto<BgmManager2>.instance.StopEvent(_bgmCode);
				SingletonAuto<BgmManager2>.instance.Request(_beforeBgmCode, isEvent: true);
			}
			_beforeBgmCode = string.Empty;
			_bgmCode = string.Empty;
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
		_keywordStrDic.Clear();
		SingletonAuto<LayerManager>.instance.ReleaseLayer(_layerLock);
		_layerLock = -1;
		SingletonUpdater<GCManager>.instance.IPhoneLowMemCollect();
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
		_runtime = runtime.Build(this, SprkSoulUpgradePlayback.Timer(GlobalTimer.DefaultLocal, this), null, ContentsRoot.layer);
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
		Dictionary<StringKey, string>.Enumerator enumerator2 = _keywordStrDic.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			StringKey key2 = enumerator2.Current.Key;
			string value2 = enumerator2.Current.Value;
			runtime.AddValue(key2, value2);
		}
	}

	public bool AddKeyword(StringKey key, long num)
	{
		if (key.IsNullOrEmpty())
		{
			VespaDebug.LogError("AddKeyword Fail , Key is Null Or Empty");
			return false;
		}
		if (_keywordDic.ContainsKey(key))
		{
			VespaDebug.LogError("AddKeyword Fail , Key is Already Contained");
			return false;
		}
		_keywordDic.Add(key, num);
		return true;
	}

	public bool AddKeyword(StringKey key, string str)
	{
		if (key.IsNullOrEmpty())
		{
			VespaDebug.LogError("AddKeyword Fail , Key is Null Or Empty");
			return false;
		}
		if (_keywordDic.ContainsKey(key))
		{
			VespaDebug.LogError("AddKeyword Fail , Key is Already Contained");
			return false;
		}
		_keywordStrDic.Add(key, str);
		return true;
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
		if (ContentsRoot == null || !_creatureDic.ContainsKey(uid))
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

	void IUpdater.ChainUpdate()
	{
		if (base.isActiveAndEnabled)
		{
			UpdateDerived();
		}
	}

	protected virtual void UpdateDerived()
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
			_battleFieldLoad.Value.ChainUpdate(SprkSoulUpgradePlayback.Timer(GlobalTimer.DefaultLocal, this), MainCamera);
		}
		if (IsPlaying && _runtime != null && !_runtime.IsPlaying)
		{
			IsPlaying = false;
			Action onEndCallback = _onEndCallback;
			_onEndCallback = null;
			Vespa_System_Action_Extension_Method.fire(onEndCallback);
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
		if (SprkGraphicsOptions.ShouldUpdatePreview(this) && IsCameraUpdate && CinemaCamera != null)
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

	private void OnMessageFast(CommandMessage message)
	{
		if (message.IsValidMessage() && message.IsValidScene(this) && message.Command == "Common/PlayBgm")
		{
			string arg = message.GetArg<string>(0);
			if (String_Extension_Method.IsNullOrEmpty(arg))
			{
				SingletonAuto<BgmManager2>.instance.StopEvent(_bgmCode);
				_bgmCode = string.Empty;
			}
			else
			{
				_bgmCode = arg;
				SingletonAuto<BgmManager2>.instance.Request(_bgmCode, isEvent: true);
			}
		}
	}

	public void OrthographicMode(bool isOn)
	{
		if (CinemaCamera != null)
		{
			CinemaCamera.OrthographicMode(isOn);
		}
	}

	public void AlowHDR(bool isOn)
	{
		if (CinemaCamera != null)
		{
			CinemaCamera.AllowHDR(isOn);
		}
	}

	public List<RenderFacade> GetRenderfacades()
	{
		List<RenderFacade> list = new List<RenderFacade>();
		for (int i = 0; i < _creatureDic.Count; i++)
		{
			RenderFacade valueByIndex = _creatureDic.GetValueByIndex(i);
			list.Add(valueByIndex);
		}
		return list;
	}

	public bool IsRuntime()
	{
		if (_runtime == null)
		{
			return false;
		}
		return true;
	}
}
