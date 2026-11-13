using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NUtil;
using NGame2.NRender;
using NGame2.NTutorial;
using NGame2.NUI.NManager.ClassBuff;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NGetNewHero2;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GetNewHero2 : FrameControlWindow, ITouchDisabler, LateUpdaterBeforePanel.IReceiver, IInspectorGuiDrawer
{
	public class OpenData
	{
		private List<Stock> _stockList = new List<Stock>();

		public static OpenData CreateSingleFromHeroInfo(HeroInfo heroInfo, EOpenMode openMode, bool hideInfo)
		{
			OpenData openData = new OpenData();
			openData.Enqueue(Stock.Create(heroInfo, openMode, hideInfo));
			return openData;
		}

		public static OpenData CreateSingle(Stock stock)
		{
			OpenData openData = new OpenData();
			openData.Enqueue(stock);
			return openData;
		}

		public bool IsEmpty()
		{
			return __Extension_Method_ICollection__.COUNT(_stockList) == 0;
		}

		public void Enqueue(Stock stock)
		{
			_stockList.Add(stock);
		}

		public Stock Dequeue()
		{
			Stock result = _stockList[0];
			_stockList.RemoveAt(0);
			return result;
		}

		public Stock Peak()
		{
			return _stockList[0];
		}
	}

	public struct Stock
	{
		public readonly HeroInfo HeroInfo;

		public readonly EOpenMode OpenMode;

		public readonly bool HideInfo;

		public readonly string NodeTreePackCode;

		public readonly string NodeTreeName;

		public readonly bool IsBundleScene;

		public readonly bool ShowInfoOnHeroLeft;

		public readonly bool ShowNewHeroOpeningEffect;

		public sbyte Star
		{
			get
			{
				if (HeroInfo != null)
				{
					return HeroInfo.Star;
				}
				return 0;
			}
			set
			{
				if (HeroInfo != null)
				{
					HeroInfo.Star = value;
				}
			}
		}

		public static Stock Create(HeroInfo heroInfo, EOpenMode openMode, bool hideInfo)
		{
			return new Stock(heroInfo, openMode, hideInfo);
		}

		public static Stock CreateCustom(HeroInfo heroInfo, EOpenMode openMode, bool hideInfo, string packCode, string treeName, bool showInfoOnHeroLeft)
		{
			return new Stock(heroInfo, openMode, hideInfo, packCode, treeName, showInfoOnHeroLeft);
		}

		public static Stock CreateCustomHideOpenningEffect(HeroInfo heroInfo, EOpenMode openMode, bool hideInfo, string packCode, string treeName, bool showInfoOnHeroLeft)
		{
			return new Stock(heroInfo, openMode, hideInfo, packCode, treeName, showInfoOnHeroLeft, hideNewHeroOpeningEffect: true);
		}

		private Stock(HeroInfo heroInfo, EOpenMode openMode, bool hideInfo)
		{
			HeroInfo = JsonMarshaler.Clone(heroInfo);
			HeroInfo.SetDefaultCostume();
			OpenMode = openMode;
			HideInfo = hideInfo;
			NodeTreePackCode = PackHelper.GetPathNameFromCode(HeroInfo.CreatureData.CodeName);
			NodeTreeName = ((HeroInfo.Transcended > 0) ? "Transcendence" : "GetNewHero");
			IsBundleScene = false;
			ShowInfoOnHeroLeft = false;
			ShowNewHeroOpeningEffect = openMode == EOpenMode.NewHero;
		}

		private Stock(HeroInfo heroInfo, EOpenMode openMode, bool hideInfo, string packCode, string treeName, bool showInfoOnHeroLeft, bool hideNewHeroOpeningEffect = false)
		{
			HeroInfo = JsonMarshaler.Clone(heroInfo);
			HeroInfo.SetDefaultCostume();
			OpenMode = openMode;
			HideInfo = hideInfo;
			NodeTreePackCode = packCode;
			NodeTreeName = treeName;
			IsBundleScene = true;
			ShowInfoOnHeroLeft = showInfoOnHeroLeft;
			ShowNewHeroOpeningEffect = openMode == EOpenMode.NewHero && !hideNewHeroOpeningEffect;
		}
	}

	public enum EOpenMode
	{
		None,
		NewHero,
		Awaken,
		Transcend,
		LimitBreak
	}

	public const string Command_PlayStarEffect = "GetNewHero/PlayStarEffect";

	public const string Command_StopStarEffect = "GetNewHero/StopStarEffect";

	public const int HeroUid = 1;

	public const string PrefabPath = "Resources/Prefab2/Window/FullScene/GetNewHero2";

	public float NewHeroOpeningTime = 2f;

	public float HeroStar_3StarInterval = 0.3f;

	public float HeroStar_5StarInterval = 0.3f;

	public float HeroStar_3StarCellWidth = 200f;

	public float HeroStar_5StarCellWidth = 100f;

	public UITexture CinemaTexture;

	public UIWidget ExitWidget;

	public Transform ExitWidgetRightPos;

	public Transform ExitWidgetLeftPos;

	public Transform ExitWidgetCenterPos;

	public HeroInfoSubComponent HeroInfoComponent;

	public UIGame3DWidget NewHeroOpeningEffect;

	public UIGrid StarGrid;

	[HideInInspector]
	public List<UIGame3DWidget> StarEffectList = new List<UIGame3DWidget>();

	private RenderTargetCinema _cinema;

	private Tuple<bool, Transform, Vector3, Coroutine> _tupleStarEffect = new Tuple<bool, Transform, Vector3, Coroutine>(i_item1: false, null, Vector3.zero, null);

	private Holder _receiverHolder = new Holder();

	private OpenData _openData;

	private Stock _currentStock;

	private Action _onCloseAction;

	private IClassBuffListener _iClassBuffListener;

	bool ITouchDisabler.IsValid => IsOpen();

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			_onCloseAction = null;
		}
	}

	public void Open(OpenData openData, Action onComplete, Action onClose)
	{
		if (IsOpen())
		{
			Close();
		}
		Open();
		if (CinemaTexture == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		if (openData == null || openData.IsEmpty())
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		_openData = openData;
		_currentStock = openData.Dequeue();
		_receiverHolder.Add<CommandMessage>(new ObjVerifier(this), OnMessageFast);
		_onCloseAction = onClose;
		VespaCoroutine.Start(this, coOpen(onComplete));
	}

	private IEnumerator coOpen(Action onComplete)
	{
		DisableUI();
		if (_currentStock.ShowNewHeroOpeningEffect)
		{
			NewHeroOpeningEffect.Play(null);
			yield return new WaitForSeconds(NewHeroOpeningTime);
		}
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		NewHeroOpeningEffect.Stop(immediatelyClear: true);
		UIHelper.EnableGameObject(CinemaTexture, enable: true);
		RenderTargetCinema.StockData stock = default(RenderTargetCinema.StockData);
		SingletonAuto<ResolutionInfo>.instance.GetScreenRenderTargetSize(out stock.RenderTargetWidth, out stock.RenderTargetHeight);
		stock.NodeTreePackCode = _currentStock.NodeTreePackCode;
		stock.NodeTreeName = _currentStock.NodeTreeName;
		_cinema = RenderTargetCinema.CreateInstance(stock);
		if (_cinema == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		if (!_currentStock.IsBundleScene)
		{
			RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, _currentStock.HeroInfo);
			renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.IncludeFacial;
			_cinema.SpawnCreature(VariableKeyword.Myself, 1, renderFacadeBuilder);
		}
		do
		{
			yield return null;
		}
		while (!_cinema.IsCompleteLoad());
		// SPRK: a hero without this cinematic would otherwise leave the screen waiting forever.
		if (!_cinema.StartNodeTree(OnEndCinema))
		{
			OnEndCinema();
		}
		CinemaTexture.mainTexture = _cinema.RenderTarget;
		CinemaTexture.alpha = 1f;
		sceneChanger.Close();
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(onComplete);
		_iClassBuffListener = SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWindowManager;
	}

	public override void Awake()
	{
		base.Awake();
		UIPanel.lateUpdateBeforePanelList.Add(base.gameObject);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (SingletonUpdater<TutorialManager>.instance.GetRunningState().AutoType == TutorialRunningState.TutorialAutoType.All || AutoRepeatBattle.IsEnabled() || AutoNextBattle.IsEnabled())
		{
			OnSystemBackButton();
		}
	}

	private void OnEndCinema()
	{
		if (!(_cinema == null) && !(_cinema.MainCamera == null))
		{
			bool flag = IsHeroLeftSide();
			UIHelper.EnableGameObject(HeroInfoComponent, !_currentStock.HideInfo);
			HeroInfoComponent.Open(this, _currentStock.HeroInfo, _currentStock.OpenMode, flag);
			UIHelper.EnableGameObject(ExitWidget, enable: true);
			float i_value = ((!_currentStock.HideInfo) ? (flag ? ExitWidgetRightPos.localPosition.x : ExitWidgetLeftPos.localPosition.x) : ExitWidgetCenterPos.localPosition.x);
			ExitWidget.transform.localPosition = Vespa_Unity_Vector3_Extension_Method.ReplaceX(ExitWidget.transform.localPosition, i_value);
		}
	}

	private bool IsHeroLeftSide()
	{
		if (_currentStock.IsBundleScene)
		{
			return _currentStock.ShowInfoOnHeroLeft;
		}
		RenderFacade rENDERFACADE = _cinema.GETRENDERFACADE(1);
		if (rENDERFACADE == null)
		{
			return false;
		}
		Transform pivot = rENDERFACADE.AssetCreature.GetPivot(EPivotType.Hit);
		if (pivot == null)
		{
			return false;
		}
		return _cinema.MainCamera.transform.worldToLocalMatrix.MultiplyPoint(pivot.position).x > 0f;
	}

	void LateUpdaterBeforePanel.IReceiver.OnUpdate()
	{
		UpdateStarTransform();
	}

	private void UpdateStarTransform()
	{
		if (!_tupleStarEffect.Item1 || _cinema == null)
		{
			return;
		}
		Transform item = _tupleStarEffect.Item2;
		if (item == null || !item.gameObject.activeInHierarchy)
		{
			StopStarEffect();
			return;
		}
		Camera cameraByLayer = GlobalAutoBehaviour<GlobalCameraManager>.instance.GetCameraByLayer(StarGrid.gameObject.layer);
		if (!(cameraByLayer == null))
		{
			Vector3 position = _cinema.MainCamera.WorldToViewportPoint(item.transform.position);
			position = cameraByLayer.ViewportToWorldPoint(position);
			Vector3 position2 = _cinema.MainCamera.WorldToViewportPoint(item.transform.position + item.transform.rotation * Vector3.up);
			position2 = cameraByLayer.ViewportToWorldPoint(position2);
			Vector3 upwards = position2 - position;
			if (upwards.sqrMagnitude < Mathf.Epsilon)
			{
				upwards = Vector3.up;
			}
			StarGrid.transform.position = position;
			StarGrid.transform.localPosition += _tupleStarEffect.Item3;
			StarGrid.transform.rotation = Quaternion.LookRotation(Vector3.forward, upwards);
		}
	}

	public override void Reset()
	{
		base.Reset();
		_receiverHolder.Clear();
		StopStarEffect();
		VespaCoroutine.StopAll(this);
		if (CinemaTexture != null)
		{
			CinemaTexture.mainTexture = null;
		}
		if (_cinema != null)
		{
			RenderTargetCinema.ReleaseInstance(_cinema);
			_cinema = null;
		}
		if (HeroInfoComponent != null)
		{
			HeroInfoComponent.Reset();
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
	}

	public override void OnSystemBackButton()
	{
		if (UIHelper.IsEnabledGameObjet(ExitWidget))
		{
			VespaCoroutine.Start(coClose(this));
		}
	}

	public void OnClickClose()
	{
		if (_openData.IsEmpty())
		{
			VespaCoroutine.Start(coClose(this));
			return;
		}
		DisableUI();
		_currentStock = _openData.Dequeue();
		VespaCoroutine.Start(coSequncePlay(OnEndCinema));
	}

	private void DisableUI()
	{
		UIHelper.EnableGameObject(CinemaTexture, enable: false);
		UIHelper.EnableGameObject(ExitWidget, enable: false);
		UIHelper.EnableGameObject(HeroInfoComponent, enable: false);
		UIHelper.EnableGameObject(StarGrid, enable: false);
	}

	private IEnumerator coSequncePlay(Action onComplete)
	{
		if (_cinema == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		UIHelper.EnableGameObject(CinemaTexture, enable: true);
		RenderTargetCinema.StockData stockData = new RenderTargetCinema.StockData
		{
			NodeTreePackCode = _currentStock.NodeTreePackCode,
			NodeTreeName = _currentStock.NodeTreeName
		};
		_cinema.LoadNodeTree(stockData.NodeTreePackCode, stockData.NodeTreeName);
		do
		{
			yield return null;
		}
		while (!_cinema.IsCompleteLoad());
		// SPRK: a hero without this cinematic would otherwise leave the screen waiting forever.
		if (!_cinema.StartNodeTree(OnEndCinema))
		{
			OnEndCinema();
		}
	}

	private void AddClassBuffPoint()
	{
		if (!_currentStock.HideInfo && _iClassBuffListener != null)
		{
			_iClassBuffListener.AddPoint();
		}
	}

	private IEnumerator coClose(GetNewHero2 This)
	{
		if (This != null)
		{
			UIHelper.EnableGameObject(This.ExitWidget, enable: false);
		}
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		Action i_action = null;
		if (This != null)
		{
			i_action = This._onCloseAction;
			This._onCloseAction = null;
			if (This.IsOpen())
			{
				This.Close();
			}
		}
		sceneChanger.Close();
		Vespa_System_Action_Extension_Method.fire(i_action);
		AddClassBuffPoint();
	}

	private void OnMessageFast(CommandMessage message)
	{
		if (message.IsValidMessage() && message.IsValidScene(_cinema))
		{
			if (message.Command == "GetNewHero/PlayStarEffect")
			{
				_tupleStarEffect.Item1 = true;
				_tupleStarEffect.Item2 = message.GetArg<Transform>(0);
				_tupleStarEffect.Item3 = message.GetArgValue<Vector3>(1);
				_tupleStarEffect.Item4 = VespaCoroutine.Start(this, coPlayStarEffect());
			}
			else if (message.Command == "GetNewHero/StopStarEffect")
			{
				StopStarEffect();
			}
		}
	}

	private IEnumerator coPlayStarEffect()
	{
		yield return null;
		sbyte heroStar = _currentStock.Star;
		UIHelper.EnableGameObject(StarGrid, enable: true);
		for (int i = 0; i < StarEffectList.Count; i++)
		{
			UIHelper.EnableGameObject(StarEffectList[i], i < heroStar);
			StarEffectList[i].Stop(immediatelyClear: true);
		}
		float t = Mathf.Clamp01(0.5f * (float)(heroStar - 3));
		float interval = Mathf.Lerp(HeroStar_3StarInterval, HeroStar_5StarInterval, t);
		float cellWidth = Mathf.Lerp(HeroStar_3StarCellWidth, HeroStar_5StarCellWidth, t);
		yield return null;
		StarGrid.cellWidth = cellWidth;
		StarGrid.Reposition();
		yield return null;
		for (int index = 0; index < heroStar && index < StarEffectList.Count; index++)
		{
			StarEffectList[index].Play(null);
			yield return new WaitForSeconds(interval);
		}
	}

	private void StopStarEffect()
	{
		_tupleStarEffect.Item1 = false;
		_tupleStarEffect.Item2 = null;
		UIHelper.EnableGameObject(StarGrid, enable: false);
		_ = _tupleStarEffect.Item3;
		VespaCoroutine.Stop(this, _tupleStarEffect.Item4);
		_tupleStarEffect.Item4 = null;
		for (int i = 0; i < StarEffectList.Count; i++)
		{
			UIHelper.EnableGameObject(StarEffectList[i], enable: false);
			StarEffectList[i].Stop(immediatelyClear: true);
		}
	}
}
