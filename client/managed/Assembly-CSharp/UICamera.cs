using System;
using System.Collections.Generic;
using NGame2;
using NNguiExtension;
using NVespa;
using NVespa.NNGUIExtension;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI Event System (UICamera)")]
[RequireComponent(typeof(Camera))]
public class UICamera : BehaviourUpdater
{
	public interface IOnPressListener
	{
		void OnPress(bool isPressed);
	}

	public interface IOnPressNoArgListener
	{
		void OnPress();
	}

	public interface IOnClickListener
	{
		void OnClick();
	}

	public interface IOnDoubleClickListener
	{
		void OnDoubleClick();
	}

	public interface IOnDragStartListener
	{
		void OnDragStart();
	}

	public interface IOnDragEndListener
	{
		void OnDragEnd();
	}

	public interface IOnDragListener
	{
		void OnDrag(Vector2 delta);
	}

	public interface IOnDragNoArgListener
	{
		void OnDrag();
	}

	public interface IOnDragOverListener
	{
		void OnDragOver(GameObject go);
	}

	public interface IOnDragOverNoArgListener
	{
		void OnDragOver();
	}

	public interface IOnDragOutListener
	{
		void OnDragOut(GameObject go);
	}

	public interface IOnDragOutNoArgListener
	{
		void OnDragOut();
	}

	public interface IOnDropListener
	{
		void OnDrop(GameObject go);
	}

	public enum ENotifyType
	{
		OnPress,
		OnClick,
		OnDoubleClick,
		OnDragStart,
		OnDragEnd,
		OnDrag,
		OnDragOver,
		OnDragOut,
		OnDrop,
		OnHover,
		OnSelect,
		OnScroll,
		OnTooltip,
		OnKey
	}

	public enum ControlScheme
	{
		Mouse,
		Touch,
		Controller
	}

	public enum ClickNotification
	{
		None,
		Always,
		BasedOnDelta
	}

	public class MouseOrTouch
	{
		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 delta;

		public Vector2 totalDelta;

		public Camera pressedCam;

		public GameObject last;

		public GameObject current;

		public GameObject pressed;

		public GameObject dragged;

		public float pressTime;

		public float clickTime;

		public ClickNotification clickNotification = ClickNotification.Always;

		public bool touchBegan = true;

		public bool pressStarted;

		public bool dragStarted;

		public float deltaTime
		{
			get
			{
				if (!touchBegan)
				{
					return 0f;
				}
				return RealTime.time - pressTime;
			}
		}

		public bool isOverUI
		{
			get
			{
				if (current != null && current != fallThrough)
				{
					return NGUITools.FindInParents<UIRoot>(current) != null;
				}
				return false;
			}
		}

		public void OnSceneChanged()
		{
			if (Vespa_Common_Class_Extension_Method.IsUnityNull(pressedCam))
			{
				pressedCam = null;
			}
			if (Vespa_Common_Class_Extension_Method.IsUnityNull(last))
			{
				last = null;
			}
			if (Vespa_Common_Class_Extension_Method.IsUnityNull(current))
			{
				current = null;
			}
			if (Vespa_Common_Class_Extension_Method.IsUnityNull(pressed))
			{
				pressed = null;
			}
			if (Vespa_Common_Class_Extension_Method.IsUnityNull(dragged))
			{
				dragged = null;
			}
		}
	}

	public enum EventType
	{
		World_3D,
		UI_3D,
		World_2D,
		UI_2D
	}

	public delegate bool GetKeyStateFunc(KeyCode key);

	public delegate float GetAxisFunc(string name);

	public delegate void OnScreenResize();

	public delegate void OnCustomInput();

	public delegate void MoveDelegate(Vector2 delta);

	public delegate void VoidDelegate(GameObject go);

	public delegate void BoolDelegate(GameObject go, bool state);

	public delegate void FloatDelegate(GameObject go, float delta);

	public delegate void VectorDelegate(GameObject go, Vector2 delta);

	public delegate void ObjectDelegate(GameObject go, GameObject obj);

	public delegate void KeyCodeDelegate(GameObject go, KeyCode key);

	private struct DepthEntry
	{
		public int depth;

		public RaycastHit hit;

		public Vector3 point;

		public GameObject go;
	}

	public class Touch
	{
		public int fingerId;

		public TouchPhase phase;

		public Vector2 position;

		public int tapCount;
	}

	public delegate int GetTouchCountCallback();

	public delegate Touch GetTouchCallback(int index);

	public static bool DisableEventHandler = false;

	public static List<UICamera> list = new List<UICamera>();

	public static GetKeyStateFunc GetKeyDown = Input.GetKeyDown;

	public static GetKeyStateFunc GetKeyUp = Input.GetKeyUp;

	public static GetKeyStateFunc GetKey = Input.GetKey;

	public static GetAxisFunc GetAxis = Input.GetAxis;

	public static OnScreenResize onScreenResize;

	public EventType eventType = EventType.UI_3D;

	public bool eventsGoToColliders;

	public LayerMask eventReceiverMask = -1;

	public bool debug;

	public bool useMouse = true;

	public bool useTouch = true;

	public bool allowMultiTouch = true;

	public bool useKeyboard = true;

	public bool useController = true;

	public bool stickyTooltip = true;

	public float tooltipDelay = 1f;

	public float mouseDragThreshold = 4f;

	public float mouseClickThreshold = 10f;

	public float touchDragThreshold = 40f;

	public float touchClickThreshold = 40f;

	public float rangeDistance = -1f;

	public string scrollAxisName = "Mouse ScrollWheel";

	public string verticalAxisName = "Vertical";

	public string horizontalAxisName = "Horizontal";

	public bool commandClick = true;

	public KeyCode submitKey0 = KeyCode.Return;

	public KeyCode submitKey1 = KeyCode.JoystickButton0;

	public KeyCode cancelKey0 = KeyCode.Escape;

	public KeyCode cancelKey1 = KeyCode.JoystickButton1;

	public static OnCustomInput onCustomInput;

	public static bool showTooltips = true;

	public static Vector2 lastTouchPosition = Vector2.zero;

	public static Vector3 lastWorldPosition = Vector3.zero;

	public static RaycastHit lastHit;

	public static UICamera current = null;

	public static Camera currentCamera = null;

	public static ControlScheme currentScheme = ControlScheme.Mouse;

	public static int currentTouchID = -100;

	public static KeyCode currentKey = KeyCode.None;

	public static MouseOrTouch currentTouch = null;

	public static bool inputHasFocus = false;

	private static GameObject mGenericHandler;

	public static GameObject fallThrough;

	public static VoidDelegate onClick;

	public static VoidDelegate onDoubleClick;

	public static BoolDelegate onHover;

	public static BoolDelegate onPress;

	public static BoolDelegate onSelect;

	public static FloatDelegate onScroll;

	public static VectorDelegate onDrag;

	public static VoidDelegate onDragStart;

	public static ObjectDelegate onDragOver;

	public static ObjectDelegate onDragOut;

	public static VoidDelegate onDragEnd;

	public static ObjectDelegate onDrop;

	public static KeyCodeDelegate onKey;

	public static BoolDelegate onTooltip;

