using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NTower;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NTower;

public class TowerWindow : BaseTower, TowerFloorComponent.Listener
{
	public interface Listener
	{
		void OnResetTower(TowerWindow caller);

		void OnClickFloor(TowerWindow caller, TowerFloorComponent component);
	}

	[Serializable]
	public class TowerOption
	{
		public float DefaultPageLerpFactor = 5f;

		public float DragPageLerpFactor = 20f;

		public float PageMoveDragThreshold = 0.4f;

		public float PageMoveDuration = 0.5f;

		public float WaitCompleteEffectDuration = 3f;

		public float ResetFadeOutTime = 1f;

		public float ResetDescentSpeed = 2000f;

		public float ResetDescentMaxDuration = 2f;

		public AnimationCurve DescentAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
	}

	[Serializable]
	public class IntroInfo
	{
		public Transform Root;

		public Transform ItemRoot;

		public Transform ProgressRoot;

		public Transform CompletedRoot;
	}

	[Serializable]
	public class PageInfo
	{
		public Transform Root;

		public UILabel TowerNameLabel;

		public UILabel PageNumberLabel;

		public Transform NextFloorDesc;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Tower/TowerWindow";

	public WindowAnimation WindowAnimation;

	public Transform RootMover;

	public UIWidget VisibleRoot;

	public Transform[] FloorRoot;

	public Transform TowerRoot;

	public UITexture TowerTexture;

	public UITexture SkyTexture;

	public Transform AreaMoveRoot;

	public IntroInfo Intro;

	public PageInfo Page;

	public TowerOption Option;

	private Listener _listener;

	private List<TowerFloorComponent> _floorComponentList = new List<TowerFloorComponent>();

	private List<GameObject> _towerObjectList = new List<GameObject>();

	private CachedAccountInfo[] _npcInfos;

	private float _viewOffset;

	private float _dragOffset;

	private float _dragSpeed;

	private float _lastUpdateOffset;

	private bool _isDragging;

	private bool _isPageMoving;

	private bool _isResetting;

	private int _pageWidth;

	private int _pageHeight;

	private AssetTexture.TextureInfo _skyTextureInfo;

	private Component _introItemComponent;

	private bool _entered;

	private Coroutine _enterFloorEffect;

	private bool _playingClearEffect;

	private List<AssetTexture> _preloadAssetList = new List<AssetTexture>();

	public int StartFloor { get; private set; }

	public int FloorCountPerPage { get; private set; }

	public void Init(TowerData towerData, TowerInfo towerInfo, CachedAccountInfo[] npcInfos, int showFloor, bool showClearEffect, bool returnFromBattle, Action onOpen, Listener listener)
	{
		Init(towerData, towerInfo);
		_listener = listener;
		_pageWidth = 1920;
		_pageHeight = 1920 * Screen.height / Screen.width;
		_npcInfos = npcInfos;
		FloorCountPerPage = ((towerData.FloorCountPerPage == 0) ? 1 : towerData.FloorCountPerPage);
		InitStartFloor(showFloor);
		_entered = false;
		UIHelper.EnableGameObject(AreaMoveRoot, enable: false);
		UIHelper.EnableGameObject(Intro.Root, enable: false);
		ShowPageTitle(-1);
		PreloadAssets(delegate
		{
			if (returnFromBattle)
			{
				if (showClearEffect)
				{
					SetViewOffset(GetViewOffset(base.CompletedFloor));
				}
				else
				{
					SetViewOffset(GetViewOffset(StartFloor));
				}
				ShowIntro(show: false);
				OnEnterTower();
			}
			else
			{
				SetViewOffset(GetViewOffset(GetStartFloorOfPage(-1)));
				ShowIntro(show: true);
			}
			if (showClearEffect)
			{
				TowerFloorComponent floorComponent = GetFloorComponent(base.CompletedFloor + 1);
				if ((bool)floorComponent)
				{
					floorComponent.SetLocked(locked: true);
				}
			}
			InitBackgroundSky();
			InitBackgroundTower();
			UpdateRootPosition(1f);
			UpdateBackgroundSkyUvRect();
			if (returnFromBattle && showClearEffect)
			{
				WindowAnimation.PlayAnimation(reverse: false, CreateClearedEffect);
			}
			else
			{
				WindowAnimation.PlayAnimation(reverse: false);
			}
		});
	}

