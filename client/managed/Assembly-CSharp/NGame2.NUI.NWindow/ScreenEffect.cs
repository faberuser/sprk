using System;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NGame3DWidget;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ScreenEffect : MonoBehaviour, IWindow, IGlobalWindow
{
	public class StockData
	{
		public string Code = string.Empty;

		public Transform FromTm;

		public TransformData ExtraTransform;

		public ILocalTimer LocalTimer;

		public bool IsBottom;

		public Action OnComplete;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/ScreenEffect";

	private static IncreaseDic<GameUIRoot.ELayerType, WindowPrefab<ScreenEffect>> _gameUiDic = new IncreaseDic<GameUIRoot.ELayerType, WindowPrefab<ScreenEffect>>();

	private static IncreaseDic<GlobalUIRoot.ELayerType, GlobalWindowPrefab<ScreenEffect>> _globalUiDic = new IncreaseDic<GlobalUIRoot.ELayerType, GlobalWindowPrefab<ScreenEffect>>();

	public UIGame3DWidget Game3dWidget;

	public UIGame3DWidget BottomWidget;

	private int _emptyFrameCount;

	EDepthOrder IWindow.DepthOrder => EDepthOrder.Topmost;

	public static IContents PlayEffect(string effectCode, TransformData extraTransform, Action onComplete)
	{
		return PlayEffect(effectCode, extraTransform, GameUIRoot.ELayerType.UI, onComplete);
	}

	public static IContents PlayEffect(string effectCode, TransformData extraTransform, GameUIRoot.ELayerType layerType, Action onComplete)
	{
		return PlayEffect(effectCode, extraTransform, layerType, null, onComplete);
	}

	public static IContents PlayEffect(string effectCode, TransformData extraTransform, GameUIRoot.ELayerType layerType, ILocalTimer localTimer, Action onComplete)
	{
		StockData stockData = new StockData();
		stockData.Code = effectCode;
		stockData.ExtraTransform = extraTransform;
		stockData.LocalTimer = localTimer;
		stockData.OnComplete = onComplete;
		return PlayEffect(layerType, stockData);
	}

	public static IContents PlayEffect(GameUIRoot.ELayerType layerType, StockData stock)
	{
		WindowPrefab<ScreenEffect> windowPrefab = _gameUiDic.GETVALUE(layerType, null);
		if (windowPrefab == null)
		{
			windowPrefab = new WindowPrefab<ScreenEffect>("Resources/Prefab2/Window/ScreenEffect", layerType);
			_gameUiDic.Add(layerType, windowPrefab);
		}
		windowPrefab.instance.OpenInternal();
		return (stock.IsBottom ? windowPrefab.instance.BottomWidget : windowPrefab.instance.Game3dWidget).PlayInstantEffect(stock.Code, stock.FromTm, stock.ExtraTransform, stock.LocalTimer, stock.OnComplete, playSync: false);
	}

	public static IContents PlayEffect(GlobalUIRoot.ELayerType layerType, StockData stock)
	{
		GlobalWindowPrefab<ScreenEffect> globalWindowPrefab = _globalUiDic.GETVALUE(layerType, null);
		if (globalWindowPrefab == null)
		{
			globalWindowPrefab = new GlobalWindowPrefab<ScreenEffect>("Resources/Prefab2/Window/ScreenEffect", layerType);
			_globalUiDic.Add(layerType, globalWindowPrefab);
		}
		globalWindowPrefab.instance.OpenInternal();
		return (stock.IsBottom ? globalWindowPrefab.instance.BottomWidget : globalWindowPrefab.instance.Game3dWidget).PlayInstantEffect(stock.Code, stock.FromTm, stock.ExtraTransform, stock.LocalTimer, stock.OnComplete, playSync: false);
	}

	public static IContents PlaySound(string soundCode, Action onComplete)
	{
		return PlaySound(soundCode, GameUIRoot.ELayerType.UI, onComplete);
	}

	public static IContents PlaySound(string soundCode, GameUIRoot.ELayerType layerType, Action onComplete)
	{
		return PlaySound(soundCode, layerType, null, onComplete);
	}

	public static IContents PlaySound(string soundCode, GameUIRoot.ELayerType layerType, ILocalTimer localTimer, Action onComplete)
	{
		WindowPrefab<ScreenEffect> windowPrefab = _gameUiDic.GETVALUE(layerType, null);
		if (windowPrefab == null)
		{
			windowPrefab = new WindowPrefab<ScreenEffect>("Resources/Prefab2/Window/ScreenEffect", layerType);
			_gameUiDic.Add(layerType, windowPrefab);
		}
		windowPrefab.instance.OpenInternal();
		return windowPrefab.instance.Game3dWidget.PlayInstantSound(soundCode, localTimer, onComplete);
	}

	public static bool IsPlayingOfLayer(GameUIRoot.ELayerType layerType)
	{
		WindowPrefab<ScreenEffect> vALUE = _gameUiDic.GETVALUE(layerType, null);
		if (vALUE == null)
		{
			return false;
		}
		if (vALUE.IsOpen())
		{
			return vALUE.instance.Game3dWidget.IsPlaying;
		}
		return false;
	}

	public static bool IsPlayingOfLayer(GlobalUIRoot.ELayerType layerType)
	{
		GlobalWindowPrefab<ScreenEffect> vALUE = _globalUiDic.GETVALUE(layerType, null);
		if (vALUE == null)
		{
			return false;
		}
		if (vALUE.IsOpen())
		{
			return vALUE.instance.Game3dWidget.IsPlaying;
		}
		return false;
	}

	public static void OnSceneChanged()
	{
		for (int i = 0; i < _gameUiDic.Count; i++)
		{
			WindowPrefab<ScreenEffect> valueByIndex = _gameUiDic.GetValueByIndex(i);
			valueByIndex.CloseWindow();
			valueByIndex.ReleaseWindow();
		}
		for (int j = 0; j < _globalUiDic.Count; j++)
		{
			GlobalWindowPrefab<ScreenEffect> valueByIndex2 = _globalUiDic.GetValueByIndex(j);
			if (!valueByIndex2.IsOpen() || (!valueByIndex2.instance.BottomWidget.IsPlaying && !valueByIndex2.instance.Game3dWidget.IsPlaying))
			{
				valueByIndex2.CloseWindow();
				valueByIndex2.ReleaseWindow();
			}
		}
	}

	internal void OpenInternal()
	{
		base.gameObject.SetActive(value: true);
	}

	void IWindow.ReserveCache(UiPreloader preloader)
	{
	}

	void IWindow.SetBasePanelDepth(int depth)
	{
	}

	bool IWindow.IsOpen()
	{
		return base.gameObject.activeSelf;
	}

	void IWindow.Close()
	{
		base.gameObject.SetActive(value: false);
	}

	void IGlobalWindow.ReserveCache(UiPreloader preloader)
	{
	}

	bool IGlobalWindow.IsOpen()
	{
		return base.gameObject.activeSelf;
	}

	void IGlobalWindow.Close()
	{
		base.gameObject.SetActive(value: false);
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (Game3dWidget != null && Game3dWidget.IsPlaying)
		{
			_emptyFrameCount = 0;
			return;
		}
		if (BottomWidget != null && BottomWidget.IsPlaying)
		{
			_emptyFrameCount = 0;
			return;
		}
		_emptyFrameCount++;
		if (_emptyFrameCount > 2)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
