using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NScene;
using NGame2.NScene.NPreloader;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NLoading;

public class LoadingDefault : MonoBehaviour, IGlobalWindow
{
	public interface ILoadingStatus
	{
		string Label { get; }

		float FillAmount { get; }
	}

	[Serializable]
	public class AnchorWidgetInfo
	{
		public EAnchorPivot Pivot;

		public UIWidget Widget;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Loading/LoadingDefault";

	private static GlobalWindowPrefab<LoadingDefault> _prefabInstance = new GlobalWindowPrefab<LoadingDefault>("Resources/Prefab2/Window/Loading/LoadingDefault", GlobalUIRoot.ELayerType.GlobalUI);

	public float ContentsFadeTime = 0.3f;

	public float BackgroundFadeTime = 0.3f;

	public UIWidget RootWidget;

	public UIWidget BackgroundWidget;

	public UIWidget ContentsWidget;

	public UIWidget GaugeGroupWidget;

	public UIWidget BasicWidget;

	public UIWidget LoadComponentWidget;

	public UILabel GaugeLabel;

	public UILabel GaugeIndicatorLabel;

	public UISprite GaugeSprite;

	public List<AnchorWidgetInfo> ListWidgetInfo = new List<AnchorWidgetInfo>();

	private ILoadingStatus _loadingStatus;

	private Component _loadingComponent;

	private float _indicatorElapsedTime;

	public static LoadingDefault instance => _prefabInstance.instance;

	public static bool isValidInstance => _prefabInstance.IsValidInstance;

	private AnimationCurve _fadeInCurve => ConstantValue.EaseInOut0_1;

	private AnimationCurve _fadeOutCurve => ConstantValue.EaseInOut1_0;

	public static void CreateInstance()
	{
		_prefabInstance.CreateInstance();
	}

	public static void ReleaseInstance()
	{
		_prefabInstance.ReleaseWindow();
	}

	public void OpenFadeIn(ILoadingStatus loadingStatus, LoadingManager.EMoveSceneType moveSceneType, Action onComplete)
	{
		_loadingStatus = loadingStatus;
		if (_loadingStatus == null)
		{
			UIHelper.EnableGameObject(GaugeGroupWidget, enable: false);
		}
		else
		{
			UIHelper.EnableGameObject(GaugeGroupWidget, enable: true);
		}
		UIHelper.SetSpriteFillAmount(GaugeSprite, 0f);
		InitLoadingComponent(moveSceneType);
		BackgroundWidget.alpha = 0f;
		ContentsWidget.alpha = 0f;
		GaugeGroupWidget.alpha = 0f;
		VespaCoroutine.Start(coFadeIn(onComplete));
	}

	public void OpenQA(int LoadingIndex, ILoadingStatus loadingStatus, Action onComplete)
	{
		_loadingStatus = loadingStatus;
		if (_loadingStatus == null)
		{
			UIHelper.EnableGameObject(GaugeGroupWidget, enable: false);
		}
		else
		{
			UIHelper.EnableGameObject(GaugeGroupWidget, enable: true);
		}
		UIHelper.SetSpriteFillAmount(GaugeSprite, 0f);
		LoadingData loadingData = GameRoot.Instance.LoadingDataContainer.GetData(LoadingIndex);
		if (loadingData == null)
		{
			loadingData = GameRoot.Instance.LoadingDataContainer.GetFirstData();
		}
		if (loadingData != null)
		{
			CreateLoadingComponent(loadingData);
		}
		BackgroundWidget.alpha = 0f;
		ContentsWidget.alpha = 0f;
		GaugeGroupWidget.alpha = 0f;
		VespaCoroutine.Start(coFadeIn(onComplete));
	}