	private void ShowIntro(bool show)
	{
		TowerFloorData data = GameRoot.Instance.TowerFloorDataContainer.GetData(base.TowerIndex, FloorCountPerPage);
		if (data != null)
		{
			if (!show)
			{
				UIHelper.EnableGameObject(Intro.Root, enable: false);
				ClearIntroItemComponent();
				return;
			}
			UIHelper.EnableGameObject(Intro.Root, enable: true);
			ClearIntroItemComponent();
			_introItemComponent = TowerFloorComponent.CreateRewardComponent<ItemIconComponent.Listener>(data, isRepeating: false, Intro.ItemRoot, null);
			int maxFloor = BaseTower.GetMaxFloor(base.TowerIndex);
			bool flag = base.CompletedFloor >= maxFloor;
			UIHelper.EnableGameObject(Intro.ProgressRoot, !flag);
			UIHelper.EnableGameObject(Intro.CompletedRoot, flag);
		}
	}

	private void ShowPageTitle(int pageNo)
	{
		bool flag = pageNo >= 0;
		UIHelper.EnableGameObject(Page.Root, flag);
		if (flag)
		{
			string stringDirect = LocalizationManager.GetStringDirect(base.TowerData.Name);
			UIHelper.SetLabelText(Page.TowerNameLabel, stringDirect);
			UIHelper.SetLabelText(Page.PageNumberLabel, pageNo + 1);
			int pageOfFloor = GetPageOfFloor(base.MaxFloor);
			bool flag2 = pageNo >= pageOfFloor;
			UIHelper.EnableGameObject(Page.NextFloorDesc, !flag2);
		}
	}