	public static MoveDelegate onMouseMove;

	private static GameObject mCurrentSelection = null;

	private static MouseOrTouch[] mMouse = new MouseOrTouch[3]
	{
		new MouseOrTouch(),
		new MouseOrTouch(),
		new MouseOrTouch()
	};

	private static GameObject mHover;

	public static MouseOrTouch controller = new MouseOrTouch();

	private static float mNextEvent = 0f;

	private static Dictionary<int, MouseOrTouch> mTouches = new Dictionary<int, MouseOrTouch>();

	private static int mWidth = 0;

	private static int mHeight = 0;

	private GameObject mTooltip;

	private Camera mCam;

	private float mTooltipTime;

	private float mNextRaycast;

	public static bool isDragging = false;

	public static GameObject hoveredObject;

	private static DepthEntry mHit = default(DepthEntry);

	private static BetterList<DepthEntry> mHits = new BetterList<DepthEntry>();

	private static Plane m2DPlane = new Plane(Vector3.back, 0f);

	private static int mNotifying = 0;

	private static bool mUsingTouchEvents = true;

	public static GetTouchCountCallback GetInputTouchCount;

	public static GetTouchCallback GetInputTouch;

	[Obsolete("Use new OnDragStart / OnDragOver / OnDragOut / OnDragEnd events instead")]
	public bool stickyPress => true;

	public static Ray currentRay
	{
		get
		{
			if (!(currentCamera != null) || currentTouch == null)
			{
				return default(Ray);
			}
			return Vespa_Unity_Camera_Extension_Method.VespaScreenPointToRay(currentCamera, currentTouch.pos);
		}
	}

	[Obsolete("Use delegates instead such as UICamera.onClick, UICamera.onHover, etc.")]
	public static GameObject genericEventHandler
	{
		get
		{
			return mGenericHandler;
		}
		set
		{
			mGenericHandler = value;
		}
	}

	private bool handlesEvents
	{
		get
		{
			if (DisableEventHandler)
			{
				return false;
			}
			return eventHandler == this;
		}
	}

	public Camera cachedCamera
	{
		get
		{
			if (mCam == null)
			{
				mCam = GetComponent<Camera>();
			}
			return mCam;
		}
	}

	public static bool isOverUI
	{
		get
		{
			if (currentTouch != null)
			{
				return currentTouch.isOverUI;
			}
			if (hoveredObject == null)
			{
				return false;
			}
			if (hoveredObject == fallThrough)
			{
				return false;
			}
			return NGUITools.FindInParents<UIRoot>(hoveredObject) != null;
		}
	}

	public static GameObject selectedObject
	{
		get
		{
			if ((bool)mCurrentSelection)
			{
				return mCurrentSelection;
			}
			return null;
		}
		set
		{
			if (mCurrentSelection == value)
			{
				return;
			}
			bool flag = false;
			if (currentTouch == null)
			{
				flag = true;
				currentTouchID = -1;
				currentTouch = mMouse[0];
				currentScheme = ControlScheme.Mouse;
			}
			inputHasFocus = false;
			if (onSelect != null)
			{
				onSelect(selectedObject, state: false);
			}
			Notify(mCurrentSelection, ENotifyType.OnSelect, false);
			mCurrentSelection = value;
			if (mCurrentSelection != null)
			{
				if (flag)
				{
					UICamera uICamera = ((mCurrentSelection != null) ? FindCameraForLayer(mCurrentSelection.layer) : list[0]);
					if (uICamera != null)
					{
						current = uICamera;
						currentCamera = uICamera.cachedCamera;
					}
				}
				inputHasFocus = mCurrentSelection.activeInHierarchy && mCurrentSelection.GetComponent<UIInput>() != null;
				if (onSelect != null)
				{
					onSelect(mCurrentSelection, state: true);
				}
				Notify(mCurrentSelection, ENotifyType.OnSelect, true);
			}
			if (flag)
			{
				current = null;
				currentCamera = null;
				currentTouch = null;
				currentTouchID = -100;
			}
		}
	}

	public static int touchCount
	{
		get
		{
			int num = 0;
			foreach (KeyValuePair<int, MouseOrTouch> mTouch in mTouches)
			{
				if (mTouch.Value.pressed != null)
				{
					num++;
				}
			}
			for (int i = 0; i < mMouse.Length; i++)
			{
				if (mMouse[i].pressed != null)
				{
					num++;
				}
			}
			if (controller.pressed != null)
			{
				num++;
			}
			return num;
		}
	}

	public static int dragCount
	{
		get
		{
			int num = 0;
			foreach (KeyValuePair<int, MouseOrTouch> mTouch in mTouches)
			{
				if (mTouch.Value.dragged != null)
				{
					num++;
				}
			}
			for (int i = 0; i < mMouse.Length; i++)
			{
				if (mMouse[i].dragged != null)
				{
					num++;
				}
			}
			if (controller.dragged != null)
			{
				num++;
			}
			return num;
		}
	}

	public static Camera mainCamera
	{
		get
		{
			UICamera uICamera = eventHandler;
			if (!(uICamera != null))
			{
				return null;
			}
			return uICamera.cachedCamera;
		}
	}

