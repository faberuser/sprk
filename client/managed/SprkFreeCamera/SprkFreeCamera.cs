using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using NGame2.NRender;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NRender3D.NRenderTargetCinema;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWorldMap.NMapLayout;
using NGame2.NUI.NWorldMap.NMapLayout.NMapViewer;
using NVespa;
using UnityEngine;

public static class SprkFreeCamera
{
	private sealed class State
	{
		public object Controller;

		public object AnchorModel;

		public Vector3 AnchorPivot;

		public Vector3 AnchorHead;

		public Transform Head;

		public float Zoom = 1f;

		public float Yaw;

		public float Pitch;

		public Vector3 Pan;

		public Vector2 Last;

		public int Button = -1;

		public object ShadowModel;

		public ShadowBounds[] Shadows;

		public void Reset()
		{
			Zoom = 1f;
			Yaw = 0f;
			Pitch = 0f;
			Pan = Vector3.zero;
			Button = -1;
		}
	}

	private sealed class LayoutState
	{
		public int Width;

		public int Height;

		public int RefreshThrough;

		public Transform Root;

		public Transform Parent;

		public Camera Camera;
	}

	private sealed class ShadowBounds
	{
		public SkinnedMeshRenderer Skin;

		public MeshFilter Filter;

		public Mesh Original;

		public Mesh Copy;

		public Bounds Source;

		public Bounds Applied;

		public bool HasApplied;

		public bool UpdateWhenOffscreen;

		public Transform[] Bones;

		public Matrix4x4[] Bindposes;

		public Bounds MeshBounds;

		public static ShadowBounds Create(Renderer renderer)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
			if (skinnedMeshRenderer != null)
			{
				return new ShadowBounds
				{
					Skin = skinnedMeshRenderer,
					Source = skinnedMeshRenderer.localBounds,
					UpdateWhenOffscreen = skinnedMeshRenderer.updateWhenOffscreen,
					Bones = skinnedMeshRenderer.bones,
					Bindposes = ((skinnedMeshRenderer.sharedMesh != null) ? skinnedMeshRenderer.sharedMesh.bindposes : null),
					MeshBounds = ((skinnedMeshRenderer.sharedMesh != null) ? skinnedMeshRenderer.sharedMesh.bounds : skinnedMeshRenderer.localBounds)
				};
			}
			MeshRenderer meshRenderer = renderer as MeshRenderer;
			MeshFilter meshFilter = ((meshRenderer != null) ? meshRenderer.GetComponent<MeshFilter>() : null);
			if (meshFilter == null || meshFilter.sharedMesh == null)
			{
				return null;
			}
			return new ShadowBounds
			{
				Filter = meshFilter,
				Original = meshFilter.sharedMesh,
				Source = meshFilter.sharedMesh.bounds
			};
		}

		public void Apply(float floor, Vector3 direction)
		{
			Matrix4x4 matrix;
			if (Skin != null)
			{
				Skin.updateWhenOffscreen = false;
				Bounds localBounds = Skin.localBounds;
				if (!HasApplied || !Same(localBounds, Applied))
				{
					Source = localBounds;
				}
				matrix = ((Skin.rootBone != null) ? Skin.rootBone.localToWorldMatrix : Skin.localToWorldMatrix);
			}
			else
			{
				if (Filter == null || Original == null || (Filter.sharedMesh != Original && Filter.sharedMesh != Copy))
				{
					return;
				}
				matrix = Filter.transform.localToWorldMatrix;
			}
			Bounds source = Source;
			if (Skin != null && Bones != null && Bindposes != null && Bones.Length == Bindposes.Length)
			{
				Matrix4x4 inverse = matrix.inverse;
				for (int i = 0; i < Bones.Length; i++)
				{
					if (Bones[i] != null)
					{
						Matrix4x4 matrix4x = inverse * Bones[i].localToWorldMatrix * Bindposes[i];
						for (int j = 0; j < 8; j++)
						{
							source.Encapsulate(matrix4x.MultiplyPoint3x4(Corner(MeshBounds, j)));
						}
					}
				}
			}
			Bounds bounds = ProjectedBounds(source, matrix, floor, direction);
			if (Skin != null)
			{
				Skin.localBounds = bounds;
			}
			else
			{
				if (Copy == null)
				{
					Copy = UnityEngine.Object.Instantiate(Original);
					Copy.name = Original.name + " (preview shadow bounds)";
					Filter.sharedMesh = Copy;
				}
				Copy.bounds = bounds;
			}
			Applied = bounds;
			HasApplied = true;
		}

		public void Restore()
		{
			if (Skin != null && HasApplied && Same(Skin.localBounds, Applied))
			{
				Skin.localBounds = Source;
			}
			if (Skin != null)
			{
				Skin.updateWhenOffscreen = UpdateWhenOffscreen;
			}
			if (Filter != null && Filter.sharedMesh == Copy)
			{
				Filter.sharedMesh = Original;
			}
			if (Copy != null)
			{
				UnityEngine.Object.Destroy(Copy);
			}
			Copy = null;
			HasApplied = false;
		}

