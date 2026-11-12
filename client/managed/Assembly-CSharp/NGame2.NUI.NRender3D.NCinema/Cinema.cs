using System;
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
using NVespa.NGui;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NGame2.NUI.NRender3D.NCinema;

public class Cinema : LinkFactoryComponent, ISceneContainer, IInspectorGuiDrawer, IUpdater, ILateUpdater, INodeTreeLoadProvider
{
	public const string PrefabPath = "Prefab2/Render3D/Cinema";

	public static PrefabAuto<Cinema> PrefabAuto = new PrefabAuto<Cinema>("Prefab2/Render3D/Cinema");

	public static Transform _cinemaRoot;

	public Camera MainCamera;

	public GameObject ContentsRoot;

	private SimplePreloader _preloader;

	private PackLoad _packLoad;

	private TreeData _treeData;

	private IRuntime _runtime;

	private int _layerLock = -1;

	private IDataProvider _provider;

	private NodeTreeLoader _nodeTreeLoader;

	private NodeTreeKeywordContainer _keywordContainer = new NodeTreeKeywordContainer();

	private CommandMessageHandlerContainer _handlerConatiner = new CommandMessageHandlerContainer();

	private IncreaseDic<int, RenderFacade> _creatureDic = new IncreaseDic<int, RenderFacade>();

	private List<IAsyncLoad<IEffect>> _effectAsyncList = new List<IAsyncLoad<IEffect>>();

	private GameObject _battleFieldGo;

	private IAsyncLoad<IBattleFieldMap2> _battleFieldLoad;

	private Holder _receiverHolder = new Holder();

	private Action _onEndCallback;

	private List<InteractionComponent> _listInteraction = new List<InteractionComponent>();

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

	public string BgmCode { get; set; }

	public RenderTexture RenderTarget { get; private set; }

	public CinemaCamera CinemaCamera { get; private set; }

	PackLoad INodeTreeLoadProvider.PackLoad
	{
		get
		{
			return _packLoad;
		}
		set
		{
			_packLoad = value;
		}
	}

	TreeData INodeTreeLoadProvider.TreeData
	{
		get
		{
			return _treeData;
		}
		set
		{
			_treeData = value;
		}
	}

	SimplePreloader INodeTreeLoadProvider.Preloader => _preloader;

	string INodeTreeLoadProvider.NodeTreePack => _provider.NodeTreePack;

	string INodeTreeLoadProvider.NodeTreeName => _provider.NodeTreeName;

	public static void OnSceneChanged()
	{
		if (Vespa_Common_Class_Extension_Method.IsUnityNull(_cinemaRoot))
		{
			_cinemaRoot = null;
		}
	}

	public static Cinema CreateInstance(IDataProvider dataProvider)
	{
		if (dataProvider == null)
		{
			return null;
		}
		Cinema cinema = PrefabAuto.Factory.Create(CinemaRoot, -1);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(cinema.transform);
		if (!cinema.Open(dataProvider))
		{
			ReleaseInstance(cinema);
			return null;
		}
		return cinema;
	}

	public static void ReleaseInstance(Cinema cinema)
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

	public bool Open(IDataProvider dataProvider)
	{
		if (MainCamera == null)
		{
			return false;
		}
		if (ContentsRoot == null)
		{
			return false;
		}
		if (dataProvider == null)
		{
			return false;
		}
		_provider = dataProvider;
		IsPlaying = false;
		InitLayer();
		InitHandlerList();
		InitMainCamera();
		InitCinemaCamera();
		InitPostProcessing();
		InitLoader();
		InitKeywordList(_provider.KeywordList);
		return true;
	}

	private void InitLoader()
	{
		_preloader = new SimplePreloader(_provider.PreloadAnimationSet);
		_nodeTreeLoader = new NodeTreeLoader(this);
	}

	private void InitHandlerList()
	{
		_handlerConatiner.RegistHandlers(_provider.HandlerList);
		RegistHandler();
	}