	private void ClearIntroItemComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _introItemComponent);
	}

	public void OnClickEnterTower()
	{
		ShowIntro(show: false);
		OnEnterTower();
		int pageOfFloor = GetPageOfFloor(StartFloor);
		MovePage(pageOfFloor, Option.PageMoveDuration, Option.DescentAnimationCurve, 0f, null);
		_entered = true;
	}

	private void OnEnterTower()
	{
		int pageOfFloor = GetPageOfFloor(StartFloor);
		CreateFloor(pageOfFloor);
		CreateFloor(pageOfFloor - 1);
		CreateFloor(pageOfFloor + 1);
		UIHelper.EnableGameObject(AreaMoveRoot, enable: true);
		ShowPageTitle(pageOfFloor);
		_entered = true;
	}

	public override void Reset()
	{
		base.Reset();
		ClearFloor();
		ClearBackgroundTower();
		ClearIntroItemComponent();
		ResetRootMover();
		_viewOffset = 0f;
		_dragOffset = 0f;
		_isPageMoving = false;
		_isResetting = false;
		_entered = false;
		_playingClearEffect = false;
		for (int i = 0; i < _preloadAssetList.Count; i++)
		{
			_preloadAssetList[i].Close();
		}
		_preloadAssetList.Clear();
	}

	protected override void OnShowRoot(bool isShow)
	{
		base.OnShowRoot(isShow);
		if (_enterFloorEffect != null)
		{
			StopCoroutine(_enterFloorEffect);
			_enterFloorEffect = null;
		}
		if (isShow)
		{
			RefreshFloorEffect();
		}
		ResetRootMover();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (!_isPageMoving)
		{
			UpdateRootPosition(Time.deltaTime * (_isDragging ? Option.DragPageLerpFactor : Option.DefaultPageLerpFactor));
			UpdateBackgroundSkyUvRect();
		}
	}

	private void ResetRootMover()
	{
		RootMover.localPosition = Vector3.zero;
		RootMover.localScale = Vector3.one;
	}

	private void RefreshFloorEffect()
	{
		_floorComponentList.ForEach(delegate(TowerFloorComponent e)
		{
			e.RefreshFloorEffect();
		});
	}

	private void PreloadAssets(Action onLoad)
	{
		ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coPreloadAssets(onLoad));
	}

	private IEnumerator coPreloadAssets(Action onLoad)
	{
		AssetTexture assetTexture = LoadBackgroundSkyAsync();
		if (assetTexture != null)
		{
			_preloadAssetList.Add(assetTexture);
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(base.TowerData.TowerImage); i++)
		{
			string towerImage = base.TowerData.TowerImage[i];
			AssetTexture assetTexture2 = LoadBackgroundTowerAsync(towerImage);
			if (assetTexture2 != null)
			{
				_preloadAssetList.Add(assetTexture2);
			}
		}
		bool flag;
		do
		{
			yield return null;
			flag = true;
			for (int j = 0; j < _preloadAssetList.Count; j++)
			{
				if (!_preloadAssetList[j].ISDONE())
				{
					flag = false;
				}
			}
		}
		while (!flag);
		Vespa_System_Action_Extension_Method.fire(onLoad);
	}

	public override bool CanReset()
	{
		if (base.CompletedFloor == 0)
		{
			return false;
		}
		return true;
	}

	public override void OnResetTower(TowerInfo towerInfo)
	{
		VespaCoroutine.Start(this, coResetTower(towerInfo));
	}

	private IEnumerator coResetTower(TowerInfo towerInfo)
	{
		base.OnResetTower(towerInfo);
		_isResetting = true;
		InitTowerInfo(towerInfo);
		InitStartFloor(towerInfo.CompletedFloor + 1);
		float fadeOutDuration = ((Option.ResetFadeOutTime > 0f) ? Option.ResetFadeOutTime : 1f);
		float startFadeOutTime = Time.time;
		while (true)
		{
			yield return null;
			float num = Time.time - startFadeOutTime;
			if (num >= fadeOutDuration)
			{
				break;
			}
			float alpha = Mathf.Clamp01(1f - num / fadeOutDuration);
			UIHelper.SetWidgetAlpha(VisibleRoot, alpha);
		}
		ClearFloor();
		CreateFloor(StartFloor);
		float viewOffset = _viewOffset;
		float num2 = Mathf.Abs(GetViewOffset(StartFloor) - viewOffset);
		float num3 = ((Option.ResetDescentSpeed > 0f) ? (num2 / Option.ResetDescentSpeed) : Option.ResetDescentMaxDuration);
		if (Option.ResetDescentMaxDuration > 0f && num3 > Option.ResetDescentMaxDuration)
		{
			num3 = Option.ResetDescentMaxDuration;
		}
		yield return ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coMovePage(0, num3, Option.DescentAnimationCurve, 0f, null));
		float fadInDuration = ((Option.ResetFadeOutTime > 0f) ? Option.ResetFadeOutTime : 1f);
		float startFadeInTime = Time.time;
		while (true)
		{
			yield return null;
			float num4 = Time.time - startFadeInTime;
			if (num4 >= fadInDuration)
			{
				break;
			}
			float alpha2 = Mathf.Clamp01(num4 / fadInDuration);
			UIHelper.SetWidgetAlpha(VisibleRoot, alpha2);
		}
		_isResetting = false;
	}

	private void MovePage(int targetPage, float descentDuration, AnimationCurve curve, float initialSpeed, Action onEnd)
	{
		VespaCoroutine.Start(coMovePage(targetPage, descentDuration, curve, initialSpeed, onEnd));
	}

	private IEnumerator coMovePage(int targetPage, float descentDuration, AnimationCurve curve, float initialSpeed, Action onEnd)
	{
		LabelPopup blockPopup = LabelPopup.Open();
		_isPageMoving = true;
		ShowPageTitle(-1);
		float startOffset = _lastUpdateOffset;
		_dragOffset = 0f;
		float targetOffset = GetViewOffset(GetStartFloorOfPage(targetPage));
		float num = targetOffset - startOffset;
		float acceleration = 0f;
		if (!Mathf.Approximately(initialSpeed, 0f) && descentDuration > 0f)
		{
			acceleration = 2f * (num - initialSpeed * descentDuration) / (descentDuration * descentDuration);
		}
		float startDescentTime = Time.time;
		float num2;
		bool flag;
		do
		{
			yield return null;
			num2 = Mathf.Min(Time.time - startDescentTime, descentDuration);
			flag = false;
			float num4;
			if (!Mathf.Approximately(acceleration, 0f))
			{
				float num3 = initialSpeed * num2 + 0.5f * acceleration * (num2 * num2);
				num4 = startOffset + num3;
				if ((num4 - targetOffset) * (startOffset - targetOffset) < 0f)
				{
					num4 = targetOffset;
					flag = true;
				}
			}
			else
			{
				float num5 = Mathf.Clamp01(num2 / descentDuration);
				if (curve != null)
				{
					num5 = Mathf.Clamp01(curve.Evaluate(num5));
				}
				num4 = Mathf.Lerp(startOffset, targetOffset, num5);
				flag = num5 >= 1f;
			}
			SetViewOffset(num4);
			UpdateRootPosition(1f);
			UpdateBackgroundSkyUvRect();
		}
		while (!flag && !(num2 >= descentDuration));
		blockPopup.Close();
		_isPageMoving = false;
		ShowPageTitle(targetPage);
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}

	private void UpdateRootPosition(float lerp)
	{
		float b = Mathf.Clamp(_viewOffset + _dragOffset, GetMinViewOffset(), GetMaxViewOffset());
		_lastUpdateOffset = Mathf.Lerp(0f - VisibleRoot.transform.localPosition.y, b, Mathf.Clamp01(lerp));
		float num = 0f - _lastUpdateOffset;
		if (!Mathf.Approximately(VisibleRoot.transform.localPosition.y, num))
		{
			VisibleRoot.transform.localPosition = new Vector3(0f, num, 0f);
			TowerRoot.transform.localPosition = new Vector3(0f, num, 0f);
		}
	}

	public void PlayEnterFloorEffect(TowerFloorComponent component)
	{
		_enterFloorEffect = VespaCoroutine.Start(this, coPlayEnterFloorEffectInternal(component));
	}

	private IEnumerator coPlayEnterFloorEffectInternal(TowerFloorComponent component)
	{
		if (!(component == null))
		{
			Vector3 totalOffset = -base.transform.InverseTransformPoint(component.transform.position);
			float moveFactor = 3f;
			float scaleFactor = 3f;
			float maxDuration = 1f;
			float elapsedTime = 0f;
			while (elapsedTime < maxDuration)
			{
				yield return null;
				elapsedTime += Time.deltaTime;
				float num = Mathf.Clamp01(elapsedTime / maxDuration);
				Vector3 vector = totalOffset * Mathf.Clamp01(num * moveFactor);
				RootMover.localPosition = new Vector3(vector.x, vector.y, 0f);
				RootMover.localScale = Vector3.one * (1f + num * scaleFactor);
			}
		}
	}

	private void ClearFloor()
	{
		FactoryBase.ReleaseAndClearList(_floorComponentList);
	}

	private void CreateClearedEffect()
	{
		VespaCoroutine.Start(coCreateClearEffect());
	}

	private IEnumerator coCreateClearEffect()
	{
		_playingClearEffect = true;
		TowerFloorComponent floorComponent = GetFloorComponent(base.CompletedFloor);
		if ((bool)floorComponent)
		{
			floorComponent.StopPassedWidget();
			floorComponent.PlayPassedWidget(justCleared: true);
			yield return new WaitForSeconds(Option.WaitCompleteEffectDuration);
		}
		int pageOfFloor = GetPageOfFloor(base.CompletedFloor);
		int pageOfFloor2 = GetPageOfFloor(base.CompletedFloor + 1);
		if (pageOfFloor2 != pageOfFloor)
		{
			yield return VespaCoroutine.Start(coMovePage(pageOfFloor2, Option.PageMoveDuration, Option.DescentAnimationCurve, 0f, null));
		}
		TowerFloorComponent floorComponent2 = GetFloorComponent(base.CompletedFloor + 1);
		if ((bool)floorComponent2)
		{
			floorComponent2.SetLocked(locked: false);
			floorComponent2.PlayOpenEffectWidget();
		}
		_playingClearEffect = false;
	}

	private void InitStartFloor(int showFloor)
	{
		StartFloor = GetStartFloor(Mathf.Min((showFloor > 0) ? showFloor : (base.CompletedFloor + 1), base.MaxFloor), FloorCountPerPage);
	}

	private void CreateFloor(int page)
	{
		int startFloorOfPage = GetStartFloorOfPage(page);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(FloorRoot); i++)
		{
			Transform parent = FloorRoot[i];
			int num = startFloorOfPage + i;
			if (num >= 1 && num <= base.MaxFloor && !GetFloorComponent(num))
			{
				TowerFloorComponent towerFloorComponent = CreateFloor(num, parent);
				if (towerFloorComponent == null)
				{
					VespaDebug.LogError("Failed to create floor component : floor=" + num);
					continue;
				}
				UpdateFloorPosition(towerFloorComponent);
				_floorComponentList.Add(towerFloorComponent);
			}
		}
	}

	private TowerFloorComponent CreateFloor(int floor, Transform parent)
	{
		TowerFloorData data = GameRoot.Instance.TowerFloorDataContainer.GetData(base.TowerData.TowerIndex, floor);
		if (data == null)
		{
			return null;
		}
		TowerFloorComponent.TowerBossType bossType = TowerFloorComponent.TowerBossType.Normal;
		if (floor == base.MaxFloor)
		{
			bossType = TowerFloorComponent.TowerBossType.Boss;
		}
		else if (floor % FloorCountPerPage == 0)
		{
			bossType = TowerFloorComponent.TowerBossType.SemiBoss;
		}
		int num = floor - 1;
		CachedAccountInfo npcInfo = ((num < __Extension_Method_ICollection__.COUNT(_npcInfos)) ? _npcInfos[num] : null);
		TowerFloorComponent towerFloorComponent = TowerFloorComponent.PrefabAuto.Factory.Create(parent);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(towerFloorComponent.transform);
		towerFloorComponent.Open(this, base.TowerData, data, base.CompletedFloor, bossType, npcInfo);
		return towerFloorComponent;
	}

	private void UpdateFloorPosition(TowerFloorComponent component)
	{
		if (!(component == null))
		{
			int num = GetStartFloor(component.FloorData.Floor, FloorCountPerPage) - 1;
			component.transform.localPosition = Vector3.up * _pageHeight * (num / FloorCountPerPage);
		}
	}

	private void UpdateFloorPosition()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_floorComponentList); i++)
		{
			TowerFloorComponent component = _floorComponentList[i];
			UpdateFloorPosition(component);
		}
	}

	private TowerFloorComponent GetFloorComponent(int floor)
	{
		return _floorComponentList.Find((TowerFloorComponent e) => e.FloorData != null && e.FloorData.Floor == floor);
	}

	private bool IsVisibleFloor(int floor)
	{
		int num = Mathf.Max(StartFloor - FloorCountPerPage, 1);
		int num2 = Mathf.Min(StartFloor + FloorCountPerPage * 2 - 1, base.MaxFloor);
		if (floor >= num)
		{
			return floor <= num2;
		}
		return false;
	}

	private void ClearInvisibleFloor()
	{
		for (int num = __Extension_Method_ICollection__.COUNT(_floorComponentList) - 1; num >= 0; num--)
		{
			TowerFloorComponent towerFloorComponent = _floorComponentList[num];
			int floor = towerFloorComponent.FloorData.Floor;
			if (!IsVisibleFloor(floor))
			{
				_floorComponentList.RemoveAt(num);
				FactoryBase.ReleaseComponent(towerFloorComponent);
			}
		}
	}

	public static int GetStartFloor(int currentFloor, int floorCountPerPage)
	{
		if (floorCountPerPage == 0)
		{
			floorCountPerPage = 1;
		}
		return (currentFloor - 1) / floorCountPerPage * floorCountPerPage + 1;
	}

	private void InitBackgroundTower()
	{
		ClearBackgroundTower();
		for (int i = 0; i <= base.MaxFloor / FloorCountPerPage; i++)
		{
			int num = (i - 1) * _pageHeight;
			GameObject gameObject = CreateBackgroundTower(i, num);
			if (!(gameObject == null))
			{
				_towerObjectList.Add(gameObject);
				continue;
			}
			break;
		}
	}

	private void ClearBackgroundTower()
	{
		foreach (GameObject towerObject in _towerObjectList)
		{
			VespaObject.DestroyGo(towerObject);
		}
		_towerObjectList.Clear();
	}

	private AssetTexture LoadBackgroundTowerAsync(string towerImage)
	{
		AssetTexture assetTexture = new AssetTexture();
		assetTexture.Open("Tower/" + towerImage);
		return assetTexture;
	}

	private GameObject CreateBackgroundTower(int page, float offset = 0f)
	{
		if (TowerTexture == null)
		{
			return null;
		}
		bool num = page == 0;
		bool flag = page == base.MaxFloor / FloorCountPerPage;
		int num2 = 0;
		num2 = ((!num) ? ((!flag) ? 1 : 2) : 0);
		num2 = Mathf.Min(num2, __Extension_Method_ICollection__.COUNT(base.TowerData.TowerImage) - 1);
		if (num2 < 0)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(TowerTexture.gameObject);
		gameObject.transform.parent = TowerTexture.transform.parent;
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(gameObject.transform);
		gameObject.transform.localPosition += Vector3.up * offset;
		string towerImage = base.TowerData.TowerImage[num2];
		AssetTexture assetTex = LoadBackgroundTowerAsync(towerImage);
		UITexture uiTexture = gameObject.GetComponent<UITexture>();
		UIHelper.ResetPlaneAssetTextureAsync(this, uiTexture, assetTex, UIHelper.EAssetTextureAssign.None, delegate
		{
			uiTexture.SetDimensions(_pageWidth, _pageHeight);
		});
		return gameObject;
	}

	private AssetTexture LoadBackgroundSkyAsync()
	{
		string skyImage = base.TowerData.SkyImage;
		if (string.IsNullOrEmpty(skyImage))
		{
			return null;
		}
		AssetTexture assetTexture = new AssetTexture();
		assetTexture.Open("Tower/" + skyImage);
		return assetTexture;
	}

	private void InitBackgroundSky()
	{
		AssetTexture assetTex = LoadBackgroundSkyAsync();
		UIHelper.ResetPlaneAssetTextureAsync(this, SkyTexture, assetTex, UIHelper.EAssetTextureAssign.None, delegate(AssetTexture.TextureInfo texInfo)
		{
			_skyTextureInfo = texInfo;
			UpdateBackgroundSkyUvRect();
		});
	}

	private void SetBackgroundSkyUvRect(float ratio)
	{
		if (_skyTextureInfo != null && !(_skyTextureInfo.Tex == null))
		{
			int width = _skyTextureInfo.Width;
			int height = _skyTextureInfo.Height;
			int num = width * SkyTexture.height / SkyTexture.width;
			int num2 = height - num;
			float num3 = Mathf.Lerp(0f, num2, ratio);
			SkyTexture.uvRect = _skyTextureInfo.CalcMinMaxUv(0f, num3, width, num3 + (float)num);
		}
	}

	private void UpdateBackgroundSkyUvRect()
	{
		float y = VisibleRoot.transform.localPosition.y;
		float num = 0f - GetMaxViewOffset();
		float backgroundSkyUvRect = y / num;
		SetBackgroundSkyUvRect(backgroundSkyUvRect);
	}

	public void OnClickNextPage()
	{
		MoveToNextPage(0f);
	}

	private void MoveToNextPage(float initialSpeed)
	{
		if (!CanAcceptInput())
		{
			return;
		}
		int num = GetPageOfFloor(StartFloor) + 1;
		int newStartFloor = GetStartFloorOfPage(num);
		if (GameRoot.Instance.TowerFloorDataContainer.GetData(base.TowerIndex, newStartFloor) != null)
		{
			_ = StartFloor;
			CreateFloor(num);
			CreateFloor(num + 1);
			MovePage(num, Option.PageMoveDuration, Option.DescentAnimationCurve, initialSpeed, delegate
			{
				InitStartFloor(newStartFloor);
				ClearInvisibleFloor();
			});
		}
	}

	public void OnClickPrevPage()
	{
		MoveToPrevPage(0f);
	}

	private void MoveToPrevPage(float initialSpeed)
	{
		if (!CanAcceptInput())
		{
			return;
		}
		int num = GetPageOfFloor(StartFloor) - 1;
		int newStartFloor = GetStartFloorOfPage(num);
		if (GameRoot.Instance.TowerFloorDataContainer.GetData(base.TowerIndex, newStartFloor) != null)
		{
			_ = StartFloor;
			CreateFloor(num);
			CreateFloor(num - 1);
			MovePage(num, Option.PageMoveDuration, Option.DescentAnimationCurve, initialSpeed, delegate
			{
				InitStartFloor(newStartFloor);
				ClearInvisibleFloor();
			});
		}
	}

	public void OnClickReset()
	{
		if (CanAcceptInput() && _listener != null)
		{
			_listener.OnResetTower(this);
		}
	}

	public void OnClickClose()
	{
		Close();
	}

	public void OnDragArea()
	{
		if (CanAcceptInput())
		{
			float num = (0f - UICamera.currentTouch.delta.y) * 1080f / (float)Screen.height;
			float b = num / Time.deltaTime;
			_dragOffset += num;
			_isDragging = true;
			_dragSpeed = Mathf.Lerp(_dragSpeed, b, 0.5f);
			if (_viewOffset + _dragOffset <= 0f)
			{
				_viewOffset = 0f;
				_dragOffset = 0f;
				_dragSpeed = 0f;
			}
		}
	}

	public void OnDragAreaStop()
	{
		_isDragging = false;
		if (Mathf.Abs(_dragOffset) > (float)_pageHeight * Option.PageMoveDragThreshold)
		{
			float dragSpeed = _dragSpeed;
			_dragSpeed = 0f;
			if (_dragOffset < 0f)
			{
				MoveToPrevPage(dragSpeed);
			}
			else
			{
				MoveToNextPage(dragSpeed);
			}
		}
		else
		{
			_dragOffset = 0f;
			_dragSpeed = 0f;
		}
	}

	private bool CanAcceptInput()
	{
		if (_playingClearEffect || _isPageMoving || _isResetting || !_entered)
		{
			return false;
		}
		return true;
	}

	private float GetMinViewOffset()
	{
		return _pageHeight * -1;
	}

	private float GetMaxViewOffset()
	{
		return _pageHeight * GetPageOfFloor(base.MaxFloor);
	}

	private float GetViewOffset(int floor)
	{
		int pageOfFloor = GetPageOfFloor(floor);
		return _pageHeight * pageOfFloor;
	}

	private void SetViewOffset(float offset)
	{
		_viewOffset = offset;
	}

	private int GetPageOfFloor(int floor)
	{
		if (FloorCountPerPage <= 0)
		{
			return 0;
		}
		return (floor - 1) / FloorCountPerPage;
	}

	private int GetStartFloorOfPage(int page)
	{
		return page * FloorCountPerPage + 1;
	}

	void TowerFloorComponent.Listener.OnClickFloor(TowerFloorComponent component)
	{
		if (CanAcceptInput() && _listener != null)
		{
			_listener.OnClickFloor(this, component);
		}
	}
}
