using System;
using System.Collections.Generic;
using NGame2.NScene;
using NGame2.NScene.NPreloader;
using NGame2.NTutorial;
using NGame2.NTutorial.NRuntime;
using NGame2.NUI.NComponent2.NTutorial;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.Mission;
using NGame2.NUtil;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NTutorial;

public class TutorialWindow : MonoBehaviour, IGlobalWindow
{
	public interface Listener
	{
	}

	public class ActionInfo
	{
		public Action Action;

		public bool Oneshot;

		public bool IsLongTouch;

		public int MinX;

		public int MinY;

		public int MaxX;

		public int MaxY;

		public bool ProcessTouch;

		public bool ProcessBack;

		public GameObject TargetGo;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Tutorial/TutorialWindow";

	private static GlobalWindowPrefab<TutorialWindow> _prefabInstance = new GlobalWindowPrefab<TutorialWindow>("Resources/Prefab2/Window/Tutorial/TutorialWindow", GlobalUIRoot.ELayerType.GlobalUI, isPooling: false);

	[SerializeField]
	private UIWidget _touchWidget;

	public UITexture FadeTexture;

	public Transform TooltipRoot;

	public UIButton SkipButton;

	private Listener _listener;

	private List<ActionInfo> _touchActionList = new List<ActionInfo>();

	private List<TutorialFocusComponent> _focusComponentList = new List<TutorialFocusComponent>();

	private List<TutorialEffectComponent> _effectComponentList = new List<TutorialEffectComponent>();

	private List<TutorialTooltipComponent> _tooltipComponentList = new List<TutorialTooltipComponent>();

	private List<TutorialArrowComponent> _arrowComponentList = new List<TutorialArrowComponent>();

	private Action _skipCallback;

	private int _lastWidgetDepth;

	private Color32[] _colorBuffer;

	private TutorialData _skipStartTutorialData;

	private TutorialData _skipEndTutorialData;

	private SkipType _skipType;

	private static GameUIRoot.ELayerType[] RaycastTargetLayer = new GameUIRoot.ELayerType[2]
	{
		GameUIRoot.ELayerType.UI,
		GameUIRoot.ELayerType.BottomUI
	};

	public static TutorialWindow instance => _prefabInstance.instance;

	public static bool isValidInstance => _prefabInstance.IsValidInstance;

	public int FadeTextureHeight => 540;

	public int FadeTextureWidth => 960;

	public bool EnableGameUi { get; set; }

	private Color _emptyColor => GameSetting.data.Tutorial.EmptyColor;

	private Color _darkColor => GameSetting.data.Tutorial.DarkColor;

	public static void ReleaseInstance()
	{
		_prefabInstance.ReleaseWindow();
		_prefabInstance.ReleasePrefab();
	}

	public static void OnSceneChanged()
	{
		if (!_prefabInstance.IsOpen())
		{
			_prefabInstance.ReleaseWindow();
			_prefabInstance.ReleasePrefab();
		}
	}

	public void Open(Listener listener)
	{
		_listener = listener;
		UIHelper.EnableGameObject(SkipButton, enable: false);
		UIHelper.EnableGameObject(_touchWidget, enable: true);
		EnableGameUi = false;
		int fadeTextureWidth = FadeTextureWidth;
		int fadeTextureHeight = FadeTextureHeight;
		Texture2D mainTexture = new Texture2D(fadeTextureWidth, fadeTextureHeight, TextureFormat.ARGB32, mipChain: false);
		_colorBuffer = new Color32[fadeTextureWidth * fadeTextureHeight];
		FadeTexture.mainTexture = mainTexture;
		FadeTexture.transform.localPosition = new Vector3(960f / (float)FadeTextureWidth, 540f / (float)FadeTextureHeight);
		Vector2 uiSize = GetUiSize();
		FadeTexture.SetDimensions((int)uiSize.x, (int)uiSize.y);
		ClearFocus(showFader: false);
		_touchActionList.Clear();
	}

	private void Update()
	{
		bool enable = true;
		bool num = IsSkipping();
		if (EnableGameUi)
		{
			enable = false;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_touchActionList))
		{
			enable = true;
		}
		if (num)
		{
			enable = false;
		}
		UIHelper.EnableGameObject(_touchWidget, enable);
		UpdateSkipButton();
	}