	public static UICamera eventHandler
	{
		get
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
			{
				UICamera uICamera = list[i];
				if (!(uICamera == null) && uICamera.enabled && NGUITools.GetActive(uICamera.gameObject))
				{
					return uICamera;
				}
			}
			return null;
		}
	}

	public static void OnSceneChanged()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			if (Vespa_Common_Class_Extension_Method.IsUnityNull(list[i]))
			{
				list.RemoveAt(i);
				i--;
			}
		}
		if (Vespa_Common_Class_Extension_Method.IsUnityNull(current))
		{
			current = null;
		}
		if (Vespa_Common_Class_Extension_Method.IsUnityNull(currentCamera))
		{
			currentCamera = null;
		}
		if (Vespa_Common_Class_Extension_Method.IsUnityNull(fallThrough))
		{
			fallThrough = null;
		}
		if (Vespa_Common_Class_Extension_Method.IsUnityNull(mCurrentSelection))
		{
			mCurrentSelection = null;
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(mMouse); j++)
		{
			mMouse[j]?.OnSceneChanged();
		}
		if (Vespa_Common_Class_Extension_Method.IsUnityNull(mHover))
		{
			mHover = null;
		}
		if (controller != null)
		{
			controller.OnSceneChanged();
		}
		foreach (KeyValuePair<int, MouseOrTouch> mTouch in mTouches)
		{
			mTouch.Value?.OnSceneChanged();
		}
		if (hoveredObject != null)
		{
			hoveredObject = null;
		}
		mHit.go = null;
		mHits.Release();
	}

	public static bool IsPressed(GameObject go)
	{
		for (int i = 0; i < 3; i++)
		{
			if (mMouse[i].pressed == go)
			{
				return true;
			}
		}
		foreach (KeyValuePair<int, MouseOrTouch> mTouch in mTouches)
		{
			if (mTouch.Value.pressed == go)
			{
				return true;
			}
		}
		if (controller.pressed == go)
		{
			return true;
		}
		return false;
	}

	public static bool IsDragged(GameObject go)
	{
		for (int i = 0; i < 3; i++)
		{
			if (mMouse[i].dragged == go)
			{
				return true;
			}
		}
		foreach (KeyValuePair<int, MouseOrTouch> mTouch in mTouches)
		{
			if (mTouch.Value.dragged == go)
			{
				return true;
			}
		}
		if (controller.dragged == go)
		{
			return true;
		}
		return false;
	}

	public static List<GameObject> GetDraggedGo()
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < 3; i++)
		{
			if (mMouse[i].dragged != null)
			{
				list.Add(mMouse[i].dragged);
			}
		}
		foreach (KeyValuePair<int, MouseOrTouch> mTouch in mTouches)
		{
			if (mTouch.Value.dragged != null)
			{
				list.Add(mTouch.Value.dragged);
			}
		}
		if (controller.dragged != null)
		{
			list.Add(controller.dragged);
		}
		return list;
	}

	public static int CompareFunc(UICamera a, UICamera b)
	{
		if (a.cachedCamera.depth < b.cachedCamera.depth)
		{
			return 1;
		}
		if (a.cachedCamera.depth > b.cachedCamera.depth)
		{
			return -1;
		}
		return 0;
	}

	private static Rigidbody FindRootRigidbody(Transform trans)
	{
		while (trans != null)
		{
			if (trans.GetComponent<UIPanel>() != null)
			{
				return null;
			}
			Rigidbody component = trans.GetComponent<Rigidbody>();
			if (component != null)
			{
				return component;
			}
			trans = trans.parent;
		}
		return null;
	}

	private static Rigidbody2D FindRootRigidbody2D(Transform trans)
	{
		while (trans != null)
		{
			if (trans.GetComponent<UIPanel>() != null)
			{
				return null;
			}
			Rigidbody2D component = trans.GetComponent<Rigidbody2D>();
			if (component != null)
			{
				return component;
			}
			trans = trans.parent;
		}
		return null;
	}

	public static bool Raycast(Vector3 inPos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			UICamera uICamera = list[i];
			if (!uICamera.enabled || !NGUITools.GetActive(uICamera.gameObject))
			{
				continue;
			}
			currentCamera = uICamera.cachedCamera;
			Vector3 vector = Vespa_Unity_Camera_Extension_Method.VespaScreenToViewportPoint(currentCamera, inPos);
			if (float.IsNaN(vector.x) || float.IsNaN(vector.y) || vector.x < 0f || vector.x > 1f || vector.y < 0f || vector.y > 1f)
			{
				continue;
			}
			Ray ray = Vespa_Unity_Camera_Extension_Method.VespaScreenPointToRay(currentCamera, inPos);
			int layerMask = currentCamera.cullingMask & (int)uICamera.eventReceiverMask;
			float enter = ((uICamera.rangeDistance > 0f) ? uICamera.rangeDistance : (currentCamera.farClipPlane - currentCamera.nearClipPlane));
			if (uICamera.eventType == EventType.World_3D)
			{
				if (!Physics.Raycast(ray, out lastHit, enter, layerMask))
				{
					continue;
				}
				lastWorldPosition = lastHit.point;
				hoveredObject = lastHit.collider.gameObject;
				if (!list[0].eventsGoToColliders)
				{
					Rigidbody rigidbody = FindRootRigidbody(hoveredObject.transform);
					if (rigidbody != null)
					{
						hoveredObject = rigidbody.gameObject;
					}
				}
				return true;
			}
			if (uICamera.eventType == EventType.UI_3D)
			{
				RaycastHit[] array = Physics.RaycastAll(ray, enter, layerMask);
				if (array.Length > 1)
				{
					for (int j = 0; j < array.Length; j++)
					{
						GameObject gameObject = array[j].collider.gameObject;
						UIWidget component = gameObject.GetComponent<UIWidget>();
						if (component != null)
						{
							if (!component.isVisible || (component.hitCheck != null && !component.hitCheck(array[j].point)))
							{
								continue;
							}
						}
						else
						{
							UIRect uIRect = NGUITools.FindInParents<UIRect>(gameObject);
							if (uIRect != null && uIRect.finalAlpha < 0.001f)
							{
								continue;
							}
						}
						mHit.depth = NGUITools.CalculateRaycastDepth(gameObject);
						if (mHit.depth != int.MaxValue)
						{
							mHit.hit = array[j];
							mHit.point = array[j].point;
							mHit.go = array[j].collider.gameObject;
							mHits.Add(mHit);
						}
					}
					mHits.Sort((DepthEntry r1, DepthEntry r2) => r2.depth.CompareTo(r1.depth));
					for (int num = 0; num < mHits.size; num++)
					{
						if (IsVisible(ref mHits.buffer[num]))
						{
							lastHit = mHits[num].hit;
							hoveredObject = mHits[num].go;
							lastWorldPosition = mHits[num].point;
							mHits.Clear();
							return true;
						}
					}
					mHits.Clear();
				}
				else
				{
					if (array.Length != 1)
					{
						continue;
					}
					GameObject gameObject2 = array[0].collider.gameObject;
					UIWidget component2 = gameObject2.GetComponent<UIWidget>();
					if (component2 != null)
					{
						if (!component2.isVisible || (component2.hitCheck != null && !component2.hitCheck(array[0].point)))
						{
							continue;
						}
					}
					else
					{
						UIRect uIRect2 = NGUITools.FindInParents<UIRect>(gameObject2);
						if (uIRect2 != null && uIRect2.finalAlpha < 0.001f)
						{
							continue;
						}
					}
					if (IsVisible(array[0].point, array[0].collider.gameObject))
					{
						lastHit = array[0];
						lastWorldPosition = array[0].point;
						hoveredObject = lastHit.collider.gameObject;
						return true;
					}
				}
			}
			else
			{
				if (uICamera.eventType == EventType.World_2D)
				{
					if (!m2DPlane.Raycast(ray, out enter))
					{
						continue;
					}
					Vector3 point = ray.GetPoint(enter);
					Collider2D collider2D = Physics2D.OverlapPoint(point, layerMask);
					if (!collider2D)
					{
						continue;
					}
					lastWorldPosition = point;
					hoveredObject = collider2D.gameObject;
					if (!uICamera.eventsGoToColliders)
					{
						Rigidbody2D rigidbody2D = FindRootRigidbody2D(hoveredObject.transform);
						if (rigidbody2D != null)
						{
							hoveredObject = rigidbody2D.gameObject;
						}
					}
					return true;
				}
				if (uICamera.eventType != EventType.UI_2D || !m2DPlane.Raycast(ray, out enter))
				{
					continue;
				}
				lastWorldPosition = ray.GetPoint(enter);
				Collider2D[] array2 = Physics2D.OverlapPointAll(lastWorldPosition, layerMask);
				if (array2.Length > 1)
				{
					for (int num2 = 0; num2 < array2.Length; num2++)
					{
						GameObject gameObject3 = array2[num2].gameObject;
						UIWidget component3 = gameObject3.GetComponent<UIWidget>();
						if (component3 != null)
						{
							if (!component3.isVisible || (component3.hitCheck != null && !component3.hitCheck(lastWorldPosition)))
							{
								continue;
							}
						}
						else
						{
							UIRect uIRect3 = NGUITools.FindInParents<UIRect>(gameObject3);
							if (uIRect3 != null && uIRect3.finalAlpha < 0.001f)
							{
								continue;
							}
						}
						mHit.depth = NGUITools.CalculateRaycastDepth(gameObject3);
						if (mHit.depth != int.MaxValue)
						{
							mHit.go = gameObject3;
							mHit.point = lastWorldPosition;
							mHits.Add(mHit);
						}
					}
					mHits.Sort((DepthEntry r1, DepthEntry r2) => r2.depth.CompareTo(r1.depth));
					for (int num3 = 0; num3 < mHits.size; num3++)
					{
						if (IsVisible(ref mHits.buffer[num3]))
						{
							hoveredObject = mHits[num3].go;
							mHits.Clear();
							return true;
						}
					}
					mHits.Clear();
				}
				else
				{
					if (array2.Length != 1)
					{
						continue;
					}
					GameObject gameObject4 = array2[0].gameObject;
					UIWidget component4 = gameObject4.GetComponent<UIWidget>();
					if (component4 != null)
					{
						if (!component4.isVisible || (component4.hitCheck != null && !component4.hitCheck(lastWorldPosition)))
						{
							continue;
						}
					}
					else
					{
						UIRect uIRect4 = NGUITools.FindInParents<UIRect>(gameObject4);
						if (uIRect4 != null && uIRect4.finalAlpha < 0.001f)
						{
							continue;
						}
					}
					if (IsVisible(lastWorldPosition, gameObject4))
					{
						hoveredObject = gameObject4;
						return true;
					}
				}
			}
		}
		return false;
	}

	private static bool IsVisible(Vector3 worldPoint, GameObject go)
	{
		UIPanel uIPanel = NGUITools.FindInParents<UIPanel>(go);
		while (uIPanel != null)
		{
			if (!uIPanel.IsVisible(worldPoint))
			{
				return false;
			}
			uIPanel = uIPanel.parentPanel;
		}
		return true;
	}

	private static bool IsVisible(ref DepthEntry de)
	{
		UIPanel uIPanel = NGUITools.FindInParents<UIPanel>(de.go);
		while (uIPanel != null)
		{
			if (!uIPanel.IsVisible(de.point))
			{
				return false;
			}
			uIPanel = uIPanel.parentPanel;
		}
		return true;
	}

	public static bool IsHighlighted(GameObject go)
	{
		if (currentScheme == ControlScheme.Mouse)
		{
			return hoveredObject == go;
		}
		if (currentScheme == ControlScheme.Controller)
		{
			return selectedObject == go;
		}
		return false;
	}

	public static UICamera FindCameraForLayer(int layer)
	{
		int num = 1 << layer;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			UICamera uICamera = list[i];
			Camera camera = uICamera.cachedCamera;
			if (camera != null && (camera.cullingMask & num) != 0)
			{
				return uICamera;
			}
		}
		return null;
	}

	private static int GetDirection(KeyCode up, KeyCode down)
	{
		if (GetKeyDown(up))
		{
			return 1;
		}
		if (GetKeyDown(down))
		{
			return -1;
		}
		return 0;
	}

	private static int GetDirection(KeyCode up0, KeyCode up1, KeyCode down0, KeyCode down1)
	{
		if (GetKeyDown(up0) || GetKeyDown(up1))
		{
			return 1;
		}
		if (GetKeyDown(down0) || GetKeyDown(down1))
		{
			return -1;
		}
		return 0;
	}

	private static int GetDirection(string axis)
	{
		float time = RealTime.time;
		if (mNextEvent < time && !string.IsNullOrEmpty(axis))
		{
			float num = GetAxis(axis);
			if (num > 0.75f)
			{
				mNextEvent = time + 0.25f;
				return 1;
			}
			if (num < -0.75f)
			{
				mNextEvent = time + 0.25f;
				return -1;
			}
		}
		return 0;
	}

	public static void Notify(GameObject go, ENotifyType notifyType, object obj)
	{
		NotifyInternal(go, notifyType, obj);
	}

	private static void NotifyInternal(GameObject go, ENotifyType notifyType, object obj)
	{
		if (go == null)
		{
			return;
		}
		switch (notifyType)
		{
		case ENotifyType.OnPress:
		{
			IOnPressListener[] components9 = go.GetComponents<IOnPressListener>();
			IOnPressNoArgListener[] components10 = go.GetComponents<IOnPressNoArgListener>();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(components9) || !__Extension_Method_ICollection__.IsNullOrEmpty(components10))
			{
				bool isPressed = obj is bool && (bool)obj;
				for (int num3 = 0; num3 < __Extension_Method_ICollection__.COUNT(components9); num3++)
				{
					components9[num3].OnPress(isPressed);
				}
				for (int num4 = 0; num4 < __Extension_Method_ICollection__.COUNT(components10); num4++)
				{
					components10[num4].OnPress();
				}
				return;
			}
			break;
		}
		case ENotifyType.OnClick:
		{
			IOnClickListener[] components11 = go.GetComponents<IOnClickListener>();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(components11))
			{
				for (int num5 = 0; num5 < __Extension_Method_ICollection__.COUNT(components11); num5++)
				{
					components11[num5].OnClick();
				}
				return;
			}
			break;
		}
		case ENotifyType.OnDoubleClick:
		{
			IOnDoubleClickListener[] components4 = go.GetComponents<IOnDoubleClickListener>();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(components4))
			{
				for (int l = 0; l < __Extension_Method_ICollection__.COUNT(components4); l++)
				{
					components4[l].OnDoubleClick();
				}
				return;
			}
			break;
		}
		case ENotifyType.OnDragStart:
		{
			IOnDragStartListener[] components8 = go.GetComponents<IOnDragStartListener>();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(components8))
			{
				for (int num2 = 0; num2 < __Extension_Method_ICollection__.COUNT(components8); num2++)
				{
					components8[num2].OnDragStart();
				}
				return;
			}
			break;
		}
		case ENotifyType.OnDragEnd:
		{
			IOnDragEndListener[] components7 = go.GetComponents<IOnDragEndListener>();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(components7))
			{
				for (int num = 0; num < __Extension_Method_ICollection__.COUNT(components7); num++)
				{
					components7[num].OnDragEnd();
				}
				return;
			}
			break;
		}
		case ENotifyType.OnDrag:
		{
			IOnDragListener[] components12 = go.GetComponents<IOnDragListener>();
			IOnDragNoArgListener[] components13 = go.GetComponents<IOnDragNoArgListener>();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(components12) || !__Extension_Method_ICollection__.IsNullOrEmpty(components13))
			{
				Vector2 delta = ((obj is Vector2) ? ((Vector2)obj) : Vector2.zero);
				for (int num6 = 0; num6 < __Extension_Method_ICollection__.COUNT(components12); num6++)
				{
					components12[num6].OnDrag(delta);
				}
				for (int num7 = 0; num7 < __Extension_Method_ICollection__.COUNT(components13); num7++)
				{
					components13[num7].OnDrag();
				}
				return;
			}
			break;
		}
		case ENotifyType.OnDragOver:
		{
			IOnDragOverListener[] components2 = go.GetComponents<IOnDragOverListener>();
			IOnDragOverNoArgListener[] components3 = go.GetComponents<IOnDragOverNoArgListener>();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(components2) || !__Extension_Method_ICollection__.IsNullOrEmpty(components3))
			{
				for (int j = 0; j < __Extension_Method_ICollection__.COUNT(components2); j++)
				{
					components2[j].OnDragOver(obj as GameObject);
				}
				for (int k = 0; k < __Extension_Method_ICollection__.COUNT(components3); k++)
				{
					components3[k].OnDragOver();
				}
				return;
			}
			break;
		}
		case ENotifyType.OnDragOut:
		{
			IOnDragOutListener[] components5 = go.GetComponents<IOnDragOutListener>();
			IOnDragOutNoArgListener[] components6 = go.GetComponents<IOnDragOutNoArgListener>();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(components5) || !__Extension_Method_ICollection__.IsNullOrEmpty(components6))
			{
				for (int m = 0; m < __Extension_Method_ICollection__.COUNT(components5); m++)
				{
					components5[m].OnDragOut(obj as GameObject);
				}
				for (int n = 0; n < __Extension_Method_ICollection__.COUNT(components6); n++)
				{
					components6[n].OnDragOut();
				}
				return;
			}
			break;
		}
		case ENotifyType.OnDrop:
		{
			IOnDropListener[] components = go.GetComponents<IOnDropListener>();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(components))
			{
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(components); i++)
				{
					components[i].OnDrop(obj as GameObject);
				}
				return;
			}
			break;
		}
		}
		NotifyInternal(go, EnumTools.GetName(notifyType), obj);
	}

	private static void NotifyInternal(GameObject go, string funcName, object obj)
	{
		if (mNotifying <= 10 && NGUITools.GetActive(go))
		{
			mNotifying++;
			Vespa_Unity_GameObject_Extension_Method.SENDMESSAGE(go, funcName, obj, SendMessageOptions.DontRequireReceiver);
			if (mGenericHandler != null && mGenericHandler != go)
			{
				Vespa_Unity_GameObject_Extension_Method.SENDMESSAGE(mGenericHandler, funcName, obj, SendMessageOptions.DontRequireReceiver);
			}
			mNotifying--;
		}
	}

	public static MouseOrTouch GetMouse(int button)
	{
		return mMouse[button];
	}

	public static MouseOrTouch GetTouch(int id)
	{
		MouseOrTouch value = null;
		if (id < 0)
		{
			return GetMouse(-id - 1);
		}
		if (!mTouches.TryGetValue(id, out value))
		{
			value = new MouseOrTouch();
			value.pressTime = RealTime.time;
			value.touchBegan = true;
			mTouches.Add(id, value);
		}
		return value;
	}

	public static void RemoveTouch(int id)
	{
		mTouches.Remove(id);
	}

	protected override void Awake()
	{
		SprkIllustrationUpscale.EnsureProbe();
		SprkPetPresentation.EnsureProbe();
		SprkFreeCamera.EnsureProbe();
		SprkLoginSettings.EnsureProbe();
		SystemOptionData.SprkRestoreSavedSettings();
		SprkConquest.Tick();
		SprkDispatch.Ensure();
		SprkAutomation.Ensure();
		base.Awake();
		mWidth = Screen.width;
		mHeight = Screen.height;
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.WP8Player || Application.platform == RuntimePlatform.BlackBerryPlayer)
		{
			useTouch = true;
			useMouse = false;
			useKeyboard = false;
			useController = false;
		}
		else if (Application.platform == RuntimePlatform.PS3 || Application.platform == RuntimePlatform.XBOX360)
		{
			useMouse = false;
			useTouch = false;
			useKeyboard = false;
			useController = true;
		}
		mMouse[0].pos = VespaInput.mousePosition;
		for (int i = 1; i < 3; i++)
		{
			mMouse[i].pos = mMouse[0].pos;
			mMouse[i].lastPos = mMouse[0].pos;
		}
		lastTouchPosition = mMouse[0].pos;
		SprkGraphicsOptions.Ensure();
	}

	private void OnEnable()
	{
		list.Add(this);
		list.Sort(CompareFunc);
	}

	private void OnDisable()
	{
		list.Remove(this);
	}

	private void Start()
	{
		if (eventType != EventType.World_3D && cachedCamera.transparencySortMode != TransparencySortMode.Orthographic)
		{
			cachedCamera.transparencySortMode = TransparencySortMode.Orthographic;
		}
		if (Application.isPlaying)
		{
			if (fallThrough == null)
			{
				UIRoot uIRoot = NGUITools.FindInParents<UIRoot>(base.gameObject);
				if (uIRoot != null)
				{
					fallThrough = uIRoot.gameObject;
				}
				else
				{
					Transform transform = base.transform;
					fallThrough = ((transform.parent != null) ? transform.parent.gameObject : base.gameObject);
				}
			}
			cachedCamera.eventMask = 0;
		}
		if (handlesEvents)
		{
			NGUIDebug.debugRaycast = debug;
		}
	}

	private void OnPreRender()
	{
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
		if (handlesEvents)
		{
			ChainUpdateInternal();
		}
	}

	private void ChainUpdateInternal()
	{
		current = this;
		if (useTouch)
		{
			ProcessTouches();
		}
		else if (useMouse)
		{
			ProcessMouse();
		}
		if (onCustomInput != null)
		{
			onCustomInput();
		}
		if (useMouse && mCurrentSelection != null)
		{
			if (cancelKey0 != KeyCode.None && GetKeyDown(cancelKey0))
			{
				currentScheme = ControlScheme.Controller;
				currentKey = cancelKey0;
				selectedObject = null;
			}
			else if (cancelKey1 != KeyCode.None && GetKeyDown(cancelKey1))
			{
				currentScheme = ControlScheme.Controller;
				currentKey = cancelKey1;
				selectedObject = null;
			}
		}
		if (mCurrentSelection == null)
		{
			inputHasFocus = false;
		}
		else if (!mCurrentSelection || !mCurrentSelection.activeInHierarchy)
		{
			inputHasFocus = false;
			mCurrentSelection = null;
		}
		if ((useKeyboard || useController) && mCurrentSelection != null)
		{
			ProcessOthers();
		}
		if (useMouse && mHover != null)
		{
			float num = ((!string.IsNullOrEmpty(scrollAxisName)) ? GetAxis(scrollAxisName) : 0f);
			if (num != 0f)
			{
				if (onScroll != null)
				{
					onScroll(mHover, num);
				}
				Notify(mHover, ENotifyType.OnScroll, num);
			}
			if (showTooltips && mTooltipTime != 0f && (mTooltipTime < RealTime.time || GetKey(KeyCode.LeftShift) || GetKey(KeyCode.RightShift)))
			{
				mTooltip = mHover;
				currentTouch = mMouse[0];
				currentTouchID = -1;
				ShowTooltip(val: true);
			}
		}
		current = null;
		currentTouchID = -100;
	}

	public override void ChainLateUpdate()
	{
		base.ChainLateUpdate();
		if (!handlesEvents)
		{
			return;
		}
		int width = Screen.width;
		int height = Screen.height;
		if (width != mWidth || height != mHeight)
		{
			mWidth = width;
			mHeight = height;
			UIRoot.Broadcast("UpdateAnchors");
			if (onScreenResize != null)
			{
				onScreenResize();
			}
		}
	}

	public void ProcessMouse()
	{
		lastTouchPosition = VespaInput.mousePosition;
		mMouse[0].delta = lastTouchPosition - mMouse[0].pos;
		mMouse[0].pos = lastTouchPosition;
		bool flag = mMouse[0].delta.sqrMagnitude > 0.001f;
		for (int i = 1; i < 3; i++)
		{
			mMouse[i].pos = mMouse[0].pos;
			mMouse[i].delta = mMouse[0].delta;
		}
		bool flag2 = false;
		bool flag3 = false;
		for (int j = 0; j < 3; j++)
		{
			if (VespaInput.GetMouseButtonDown(j))
			{
				currentScheme = ControlScheme.Mouse;
				flag3 = true;
				flag2 = true;
			}
			else if (VespaInput.GetMouseButton(j))
			{
				currentScheme = ControlScheme.Mouse;
				flag2 = true;
			}
		}
		if (flag2 || flag || mNextRaycast < RealTime.time)
		{
			mNextRaycast = RealTime.time + 0.02f;
			if (!Raycast(VespaInput.mousePosition))
			{
				hoveredObject = fallThrough;
			}
			if (hoveredObject == null)
			{
				hoveredObject = mGenericHandler;
			}
			for (int k = 0; k < 3; k++)
			{
				mMouse[k].current = hoveredObject;
			}
		}
		bool flag4 = mMouse[0].last != mMouse[0].current;
		if (flag4)
		{
			currentScheme = ControlScheme.Mouse;
		}
		currentTouch = mMouse[0];
		currentTouchID = -1;
		if (flag2)
		{
			mTooltipTime = 0f;
		}
		else if (flag && (!stickyTooltip || flag4))
		{
			if (mTooltipTime != 0f)
			{
				mTooltipTime = RealTime.time + tooltipDelay;
			}
			else if (mTooltip != null)
			{
				ShowTooltip(val: false);
			}
		}
		if (flag && onMouseMove != null)
		{
			onMouseMove(currentTouch.delta);
			currentTouch = null;
		}
		if ((flag3 || !flag2) && mHover != null && flag4)
		{
			currentScheme = ControlScheme.Mouse;
			if (mTooltip != null)
			{
				ShowTooltip(val: false);
			}
			if (onHover != null)
			{
				onHover(mHover, state: false);
			}
			Notify(mHover, ENotifyType.OnHover, false);
			mHover = null;
		}
		for (int l = 0; l < 3; l++)
		{
			bool mouseButtonDown = VespaInput.GetMouseButtonDown(l);
			bool mouseButtonUp = VespaInput.GetMouseButtonUp(l);
			if (mouseButtonDown || mouseButtonUp)
			{
				currentScheme = ControlScheme.Mouse;
			}
			currentTouch = mMouse[l];
			currentTouchID = -1 - l;
			currentKey = (KeyCode)(323 + l);
			if (mouseButtonDown)
			{
				currentTouch.pressedCam = currentCamera;
			}
			else if (currentTouch.pressed != null)
			{
				currentCamera = currentTouch.pressedCam;
			}
			ProcessTouch(mouseButtonDown, mouseButtonUp);
			currentKey = KeyCode.None;
		}
		if (!flag2 && flag4)
		{
			currentScheme = ControlScheme.Mouse;
			mTooltipTime = RealTime.time + tooltipDelay;
			mHover = mMouse[0].current;
			currentTouch = mMouse[0];
			currentTouchID = -1;
			if (onHover != null)
			{
				onHover(mHover, state: true);
			}
			Notify(mHover, ENotifyType.OnHover, true);
		}
		currentTouch = null;
		mMouse[0].last = mMouse[0].current;
		for (int m = 1; m < 3; m++)
		{
			mMouse[m].last = mMouse[0].last;
		}
	}

	public void ProcessTouches()
	{
		currentScheme = ControlScheme.Touch;
		int num = ((GetInputTouchCount == null) ? VespaInput.touchCount : GetInputTouchCount());
		for (int i = 0; i < num; i++)
		{
			TouchPhase phase;
			int fingerId;
			Vector2 position;
			int tapCount;
			if (GetInputTouch == null)
			{
				UnityEngine.Touch touch = VespaInput.GetTouch(i);
				phase = touch.phase;
				fingerId = touch.fingerId;
				position = touch.position;
				tapCount = touch.tapCount;
			}
			else
			{
				Touch touch2 = GetInputTouch(i);
				phase = touch2.phase;
				fingerId = touch2.fingerId;
				position = touch2.position;
				tapCount = touch2.tapCount;
			}
			currentTouchID = ((!allowMultiTouch) ? 1 : fingerId);
			currentTouch = GetTouch(currentTouchID);
			bool flag = phase == TouchPhase.Began || currentTouch.touchBegan;
			bool flag2 = phase == TouchPhase.Canceled || phase == TouchPhase.Ended;
			currentTouch.touchBegan = false;
			currentTouch.delta = (flag ? Vector2.zero : (position - currentTouch.pos));
			currentTouch.pos = position;
			if (!Raycast(currentTouch.pos))
			{
				hoveredObject = fallThrough;
			}
			if (hoveredObject == null)
			{
				hoveredObject = mGenericHandler;
			}
			currentTouch.last = currentTouch.current;
			currentTouch.current = hoveredObject;
			lastTouchPosition = currentTouch.pos;
			if (flag)
			{
				currentTouch.pressedCam = currentCamera;
			}
			else if (currentTouch.pressed != null)
			{
				currentCamera = currentTouch.pressedCam;
			}
			if (tapCount > 1)
			{
				currentTouch.clickTime = RealTime.time;
			}
			ProcessTouch(flag, flag2);
			if (flag2)
			{
				RemoveTouch(currentTouchID);
			}
			currentTouch.last = null;
			currentTouch = null;
			if (!allowMultiTouch)
			{
				break;
			}
		}
		if (num == 0)
		{
			if (mUsingTouchEvents)
			{
				mUsingTouchEvents = false;
			}
			else if (useMouse)
			{
				ProcessMouse();
			}
		}
		else
		{
			mUsingTouchEvents = true;
		}
	}

	private void ProcessFakeTouches()
	{
		bool mouseButtonDown = VespaInput.GetMouseButtonDown(0);
		bool mouseButtonUp = VespaInput.GetMouseButtonUp(0);
		bool mouseButton = VespaInput.GetMouseButton(0);
		if (mouseButtonDown || mouseButtonUp || mouseButton)
		{
			currentTouchID = 1;
			currentTouch = mMouse[0];
			currentTouch.touchBegan = mouseButtonDown;
			if (mouseButtonDown)
			{
				currentTouch.pressTime = RealTime.time;
			}
			Vector2 vector = VespaInput.mousePosition;
			currentTouch.delta = (mouseButtonDown ? Vector2.zero : (vector - currentTouch.pos));
			currentTouch.pos = vector;
			if (!Raycast(currentTouch.pos))
			{
				hoveredObject = fallThrough;
			}
			if (hoveredObject == null)
			{
				hoveredObject = mGenericHandler;
			}
			currentTouch.last = currentTouch.current;
			currentTouch.current = hoveredObject;
			lastTouchPosition = currentTouch.pos;
			if (mouseButtonDown)
			{
				currentTouch.pressedCam = currentCamera;
			}
			else if (currentTouch.pressed != null)
			{
				currentCamera = currentTouch.pressedCam;
			}
			ProcessTouch(mouseButtonDown, mouseButtonUp);
			if (mouseButtonUp)
			{
				RemoveTouch(currentTouchID);
			}
			currentTouch.last = null;
			currentTouch = null;
		}
	}

	public void ProcessOthers()
	{
		currentTouchID = -100;
		currentTouch = controller;
		bool flag = false;
		bool flag2 = false;
		if (submitKey0 != KeyCode.None && GetKeyDown(submitKey0))
		{
			currentKey = submitKey0;
			flag = true;
		}
		if (submitKey1 != KeyCode.None && GetKeyDown(submitKey1))
		{
			currentKey = submitKey1;
			flag = true;
		}
		if (submitKey0 != KeyCode.None && GetKeyUp(submitKey0))
		{
			currentKey = submitKey0;
			flag2 = true;
		}
		if (submitKey1 != KeyCode.None && GetKeyUp(submitKey1))
		{
			currentKey = submitKey1;
			flag2 = true;
		}
		if (flag || flag2)
		{
			currentScheme = ControlScheme.Controller;
			currentTouch.last = currentTouch.current;
			currentTouch.current = mCurrentSelection;
			ProcessTouch(flag, flag2);
			currentTouch.last = null;
		}
		int num = 0;
		int num2 = 0;
		if (useKeyboard)
		{
			if (inputHasFocus)
			{
				num += GetDirection(KeyCode.UpArrow, KeyCode.DownArrow);
				num2 += GetDirection(KeyCode.RightArrow, KeyCode.LeftArrow);
			}
			else
			{
				num += GetDirection(KeyCode.W, KeyCode.UpArrow, KeyCode.S, KeyCode.DownArrow);
				num2 += GetDirection(KeyCode.D, KeyCode.RightArrow, KeyCode.A, KeyCode.LeftArrow);
			}
		}
		if (useController)
		{
			if (!string.IsNullOrEmpty(verticalAxisName))
			{
				num += GetDirection(verticalAxisName);
			}
			if (!string.IsNullOrEmpty(horizontalAxisName))
			{
				num2 += GetDirection(horizontalAxisName);
			}
		}
		if (num != 0)
		{
			currentScheme = ControlScheme.Controller;
			KeyCode keyCode = ((num > 0) ? KeyCode.UpArrow : KeyCode.DownArrow);
			if (onKey != null)
			{
				onKey(mCurrentSelection, keyCode);
			}
			Notify(mCurrentSelection, ENotifyType.OnKey, keyCode);
		}
		if (num2 != 0)
		{
			currentScheme = ControlScheme.Controller;
			KeyCode keyCode2 = ((num2 > 0) ? KeyCode.RightArrow : KeyCode.LeftArrow);
			if (onKey != null)
			{
				onKey(mCurrentSelection, keyCode2);
			}
			Notify(mCurrentSelection, ENotifyType.OnKey, keyCode2);
		}
		if (useKeyboard && GetKeyDown(KeyCode.Tab))
		{
			currentKey = KeyCode.Tab;
			currentScheme = ControlScheme.Controller;
			if (onKey != null)
			{
				onKey(mCurrentSelection, KeyCode.Tab);
			}
			Notify(mCurrentSelection, ENotifyType.OnKey, KeyCode.Tab);
		}
		if (cancelKey0 != KeyCode.None && GetKeyDown(cancelKey0))
		{
			currentKey = cancelKey0;
			currentScheme = ControlScheme.Controller;
			if (onKey != null)
			{
				onKey(mCurrentSelection, KeyCode.Escape);
			}
			Notify(mCurrentSelection, ENotifyType.OnKey, KeyCode.Escape);
		}
		if (cancelKey1 != KeyCode.None && GetKeyDown(cancelKey1))
		{
			currentKey = cancelKey1;
			currentScheme = ControlScheme.Controller;
			if (onKey != null)
			{
				onKey(mCurrentSelection, KeyCode.Escape);
			}
			Notify(mCurrentSelection, ENotifyType.OnKey, KeyCode.Escape);
		}
		currentTouch = null;
		currentKey = KeyCode.None;
	}

	private void ProcessPress(bool pressed, float click, float drag)
	{
		if (pressed)
		{
			if (mTooltip != null)
			{
				ShowTooltip(val: false);
			}
			currentTouch.pressStarted = true;
			if (onPress != null && (bool)currentTouch.pressed)
			{
				onPress(currentTouch.pressed, state: false);
			}
			Notify(currentTouch.pressed, ENotifyType.OnPress, false);
			GlobalUIRoot.OnPress(currentTouch, press: false);
			currentTouch.pressed = currentTouch.current;
			currentTouch.dragged = currentTouch.current;
			currentTouch.clickNotification = ClickNotification.BasedOnDelta;
			currentTouch.totalDelta = Vector2.zero;
			currentTouch.dragStarted = false;
			if (onPress != null && (bool)currentTouch.pressed)
			{
				onPress(currentTouch.pressed, state: true);
			}
			if (UITooltipTrigger.working != null)
			{
				UITooltipTrigger.working.ReleaseTooltip();
			}
			Notify(currentTouch.pressed, ENotifyType.OnPress, true);
			GlobalUIRoot.OnPress(currentTouch, press: true);
			if (mCam.cullingMask != (mCam.cullingMask | (1 << currentTouch.pressed.layer)))
			{
				UIPopupRoot.OnNativePressedEvent(currentTouch.pressed, pressed: true);
			}
			if (mTooltip != null)
			{
				ShowTooltip(val: false);
			}
			selectedObject = currentTouch.pressed;
		}
		else
		{
			if (!(currentTouch.pressed != null) || (currentTouch.delta.sqrMagnitude == 0f && !(currentTouch.current != currentTouch.last)))
			{
				return;
			}
			currentTouch.totalDelta += currentTouch.delta;
			float sqrMagnitude = currentTouch.totalDelta.sqrMagnitude;
			bool flag = false;
			if (!currentTouch.dragStarted && currentTouch.last != currentTouch.current)
			{
				currentTouch.dragStarted = true;
				currentTouch.delta = currentTouch.totalDelta;
				isDragging = true;
				if (onDragStart != null)
				{
					onDragStart(currentTouch.dragged);
				}
				Notify(currentTouch.dragged, ENotifyType.OnDragStart, null);
				if (onDragOver != null)
				{
					onDragOver(currentTouch.last, currentTouch.dragged);
				}
				Notify(currentTouch.last, ENotifyType.OnDragOver, currentTouch.dragged);
				isDragging = false;
			}
			else if (!currentTouch.dragStarted && drag < sqrMagnitude)
			{
				flag = true;
				currentTouch.dragStarted = true;
				currentTouch.delta = currentTouch.totalDelta;
			}
			if (!currentTouch.dragStarted)
			{
				return;
			}
			if (mTooltip != null)
			{
				ShowTooltip(val: false);
			}
			isDragging = true;
			bool num = currentTouch.clickNotification == ClickNotification.None;
			if (flag)
			{
				if (onDragStart != null)
				{
					onDragStart(currentTouch.dragged);
				}
				Notify(currentTouch.dragged, ENotifyType.OnDragStart, null);
				if (onDragOver != null)
				{
					onDragOver(currentTouch.last, currentTouch.dragged);
				}
				Notify(currentTouch.current, ENotifyType.OnDragOver, currentTouch.dragged);
			}
			else if (currentTouch.last != currentTouch.current)
			{
				if (onDragStart != null)
				{
					onDragStart(currentTouch.dragged);
				}
				Notify(currentTouch.last, ENotifyType.OnDragOut, currentTouch.dragged);
				if (onDragOver != null)
				{
					onDragOver(currentTouch.last, currentTouch.dragged);
				}
				Notify(currentTouch.current, ENotifyType.OnDragOver, currentTouch.dragged);
			}
			if (onDrag != null)
			{
				onDrag(currentTouch.dragged, currentTouch.delta);
			}
			Notify(currentTouch.dragged, ENotifyType.OnDrag, currentTouch.delta);
			GlobalUIRoot.OnDrag(currentTouch);
			currentTouch.last = currentTouch.current;
			isDragging = false;
			if (num)
			{
				currentTouch.clickNotification = ClickNotification.None;
			}
			else if (currentTouch.clickNotification == ClickNotification.BasedOnDelta && click < sqrMagnitude)
			{
				currentTouch.clickNotification = ClickNotification.None;
			}
		}
	}

	private void ProcessRelease(bool isMouse, float drag)
	{
		if (currentTouch == null)
		{
			return;
		}
		currentTouch.pressStarted = false;
		if (mTooltip != null)
		{
			ShowTooltip(val: false);
		}
		if (currentTouch.pressed != null)
		{
			if (currentTouch.dragStarted)
			{
				if (onDragOut != null)
				{
					onDragOut(currentTouch.last, currentTouch.dragged);
				}
				Notify(currentTouch.last, ENotifyType.OnDragOut, currentTouch.dragged);
				if (onDragEnd != null)
				{
					onDragEnd(currentTouch.dragged);
				}
				Notify(currentTouch.dragged, ENotifyType.OnDragEnd, null);
			}
			if (onPress != null)
			{
				onPress(currentTouch.pressed, state: false);
			}
			Notify(currentTouch.pressed, ENotifyType.OnPress, false);
			if (mCam.cullingMask != (mCam.cullingMask | (1 << currentTouch.pressed.layer)))
			{
				UIPopupRoot.OnNativePressedEvent(currentTouch.pressed, pressed: false);
			}
			if (isMouse)
			{
				if (onHover != null)
				{
					onHover(currentTouch.current, state: true);
				}
				Notify(currentTouch.current, ENotifyType.OnHover, true);
			}
			mHover = currentTouch.current;
			if (currentTouch.dragged == currentTouch.current || (currentScheme != ControlScheme.Controller && currentTouch.clickNotification != ClickNotification.None && currentTouch.totalDelta.sqrMagnitude < drag))
			{
				if (currentTouch.clickNotification != ClickNotification.None && currentTouch.pressed == currentTouch.current)
				{
					float time = RealTime.time;
					if (onClick != null)
					{
						onClick(currentTouch.pressed);
					}
					Notify(currentTouch.pressed, ENotifyType.OnClick, null);
					GlobalUIRoot.OnTouch(currentTouch);
					if (currentTouch.clickTime + 0.35f > time)
					{
						if (onDoubleClick != null)
						{
							onDoubleClick(currentTouch.pressed);
						}
						Notify(currentTouch.pressed, ENotifyType.OnDoubleClick, null);
					}
					currentTouch.clickTime = time;
				}
			}
			else if (currentTouch.dragStarted)
			{
				if (onDrop != null)
				{
					onDrop(currentTouch.current, currentTouch.dragged);
				}
				Notify(currentTouch.current, ENotifyType.OnDrop, currentTouch.dragged);
			}
		}
		currentTouch.dragStarted = false;
		currentTouch.pressed = null;
		currentTouch.dragged = null;
	}

	public void ProcessTouch(bool pressed, bool released)
	{
		bool flag = currentScheme == ControlScheme.Mouse;
		float num = (flag ? mouseDragThreshold : touchDragThreshold);
		float num2 = (flag ? mouseClickThreshold : touchClickThreshold);
		num *= num;
		num2 *= num2;
		if (currentTouch.pressed != null)
		{
			if (released)
			{
				ProcessRelease(flag, num);
			}
			ProcessPress(pressed, num2, num);
		}
		else if (flag || pressed || released)
		{
			ProcessPress(pressed, num2, num);
			if (released)
			{
				ProcessRelease(flag, num);
			}
		}
	}

	public void ShowTooltip(bool val)
	{
		mTooltipTime = 0f;
		if (onTooltip != null)
		{
			onTooltip(mTooltip, val);
		}
		Notify(mTooltip, ENotifyType.OnTooltip, val);
		if (!val)
		{
			mTooltip = null;
		}
	}

	private void OnApplicationPause()
	{
		MouseOrTouch mouseOrTouch = currentTouch;
		if (useTouch)
		{
			BetterList<int> betterList = new BetterList<int>();
			foreach (KeyValuePair<int, MouseOrTouch> mTouch in mTouches)
			{
				if (mTouch.Value != null && (bool)mTouch.Value.pressed)
				{
					currentTouch = mTouch.Value;
					currentTouchID = mTouch.Key;
					currentScheme = ControlScheme.Touch;
					currentTouch.clickNotification = ClickNotification.None;
					ProcessTouch(pressed: false, released: true);
					betterList.Add(currentTouchID);
				}
			}
			for (int i = 0; i < betterList.size; i++)
			{
				RemoveTouch(betterList[i]);
			}
		}
		if (useMouse)
		{
			for (int j = 0; j < 3; j++)
			{
				if ((bool)mMouse[j].pressed)
				{
					currentTouch = mMouse[j];
					currentTouchID = -1 - j;
					currentKey = (KeyCode)(323 + j);
					currentScheme = ControlScheme.Mouse;
					currentTouch.clickNotification = ClickNotification.None;
					ProcessTouch(pressed: false, released: true);
				}
			}
		}
		if (useController && (bool)controller.pressed)
		{
			currentTouch = controller;
			currentTouchID = -100;
			currentScheme = ControlScheme.Controller;
			currentTouch.last = currentTouch.current;
			currentTouch.current = mCurrentSelection;
			currentTouch.clickNotification = ClickNotification.None;
			ProcessTouch(pressed: false, released: true);
			currentTouch.last = null;
		}
		currentTouch = mouseOrTouch;
	}
}