	private void InitKeywordList(List<NodeTreeKeyword> _list)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_list))
		{
			_keywordContainer.AddRange(_list);
		}
	}

	private void InitLayer()
	{
		_layerLock = SingletonAuto<LayerManager>.instance.LockLayer();
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(base.gameObject, _layerLock, forceRecursive: true);
	}

	private void InitMainCamera()
	{
		RenderTarget = SprkGraphicsOptions.CreateScreenTarget();
		MainCamera.cullingMask = 1 << _layerLock;
		MainCamera.targetTexture = RenderTarget;
		Vespa_Unity_Component_Extension_Method.GetComponentAuto<TargetTextureCache>(MainCamera);
	}

	private void InitCinemaCamera()
	{
		CinemaCamera = new CinemaCamera(MainCamera);
	}

	private void InitPostProcessing()
	{
		PostProcessVolume componentAuto = Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<PostProcessVolume>(MainCamera.gameObject);
		componentAuto.profile = VespaResource.Load<PostProcessProfile>("Effect/PostProcessPreview");
		componentAuto.isGlobal = true;
		componentAuto.enabled = SystemOptionData.Render_EnableRimLight;
	}

	public void ChangeCinemaTexture(UITexture texture)
	{
		if (RenderTarget != null && _provider != null && _provider.TextureWidth == texture.width && _provider.TextureHeight == texture.height)
		{
			_provider.SetRenderTexture(RenderTarget);
			return;
		}
		if (RenderTarget != null)
		{
			RenderTarget.Release();
			VespaObject.DestroyObject(RenderTarget);
			RenderTarget = null;
		}
		_provider.TextureWidth = texture.width;
		_provider.TextureHeight = texture.height;
		int width = texture.width;
		int height = texture.height;
		SingletonAuto<ResolutionInfo>.instance.CalcRenderTargetSize(texture.width, texture.height, out width, out height);
		RenderTarget = SprkGraphicsOptions.CreateScreenTarget();
		MainCamera.targetTexture = RenderTarget;
		_provider.SetRenderTexture(RenderTarget);
	}

	public void Close()
	{
		_nodeTreeLoader = null;
		if (_preloader != null)
		{
			_preloader.ReleaseAsyncLoadAll();
		}
		_packLoad = null;
		_treeData = null;
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
		if (RenderTarget != null)
		{
			RenderTarget.Release();
			VespaObject.DestroyObject(RenderTarget);
			RenderTarget = null;
		}
		if (!String_Extension_Method.IsNullOrEmpty(BgmCode))
		{
			SingletonAuto<BgmManager2>.instance.StopEvent(BgmCode);
			BgmCode = string.Empty;
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
		_keywordContainer.Clear();
		SingletonAuto<LayerManager>.instance.ReleaseLayer(_layerLock);
		_layerLock = -1;
		SingletonUpdater<GCManager>.instance.IPhoneLowMemCollect();
	}

	public void LoadNodeTree(string nodeTreepPack, string nodeTreeName)
	{
		if (_preloader != null)
		{
			_preloader.ReleaseAsyncLoadAll();
		}
		_packLoad = null;
		_treeData = null;
		if (_runtime != null)
		{
			_runtime.RequestStop(ERequestStop.Normal);
			_runtime.ReleaseSelf();
			_runtime = null;
		}
		_provider.NodeTreePack = nodeTreepPack;
		_provider.NodeTreeName = nodeTreeName;
		_nodeTreeLoader.LoadStart();
	}

	public bool IsCompletedLoad()
	{
		if (_nodeTreeLoader.State == NodeTreeLoader.EState.Loading)
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

	public bool StartNodeTree(Action onEnd = null)
	{
		if (!IsCompletedLoad())
		{
			return false;
		}
		if (_treeData == null)
		{
			return false;
		}
		BuildChildRootRuntime runtime = default(BuildChildRootRuntime);
		if (!runtime.Open(_treeData))
		{
			return false;
		}
		_keywordContainer.RuntimeAddKeywords(runtime);
		_runtime = runtime.Build(this, GlobalTimer.DefaultLocal, null, ContentsRoot.layer);
		SceneAutoBehaviour<RuntimeGlobalManager>.instance.AddRuntime(_runtime.CaptureSelf(), immediatelyStart: false);
		IsPlaying = true;
		_onEndCallback = onEnd;
		return true;
	}

	void IUpdater.ChainUpdate()
	{
		if ((_nodeTreeLoader == null || !_nodeTreeLoader.Loading()) && base.isActiveAndEnabled)
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
		if (_provider != null && _provider.EnableInteraction && !__Extension_Method_ICollection__.IsNullOrEmpty(_listInteraction))
		{
			Vector3 mousePosition = VespaInput.mousePosition;
			Ray ray = Vespa_Unity_Camera_Extension_Method.VespaScreenPointToRay(MainCamera, mousePosition);
			Debug.DrawRay(ray.origin, ray.direction * 10000f);
			Physics.Raycast(ray, out var hitInfo, 10000f);
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_listInteraction); j++)
			{
				InteractionComponent interactionComponent = _listInteraction[j];
				if (interactionComponent != null)
				{
					interactionComponent.Click(hitInfo);
				}
			}
		}
		for (int k = 0; k < _effectAsyncList.Count; k++)
		{
			IAsyncLoad<IEffect> asyncLoad = _effectAsyncList[k];
			asyncLoad.Value.Update();
			if (asyncLoad.Value.IsEnd())
			{
				EffectContainer.ReleaseEffect(asyncLoad, detachParent: true);
				_effectAsyncList.RemoveAt(k);
				k--;
			}
		}
		if (_battleFieldLoad != null)
		{
			_battleFieldLoad.Value.ChainUpdate(GlobalTimer.DefaultLocal, MainCamera);
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
		if (SprkGraphicsOptions.ShouldUpdatePreview(this) && CinemaCamera != null)
		{
			CinemaCamera.ChainUpdate();
		}
	}

	private void RegistHandler()
	{
		_receiverHolder.Add<CommandMessage>(new ObjVerifier(this), OnMessageFast);
	}

	private void OnMessageFast(CommandMessage message)
	{
		_handlerConatiner.MessageExecute(this, message);
	}

	public void AddKeyWord(StringKey key, long uid)
	{
		_keywordContainer.Add(key, uid);
	}

	public void AddKeyWord(StringKey key, string str)
	{
		_keywordContainer.Add(key, str);
	}

	public void RuntimeAddKeywords(BuildChildRootRuntime runtime)
	{
		_keywordContainer.RuntimeAddKeywords(runtime);
	}

	ICamera ISceneContainer.GetCamera()
	{
		return CinemaCamera;
	}

	RenderFacade ISceneContainer.GetRenderFacade(int uid)
	{
		return _creatureDic.GETVALUE(uid, null);
	}

	public RenderFacade GetRenderFacade(string code)
	{
		foreach (KeyValuePair<int, RenderFacade> item in _creatureDic._list)
		{
			RenderFacade value = item.Value;
			if (value == null)
			{
				continue;
			}
			CreatureInstance creatureInstance = ((value.AssetCreature == null) ? null : (value.AssetCreature as CreatureInstance));
			if (creatureInstance != null)
			{
				string code2 = creatureInstance.Code;
				if (string.Equals(code, code2, StringComparison.OrdinalIgnoreCase))
				{
					return value;
				}
			}
		}
		return null;
	}

	Creature ISceneContainer.GetBattleCreature(int uid)
	{
		return null;
	}

	int ISceneContainer.GetCreatureCount()
	{
		return __Extension_Method_ICollection__.COUNT(_creatureDic);
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
		renderFacadeBuilder.SetPreload(_provider.PreloadAnimationSet);
		renderFacadeBuilder.ParentTm = ContentsRoot.transform;
		RenderFacade renderFacade = renderFacadeBuilder.Build();
		if (renderFacade == null)
		{
			return 0;
		}
		renderFacade.Action.SetBattleMode(isBattleAction: false, blendMotion: false);
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(renderFacade.gameObject, ContentsRoot.layer);
		_creatureDic.Add(uid, renderFacade);
		string code = renderFacadeBuilder.Code;
		RenderFacadeInteractionProvider renderFacadeInteractionDataProvider = _provider.GetRenderFacadeInteractionDataProvider(code);
		if (renderFacadeInteractionDataProvider != null)
		{
			InteractionComponent interactionComponent = InteractionComponent.PrefabAuto.Factory.Create(renderFacade.gameObject.transform, _layerLock);
			if (interactionComponent != null)
			{
				renderFacadeInteractionDataProvider.SetRenderfacade(renderFacade);
				interactionComponent.Set(renderFacadeInteractionDataProvider);
				_listInteraction.Add(interactionComponent);
			}
		}
		renderFacade.ChainUpdate();
		if (!key.IsNullOrEmpty())
		{
			AddKeyWord(key, uid);
		}
		return uid;
	}

	public void DestroyCreature(int uid)
	{
		if (!(ContentsRoot == null) && _creatureDic.ContainsKey(uid))
		{
			RenderFacade vALUE = _creatureDic.GETVALUE(uid, null);
			if (!(vALUE == null))
			{
				RenderFacadeBuilder.ReleaseRenderFacade(vALUE);
				_creatureDic.RemoveSlow(uid);
				_keywordContainer.Remove(uid);
			}
		}
	}

	public void DrawGUI()
	{
		using (new BlockManager().TitleBox("Creature : " + __Extension_Method_ICollection__.COUNT(_creatureDic)).Vertical())
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_creatureDic); i++)
			{
				ICreature assetCreature = _creatureDic.GetValueByIndex(i).AssetCreature;
				if (assetCreature == null)
				{
					continue;
				}
				ICreatureOpenData creatureOpenData = assetCreature.GetCreatureOpenData();
				if (creatureOpenData != null)
				{
					CreatureStock stock = creatureOpenData.GetStock();
					using (new BlockManager().Horizontal())
					{
						GUILayout.Label($"{(i + 1).ToString()}. Code : {stock.Code}");
					}
				}
			}
		}
		using (new BlockManager().TitleBox("Effect : " + __Extension_Method_ICollection__.COUNT(_effectAsyncList)).Vertical())
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_effectAsyncList); j++)
			{
				using (new BlockManager().Horizontal())
				{
					IAsyncLoad<IEffect> asyncLoad = _effectAsyncList[j];
					if (asyncLoad == null)
					{
						GUILayout.Label("effect is Null");
						continue;
					}
					if (!asyncLoad.IsValid)
					{
						GUILayout.Label("INAVLID EFFECT");
					}
					GUILayout.Label($"{(j + 1).ToString()}. Code : {asyncLoad.KeyDesc}", GUILayout.Width(500f));
					IEffect value = asyncLoad.Value;
					if (value != null)
					{
						GUILayout.Label($"IsEmpty : {value.IsEmpty().ToString()} \t IsEnd : {value.IsEnd().ToString()}");
					}
				}
			}
		}
		_handlerConatiner.DrawGUI();
		_keywordContainer.DrawGUI();
	}
}