	public ActionInfo GetFirstTouchActionInfo()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_touchActionList))
		{
			return null;
		}
		return _touchActionList[0];
	}

	public void ForceRunTouchAction()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_touchActionList))
		{
			ActionInfo[] array = _touchActionList.ToArray();
			foreach (ActionInfo actionInfo in array)
			{
				ProcessTouchAction(actionInfo);
			}
			_touchActionList.Clear();
		}
	}

	public Vector2 GetUiSize()
	{
		int activeHeight = GlobalUIRoot.instance.UIRoot.activeHeight;
		Camera camera = GlobalUIRoot.instance.UI.Camera;
		float num = (float)camera.pixelWidth / (float)camera.pixelHeight;
		if (num >= 1.7777778f)
		{
			return new Vector2((float)activeHeight * num, activeHeight);
		}
		return new Vector2(1920f, activeHeight);
	}

	public static Vector2 GetStaticUiSize()
	{
		int activeHeight = GlobalUIRoot.instance.UIRoot.activeHeight;
		Camera camera = GlobalUIRoot.instance.UI.Camera;
		float num = (float)camera.pixelWidth / (float)camera.pixelHeight;
		if (num >= 1.7777778f)
		{
			return new Vector2((float)activeHeight * num, activeHeight);
		}
		return new Vector2(1920f, activeHeight);
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
		FactoryBase.ReleaseAndClearList(_focusComponentList);
		FactoryBase.ReleaseAndClearList(_effectComponentList);
		FactoryBase.ReleaseAndClearList(_tooltipComponentList);
		FactoryBase.ReleaseAndClearList(_arrowComponentList);
		VespaObject.DestroyObject(FadeTexture.mainTexture);
	}

	public Rect Normalize(int minX, int minY, int maxX, int maxY)
	{
		Vector2 uiSize = GetUiSize();
		float xMin = ((float)minX + uiSize.x / 2f) / uiSize.x;
		float xMax = ((float)maxX + uiSize.x / 2f) / uiSize.x;
		float yMin = ((float)minY + uiSize.y / 2f) / uiSize.y;
		float yMax = ((float)maxY + uiSize.y / 2f) / uiSize.y;
		return new Rect
		{
			xMin = xMin,
			xMax = xMax,
			yMin = yMin,
			yMax = yMax
		};
	}

	public ActionInfo SetTouchCallback(Action onTouch, bool oneshot)
	{
		return SetTouchCallback(onTouch, oneshot, 0, 0, 0, 0, processTouch: false, processBack: false, null);
	}

	public ActionInfo SetTouchCallback(Action onTouch, bool oneshot, int minX, int minY, int maxX, int maxY, bool processTouch, bool processBack, GameObject targetGo)
	{
		ActionInfo actionInfo = new ActionInfo
		{
			Action = onTouch,
			Oneshot = oneshot,
			MinX = minX,
			MinY = minY,
			MaxX = maxX,
			MaxY = maxY,
			ProcessTouch = processTouch,
			ProcessBack = processBack,
			TargetGo = targetGo
		};
		_touchActionList.Add(actionInfo);
		return actionInfo;
	}

	public ActionInfo SetLongTouchCallback(Action onTouch, bool oneshot, int minX, int minY, int maxX, int maxY, GameObject targetGo)
	{
		ActionInfo actionInfo = new ActionInfo
		{
			Action = onTouch,
			Oneshot = oneshot,
			IsLongTouch = true,
			MinX = minX,
			MinY = minY,
			MaxX = maxX,
			MaxY = maxY,
			ProcessTouch = false,
			ProcessBack = false,
			TargetGo = targetGo
		};
		_touchActionList.Add(actionInfo);
		return actionInfo;
	}

	public bool RemoveTouchCallback(ActionInfo actionInfo)
	{
		return _touchActionList.Remove(actionInfo);
	}

	public void OnSystemBackButton()
	{
		ActionInfo[] array = _touchActionList.ToArray();
		foreach (ActionInfo actionInfo in array)
		{
			if (actionInfo.ProcessBack)
			{
				ProcessTouchAction(actionInfo);
				return;
			}
		}
		if (!SystemBackButtonBlock.IsRealBackButton)
		{
			return;
		}
		if (SingletonUpdater<TutorialManager>.instance.SystemBackButtonSkip > 0)
		{
			GlobalCenterMessage.Open(LocalizationManager.GetString(Tutorial.TUTORIAL_BACKBUTTON_WARNING) + $"({SingletonUpdater<TutorialManager>.instance.SystemBackButtonSkip})");
			SingletonUpdater<TutorialManager>.instance.SystemBackButtonSkip--;
		}
		else if (!QuitGamePopup.IsOpenGlobal())
		{
			QuitGamePopup.OpenGlobal(delegate
			{
				GlobalAutoBehaviour<LoadingManager>.instance.Quit();
			});
		}
	}

	public void OnTouch()
	{
		Vector3 vector = UICamera.lastHit.point * GlobalUIRoot.instance.UIRoot.activeHeight * 0.5f;
		ActionInfo[] array = _touchActionList.ToArray();
		foreach (ActionInfo actionInfo in array)
		{
			if (!actionInfo.IsLongTouch && (actionInfo.MinX == actionInfo.MaxX || (!(vector.x < (float)actionInfo.MinX) && !(vector.x > (float)actionInfo.MaxX))) && (actionInfo.MinY == actionInfo.MaxY || (!(vector.y < (float)actionInfo.MinY) && !(vector.y > (float)actionInfo.MaxY))))
			{
				ProcessTouchAction(actionInfo);
			}
		}
	}

	public void OnLongTouch()
	{
		Vector3 vector = UICamera.lastHit.point * GlobalUIRoot.instance.UIRoot.activeHeight * 0.5f;
		ActionInfo[] array = _touchActionList.ToArray();
		foreach (ActionInfo actionInfo in array)
		{
			if (actionInfo.IsLongTouch && (actionInfo.MinX == actionInfo.MaxX || (!(vector.x < (float)actionInfo.MinX) && !(vector.x > (float)actionInfo.MaxX))) && (actionInfo.MinY == actionInfo.MaxY || (!(vector.y < (float)actionInfo.MinY) && !(vector.y > (float)actionInfo.MaxY))))
			{
				ProcessTouchAction(actionInfo);
			}
		}
	}

	public void ProcessTouchAction(ActionInfo actionInfo)
	{
		if (actionInfo == null)
		{
			return;
		}
		Vespa_System_Action_Extension_Method.fire(actionInfo.Action);
		if (actionInfo.Oneshot)
		{
			_touchActionList.Remove(actionInfo);
		}
		if (actionInfo.ProcessTouch)
		{
			GameObject gameObject = actionInfo.TargetGo;
			if (gameObject == null)
			{
				int x = (actionInfo.MinX + actionInfo.MaxX) / 2;
				int y = (actionInfo.MinY + actionInfo.MaxY) / 2;
				gameObject = GetCollidedUiObject(x, y);
			}
			ProcessTouchAction(gameObject);
		}
	}

	public static void ProcessTouchAction(GameObject touchTargetGo)
	{
		if (!(touchTargetGo == null))
		{
			if (UICamera.currentTouch == null)
			{
				UICamera.currentTouch = UICamera.GetMouse(0);
			}
			UICamera.currentTouch.current = touchTargetGo;
			UIEventTrigger.current = null;
			UICamera.Notify(touchTargetGo, UICamera.ENotifyType.OnPress, true);
			UICamera.Notify(touchTargetGo, UICamera.ENotifyType.OnClick, null);
			UICamera.Notify(touchTargetGo, UICamera.ENotifyType.OnPress, false);
		}
	}

	public GameObject GetCollidedUiObject(int x, int y)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(RaycastTargetLayer); i++)
		{
			GameObject collidedUiObject = GetCollidedUiObject(x, y, RaycastTargetLayer[i]);
			if (collidedUiObject != null)
			{
				return collidedUiObject;
			}
		}
		return null;
	}

	public GameObject GetCollidedUiObject(int x, int y, GameUIRoot.ELayerType layerType)
	{
		LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(layerType);
		if (layerInfo == null)
		{
			return null;
		}
		Camera camera = layerInfo.Camera;
		Vector2 uiSize = GetUiSize();
		Vector2 vector = new Vector2
		{
			x = (float)x / uiSize.x + 0.5f,
			y = (float)y / uiSize.y + 0.5f
		};
		Vector3 origin = Vespa_Unity_Vector3_Extension_Method.ReplaceZ(camera.ViewportToWorldPoint(vector), -10f);
		int layerMask = 1 << LayerMask.NameToLayer(layerInfo.LayerName);
		RaycastHit[] array = Physics.RaycastAll(new Ray(origin, camera.transform.forward), 9999f, layerMask);
		GameObject gameObject = null;
		int num = int.MinValue;
		if (__Extension_Method_ICollection__.COUNT(array) > 0)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array); i++)
			{
				RaycastHit raycastHit = array[i];
				int num2 = NGUITools.CalculateRaycastDepth(raycastHit.collider.gameObject);
				if (gameObject == null || num2 > num)
				{
					num = num2;
					gameObject = raycastHit.collider.gameObject;
				}
			}
			return gameObject;
		}
		return null;
	}

	public void ClearFocus(bool showFader)
	{
		FactoryBase.ReleaseAndClearList(_arrowComponentList);
		FactoryBase.ReleaseAndClearList(_focusComponentList);
		FactoryBase.ReleaseAndClearList(_effectComponentList);
		ResetFocusArea();
		ShowFocus(showFader);
	}

	public void ClearTooltip()
	{
		FactoryBase.ReleaseAndClearList(_arrowComponentList);
		FactoryBase.ReleaseAndClearList(_tooltipComponentList);
	}

	private TutorialFocusComponent CreateFocusComponent()
	{
		TutorialFocusComponent tutorialFocusComponent = TutorialFocusComponent.PrefabAuto.Factory.Create(TooltipRoot);
		tutorialFocusComponent.Open();
		Tools.RearrangeWidgetDepth(tutorialFocusComponent.transform, GetNextWidgetDepth());
		_focusComponentList.Add(tutorialFocusComponent);
		return tutorialFocusComponent;
	}

	private TutorialEffectComponent CreateEffectComponent()
	{
		TutorialEffectComponent tutorialEffectComponent = TutorialEffectComponent.PrefabAuto.Factory.Create(TooltipRoot);
		Tools.RearrangeWidgetDepth(tutorialEffectComponent.transform, GetNextWidgetDepth());
		_effectComponentList.Add(tutorialEffectComponent);
		return tutorialEffectComponent;
	}

	private TutorialTooltipComponent CreateTooltipComponent()
	{
		TutorialTooltipComponent tutorialTooltipComponent = TutorialTooltipComponent.PrefabAuto.Factory.Create(TooltipRoot);
		Tools.RearrangeWidgetDepth(tutorialTooltipComponent.transform, GetNextWidgetDepth());
		_tooltipComponentList.Add(tutorialTooltipComponent);
		return tutorialTooltipComponent;
	}

	private TutorialArrowComponent CreateArrowComponent()
	{
		TutorialArrowComponent tutorialArrowComponent = TutorialArrowComponent.PrefabAuto.Factory.Create(TooltipRoot);
		Tools.RearrangeWidgetDepth(tutorialArrowComponent.transform, GetNextWidgetDepth());
		_arrowComponentList.Add(tutorialArrowComponent);
		return tutorialArrowComponent;
	}

	private int GetNextWidgetDepth()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_focusComponentList) && __Extension_Method_ICollection__.IsNullOrEmpty(_effectComponentList) && __Extension_Method_ICollection__.IsNullOrEmpty(_tooltipComponentList) && __Extension_Method_ICollection__.IsNullOrEmpty(_arrowComponentList))
		{
			_lastWidgetDepth = 10;
			return _lastWidgetDepth;
		}
		_lastWidgetDepth += 10;
		return _lastWidgetDepth;
	}

	public void ShowTooltip(int minX, int minY, int maxX, int maxY, string text, string iconName, bool isAppend)
	{
		if (!IsSkipping())
		{
			if (!isAppend)
			{
				ClearTooltip();
			}
			CreateTooltipComponent().SetTooltipText(minX, minY, maxX, maxY, text, iconName);
		}
	}

	public void ShowTooltip2(UIWidget.Pivot pivot, int x, int y, int marginX, int marginY, string text, string iconName, bool isAppend)
	{
		if (!IsSkipping())
		{
			if (!isAppend)
			{
				ClearTooltip();
			}
			CreateTooltipComponent().SetTooltipText2(pivot, x, y, marginX, marginY, text, iconName);
		}
	}

	public void ShowTooltipArrow(Vector2 fromPivotOffset, Vector2 toPivotOffset)
	{
		if (!IsSkipping())
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_focusComponentList))
			{
				VespaDebug.LogError(Define.LogPrefix + " ShowTooltipArrow: Empty Focus");
				return;
			}
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_tooltipComponentList))
			{
				VespaDebug.LogError(Define.LogPrefix + " ShowTooltipArrow: Empty Tooltip");
				return;
			}
			TutorialTooltipComponent tutorialTooltipComponent = _tooltipComponentList[_tooltipComponentList.Count - 1];
			TutorialFocusComponent tutorialFocusComponent = _focusComponentList[_focusComponentList.Count - 1];
			int num = Mathf.RoundToInt(NVespa.Math.LerpOver(tutorialTooltipComponent.MinX, tutorialTooltipComponent.MaxX, fromPivotOffset.x));
			int num2 = Mathf.RoundToInt(NVespa.Math.LerpOver(tutorialTooltipComponent.MinY, tutorialTooltipComponent.MaxY, fromPivotOffset.y));
			int num3 = Mathf.RoundToInt(NVespa.Math.LerpOver(tutorialFocusComponent.MinX, tutorialFocusComponent.MaxX, toPivotOffset.x));
			int num4 = Mathf.RoundToInt(NVespa.Math.LerpOver(tutorialFocusComponent.MinY, tutorialFocusComponent.MaxY, toPivotOffset.y));
			CreateArrowComponent().SetTooltipArrow(new Vector3(num, num2), new Vector3(num3, num4));
		}
	}

	private void FillTexture(int x, int y, int width, int height, Color color)
	{
		Color32 color2 = color;
		Texture2D texture2D = FadeTexture.mainTexture as Texture2D;
		width += AdjustNagative(ref x);
		height += AdjustNagative(ref y);
		width = AdjustTextureSizeOverflow(x, width, texture2D.width);
		height = AdjustTextureSizeOverflow(y, height, texture2D.height);
		width = Mathf.Clamp(width, 0, texture2D.width);
		height = Mathf.Clamp(height, 0, texture2D.height);
		for (int i = 0; i < width * height; i++)
		{
			_colorBuffer[i] = color2;
		}
		texture2D.SetPixels32(x, y, width, height, _colorBuffer);
		texture2D.Apply();
	}

	private int AdjustNagative(ref int value)
	{
		if (value < 0)
		{
			int result = value;
			value = 0;
			return result;
		}
		return 0;
	}

	private int AdjustTextureSizeOverflow(int coordinate, int size, int sizeMax)
	{
		if (coordinate >= 0 && coordinate + size > sizeMax)
		{
			return sizeMax - coordinate;
		}
		return size;
	}

	private void FillSmoothTexture(int x, int y, int width, int height, int smoothThickness, Color color, Color backgroundColor)
	{
		Texture2D texture2D = FadeTexture.mainTexture as Texture2D;
		width = Mathf.Min(width, texture2D.width);
		height = Mathf.Min(height, texture2D.height);
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				int a = Mathf.Min(j, width - 1 - j);
				int b = Mathf.Min(i, height - 1 - i);
				float i_t = Mathf.Clamp01((float)Mathf.Min(a, b) / (float)smoothThickness);
				_colorBuffer[i * width + j] = Vespa_Unity_Color_Extension_Method.Lerp(backgroundColor, color, i_t);
			}
		}
		texture2D.SetPixels32(x, y, width, height, _colorBuffer);
		texture2D.Apply();
	}

	public void ResetFocusArea()
	{
		if ((bool)FadeTexture && (bool)FadeTexture.mainTexture)
		{
			FillTexture(0, 0, FadeTexture.mainTexture.width, FadeTexture.mainTexture.height, _darkColor);
		}
	}

	private TutorialFocusComponent GetEmptyFocusComponent()
	{
		return CreateFocusComponent();
	}

	public void FocusByUiPosition(int minX, int minY, int maxX, int maxY, bool isAppend)
	{
		if (!IsSkipping())
		{
			if (!isAppend)
			{
				ClearFocus(showFader: true);
			}
			else
			{
				ShowFocus(show: true);
			}
			Rect rect = Normalize(minX, minY, maxX, maxY);
			float num = (float)FadeTextureWidth * rect.xMin;
			float num2 = (float)FadeTextureHeight * rect.yMin;
			float num3 = rect.width * (float)FadeTextureWidth;
			float num4 = rect.height * (float)FadeTextureHeight;
			FillTexture((int)num, (int)num2, (int)num3, (int)num4, _emptyColor);
			CreateFocusComponent().SetFocusOutline(minX, minY, maxX, maxY);
		}
	}

	public void FocusEffectByUiPosition(int posX, int posY, float angle, float scale, string effectCode, bool isAppend)
	{
		if (!IsSkipping())
		{
			if (!isAppend)
			{
				ClearFocus(showFader: false);
			}
			TransformData transformData = new TransformData();
			transformData.Position = new Vector3(posX, posY, 0f);
			transformData.RotationEuler = new Vector3(0f, 0f, angle);
			if (Mathf.Approximately(scale, 0f))
			{
				transformData.Scale = 100f;
			}
			else
			{
				transformData.Scale = scale;
			}
			CreateEffectComponent().SetAssetEffect(effectCode, transformData);
		}
	}

	public void FocusByBoxCollider(BoxCollider boxCollider, int minOffsetX, int minOffsetY, int maxOffsetX, int maxOffsetY, bool isAppend)
	{
		CalcPositionByBoxCollider(boxCollider, minOffsetX, minOffsetY, maxOffsetX, maxOffsetY, out var minX, out var minY, out var maxX, out var maxY);
		FocusByUiPosition(minX, minY, maxX, maxY, isAppend);
	}

	public void FocusEffectByBoxCollider(BoxCollider boxCollider, string effectCode, int offsetX, int offsetY, float angle, float scale, bool isAppend)
	{
		CalcPositionByBoxCollider(boxCollider, 0, 0, 0, 0, out var minX, out var minY, out var maxX, out var maxY);
		int num = (minX + maxX) / 2;
		int num2 = (minY + maxY) / 2;
		FocusEffectByUiPosition(num + offsetX, num2 + offsetY, angle, scale, effectCode, isAppend);
	}

	public void CalcPositionByBoxCollider(BoxCollider boxCollider, int minOffsetX, int minOffsetY, int maxOffsetX, int maxOffsetY, out int minX, out int minY, out int maxX, out int maxY)
	{
		LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(boxCollider.gameObject.layer);
		Camera camera = ((layerInfo != null) ? layerInfo.Camera : GameUIRoot.instance.UI.Camera);
		Vector2 uiSize = GetUiSize();
		Vector3 min = boxCollider.bounds.min;
		Vector3 max = boxCollider.bounds.max;
		Vector3 vector = camera.WorldToViewportPoint(min);
		Vector3 vector2 = camera.WorldToViewportPoint(max);
		Vector3 lhs = new Vector3((vector.x - 0.5f) * uiSize.x, (vector.y - 0.5f) * uiSize.y);
		Vector3 rhs = new Vector3((vector2.x - 0.5f) * uiSize.x, (vector2.y - 0.5f) * uiSize.y);
		Vector3 vector3 = Vector3.Min(lhs, rhs);
		Vector3 vector4 = Vector3.Max(lhs, rhs);
		minX = (int)(vector3.x + (float)minOffsetX);
		maxX = (int)(vector4.x + (float)maxOffsetX);
		minY = (int)(vector3.y + (float)minOffsetY);
		maxY = (int)(vector4.y + (float)maxOffsetY);
	}

	public void FocusByUiWidget(UIWidget widget, int minOffsetX, int minOffsetY, int maxOffsetX, int maxOffsetY, bool isAppend)
	{
		CalcPositionByWidget(widget, minOffsetX, minOffsetY, maxOffsetX, maxOffsetY, out var minX, out var minY, out var maxX, out var maxY);
		FocusByUiPosition(minX, minY, maxX, maxY, isAppend);
	}

	public void FocusEffectByUiWidget(UIWidget widget, string effectCode, int offsetX, int offsetY, float angle, float scale, bool isAppend)
	{
		CalcPositionByWidget(widget, 0, 0, 0, 0, out var minX, out var minY, out var maxX, out var maxY);
		int num = (minX + maxX) / 2;
		int num2 = (minY + maxY) / 2;
		FocusEffectByUiPosition(num + offsetX, num2 + offsetY, angle, scale, effectCode, isAppend);
	}

	public void CalcPositionByWidget(UIWidget widget, int minOffsetX, int minOffsetY, int maxOffsetX, int maxOffsetY, out int minX, out int minY, out int maxX, out int maxY)
	{
		LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(widget.gameObject.layer);
		Camera camera = ((layerInfo != null) ? layerInfo.Camera : GameUIRoot.instance.UI.Camera);
		Vector2 uiSize = GetUiSize();
		Vector3[] worldCorners = widget.worldCorners;
		Vector3[] array = new Vector3[4];
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(worldCorners); i++)
		{
			Vector3 vector = camera.WorldToViewportPoint(worldCorners[i]);
			array[i] = new Vector3((vector.x - 0.5f) * uiSize.x, (vector.y - 0.5f) * uiSize.y);
		}
		Vector3 vector2 = Vector3.Min(Vector3.Min(array[0], array[1]), Vector3.Min(array[2], array[3]));
		Vector3 vector3 = Vector3.Max(Vector3.Max(array[0], array[1]), Vector3.Max(array[2], array[3]));
		minX = (int)(vector2.x + (float)minOffsetX);
		maxX = (int)(vector3.x + (float)maxOffsetX);
		minY = (int)(vector2.y + (float)minOffsetY);
		maxY = (int)(vector3.y + (float)maxOffsetY);
	}

	public void ShowFocus(bool show)
	{
		FadeTexture.enabled = show;
	}

	public void SetSkipCallback(Action callback)
	{
		_skipCallback = callback;
	}

	public void RemoveSkipCallback()
	{
		_skipCallback = null;
	}

	public void RemoveSkipStartTutorialData()
	{
		_skipStartTutorialData = null;
	}

	public void SetSkipStart(TutorialData skipStartTutorialData)
	{
		_skipStartTutorialData = skipStartTutorialData;
		_skipEndTutorialData = null;
		_skipType = TutorialUtility.GetArgEnum<SkipType>(skipStartTutorialData, 0);
	}

	public void ClearSkip()
	{
		_skipStartTutorialData = null;
		_skipEndTutorialData = null;
		OnSkipEnd();
	}

	public bool NeedShowSkipButton()
	{
		if (IsSkipping())
		{
			return false;
		}
		if (_skipCallback != null)
		{
			return true;
		}
		if (_skipStartTutorialData != null)
		{
			return true;
		}
		return false;
	}

	public bool IsSkipping()
	{
		return _skipEndTutorialData != null;
	}

	public SkipType GetSkipType()
	{
		return _skipType;
	}

	private void UpdateSkipButton()
	{
		bool flag = NeedShowSkipButton();
		if (flag != UIHelper.IsEnabledGameObjet(SkipButton))
		{
			UIHelper.EnableGameObject(SkipButton, flag);
		}
	}

	public void OnClickSkip()
	{
		if (_skipCallback != null)
		{
			Vespa_System_Action_Extension_Method.fire(_skipCallback);
		}
		else if (_skipStartTutorialData != null)
		{
			OnClickSkipAction();
			SkipTutorialToSkipEnd();
		}
	}

	private void OnClickSkipAction()
	{
		if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<MissionMain>())
		{
			MissionMain window = GlobalAutoBehaviour<GlobalManagement>.instance.GetWindow<MissionMain>();
			if (window != null)
			{
				window.Close2(immediately: true);
			}
		}
	}

	private void SkipTutorialToSkipEnd()
	{
		TutorialData skipStartTutorialData = _skipStartTutorialData;
		if (skipStartTutorialData == null)
		{
			VespaDebug.LogError("_skipStartTutorialData is null.");
			return;
		}
		VespaDebug.LogError("SkipTutorial. skipStart tutorialIndex=" + skipStartTutorialData.Index);
		TutorialData tutorialData = null;
		int num = skipStartTutorialData.Sequence + 1;
		while (true)
		{
			TutorialData data = GameRoot.Instance.TutorialDataContainer.GetData(skipStartTutorialData.Index, num);
			if (data == null)
			{
				break;
			}
			if (data.Command == typeof(SkipEnd).Name)
			{
				tutorialData = data;
				break;
			}
			num++;
		}
		if (tutorialData == null)
		{
			VespaDebug.LogError("skipEndTutorialData is null. skipStart tutorialIndex=" + skipStartTutorialData.Index);
			return;
		}
		_skipEndTutorialData = tutorialData;
		ForceRunTouchAction();
		if (_skipType == SkipType.Jump)
		{
			SingletonUpdater<TutorialManager>.instance.JumpToSkipEnd();
		}
		OnSkipStart();
	}

	private void OnSkipStart()
	{
		ClearFocus(showFader: false);
		ClearTooltip();
	}

	private void OnSkipEnd()
	{
		ClearFocus(showFader: false);
	}
}