	private IEnumerator coFadeIn(Action onComplete)
	{
		base.gameObject.SetActive(value: true);
		yield return null;
		float beginTime = Time.realtimeSinceStartup;
		float num;
		do
		{
			yield return null;
			num = Time.realtimeSinceStartup - beginTime;
			float num2 = Mathf.Clamp01(num / Vespa_Float_Extension_Method.NoneZero(BackgroundFadeTime));
			float alpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(_fadeInCurve, num2, num2);
			BackgroundWidget.alpha = alpha;
		}
		while (!(num >= BackgroundFadeTime));
		beginTime = Time.realtimeSinceStartup;
		float num3;
		do
		{
			yield return null;
			num3 = Time.realtimeSinceStartup - beginTime;
			float num4 = Mathf.Clamp01(num3 / Vespa_Float_Extension_Method.NoneZero(ContentsFadeTime));
			float alpha2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(_fadeInCurve, num4, num4);
			ContentsWidget.alpha = alpha2;
			GaugeGroupWidget.alpha = alpha2;
		}
		while (!(num3 >= ContentsFadeTime));
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	public void CloseFadeOut(Action onComplete, bool releaseSelf)
	{
		VespaCoroutine.Start(coCloseFadeOut(onComplete, releaseSelf));
	}

	private IEnumerator coCloseFadeOut(Action onComplete, bool releaseSelf)
	{
		yield return VespaCoroutine.Start(coWaitForComponentCompleted());
		yield return VespaCoroutine.Start(coFadeOut());
		Vespa_System_Action_Extension_Method.fire(onComplete);
		if (releaseSelf)
		{
			ReleaseInstance();
		}
	}

	private IEnumerator coWaitForComponentCompleted()
	{
		float waitStartTime = Time.time;
		while (!instance.IsComponentCompleted())
		{
			if (Time.time - waitStartTime > 10f)
			{
				VespaDebug.LogError("Loading component not completed.");
				break;
			}
			yield return null;
		}
	}

	private IEnumerator coFadeOut()
	{
		yield return null;
		float beginTime = Time.realtimeSinceStartup;
		float num;
		do
		{
			yield return null;
			num = Time.realtimeSinceStartup - beginTime;
			float num2 = Mathf.Clamp01(num / Vespa_Float_Extension_Method.NoneZero(ContentsFadeTime));
			float alpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(_fadeOutCurve, num2, 1f - num2);
			ContentsWidget.alpha = alpha;
			GaugeGroupWidget.alpha = alpha;
		}
		while (!(num >= ContentsFadeTime));
		beginTime = Time.realtimeSinceStartup;
		float num3;
		do
		{
			yield return null;
			num3 = Time.realtimeSinceStartup - beginTime;
			float num4 = Mathf.Clamp01(num3 / Vespa_Float_Extension_Method.NoneZero(BackgroundFadeTime));
			float alpha2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(_fadeOutCurve, num4, 1f - num4);
			BackgroundWidget.alpha = alpha2;
		}
		while (!(num3 >= BackgroundFadeTime));
	}

	private void InitLoadingComponent(LoadingManager.EMoveSceneType moveSceneType)
	{
		int chapterIndex = 0;
		int dungeonIndex = 0;
		if (SingletonAuto<BattleContext>.isValidInstance)
		{
			chapterIndex = SingletonAuto<BattleContext>.instance.LastBattleChapterIndex;
			dungeonIndex = SingletonAuto<BattleContext>.instance.LastBattleDungeonIndex;
		}
		LoadingDataContainer loadingDataContainer = GameRoot.Instance.LoadingDataContainer;
		if (loadingDataContainer != null)
		{
			LoadingData loadingData = moveSceneType switch
			{
				LoadingManager.EMoveSceneType.StartBattle => loadingDataContainer.GetLoadingDataByStartChapterDungeon(chapterIndex, dungeonIndex), 
				LoadingManager.EMoveSceneType.ReturnToLobby => loadingDataContainer.GetLoadingDataByEndChapterDungeon(chapterIndex, dungeonIndex), 
				LoadingManager.EMoveSceneType.StartGame => loadingDataContainer.GetLoadingData(LoadingData.EGroupType.StartGame), 
				_ => loadingDataContainer.GetLoadingData(LoadingData.EGroupType.Default), 
			};
			if (loadingData != null)
			{
				CreateLoadingComponent(loadingData);
			}
		}
	}

	private void CreateLoadingComponent(LoadingData loadingData)
	{
		FactoryBase.ReleaseComponent(_loadingComponent);
		_loadingComponent = SingletonAuto<LoadingComponentFactory>.instance.Create(loadingData, LoadComponentWidget.transform);
		if ((bool)_loadingComponent)
		{
			SetupAnchors(_loadingComponent as ILoadingComponent);
			RootWidget.RearrangeWidgetDepth();
		}
	}

	private void SetupAnchors(ILoadingComponent loadingComp)
	{
		SetupAnchor(loadingComp, EAnchorPivot.TopLeft);
		SetupAnchor(loadingComp, EAnchorPivot.TopRight);
		SetupAnchor(loadingComp, EAnchorPivot.BottomLeft);
		SetupAnchor(loadingComp, EAnchorPivot.BottomCenter);
		SetupAnchor(loadingComp, EAnchorPivot.BottomRight);
		SetupAnchor(loadingComp, EAnchorPivot.FullScreen);
	}

	private void SetupAnchor(ILoadingComponent loadingComp, EAnchorPivot pivot)
	{
		UIWidget anchorWidget = loadingComp.GetAnchorWidget(pivot);
		UIWidget anchorTarget = GetAnchorTarget(pivot);
		if (anchorWidget != null && anchorTarget != null)
		{
			anchorWidget.updateAnchors = UIRect.AnchorUpdate.OnUpdate;
			anchorWidget.SetAnchor(anchorTarget.gameObject);
		}
	}

	private UIWidget GetAnchorTarget(EAnchorPivot pivot)
	{
		return ListWidgetInfo.Find((AnchorWidgetInfo info) => info.Pivot == pivot)?.Widget;
	}

	private void Awake()
	{
	}

	private void Update()
	{
		if (_loadingStatus != null)
		{
			UIHelper.SetLabelText(GaugeLabel, _loadingStatus.Label);
			UIHelper.SetSpriteFillAmount(GaugeSprite, _loadingStatus.FillAmount);
			_indicatorElapsedTime = Mathf.Repeat(_indicatorElapsedTime + UnitySystemTimer.deltaTime * 2f, 4f);
			if (_indicatorElapsedTime < 1f)
			{
				UIHelper.SetLabelText(GaugeIndicatorLabel, string.Empty);
			}
			else if (_indicatorElapsedTime < 2f)
			{
				UIHelper.SetLabelText(GaugeIndicatorLabel, ".");
			}
			else if (_indicatorElapsedTime < 3f)
			{
				UIHelper.SetLabelText(GaugeIndicatorLabel, "..");
			}
			else
			{
				UIHelper.SetLabelText(GaugeIndicatorLabel, "...");
			}
		}
	}

	void IGlobalWindow.ReserveCache(UiPreloader preloader)
	{
	}

	public bool IsOpen()
	{
		return true;
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		UIHelper.SetSpriteFillAmount(GaugeSprite, 0f);
		FactoryBase.ReleaseComponent(_loadingComponent);
		_loadingComponent = null;
	}

	public bool IsComponentCompleted()
	{
		if (!(_loadingComponent is ILoadingComponent loadingComponent))
		{
			return true;
		}
		return loadingComponent.IsCompleted();
	}
}