		private static bool Same(Bounds a, Bounds b)
		{
			if ((a.center - b.center).sqrMagnitude < 1E-10f)
			{
				return (a.size - b.size).sqrMagnitude < 1E-10f;
			}
			return false;
		}
	}

	private static readonly ConditionalWeakTable<HeroFullSceneViewBase, State> states = new ConditionalWeakTable<HeroFullSceneViewBase, State>();

	private static readonly ConditionalWeakTable<BaseWindow, LayoutState> layouts = new ConditionalWeakTable<BaseWindow, LayoutState>();

	private static readonly FieldInfo cinemaField = typeof(HeroFullSceneViewBase).GetField("_cinema", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo detailTab = typeof(HeroDetailView).GetField("_toggle", BindingFlags.Instance | BindingFlags.NonPublic);

	private static bool probeStarted;

	private static float Sensitivity => ReadSensitivity("ModelCameraSensitivity");

	private static float LobbySensitivity => ReadSensitivity("LobbyCameraSensitivity");

	public static bool IsScreenLayout(Transform root)
	{
		if (root == null)
		{
			return false;
		}
		UIRect component = root.GetComponent<UIRect>();
		if (component == null)
		{
			return false;
		}
		UIRect.AnchorPoint leftAnchor = component.leftAnchor;
		UIRect.AnchorPoint rightAnchor = component.rightAnchor;
		if (leftAnchor.target == null || leftAnchor.target != rightAnchor.target || leftAnchor.relative != 0f || rightAnchor.relative != 1f || leftAnchor.absolute != 0 || rightAnchor.absolute != 0)
		{
			return false;
		}
		UIPanel component2 = leftAnchor.target.GetComponent<UIPanel>();
		if (!(leftAnchor.target.GetComponent<Camera>() != null))
		{
			if (component2 != null)
			{
				return component2.clipping == UIDrawCall.Clipping.None;
			}
			return false;
		}
		return true;
	}

	public static void InvalidateLayout(BaseWindow window)
	{
		if (!(window == null))
		{
			layouts.Remove(window);
			HeroFullSceneViewBase heroFullSceneViewBase = window as HeroFullSceneViewBase;
			if (heroFullSceneViewBase != null && states.TryGetValue(heroFullSceneViewBase, out var value))
			{
				RestoreShadows(value);
			}
		}
	}

	public static Transform LayoutRoot(BaseWindow window)
	{
		if (window is LobbyTopmostMenu || window is LobbyRightMenu || window is LobbyRightGridWindow)
		{
			return window.transform;
		}
		Transform transform = window.transform.Find("Panel/Root");
		if (!(window is HeroDeck2) && !SupportsWindow(window.GetType()) && !IsScreenLayout(transform))
		{
			return null;
		}
		return transform;
	}

	public static void RefreshLayout(BaseWindow window)
	{
		if (window == null || !window.isActiveAndEnabled || !window.IsOpen())
		{
			return;
		}
		Transform transform = LayoutRoot(window);
		if (!(transform == null) && transform.gameObject.activeInHierarchy)
		{
			LayoutState orCreateValue = layouts.GetOrCreateValue(window);
			Camera camera = NGUITools.FindCameraForLayer(transform.gameObject.layer);
			if (orCreateValue.Root != transform || orCreateValue.Parent != transform.parent || orCreateValue.Camera != camera || orCreateValue.Width != Screen.width || orCreateValue.Height != Screen.height)
			{
				orCreateValue.RefreshThrough = Time.frameCount + 2;
			}
			if (Time.frameCount <= orCreateValue.RefreshThrough)
			{
				RefreshLayoutRoot(transform);
				orCreateValue.Root = transform;
				orCreateValue.Parent = transform.parent;
				orCreateValue.Camera = camera;
				orCreateValue.Width = Screen.width;
				orCreateValue.Height = Screen.height;
			}
		}
	}

	public static void RefreshLayoutRoot(Transform root)
	{
		UIRect[] componentsInChildren = root.GetComponentsInChildren<UIRect>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].ResetAnchors();
		}
		componentsInChildren = root.GetComponentsInChildren<UIRect>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].ForceUpdateAnchors(includeChildren: false);
		}
	}

	public static bool SupportsWindow(Type type)
	{
		if (!typeof(HeroModelView).IsAssignableFrom(type) && !typeof(HeroCostumeView).IsAssignableFrom(type))
		{
			return typeof(HeroDetailView).IsAssignableFrom(type);
		}
		return true;
	}

	private static bool Eligible(HeroFullSceneViewBase window)
	{
		if (window == null || !SupportsWindow(window.GetType()) || !window.isActiveAndEnabled || !window.IsOpen())
		{
			return false;
		}
		return true;
	}

	private static State Get(HeroFullSceneViewBase window)
	{
		State orCreateValue = states.GetOrCreateValue(window);
		if (orCreateValue.Controller != window.CameraController)
		{
			RestoreShadows(orCreateValue);
			orCreateValue.Reset();
			orCreateValue.Controller = window.CameraController;
			orCreateValue.AnchorModel = null;
			orCreateValue.Head = null;
		}
		return orCreateValue;
	}

	private static float ReadSensitivity(string key)
	{
		return (float)Mathf.Clamp(SprkTestPrefs.GetInt("Sprk.Graphics." + key, SprkTestPrefs.GetInt("Sprk.Graphics.CameraSensitivity", 100)), 50, 150) / 100f;
	}

	private static Vector2 ScaleDrag(Vector2 delta)
	{
		return delta * Sensitivity;
	}

	private static void ZoomStep(State state, float wheel)
	{
		state.Zoom = Mathf.Clamp(state.Zoom * Mathf.Exp((0f - wheel) * 0.15f * Sensitivity), 0.02f, 8f);
	}

	public static Vector2 ScaleLobbyDrag(Vector2 delta)
	{
		if (!Application.isMobilePlatform && Input.touchCount <= 0)
		{
			return delta * LobbySensitivity;
		}
		return delta;
	}

	public static float ScaleLobbyZoom(float delta)
	{
		if (!Application.isMobilePlatform && Input.touchCount <= 0)
		{
			return delta * LobbySensitivity;
		}
		return delta;
	}

	public static void Reset(HeroFullSceneViewBase window)
	{
		if (!(window == null))
		{
			if (states.TryGetValue(window, out var value))
			{
				RestoreShadows(value);
			}
			states.Remove(window);
			layouts.Remove(window);
		}
	}

	public static bool InputCamera(HeroFullSceneViewBase window)
	{
		if (!Eligible(window) || Application.isMobilePlatform || Input.touchCount > 0)
		{
			return false;
		}
		State state = Get(window);
		if (window.CameraController == null || !Application.isFocused || UICamera.inputHasFocus)
		{
			state.Button = -1;
			return true;
		}
		UITexture hero3DTexture = window.Hero3DTexture;
		GameObject hoveredObject = UICamera.hoveredObject;
		bool flag = OwnsPointer(hero3DTexture, hoveredObject);
		Vector2 vector = Input.mousePosition;
		if (state.Button >= 0 && !Input.GetMouseButton(state.Button))
		{
			state.Button = -1;
		}
		if (state.Button < 0 && flag)
		{
			for (int i = 0; i < 3; i++)
			{
				if (Input.GetMouseButtonDown(i))
				{
					state.Button = i;
					state.Last = vector;
					break;
				}
			}
		}
		if (state.Button >= 0)
		{
			Vector2 delta = ScaleDrag(vector - state.Last);
			state.Last = vector;
			if (state.Button == 0 && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
			{
				Orbit(state, delta);
			}
			else
			{
				RenderTargetCinema renderTargetCinema = cinemaField.GetValue(window) as RenderTargetCinema;
				if (renderTargetCinema != null && renderTargetCinema.CinemaCamera != null)
				{
					CinemaCamera.CameraInfoData cameraInfo = renderTargetCinema.CinemaCamera.CameraInfo;
					Quaternion quaternion = Quaternion.LookRotation(cameraInfo.LookDir);
					float num = 2f * Mathf.Max(0.15f, cameraInfo.TargetDistance) * Mathf.Tan(cameraInfo.FieldOfView * (MathF.PI / 180f) * 0.5f) / (float)Mathf.Max(1, Screen.height);
					state.Pan = Vector3.ClampMagnitude(state.Pan - quaternion * new Vector3(delta.x, delta.y, 0f) * num, 40f);
				}
			}
		}
		if (flag)
		{
			ZoomStep(state, Input.mouseScrollDelta.y);
			if (Input.GetKeyDown(KeyCode.R))
			{
				state.Reset();
			}
		}
		return true;
	}

	public static bool OwnsPointer(UITexture texture, GameObject hovered)
	{
		if (texture != null && hovered != null)
		{
			if (!(hovered == texture.gameObject))
			{
				if (hovered.transform.IsChildOf(texture.transform) && hovered.GetComponentInParent<UIButton>() == null)
				{
					return hovered.GetComponentInParent<UIInput>() == null;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private static void Orbit(State state, Vector2 delta)
	{
		state.Yaw = Mathf.Repeat(state.Yaw + delta.x * 0.3f + 180f, 360f) - 180f;
		state.Pitch = Mathf.Clamp(state.Pitch - delta.y * 0.3f, -85f, 85f);
	}

	public static void EnsureProbe()
	{
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_FREE_CAMERA_TEST")) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR")) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE")) && !probeStarted)
		{
			probeStarted = true;
			GameObject gameObject = new GameObject("SprkFreeCameraTests");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			gameObject.AddComponent<SprkFreeCameraTests>();
		}
	}

	private static void RunLayoutProbe(Action<bool, string> check)
	{
		GameObject gameObject = new GameObject("UltrawideLayoutProbe");
		gameObject.SetActive(value: false);
		gameObject.transform.localScale = Vector3.one * 0.0018518518f;
		GameObject gameObject2 = new GameObject("UltrawideLayoutCamera");
		Camera camera = gameObject2.AddComponent<Camera>();
		camera.orthographic = true;
		camera.orthographicSize = 1f;
		camera.cullingMask = 1073741824;
		string[] array = new string[8] { "Prefab2/Window/Hero/HeroDeck2", "Prefab2/Window/FullScene/HeroDetailView", "Prefab2/Window/FullScene/HeroCostumeView", "Prefab2/Window/BattlePartySetting/BattlePartySetting", "Prefab2/Window/Raid/RaidPartySetting", "Prefab2/Window/Raid/RaidSinglePartySetting", "Prefab2/Lobby/TopmostMenu", "Prefab2/Lobby/RightMenu" };
		foreach (string text in array)
		{
			GameObject gameObject3 = Resources.Load<GameObject>(text);
			check(gameObject3 != null, "Load real layout prefab: " + text);
			GameObject gameObject4 = UnityEngine.Object.Instantiate(gameObject3, gameObject.transform, worldPositionStays: false);
			Transform[] componentsInChildren = gameObject4.GetComponentsInChildren<Transform>(includeInactive: true);
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].gameObject.layer = 30;
			}
			BaseWindow component = gameObject4.GetComponent<BaseWindow>();
			check(component != null, "Native window component");
			Transform transform = LayoutRoot(component);
			check(transform != null, "Native layout root");
			if (text.Contains("PartySetting"))
			{
				check(IsScreenLayout(transform), "Party layout included by full-width anchors: " + text);
			}
			HeroDetailView component2 = gameObject4.GetComponent<HeroDetailView>();
			if (component2 != null)
			{
				State state = Get(component2);
				state.Yaw = 65f;
				state.Pitch = -20f;
				state.Zoom = 0.4f;
				state.Pan = new Vector3(1f, 2f, 3f);
				state.AnchorModel = new object();
				state.AnchorPivot = new Vector3(2f, 1f, 0f);
				string[] array2 = new string[5] { "Animation", "Cinema", "Costume", "Skill", "Animation" };
				foreach (string text2 in array2)
				{
					detailTab.SetValue(component2, Enum.Parse(detailTab.FieldType, text2));
					State state2 = Get(component2);
					check(state2 == state && state2.Yaw == 65f && state2.Pitch == -20f && state2.Zoom == 0.4f && state2.Pan == new Vector3(1f, 2f, 3f) && state2.AnchorPivot == new Vector3(2f, 1f, 0f), "Camera state retained across tab: " + text2);
					Orbit(state2, new Vector2(10f, 5f));
					check(state2.Yaw == 68f && state2.Pitch == -21.5f, "Camera controls available after tab: " + text2);
					state2.Yaw = 65f;
					state2.Pitch = -20f;
				}
				Reset(component2);
				check(Get(component2) != state, "Closing/recycling a preview still resets state");
				detailTab.SetValue(component2, Enum.Parse(detailTab.FieldType, "Animation"));
				FieldInfo field = typeof(HeroDetailView).GetField("_heroViewAnimationComponentList", BindingFlags.Instance | BindingFlags.NonPublic);
				HeroViewAnimationComponent item = gameObject4.AddComponent<HeroViewAnimationComponent>();
				field.SetValue(component2, new List<HeroViewAnimationComponent> { item });
				UIEventTrigger component3 = component2.Hero3DTexture.GetComponent<UIEventTrigger>();
				check(component3 != null && component3.onClick.Count > 0 && component3.onPress.Count > 0 && component3.onRelease.Count > 0, "Native preview pointer bindings preserved");
				for (int k = 0; k < 8; k++)
				{
					EventDelegate.Execute(component3.onClick);
				}
				check(((IList)field.GetValue(component2)).Count == 1, "Model clicks preserve animation options");
			}
			UIWidget uIWidget = (text.EndsWith("TopmostMenu") ? gameObject4.transform.Find("Panel/Root/Sprite_Topbar_Normal").GetComponent<UIWidget>() : (text.EndsWith("RightMenu") ? gameObject4.transform.Find("Panel/MenuAnchor").GetComponent<UIWidget>() : transform.GetComponent<UIWidget>()));
			layouts.Add(component, new LayoutState
			{
				Root = transform,
				Width = 1920,
				Height = 1080
			});
			InvalidateLayout(component);
			check(!layouts.TryGetValue(component, out var _), "Recycled window discards cached layout dimensions");
			Vector2[] array3 = new Vector2[4]
			{
				new Vector2(1920f, 1080f),
				new Vector2(3440f, 1440f),
				new Vector2(5120f, 1440f),
				new Vector2(1920f, 1080f)
			};
			for (int j = 0; j < array3.Length; j++)
			{
				Vector2 vector = array3[j];
				camera.pixelRect = new Rect(0f, 0f, vector.x, vector.y);
				camera.aspect = vector.x / vector.y;
				RefreshLayoutRoot(transform);
				Vector3[] worldCorners = uIWidget.worldCorners;
				Vector3 vector2 = camera.WorldToViewportPoint(worldCorners[0]);
				Vector3 vector3 = camera.WorldToViewportPoint(worldCorners[2]);
				bool arg = Mathf.Abs(vector2.x) < 0.002f && Mathf.Abs(vector3.x - 1f) < 0.002f;
				string[] obj = new string[8] { "Real prefab fills display: ", text, " ", null, null, null, null, null };
				Vector2 vector4 = vector;
				obj[3] = vector4.ToString();
				obj[4] = " edges=";
				obj[5] = vector2.x.ToString();
				obj[6] = ",";
				obj[7] = vector3.x.ToString();
				check(arg, string.Concat(obj));
				HeroFullSceneViewBase component4 = gameObject4.GetComponent<HeroFullSceneViewBase>();
				if (component4 != null)
				{
					worldCorners = component4.Hero3DTexture.worldCorners;
					vector2 = camera.WorldToViewportPoint(worldCorners[0]);
					vector3 = camera.WorldToViewportPoint(worldCorners[2]);
					check(Mathf.Abs(vector2.x) < 0.002f && Mathf.Abs(vector3.x - 1f) < 0.002f, "Preview background fills display");
				}
			}
			UnityEngine.Object.Destroy(gameObject4);
		}
		GameObject gameObject5 = new GameObject("FixedPopupLayout");
		UIWidget uIWidget2 = gameObject5.AddComponent<UIWidget>();
		uIWidget2.leftAnchor.target = camera.transform;
		uIWidget2.rightAnchor.target = camera.transform;
		uIWidget2.leftAnchor.relative = 0.5f;
		uIWidget2.rightAnchor.relative = 0.5f;
		check(!IsScreenLayout(gameObject5.transform), "Fixed-width popup is excluded");
		UnityEngine.Object.Destroy(gameObject5);
		UnityEngine.Object.Destroy(gameObject);
		UnityEngine.Object.Destroy(gameObject2);
	}

	public static void RunProbe()
	{
		Action<bool, string> action = delegate(bool ok, string message)
		{
			if (!ok)
			{
				throw new Exception(message);
			}
		};
		RunLayoutProbe(action);
		action(SupportsWindow(typeof(HeroDetailView)) && SupportsWindow(typeof(HeroCostumeView)) && SupportsWindow(typeof(HeroModelView)) && !SupportsWindow(typeof(HeroFullSceneViewBase)), "Preview window scope");
		string[] array = new string[3] { "Sprk.Graphics.CameraSensitivity", "Sprk.Graphics.LobbyCameraSensitivity", "Sprk.Graphics.ModelCameraSensitivity" };
		bool[] array2 = Array.ConvertAll(array, (string k) => SprkTestPrefs.HasKey(k));
		int[] array3 = Array.ConvertAll(array, (string k) => SprkTestPrefs.GetInt(k, 100));
		string[] array4 = array;
		for (int num = 0; num < array4.Length; num++)
		{
			SprkTestPrefs.DeleteKey(array4[num]);
		}
		int num2 = SprkTestPrefs.GetInt("Sprk.Graphics.CameraSensitivity", 100);
		SprkTestPrefs.DeleteKey("Sprk.Graphics.CameraSensitivity");
		action(Sensitivity == 1f && LobbySensitivity == 1f, "Default camera sensitivities");
		SprkTestPrefs.SetInt("Sprk.Graphics.CameraSensitivity", 75);
		action(Sensitivity == 0.75f && LobbySensitivity == 0.75f, "Legacy sensitivity fallback for both cameras");
		SprkTestPrefs.SetInt("Sprk.Graphics.LobbyCameraSensitivity", 50);
		int[] array5 = new int[5] { 50, 75, 100, 125, 150 };
		foreach (int num3 in array5)
		{
			SprkTestPrefs.SetInt("Sprk.Graphics.ModelCameraSensitivity", num3);
			State state = new State();
			Vector2 vector = ScaleDrag(new Vector2(100f, 40f));
			Orbit(state, vector);
			action(Mathf.Abs(state.Yaw - (float)(30 * num3) / 100f) < 0.001f && Mathf.Abs(state.Pitch + (float)(12 * num3) / 100f) < 0.001f, "Scaled orbit sensitivity");
			action(Vector2.Distance(vector, new Vector2(num3, (float)num3 * 0.4f)) < 0.001f, "Scaled pan input");
			action(LobbySensitivity == 0.5f, "Model setting does not change lobby");
			ZoomStep(state, 1f);
			action(Mathf.Abs(state.Zoom - Mathf.Exp(-0.15f * (float)num3 / 100f)) < 0.001f, "Scaled wheel sensitivity");
			state.Reset();
			action(Sensitivity == (float)num3 / 100f, "Reset preserves selected sensitivity");
		}
		GameObject gameObject = new GameObject("LobbySensitivityProbe");
		gameObject.SetActive(value: false);
		MapViewer mapViewer = gameObject.AddComponent<MapViewer>();
		WorldMapMode worldMapMode = new WorldMapMode();
		worldMapMode.Open(new Mediator
		{
			MapComponent = new MapComponent
			{
				Viewer = mapViewer
			},
			MapWidth = 100000,
			MapHeight = 100000
		});
		Type type = worldMapMode.GetType();
		LocalAccountFloat localAccountFloat = (LocalAccountFloat)type.GetField("_orthoSize", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(worldMapMode);
		MethodInfo method = type.GetMethod("DragWorldMap", BindingFlags.Instance | BindingFlags.NonPublic);
		MethodInfo method2 = type.GetMethod("ZoomWorldMap", BindingFlags.Instance | BindingFlags.NonPublic);
		FieldInfo field = type.GetField("_targetViewPos", BindingFlags.Instance | BindingFlags.NonPublic);
		FieldInfo field2 = type.GetField("_validViewPos", BindingFlags.Instance | BindingFlags.NonPublic);
		float num4 = Mathf.Max(1920f / (float)Screen.width, 1080f / (float)Screen.height);
		array5 = new int[5] { 50, 75, 100, 125, 150 };
		foreach (int num5 in array5)
		{
			SprkTestPrefs.SetInt("Sprk.Graphics.LobbyCameraSensitivity", num5);
			action(Sensitivity == 1.5f, "Lobby setting does not change model");
			localAccountFloat.Value = 1f;
			field2.SetValue(worldMapMode, new Vector3(100f, 50f, 0f));
			method.Invoke(worldMapMode, new object[1]
			{
				new Vector2(100f, 40f)
			});
			Vector3 a = (Vector3)field.GetValue(worldMapMode) - new Vector3(100f, 50f, 0f);
			action(Vector3.Distance(a, -new Vector3(100f, 40f, 0f) * num4 * ((float)num5 / 100f)) < 0.001f, "Native lobby drag sensitivity");
			method2.Invoke(worldMapMode, new object[1] { 40f });
			action(Mathf.Abs(localAccountFloat.Value - Mathf.Pow(2f, 40f * ((float)num5 / 100f) / (float)Screen.height * mapViewer.ZoomSpeed)) < 0.001f, "Native lobby wheel sensitivity");
			action(mapViewer.ZoomSpeed == 1f, "Preserve native zoom speed setting");
		}
		UnityEngine.Object.Destroy(gameObject);
		for (int num6 = 0; num6 < array.Length; num6++)
		{
			if (array2[num6])
			{
				SprkTestPrefs.SetInt(array[num6], array3[num6]);
			}
			else
			{
				SprkTestPrefs.DeleteKey(array[num6]);
			}
		}
		SprkTestPrefs.Save();
		State state2 = new State();
		Orbit(state2, new Vector2(200f, 100f));
		action(Mathf.Abs(state2.Yaw - 60f) < 0.001f && Mathf.Abs(state2.Pitch + 30f) < 0.001f, "Orbit axes");
		Orbit(state2, new Vector2(10000f, 10000f));
		action(Mathf.Abs(state2.Yaw) <= 180f && state2.Pitch == -85f, "Orbit bounds");
		Vector3 vector2 = Quaternion.Euler(20f, 30f, 0f) * Vector3.forward;
		action(Vector3.Distance(Direction(vector2, 0f, 0f), vector2) < 0.001f, "Native view preservation");
		for (int num7 = -720; num7 <= 720; num7 += 30)
		{
			for (int num8 = -180; num8 <= 180; num8 += 15)
			{
				Vector3 vector3 = Direction(vector2, num7, num8);
				action(!float.IsNaN(vector3.x) && Mathf.Abs(vector3.magnitude - 1f) < 0.001f && Mathf.Abs(vector3.y) < 0.997f, "Stable pole limits");
			}
		}
		action(Distance(8f, 0.001f) == 0.15f && Distance(8f, 100f) == 40f, "Zoom bounds");
		action(Distance(8f, 0.5f) == 4f && Distance(8f, 2f) == 16f, "Zoom factor");
		state2.Pan = new Vector3(1f, 2f, 3f);
		state2.Zoom = 0.2f;
		state2.Reset();
		action(state2.Pan == Vector3.zero && state2.Zoom == 1f && state2.Yaw == 0f && state2.Pitch == 0f && state2.Button == -1, "Reset");
		GameObject gameObject2 = new GameObject("ProbeCamera");
		Camera camera = gameObject2.AddComponent<Camera>();
		CinemaCamera cinemaCamera = new CinemaCamera(camera);
		GameObject gameObject3 = new GameObject("AnimatedHeadProbe");
		Vector3 vector4 = new Vector3(0f, 2f, 0f);
		float[] array6 = new float[3] { 0f, 0.5f, 1f };
		for (int num = 0; num < array6.Length; num++)
		{
			float num9 = array6[num];
			float[] array7 = new float[2] { 0f, 65f };
			foreach (float yaw in array7)
			{
				float[] array8 = new float[3] { 0.5f, 1f, 1.8f };
				foreach (float zoom in array8)
				{
					State state3 = new State();
					object model = new object();
					Vector3 a2 = Vector3.zero;
					for (int num12 = 0; num12 < 32; num12++)
					{
						float f = (float)num12 * MathF.PI / 8f;
						gameObject3.transform.position = vector4 + new Vector3(Mathf.Sin(f) * 0.1f, Mathf.Sin(f) * 0.3f, 0f);
						cinemaCamera.CameraInfo.TargetPos = Vector3.Lerp(gameObject3.transform.position + new Vector3(0.1f, 0.2f, 0f), new Vector3(0f, 1f, 0f), num9) + new Vector3(-0.6f, 0f, 0f);
						cinemaCamera.CameraInfo.LookDir = Vector3.forward;
						cinemaCamera.CameraInfo.TargetDistance = 4f;
						cinemaCamera.CameraInfo.FieldOfView = 45f;
						Vector3 pivot = StabilizeOrbit(state3, model, new Vector3(0f, 1f + Mathf.Sin(f) * 0.2f, 0f), gameObject3.transform, cinemaCamera.CameraInfo, num9);
						ApplyCamera(cinemaCamera, new Vector3(0.2f, 0.1f, 0f), yaw, 15f, zoom, pivot);
						if (num12 == 0)
						{
							a2 = camera.transform.position;
						}
						action(Vector3.Distance(a2, camera.transform.position) < 0.001f, "Fixed orbit during animated head/root motion at native zoom blend " + num9);
						if (num12 == 12)
						{
							state3.Reset();
						}
					}
					object model2 = new object();
					Vector3 vector5 = new Vector3(1f, 2f, 3f);
					action(StabilizeOrbit(state3, model2, vector5, gameObject3.transform, cinemaCamera.CameraInfo, num9) == vector5, "Anchor refreshes for a replaced model");
				}
			}
		}
		UnityEngine.Object.Destroy(gameObject3);
		cinemaCamera.CameraInfo.TargetPos = new Vector3(1f, 2f, 0f);
		cinemaCamera.CameraInfo.TargetDistance = 4f;
		cinemaCamera.CameraInfo.LookDir = Direction(Vector3.forward, 40f, 25f);
		cinemaCamera.ChainUpdate();
		action(Vector3.Distance(camera.transform.position, cinemaCamera.CameraInfo.TargetPos - camera.transform.forward * 4f) < 0.001f, "Native camera transform");
		ApplyCamera(cinemaCamera, new Vector3(2f, 1f, 0f), 20f, 10f, 0.5f, cinemaCamera.CameraInfo.TargetPos);
		action(Mathf.Abs(cinemaCamera.CameraInfo.TargetDistance - 2f) < 0.001f && Vector3.Distance(cinemaCamera.CameraInfo.TargetPos, new Vector3(3f, 3f, 0f)) < 0.001f, "Pan/zoom camera application");
		action(Vector3.Distance(camera.transform.position, cinemaCamera.CameraInfo.TargetPos - camera.transform.forward * 2f) < 0.001f, "Applied camera transform");
		camera.aspect = 1.7777778f;
		camera.nearClipPlane = 0.01f;
		camera.farClipPlane = 1000f;
		Vector3 vector6 = new Vector3(2f, 1f, 3f);
		Vector3 vector7 = Quaternion.Euler(12f, 25f, 0f) * Vector3.forward;
		Quaternion quaternion = Quaternion.LookRotation(vector7);
		array6 = new float[2] { 1.7777778f, 2.3888888f };
		foreach (float aspect in array6)
		{
			float[] array7 = new float[2] { 0f, -1.2f };
			foreach (float x in array7)
			{
				float[] array8 = new float[5] { -150f, -60f, 0f, 60f, 150f };
				foreach (float num13 in array8)
				{
					float[] array9 = new float[3] { -35f, 0f, 35f };
					foreach (float num15 in array9)
					{
						float[] array10 = new float[3] { 0.5f, 1f, 1.8f };
						foreach (float num17 in array10)
						{
							camera.aspect = aspect;
							Vector3 targetPos = vector6 + quaternion * new Vector3(x, 0.35f, 0f);
							cinemaCamera.CameraInfo.TargetPos = targetPos;
							cinemaCamera.CameraInfo.LookDir = vector7;
							cinemaCamera.CameraInfo.TargetDistance = 5f;
							cinemaCamera.CameraInfo.FieldOfView = 45f;
							cinemaCamera.ChainUpdate();
							Vector3 position = camera.transform.position;
							Vector3 vector8 = camera.WorldToViewportPoint(vector6);
							ApplyCamera(cinemaCamera, Vector3.zero, num13, num15, num17, vector6);
							Vector3 vector9 = camera.WorldToViewportPoint(vector6);
							action(vector9.z > 0f && Mathf.Abs(vector9.x - vector8.x) < 0.001f && Mathf.Abs(vector9.y - vector8.y) < 0.001f, "Hero remains framed during orbit/zoom");
							action(Mathf.Abs(Vector3.Distance(camera.transform.position, vector6) - Vector3.Distance(position, vector6) * num17) < 0.001f, "Orbit radius is centered on hero");
							if (num13 == 0f && num15 == 0f && num17 == 1f)
							{
								action(Vector3.Distance(position, camera.transform.position) < 0.001f, "Native/reset framing preserved");
							}
						}
					}
				}
			}
		}
		GameObject gameObject4 = new GameObject("ModelArea");
		UITexture uITexture = gameObject4.AddComponent<UITexture>();
		GameObject gameObject5 = new GameObject("PreviewAspectUICamera");
		Camera camera2 = gameObject5.AddComponent<Camera>();
		camera2.orthographic = true;
		camera2.orthographicSize = 540f;
		camera2.cullingMask = 1073741824;
		gameObject4.layer = 30;
		camera2.rect = new Rect(0f, 0f, 1f, 1f);
		RenderTexture renderTexture = (camera.targetTexture = new RenderTexture(1280, 720, 0));
		camera.transform.position = new Vector3(0f, 0f, -5f);
		camera.transform.rotation = Quaternion.identity;
		Vector2[] array11 = new Vector2[5]
		{
			new Vector2(1920f, 1080f),
			new Vector2(2560f, 1440f),
			new Vector2(3440f, 1440f),
			new Vector2(5120f, 1440f),
			new Vector2(1920f, 1080f)
		};
		for (int num = 0; num < array11.Length; num++)
		{
			Vector2 vector10 = array11[num];
			uITexture.width = (int)vector10.x;
			uITexture.height = (int)vector10.y;
			uITexture.mainTexture = renderTexture;
			array6 = new float[2] { 1f, 0.8f };
			for (int num10 = 0; num10 < array6.Length; num10++)
			{
				float num18 = array6[num10];
				uITexture.uvRect = new Rect(0f, 0f, num18, 1f);
				camera.aspect = 1.7777778f;
				FitPreviewAspect(camera, uITexture);
				Vector4 drawingDimensions = uITexture.drawingDimensions;
				Vector3 a3 = camera2.WorldToScreenPoint(uITexture.transform.TransformPoint(drawingDimensions.x, drawingDimensions.y, 0f));
				Vector3 b = camera2.WorldToScreenPoint(uITexture.transform.TransformPoint(drawingDimensions.z, drawingDimensions.y, 0f));
				Vector3 b2 = camera2.WorldToScreenPoint(uITexture.transform.TransformPoint(drawingDimensions.x, drawingDimensions.w, 0f));
				float num19 = Vector3.Distance(a3, b);
				float num20 = Vector3.Distance(a3, b2);
				Vector3 vector11 = camera.WorldToViewportPoint(Vector3.zero);
				Vector3 vector12 = camera.WorldToViewportPoint(Vector3.right);
				Vector3 vector13 = camera.WorldToViewportPoint(Vector3.up);
				float num21 = (vector12.x - vector11.x) * num19 / num18;
				float num22 = (vector13.y - vector11.y) * num20;
				bool arg = Mathf.Abs(num21 / num22 - 1f) < 0.001f;
				Vector2 vector14 = vector10;
				action(arg, "Preview proportions after aspect/resolution change: " + vector14.ToString() + " crop=" + num18);
			}
		}
		uITexture.uvRect = new Rect(0f, 0f, 1f, 1f);
		camera.targetTexture = null;
		UnityEngine.Object.Destroy(renderTexture);
		UnityEngine.Object.Destroy(gameObject5);
		GameObject gameObject6 = new GameObject("ModelPointer");
		gameObject6.transform.SetParent(gameObject4.transform);
		GameObject gameObject7 = new GameObject("MenuButton");
		gameObject7.transform.SetParent(gameObject4.transform);
		gameObject7.AddComponent<UIButton>();
		action(OwnsPointer(uITexture, gameObject4) && OwnsPointer(uITexture, gameObject6) && !OwnsPointer(uITexture, gameObject7) && !OwnsPointer(uITexture, gameObject2) && !OwnsPointer(uITexture, null), "Pointer capture scope");
		UnityEngine.Object.Destroy(gameObject4);
		UnityEngine.Object.Destroy(gameObject2);
		Directory.CreateDirectory(Environment.GetEnvironmentVariable("SPRK_FREE_CAMERA_TEST"));
		File.WriteAllText(Path.Combine(Environment.GetEnvironmentVariable("SPRK_FREE_CAMERA_TEST"), "result.txt"), "PASS: fullscreen party and raid layout scope, recycled window cache invalidation and fixed popup exclusion; camera state and orbit retained across Animation/Cutscene/Costume/Skills and reset on window recycling; fixed camera under animated head/root movement across zoom blends, orbit, pan, zoom and reset; anchor refresh on model replacement; See Motions native preview click events do not select animations, press/release bindings preserved; real Heroes List, See Motions, Dressing Room and party prefabs fill the viewport through 16:9, ultrawide, 32:9 and back; preview proportions across aspect/resolution changes with stale 16:9 render target and UV cropping; five sensitivity levels, baseline default, scaled orbit/pan/wheel, orbit axes and limits, native orientation, zoom bounds, reset, native CinemaCamera transform, pan/zoom application, pointer capture scope, native lobby drag/zoom sensitivity, hero-centered orbit and stable off-center framing; loaded sensitivity=" + num2);
	}

	public static Vector3 Direction(Vector3 original, float yaw, float pitch)
	{
		original = ((original.sqrMagnitude < 0.0001f) ? Vector3.forward : original.normalized);
		float num = Mathf.Atan2(original.x, original.z) * 57.29578f;
		return Quaternion.Euler(Mathf.Clamp((0f - Mathf.Asin(Mathf.Clamp(original.y, -1f, 1f))) * 57.29578f + pitch, -85f, 85f), num + yaw, 0f) * Vector3.forward;
	}

	public static float Distance(float original, float zoom)
	{
		return Mathf.Clamp(original * zoom, 0.15f, 40f);
	}

	public static void FitPreviewAspect(Camera camera, UITexture texture)
	{
		if (camera == null || texture == null || texture.mainTexture == null)
		{
			return;
		}
		Camera anchorCamera = texture.anchorCamera;
		if (!(anchorCamera == null))
		{
			Vector4 drawingDimensions = texture.drawingDimensions;
			Transform transform = texture.transform;
			Vector3 a = anchorCamera.WorldToScreenPoint(transform.TransformPoint(drawingDimensions.x, drawingDimensions.y, 0f));
			Vector3 b = anchorCamera.WorldToScreenPoint(transform.TransformPoint(drawingDimensions.z, drawingDimensions.y, 0f));
			Vector3 b2 = anchorCamera.WorldToScreenPoint(transform.TransformPoint(drawingDimensions.x, drawingDimensions.w, 0f));
			float num = Vector3.Distance(a, b);
			float num2 = Vector3.Distance(a, b2);
			Rect uvRect = texture.uvRect;
			if (!(num < 0.001f) && !(num2 < 0.001f) && !(Mathf.Abs(uvRect.width) < 0.0001f) && !(Mathf.Abs(uvRect.height) < 0.0001f))
			{
				camera.aspect = num / num2 * Mathf.Abs(uvRect.height / uvRect.width);
			}
		}
	}

	public static void Apply(HeroFullSceneViewBase window)
	{
		if (!Eligible(window))
		{
			return;
		}
		State state = Get(window);
		RenderTargetCinema renderTargetCinema = cinemaField.GetValue(window) as RenderTargetCinema;
		if (renderTargetCinema == null)
		{
			return;
		}
		FitPreviewAspect(renderTargetCinema.MainCamera, window.Hero3DTexture);
		if (renderTargetCinema.CinemaCamera == null || window.CameraController == null)
		{
			return;
		}
		CinemaCamera.CameraInfoData cameraInfo = renderTargetCinema.CinemaCamera.CameraInfo;
		RenderFacade renderFacade = ((MotionSequence.IDataProvider)window).GetRenderFacade();
		if (renderFacade != null && renderFacade.AssetCreatureAsync.ISDONE() && renderFacade.Pivot != null)
		{
			if (state.ShadowModel != renderFacade.Pivot)
			{
				RestoreShadows(state);
				state.ShadowModel = renderFacade.Pivot;
				List<ShadowBounds> list = new List<ShadowBounds>();
				Renderer[] componentsInChildren = renderFacade.GetComponentsInChildren<Renderer>(includeInactive: true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					ShadowBounds shadowBounds = ShadowBounds.Create(componentsInChildren[i]);
					if (shadowBounds != null)
					{
						list.Add(shadowBounds);
					}
				}
				state.Shadows = list.ToArray();
			}
			Vector4 globalVector = Shader.GetGlobalVector("_ShadowDirection");
			if (state.Shadows != null && Mathf.Abs(globalVector.y) > 0.001f)
			{
				ShadowBounds[] shadows = state.Shadows;
				for (int i = 0; i < shadows.Length; i++)
				{
					shadows[i].Apply(renderFacade.transform.position.y, new Vector3(globalVector.x, globalVector.y, globalVector.z));
				}
			}
		}
		Vector3 pivot = ((renderFacade != null && renderFacade.ShapeBounds.size.y > 0.01f) ? renderFacade.transform.TransformPoint(renderFacade.ShapeBounds.center) : (cameraInfo.TargetPos - window.CameraController.ExtraOffsetByDistance * cameraInfo.TargetDistance));
		if (renderFacade != null && renderFacade.AssetCreatureAsync.ISDONE() && renderFacade.Pivot != null && renderFacade.ShapeBounds.size.y > 0.01f)
		{
			Transform head = ((state.AnchorModel == renderFacade && state.Head != null) ? state.Head : renderFacade.Pivot.FindTransform("Bip001 Head"));
			pivot = StabilizeOrbit(state, renderFacade, pivot, head, cameraInfo, window.CameraController.ZoomOutRate);
		}
		ApplyCamera(renderTargetCinema.CinemaCamera, state.Pan, state.Yaw, state.Pitch, state.Zoom, pivot);
	}

	private static Vector3 StabilizeOrbit(State state, object model, Vector3 pivot, Transform head, CinemaCamera.CameraInfoData info, float zoomOutRate)
	{
		if (state.AnchorModel != model || state.Head != head)
		{
			state.AnchorModel = model;
			state.AnchorPivot = pivot;
			state.Head = head;
			if (head != null)
			{
				state.AnchorHead = head.position;
			}
		}
		if (head != null)
		{
			info.TargetPos -= (head.position - state.AnchorHead) * (1f - Mathf.Clamp01(zoomOutRate));
		}
		return state.AnchorPivot;
	}

	private static void ApplyCamera(CinemaCamera camera, Vector3 pan, float yaw, float pitch, float zoom, Vector3 pivot)
	{
		CinemaCamera.CameraInfoData cameraInfo = camera.CameraInfo;
		Quaternion rotation = Quaternion.LookRotation(cameraInfo.LookDir);
		Vector3 vector = Direction(cameraInfo.LookDir, yaw, pitch);
		Quaternion quaternion = Quaternion.LookRotation(vector) * Quaternion.Inverse(rotation);
		float num = Distance(cameraInfo.TargetDistance, zoom);
		float num2 = num / Mathf.Max(0.0001f, cameraInfo.TargetDistance);
		cameraInfo.TargetPos = pivot + quaternion * (cameraInfo.TargetPos - pivot) * num2 + pan;
		cameraInfo.LookDir = vector;
		cameraInfo.TargetDistance = num;
		camera.ChainUpdate();
	}

	private static Bounds ProjectedBounds(Bounds source, Matrix4x4 matrix, float floor, Vector3 direction)
	{
		Matrix4x4 inverse = matrix.inverse;
		Bounds result = source;
		for (int i = 0; i < 8; i++)
		{
			Vector3 point = Corner(source, i);
			Vector3 point2 = matrix.MultiplyPoint3x4(point);
			point2 -= direction * ((point2.y - floor) / direction.y);
			result.Encapsulate(inverse.MultiplyPoint3x4(point2));
		}
		result.Expand(0.05f);
		return result;
	}

	private static Vector3 Corner(Bounds bounds, int i)
	{
		return bounds.center + Vector3.Scale(bounds.extents, new Vector3(((i & 1) != 0) ? 1 : (-1), ((i & 2) != 0) ? 1 : (-1), ((i & 4) != 0) ? 1 : (-1)));
	}

	private static void RestoreShadows(State state)
	{
		if (state.Shadows != null)
		{
			ShadowBounds[] shadows = state.Shadows;
			for (int i = 0; i < shadows.Length; i++)
			{
				shadows[i].Restore();
			}
		}
		state.Shadows = null;
		state.ShadowModel = null;
	}

	private static int ProbeIsaiahShadowBounds()
	{
		AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "Documents/Patch/StandaloneWindows/Assetbundle/OriginalSpec/Creature_Hero_Isaiah_Star5.unity3d"));
		if (assetBundle == null)
		{
			throw new Exception("Native Isaiah model bundle unavailable");
		}
		GameObject gameObject = new GameObject("IsaiahBoundsFixture");
		gameObject.SetActive(value: false);
		int num = 0;
		try
		{
			GameObject[] array = assetBundle.LoadAllAssets<GameObject>();
			for (int i = 0; i < array.Length; i++)
			{
				Renderer[] componentsInChildren = UnityEngine.Object.Instantiate(array[i], gameObject.transform, worldPositionStays: false).GetComponentsInChildren<Renderer>(includeInactive: true);
				foreach (Renderer renderer in componentsInChildren)
				{
					ShadowBounds shadowBounds = ShadowBounds.Create(renderer);
					if (shadowBounds == null)
					{
						continue;
					}
					try
					{
						shadowBounds.Apply(0f, new Vector3(-0.3f, -1f, 0.4f));
						Bounds bounds = renderer.bounds;
						if (float.IsNaN(bounds.size.x) || float.IsInfinity(bounds.size.y))
						{
							throw new Exception("Invalid Isaiah shadow bounds");
						}
						for (int k = 0; k < 5; k++)
						{
							shadowBounds.Apply(0f, new Vector3(-0.3f, -1f, 0.4f));
						}
						if ((renderer.bounds.size - bounds.size).sqrMagnitude > 1E-06f)
						{
							throw new Exception("Isaiah bounds accumulated across frames");
						}
						num++;
					}
					finally
					{
						shadowBounds.Restore();
					}
				}
			}
			if (num == 0)
			{
				throw new Exception("Isaiah model contained no renderers");
			}
			return num;
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(gameObject);
			assetBundle.Unload(unloadAllLoadedObjects: true);
		}
	}

	public static string ProbePreviewShadows()
	{
		RestoredPortal.EnsureNativeShaderVariables();
		Shader shader = Shader.Find("Vespa/ToonColor2/Shadow");
		if (shader == null || !shader.isSupported)
		{
			throw new Exception("Native planar shadow shader unavailable");
		}
		Vector4 globalVector = Shader.GetGlobalVector("_ShadowDirection");
		GameObject gameObject = new GameObject("PreviewShadowProbeCamera");
		GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
		gameObject2.name = "OffscreenShadowCaster";
		gameObject2.layer = 30;
		Material material = new Material(shader);
		material.SetTexture("_AlphaTex", Texture2D.whiteTexture);
		material.SetColor("_Color", new Color(0f, 0f, 0f, 0.5f));
		material.EnableKeyword("EXTRARENDERFX_OFF");
		gameObject2.GetComponent<Renderer>().sharedMaterial = material;
		gameObject2.transform.position = new Vector3(0f, 3f, 0f);
		Camera camera = gameObject.AddComponent<Camera>();
		camera.cullingMask = 1073741824;
		camera.orthographic = true;
		camera.orthographicSize = 0.65f;
		camera.transform.position = new Vector3(0f, 2f, -4f);
		camera.transform.LookAt(Vector3.zero);
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = Color.white;
		RenderTexture target = new RenderTexture(160, 160, 24);
		camera.targetTexture = target;
		Texture2D picture = new Texture2D(160, 160, TextureFormat.RGB24, mipChain: false);
		ShadowBounds shadowBounds = ShadowBounds.Create(gameObject2.GetComponent<Renderer>());
		Mesh sharedMesh = gameObject2.GetComponent<MeshFilter>().sharedMesh;
		try
		{
			Shader.SetGlobalVector("_ShadowDirection", new Vector4(0f, -1f, 0f, 0f));
			if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camera), gameObject2.GetComponent<Renderer>().bounds))
			{
				throw new Exception("Fixture caster must be offscreen");
			}
			Func<int> func = delegate
			{
				camera.Render();
				RenderTexture active = RenderTexture.active;
				RenderTexture.active = target;
				picture.ReadPixels(new Rect(0f, 0f, 160f, 160f), 0, 0);
				picture.Apply();
				RenderTexture.active = active;
				int num8 = 0;
				Color32[] pixels = picture.GetPixels32();
				for (int i = 0; i < pixels.Length; i++)
				{
					Color32 color = pixels[i];
					if (color.r < 240 || color.g < 240 || color.b < 240)
					{
						num8++;
					}
				}
				return num8;
			};
			int num = func();
			shadowBounds.Apply(0f, Vector3.down);
			int num2 = func();
			if (num != 0 || num2 < 100)
			{
				throw new Exception("Native shadow reproduction failed: before=" + num + ", after=" + num2);
			}
			Bounds bounds = shadowBounds.Copy.bounds;
			for (int num3 = 0; num3 < 50; num3++)
			{
				shadowBounds.Apply(0f, Vector3.down);
			}
			if ((shadowBounds.Copy.bounds.size - bounds.size).sqrMagnitude > 1E-06f)
			{
				throw new Exception("Preview shadow bounds grew between frames");
			}
			shadowBounds.Restore();
			if (gameObject2.GetComponent<MeshFilter>().sharedMesh != sharedMesh || func() != num)
			{
				throw new Exception("Preview shadow mesh/bounds were not restored");
			}
			gameObject2.GetComponent<Renderer>().enabled = false;
			GameObject gameObject3 = new GameObject("SkinnedOffscreenShadowCaster");
			gameObject3.layer = 30;
			gameObject3.transform.position = gameObject2.transform.position;
			GameObject gameObject4 = new GameObject("OffsetRootBone");
			gameObject4.transform.SetParent(gameObject3.transform, worldPositionStays: false);
			gameObject4.transform.localPosition = new Vector3(0f, 0.25f, 0f);
			SkinnedMeshRenderer skinnedMeshRenderer = gameObject3.AddComponent<SkinnedMeshRenderer>();
			Mesh mesh = UnityEngine.Object.Instantiate(sharedMesh);
			try
			{
				BoneWeight[] array = new BoneWeight[mesh.vertexCount];
				for (int num4 = 0; num4 < array.Length; num4++)
				{
					array[num4] = new BoneWeight
					{
						boneIndex0 = 0,
						weight0 = 1f
					};
				}
				mesh.boneWeights = array;
				mesh.bindposes = new Matrix4x4[1] { gameObject4.transform.worldToLocalMatrix * gameObject3.transform.localToWorldMatrix };
				skinnedMeshRenderer.sharedMesh = mesh;
				skinnedMeshRenderer.bones = new Transform[1] { gameObject4.transform };
				skinnedMeshRenderer.rootBone = gameObject4.transform;
				skinnedMeshRenderer.sharedMaterial = material;
				skinnedMeshRenderer.updateWhenOffscreen = true;
				skinnedMeshRenderer.localBounds = new Bounds(new Vector3(0f, -0.25f, 0f), Vector3.one);
				if ((skinnedMeshRenderer.bounds.center - gameObject2.transform.position).sqrMagnitude > 0.0001f)
				{
					throw new Exception("Skinned bounds must use root bone space");
				}
				ShadowBounds shadowBounds2 = ShadowBounds.Create(skinnedMeshRenderer);
				try
				{
					if (func() != 0)
					{
						throw new Exception("Fixture skinned caster must be offscreen");
					}
					shadowBounds2.Apply(0f, Vector3.down);
					int num5 = func();
					if (num5 < 100)
					{
						throw new Exception("Skinned preview shadow was still culled: " + num5);
					}
					Bounds localBounds = skinnedMeshRenderer.localBounds;
					for (int num6 = 0; num6 < 50; num6++)
					{
						shadowBounds2.Apply(0f, Vector3.down);
					}
					if ((skinnedMeshRenderer.localBounds.size - localBounds.size).sqrMagnitude > 1E-06f)
					{
						throw new Exception("Skinned bounds grew between frames");
					}
					skinnedMeshRenderer.localBounds = new Bounds(new Vector3(0.1f, -0.25f, 0f), Vector3.one);
					shadowBounds2.Apply(0f, Vector3.down);
					if ((shadowBounds2.Source.center - new Vector3(0.1f, -0.25f, 0f)).sqrMagnitude > 1E-06f)
					{
						throw new Exception("Animated bounds were not preserved");
					}
					gameObject4.transform.localPosition += new Vector3(0.2f, 0f, 0f);
					shadowBounds2.Apply(0f, Vector3.down);
					if (func() < 100)
					{
						throw new Exception("Moving bone lost its projected shadow");
					}
					shadowBounds2.Restore();
					if (!skinnedMeshRenderer.updateWhenOffscreen)
					{
						throw new Exception("Native offscreen skinning flag was not restored");
					}
					if (func() != 0)
					{
						throw new Exception("Skinned preview bounds did not restore");
					}
					int num7 = ProbeIsaiahShadowBounds();
					return "PASS: native shader shadow disappears with offscreen caster bounds (" + num + " pixels), remains visible with projection bounds (static=" + num2 + ", skinned=" + num5 + " pixels); offset root bone, 50 stable updates, latest animated pose and cleanup verified; " + num7 + " real Isaiah model renderers validated.";
				}
				finally
				{
					shadowBounds2.Restore();
				}
			}
			finally
			{
				UnityEngine.Object.Destroy(gameObject3);
				UnityEngine.Object.Destroy(mesh);
			}
		}
		finally
		{
			shadowBounds.Restore();
			Shader.SetGlobalVector("_ShadowDirection", globalVector);
			UnityEngine.Object.Destroy(gameObject2);
			UnityEngine.Object.Destroy(gameObject);
			UnityEngine.Object.Destroy(material);
			UnityEngine.Object.Destroy(picture);
			target.Release();
			UnityEngine.Object.Destroy(target);
		}
	}
}
