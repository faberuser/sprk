using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NObject;
using NGame2.NRender;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NRender3D.NCinema;
using NGame2.NUI.NWindow;
using NGame2.NUtil;
using NGameAsset;
using NShader;
using NShared;
using NTEC.PPU;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(32000)]
public sealed class SprkGraphicsOptions : MonoBehaviour
{
	private sealed class TemporalResources
	{
		public bool active;

		public DepthTextureMode owned;
	}

	private sealed class TargetSize
	{
		public int width;

		public int height;

		public bool world;

		public int scale = -1;

		public int samples = -1;

		public int actualWidth;

		public int actualHeight;

		public int actualSamples;

		public TargetSize(int w, int h)
		{
			width = w;
			height = h;
		}
	}

	private delegate bool WindowVisitor(IntPtr window, IntPtr state);

	private struct WindowRect
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	private sealed class GraphicsHelpInfo
	{
		public string title;

		public string description;

		public GraphicsHelpInfo(string title, string description)
		{
			this.title = title;
			this.description = description;
		}
	}

	public sealed class FilterValues
	{
		public int X;

		public int Y;

		public int Temperature;

		public int Sharpness;

		public int Brightness;

		public int Contrast;

		public int Saturation;

		public int Grayscale;

		public int Shadows;

		public int Bloom;

		public int Vignette;

		public int Grain;

		public int Intensity;

		public bool Neutral => ToArray().Take(12).All((int v) => v == 0);

		public FilterValues Clone()
		{
			return (FilterValues)MemberwiseClone();
		}

		public int[] ToArray()
		{
			return new int[13]
			{
				X, Y, Temperature, Sharpness, Brightness, Contrast, Saturation, Grayscale, Shadows, Bloom,
				Vignette, Grain, Intensity
			};
		}

		public static FilterValues FromArray(int[] a)
		{
			return new FilterValues
			{
				X = a[0],
				Y = a[1],
				Temperature = a[2],
				Sharpness = a[3],
				Brightness = a[4],
				Contrast = a[5],
				Saturation = a[6],
				Grayscale = a[7],
				Shadows = a[8],
				Bloom = a[9],
				Vignette = a[10],
				Grain = a[11],
				Intensity = a[12]
			};
		}

		public string Serialize()
		{
			return string.Join(",", (from v in ToArray()
				select v.ToString(CultureInfo.InvariantCulture)).ToArray());
		}

		public static FilterValues Parse(string text, FilterValues fallback)
		{
			if (string.IsNullOrEmpty(text))
			{
				return fallback.Clone();
			}
			string[] array = text.Split(',');
			if (array.Length != FilterFields.Length)
			{
				return fallback.Clone();
			}
			int[] array2 = new int[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				if (!int.TryParse(array[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
				{
					return fallback.Clone();
				}
				array2[i] = Mathf.Clamp(result, FilterFields[i].Min, FilterFields[i].Max);
			}
			return FromArray(array2);
		}

		public bool SameAs(FilterValues other)
		{
			if (other != null)
			{
				return ToArray().SequenceEqual(other.ToArray());
			}
			return false;
		}

		public int Get(int field)
		{
			return ToArray()[field];
		}

		public FilterValues With(int field, int value)
		{
			int[] array = ToArray();
			array[field] = Mathf.Clamp(value, FilterFields[field].Min, FilterFields[field].Max);
			return FromArray(array);
		}
	}

	public sealed class FilterField
	{
		public readonly string Name;

		public readonly string Unit;

		public readonly int Min;

		public readonly int Max;

		public FilterField(string name, int min, int max, string unit = "")
		{
			Name = name;
			Min = min;
			Max = max;
			Unit = unit;
		}
	}

	private sealed class FilterStack
	{
		public GameObject host;

		public PostProcessProfile profile;

		public PostProcessVolume volume;

		public ColorGrading grading;

		public Bloom bloom;

		public Vignette vignette;

		public Grain grain;

		public Sharpen sharpen;
	}

	private static SprkGraphicsOptions instance;

	private static readonly FieldInfo resourceField = typeof(PostProcessLayer).GetField("m_Resources", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo releaseTemporalHistory = typeof(TemporalAntialiasing).GetMethod("Release", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private PostProcessResources resources;

	private readonly Dictionary<Camera, PostProcessLayer> layers = new Dictionary<Camera, PostProcessLayer>();

	private readonly Dictionary<Camera, RenderTexture> redirected = new Dictionary<Camera, RenderTexture>();

	private readonly Dictionary<Camera, int> signatures = new Dictionary<Camera, int>();

	private readonly Dictionary<Camera, Vector3> positions = new Dictionary<Camera, Vector3>();

	private readonly Dictionary<Camera, Quaternion> rotations = new Dictionary<Camera, Quaternion>();

	private readonly HashSet<PostProcessLayer> helperLayers = new HashSet<PostProcessLayer>();

	private readonly Dictionary<PostProcessLayer, bool> finalBlitDefaults = new Dictionary<PostProcessLayer, bool>();

	private readonly Dictionary<Camera, bool> uiCameras = new Dictionary<Camera, bool>();

	private readonly Dictionary<Camera, TemporalResources> temporalResources = new Dictionary<Camera, TemporalResources>();

	private Camera[] cameraBuffer = new Camera[16];

	private readonly List<Camera> cameras = new List<Camera>();

	private readonly List<Camera> onscreen = new List<Camera>();

	private readonly List<Camera> worldCameras = new List<Camera>();

	private readonly List<Camera> removeCameras = new List<Camera>();

	private readonly List<RenderTexture> removeTargets = new List<RenderTexture>();

	private RenderTexture world;

	private Camera lastWorld;

	private Camera presentationCamera;

	private int presentationFrame = -1;

	private int lastLogged;

	private string status = "";

	private readonly Dictionary<Camera, CommandBuffer> presentationCommands = new Dictionary<Camera, CommandBuffer>();

	private readonly List<RenderTexture> retainedWorlds = new List<RenderTexture>();

	private const int MaxRetainedWorlds = 2;

	private static bool preferencesReady;

	private static readonly int[] postModes = new int[5] { 0, 1, 2, 0, 3 };

	private static readonly string[] adjustmentNames = new string[4] { "FXAA Quality", "SMAA Quality", "MSAA Samples", "TAA Sharpness" };

	private static int[] supportedSamples;

	private static readonly Dictionary<RenderTexture, TargetSize> ownedTargets = new Dictionary<RenderTexture, TargetSize>();

	private static readonly List<SprkParticleDensity> particleBudgets = new List<SprkParticleDensity>();

	private static readonly FieldInfo distanceEmitter = typeof(ParticleObject).GetField("_isDistanceEmit", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly PropertyInfo assetParticleRoot = typeof(HeroPreview).Assembly.GetType("NGameAsset.NEffect.ParticleInstance").GetProperty("InstanceGo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private const int CustomResolution = 5;

	private static readonly int[] resolutionValues = new int[5] { 1, 2, 3, 4, 5 };

	private const int ResizeStyle = 327680;

	private IntPtr gameWindow;

	private int originalResizeStyle;

	private bool resizeStyleCaptured;

	private int savedWidth;

	private int savedHeight;

	private float saveSizeAt;

	private float nextWindowStyleCheck;

	private bool lastCustomWindow;

	private static readonly int[] frameValues = new int[5] { 30, 60, 144, 2, 4 };

	private static readonly int lobbyMask = LayerMask.GetMask("BottomUI", "BottomFixedUI", "Bottom3D", "Bottom3DUI", "Pet", "BattleBackgroundUI");

	private static readonly int modelMask = LayerMask.GetMask("Bottom3D", "Pet");

	private static readonly int battleUiMask = LayerMask.GetMask("BattleUI");

	private static readonly int[] adaptivePreferenceValues = new int[5] { 0, 3, 1, 2, 4 };

	private static readonly int[] adaptiveFrameTargets = new int[5] { 0, 30, 60, 120, 144 };

	private bool battleActive;

	private int battleEffectMask;

	private static float battleDensityFactor = 1f;

	private float nextEffectBudget;

	private int effectSettings = -1;

	private readonly SprkBattleResolution adaptiveBattle = new SprkBattleResolution();

	private readonly FrameTiming[] battleTiming = new FrameTiming[1];

	private float nextBattleTiming;

	private float lastValidBattleTiming;

	private ulong lastBattleTimestamp;

	private static readonly Dictionary<string, GraphicsHelpInfo> graphicsHelp = new Dictionary<string, GraphicsHelpInfo>
	{
		{
			"Anti-Aliasing",
			new GraphicsHelpInfo("Anti-Aliasing", "Smooths jagged edges in the 3D world and model previews.\nOff: fastest.\nFXAA: fast, but slightly softer.\nSMAA: sharper edges at moderate cost.\nMSAA: smooths geometry edges; high sample counts can be expensive.\nTAA: reduces shimmering over time, but can soften motion or leave trails.")
		},
		{
			"FXAA Quality",
			new GraphicsHelpInfo("FXAA Quality", "Only applies when Anti-Aliasing is FXAA.\nFast uses a cheaper filter. Standard spends more GPU work on edge smoothing. FXAA can soften fine detail; it does not increase render resolution.")
		},
		{
			"SMAA Quality",
			new GraphicsHelpInfo("SMAA Quality", "Only applies when Anti-Aliasing is SMAA.\nHigher quality searches more carefully for edges, improving smoothing at extra GPU cost. Lower quality favors performance.")
		},
		{
			"MSAA Samples",
			new GraphicsHelpInfo("MSAA Samples", "Only applies when Anti-Aliasing is MSAA.\nHigher sample counts smooth geometry edges at extra GPU and video-memory cost. This combines with Render Scale: 200% scale with 8x MSAA is especially demanding. Only supported sample counts are offered.")
		},
		{
			"TAA Sharpness",
			new GraphicsHelpInfo("TAA Sharpness", "Only applies when Anti-Aliasing is TAA.\nHigher settings sharpen the final image, but can add halos or emphasize shimmering. This does not change render resolution or eliminate TAA motion trails.")
		},
		{
			"Render Scale",
			new GraphicsHelpInfo("Render Scale", "Sets World Scale and Preview Scale together. Choose 100%, 125%, 150%, 175% or 200% for a shared setting.\nChoose Custom to enable the two child rows and set them independently. Switching presets preserves your saved Custom values.\nHigher scales cost more GPU work and video memory; 200% renders four times as many pixels as 100%.")
		},
		{
			"World Scale",
			new GraphicsHelpInfo("World Scale", "Controls the main 3D scene, including heroes and monsters in battle, lobby characters and the world map.\n100% uses the game's output resolution; 200% renders four times as many pixels. The UI stays at output resolution.\nChoose Custom under Render Scale to edit this setting independently.")
		},
		{
			"Preview Scale",
			new GraphicsHelpInfo("Preview Scale", "Controls dedicated model views: hero-detail previews, See Motions, Dressing Room and preview cinematics.\nHeroes and monsters rendered directly in battle, lobby characters and the world map use World Scale instead.\nChoose Custom under Render Scale to edit this independently. Adaptive battle reductions do not lower preview resolution.")
		},
		{
			"Particle Density",
			new GraphicsHelpInfo("Particle Density", "Reduces particle density in supported decorative effects.\nFull: original density.\nReduced: 75% emission.\nMinimal: 50% emission.\nThis can help busy scenes. Some effects stay unchanged to preserve important visuals. Works together with Battle Effects.")
		},
		{
			"Battle Effects",
			new GraphicsHelpInfo("Battle Effects", "Shares a budget between supported decorative particles during battle.\nBalanced and Minimal reduce emission when many effects are active. Full keeps the original density.\nWorks together with Particle Density. Some effects remain unchanged; this does not limit heroes or monsters.")
		},
		{
			"Effect Coverage",
			new GraphicsHelpInfo("Effect Coverage", "Limits how much of the screen each supported decorative particle can cover during battle.\nReduced and Minimal make large particles smaller to reduce overlap and GPU work. Full preserves original sizes. Some effects stay unchanged.")
		},
		{
			"Adaptive Battle Scale",
			new GraphicsHelpInfo("Adaptive Battle Scale", "Adjusts battle world resolution during sustained GPU overload. Choose a 30, 60, 120 or 144 FPS target, or Off.\nScale falls in 25-point steps, as low as 75%, then recovers up to your World Scale. UI and previews keep their resolution.\nThis is not an FPS limit or guarantee, and cannot fix CPU-heavy combat. Set the frame limit separately under Max Frame.")
		},
		{
			"Global Filter",
			new GraphicsHelpInfo("Global Filter", "Applies a color filter to the 3D world and model previews. The UI keeps its original colors.\nChoose a preset, then fine-tune it with the Color Palette (left/right: saturation, up/down: shadow and midtone lightness) or the Advanced sliders. Each preset keeps its own adjustments.\nFilter Intensity blends the preset with the original image. Default turns the filter off.")
		},
		{
			"Lobby Camera Sensitivity",
			new GraphicsHelpInfo("Lobby Camera Sensitivity", "Changes mouse drag and wheel-zoom speed in the lobby and world map.\n100% is normal speed. Lower values give finer control; higher values move faster. This changes camera controls, not image quality or rendering performance.")
		},
		{
			"Model Camera Sensitivity",
			new GraphicsHelpInfo("Model Camera Sensitivity", "Changes mouse orbit, pan and wheel-zoom speed in hero/model views, including See Motions and Dressing Room.\n100% is normal speed. Lower values give finer control; higher values move faster. This does not change Preview Scale or image quality.")
		},
		{
			"Graphic_FrameControl",
			new GraphicsHelpInfo("Max Frame", "30 / 60 / 144: requested FPS limits. Selecting one switches V-Sync off.\nAuto: V-Sync follows your display's refresh rate.\nAuto+: uses the FPS target configured for the current scene or content.\nV-Sync can override an FPS limit. Actual FPS depends on hardware and scene load. Adaptive Battle Scale changes resolution separately.")
		},
		{
			"Graphic_VSyncControl",
			new GraphicsHelpInfo("V-Sync", "Synchronizes frame presentation with your display to reduce tearing. It can add input delay and make the display refresh rate take precedence over an FPS limit.\nSelecting a fixed Max Frame limit turns V-Sync off; Auto turns it on. This setting is separate from Adaptive Battle Scale.")
		},
		{
			"Graphic_WindowControl",
			new GraphicsHelpInfo("Screen", "Full uses borderless fullscreen at your desktop resolution.\nWindow uses the selected Resolution preset or a resizable Custom window.\nRender Scale controls internal 3D detail separately from the size of the game window.")
		},
		{
			"Graphic_ResolutionControl",
			new GraphicsHelpInfo("Resolution", "Window presets: HD 1280x720, FHD 1920x1080, QHD 2560x1440 and UHD 3840x2160.\nCustom lets you resize the window and remembers its last size. Fullscreen uses your desktop resolution.\nHigher output resolution increases rendering cost; Render Scale multiplies the 3D resolution further.")
		},
		{
			"Spec",
			new GraphicsHelpInfo("Texture", "Controls texture detail. Higher settings retain more detail and use more video memory. Lower settings use smaller texture mip levels and may help when video memory is limited.\nThis is separate from output resolution, Render Scale and Anti-Aliasing.")
		}
	};

	private static readonly string[] scaleChoices = new string[5] { "100%", "125%", "150%", "175%", "200%" };

	public static readonly FilterField[] FilterFields = new FilterField[13]
	{
		new FilterField("Palette X", -5, 5),
		new FilterField("Palette Y", -5, 5),
		new FilterField("Color Temperature", -50, 50, "K"),
		new FilterField("Sharpness", 0, 50),
		new FilterField("Brightness", -50, 50),
		new FilterField("Contrast", -50, 50),
		new FilterField("Saturation", -50, 50),
		new FilterField("Grayscale", -50, 50),
		new FilterField("Shadows", -50, 50),
		new FilterField("Bloom", -50, 50),
		new FilterField("Vignette", 0, 50),
		new FilterField("Film Grain", 0, 50),
		new FilterField("Filter Intensity", 0, 100)
	};

	public const int FilterFieldIntensity = 12;

	public static readonly string[] FilterPresetNames = new string[8] { "Default", "Vivid", "Soft", "Film", "Dystopian", "Noir", "Warm", "Cool" };

	private static readonly int[][] filterPresetLooks = new int[8][]
	{
		new int[13]
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 100
		},
		new int[13]
		{
			0, 0, 0, 10, 0, 30, 40, -15, -20, 0,
			0, 0, 60
		},
		new int[13]
		{
			0, 1, 5, 0, 10, -20, -10, 0, 15, 20,
			5, 0, 70
		},
		new int[13]
		{
			0, 0, 10, 0, 0, 15, -15, 0, 10, 0,
			20, 25, 80
		},
		new int[13]
		{
			-2, -2, -25, 10, 0, 25, -30, 0, -15, 0,
			25, 5, 80
		},
		new int[13]
		{
			0, 0, 0, 15, 0, 40, 0, 50, -25, 0,
			35, 15, 100
		},
		new int[13]
		{
			1, 1, 30, 0, 5, 5, 10, 0, 0, 10,
			0, 0, 70
		},
		new int[13]
		{
			0, 0, -30, 0, 0, 10, 5, 0, 0, 0,
			0, 0, 70
		}
	};

	public const int DefaultFilterIntensity = 50;

	private static int previewFilterPreset = -1;

	private static FilterValues previewFilterValues;

	private static int worldFilterLayer = -1;

	private static int previewFilterLayer = -1;

	private FilterStack worldFilter;

	private FilterStack previewFilter;

	private readonly Dictionary<Camera, Camera> filterCameras = new Dictionary<Camera, Camera>();

	private readonly HashSet<Camera> filterCameraSet = new HashSet<Camera>();

	private readonly Dictionary<Camera, bool> filterAnchors = new Dictionary<Camera, bool>();

	private readonly Dictionary<RenderTexture, Camera> previewAnchors = new Dictionary<RenderTexture, Camera>();

	private readonly List<Camera> removeFilterCameras = new List<Camera>();

	private string appliedFilter;

	private string filterStatus = "Inactive";

	private PropertySheet discardAlpha;

	private PropertySheet copySheet;

	private CommandBuffer opaqueAlpha;

	public const string GlobalFilterRowName = "SPRK Global Filter";

	public static bool FilterPanelOpen;

	private static int thumbFilterLayer = -1;

	private RenderTexture filterSnapshot;

	private CommandBuffer snapshotCommand;

	private Camera snapshotOwner;

	private int snapshotFrame = -100;

	private FilterStack thumbFilter;

	private Camera thumbCamera;

	private Texture2D fallbackSnapshot;

	public static string Status
	{
		get
		{
			if (!(instance == null))
			{
				return instance.status;
			}
			return "Not initialized";
		}
	}

	public int RetainedWorldReuses { get; private set; }

	private static long RetainedWorldBudget => System.Math.Min(1024L, (long)System.Math.Max(0, SystemInfo.graphicsMemorySize) / 8L) * 1024 * 1024;

	private static int Method
	{
		get
		{
			EnsurePreset();
			return Mathf.Clamp(SprkTestPrefs.GetInt("Sprk.Graphics.Method", 4), 0, 4);
		}
	}

	public static int Samples
	{
		get
		{
			if (Method != 3)
			{
				return 1;
			}
			return SelectedSamples();
		}
	}

	public static int SkippedPreviewUpdates { get; private set; }

	public static int SkippedPreviewVisualUpdates { get; private set; }

	public static float ParticleDensity => 1f - 0.25f * (float)Mathf.Clamp(SprkTestPrefs.GetInt("Sprk.Graphics.ParticleDensity", 0), 0, 2);

	private static bool CustomWindow => SystemOptionData.Graphic_ResolutionControl == EResolutionControl.Custom;

	private static int AdaptiveBattleTarget => adaptiveFrameTargets[ReadIndex("AdaptiveBattleScale")];

	public static int CurrentEffectCoverage
	{
		get
		{
			if (!(instance != null) || !instance.battleActive)
			{
				return 0;
			}
			return ReadIndex("EffectCoverage");
		}
	}

	public static int EffectiveWorldScale
	{
		get
		{
			if (!(instance != null) || !instance.battleActive || ReadIndex("AdaptiveBattleScale") == 0)
			{
				return Scale;
			}
			return instance.adaptiveBattle.Current;
		}
	}

	private static int RenderScalePreset
	{
		get
		{
			EnsureScaleSettings();
			int num = SprkTestPrefs.GetInt("Sprk.Graphics.RenderScale", 100);
			if (num != 0)
			{
				return NormalizeScale(num);
			}
			return 0;
		}
	}

	private static bool CustomScaleMode => RenderScalePreset == 0;

	public static int Scale
	{
		get
		{
			int renderScalePreset = RenderScalePreset;
			if (renderScalePreset != 0)
			{
				return renderScalePreset;
			}
			return NormalizeScale(SprkTestPrefs.GetInt("Sprk.Graphics.Scale", 100));
		}
	}

	private static int PreviewScale
	{
		get
		{
			int renderScalePreset = RenderScalePreset;
			if (renderScalePreset != 0)
			{
				return renderScalePreset;
			}
			return NormalizeScale(SprkTestPrefs.GetInt("Sprk.Graphics.PreviewScale", 100));
		}
	}

	public static int FilterPreset
	{
		get
		{
			return Mathf.Clamp(SprkTestPrefs.GetInt("Sprk.Graphics.Filter.Preset", 0), 0, FilterPresetNames.Length - 1);
		}
		set
		{
			SprkTestPrefs.SetInt("Sprk.Graphics.Filter.Preset", Mathf.Clamp(value, 0, FilterPresetNames.Length - 1));
			SprkTestPrefs.Save();
		}
	}

	public static int ActiveFilterPreset
	{
		get
		{
			if (previewFilterPreset < 0)
			{
				return FilterPreset;
			}
			return previewFilterPreset;
		}
	}

	public static FilterValues ActiveFilterValues
	{
		get
		{
			if (previewFilterPreset < 0 || previewFilterValues == null)
			{
				return SavedFilter(ActiveFilterPreset);
			}
			return previewFilterValues;
		}
	}

	public static bool GlobalFilterActive
	{
		get
		{
			if (!FilterPanelOpen)
			{
				return FilterHasEffect(ActiveFilterPreset, ActiveFilterValues);
			}
			return true;
		}
	}

	public static string FilterStatus
	{
		get
		{
			if (!(instance == null))
			{
				return instance.filterStatus;
			}
			return "Not initialized";
		}
	}

	public static int FilterCameraCount
	{
		get
		{
			if (!(instance == null))
			{
				return instance.filterCameras.Count((KeyValuePair<Camera, Camera> p) => p.Value != null && p.Value.enabled);
			}
			return 0;
		}
	}

	public static int WorldFilterMask
	{
		get
		{
			if (worldFilterLayer >= 0)
			{
				return 1 << worldFilterLayer;
			}
			return 0;
		}
	}

	public static int PreviewFilterMask
	{
		get
		{
			if (previewFilterLayer >= 0)
			{
				return 1 << previewFilterLayer;
			}
			return 0;
		}
	}

	public static int ThumbFilterMask
	{
		get
		{
			if (thumbFilterLayer >= 0)
			{
				return 1 << thumbFilterLayer;
			}
			return 0;
		}
	}

	public static bool HasFilterSnapshot
	{
		get
		{
			if (instance != null && instance.filterSnapshot != null)
			{
				return Time.frameCount - instance.snapshotFrame < 5;
			}
			return false;
		}
	}

	public static float SnapshotAspect
	{
		get
		{
			if (Screen.height > 0)
			{
				return (float)Screen.width / (float)Screen.height;
			}
			return 1.7777778f;
		}
	}

	public static bool HasMainRenderTarget(Camera camera)
	{
		if (!(camera.targetTexture == null))
		{
			if (instance != null && instance.world != null && camera.targetTexture == instance.world)
			{
				return instance.redirected.ContainsKey(camera);
			}
			return false;
		}
		return true;
	}

	private void OnEnable()
	{
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(BeforeUI));
		UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;
	}

	private void OnDisable()
	{
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(BeforeUI));
		UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneChanged;
		ReleaseFilterCameras();
		RestoreTargets();
		ReleaseWorld();
		RestoreFinalBlits();
		ResetBattleControls();
		foreach (SprkParticleDensity particleBudget in particleBudgets)
		{
			if (particleBudget != null)
			{
				particleBudget.Apply(1f, 0);
			}
		}
		foreach (KeyValuePair<Camera, PostProcessLayer> layer in layers)
		{
			if (layer.Key != null && layer.Value != null)
			{
				UpdateTemporalResources(layer.Key, layer.Value, 0);
			}
		}
		foreach (PostProcessLayer helperLayer in helperLayers)
		{
			if (helperLayer != null)
			{
				helperLayer.enabled = false;
			}
		}
		foreach (KeyValuePair<Camera, CommandBuffer> presentationCommand in presentationCommands)
		{
			if (presentationCommand.Key != null)
			{
				presentationCommand.Key.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, presentationCommand.Value);
			}
			presentationCommand.Value.Release();
		}
		presentationCommands.Clear();
	}

	private void SceneChanged(Scene a, Scene b)
	{
		ResetBattleControls();
		ResetHistories();
		RestoreTargets();
		ReleaseWorld();
		RestoreFinalBlits();
		PruneCaches();
	}

	private void OnApplicationFocus(bool focused)
	{
		if (focused)
		{
			SprkTestPrefs.ClearGraphicsCache();
			nextWindowStyleCheck = 0f;
		}
	}

	private void UpdateTemporalResources(Camera camera, PostProcessLayer layer, int effective)
	{
		if (!this.temporalResources.TryGetValue(camera, out var value))
		{
			value = (this.temporalResources[camera] = new TemporalResources());
		}
		if (effective == 3)
		{
			if (!value.active)
			{
				value.owned = (DepthTextureMode.Depth | DepthTextureMode.MotionVectors) & ~camera.depthTextureMode;
			}
			value.active = true;
		}
		else if (value.active)
		{
			releaseTemporalHistory.Invoke(layer.temporalAntialiasing, null);
			camera.depthTextureMode &= ~value.owned;
			value.active = false;
			value.owned = DepthTextureMode.None;
		}
	}

	private void ResetHistories()
	{
		foreach (PostProcessLayer value in layers.Values)
		{
			if (value != null)
			{
				value.ResetHistory();
			}
		}
	}

	private void RestoreTargets()
	{
		foreach (CommandBuffer value in presentationCommands.Values)
		{
			value.Clear();
		}
		foreach (KeyValuePair<Camera, RenderTexture> item in redirected)
		{
			if (item.Key != null && item.Key.targetTexture == world)
			{
				item.Key.targetTexture = item.Value;
			}
		}
		redirected.Clear();
		lastWorld = null;
		presentationCamera = null;
		presentationFrame = -1;
	}

	private void ReleaseWorld()
	{
		DestroyWorldTarget(world);
		world = null;
		ReleaseRetainedWorlds();
	}

	private static void DestroyWorldTarget(RenderTexture target)
	{
		if (target != null)
		{
			ownedTargets.Remove(target);
			target.Release();
			UnityEngine.Object.Destroy(target);
		}
	}

	private static long WorldTargetBytes(RenderTexture rt)
	{
		return (long)rt.width * (long)rt.height * Mathf.Max(1, rt.antiAliasing) * 8;
	}

	private void ReleaseRetainedWorlds()
	{
		foreach (RenderTexture retainedWorld in retainedWorlds)
		{
			DestroyWorldTarget(retainedWorld);
		}
		retainedWorlds.Clear();
	}

	private void RetireWorld(bool retain)
	{
		if (world == null)
		{
			return;
		}
		if (!retain || !world.IsCreated() || !ownedTargets.ContainsKey(world))
		{
			DestroyWorldTarget(world);
			world = null;
			return;
		}
		retainedWorlds.Insert(0, world);
		world = null;
		long retainedWorldBudget = RetainedWorldBudget;
		long num = 0L;
		for (int i = 0; i < retainedWorlds.Count; i++)
		{
			RenderTexture renderTexture = retainedWorlds[i];
			long num2 = ((renderTexture == null || !renderTexture.IsCreated()) ? 0 : WorldTargetBytes(renderTexture));
			if (num2 == 0L || i >= 2 || num + num2 > retainedWorldBudget)
			{
				DestroyWorldTarget(renderTexture);
				retainedWorlds.RemoveAt(i);
				i--;
			}
			else
			{
				num += num2;
			}
		}
	}

	private RenderTexture TakeRetainedWorld(int width, int height, int scale, int samples)
	{
		for (int i = 0; i < retainedWorlds.Count; i++)
		{
			RenderTexture renderTexture = retainedWorlds[i];
			if (!(renderTexture == null) && renderTexture.IsCreated() && ownedTargets.TryGetValue(renderTexture, out var value) && value.width == width && value.height == height && value.scale == scale && value.samples == samples)
			{
				retainedWorlds.RemoveAt(i);
				RetainedWorldReuses++;
				return renderTexture;
			}
		}
		return null;
	}

	private void BeforeUI(Camera camera)
	{
		if (world == null || !world.IsCreated() || lastWorld == null || camera.depth <= lastWorld.depth || camera.targetTexture != null || !IsOverlayCamera(camera) || presentationFrame == Time.frameCount)
		{
			return;
		}
		presentationFrame = Time.frameCount;
		if (presentationCamera != camera)
		{
			foreach (CommandBuffer value2 in presentationCommands.Values)
			{
				value2.Clear();
			}
			presentationCamera = camera;
		}
		if (!presentationCommands.TryGetValue(camera, out var value))
		{
			value = new CommandBuffer
			{
				name = "SPRK world before UI"
			};
			presentationCommands[camera] = value;
			camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, value);
		}
		if (value.sizeInBytes == 0)
		{
			value.Blit(world, BuiltinRenderTextureType.CameraTarget);
		}
	}

	private bool FindResources()
	{
		if (resources != null)
		{
			return true;
		}
		GameObject gameObject = Resources.Load<GameObject>("Prefab2/Render3D/RenderTargetCinema");
		if (gameObject != null)
		{
			PostProcessLayer componentInChildren = gameObject.GetComponentInChildren<PostProcessLayer>(includeInactive: true);
			if (componentInChildren != null)
			{
				resources = (PostProcessResources)resourceField.GetValue(componentInChildren);
			}
		}
		if (resources == null)
		{
			resources = Resources.FindObjectsOfTypeAll<PostProcessResources>().FirstOrDefault();
		}
		if (resources != null)
		{
			return resources.shaders != null;
		}
		return false;
	}

	public static void PollComparisonKeys(HeroFullSceneViewBase window)
	{
	}

	private static void EnsurePreset()
	{
		if (preferencesReady)
		{
			return;
		}
		preferencesReady = true;
		if (!SprkTestPrefs.HasKey("Sprk.Graphics.Method"))
		{
			int num = 1;
			if (SprkTestPrefs.HasKey("Sprk.Graphics.Preset"))
			{
				num = ((SprkTestPrefs.GetInt("Sprk.Graphics.Preset", 3) > 0) ? 1 : 0);
			}
			else if (SprkTestPrefs.HasKey("Sprk.Graphics.PostAA"))
			{
				num = ((SprkTestPrefs.GetInt("Sprk.Graphics.PostAA", 0) > 0 || SprkTestPrefs.GetInt("Sprk.Graphics.MSAA", 0) > 0) ? 1 : 0);
			}
			else if (SprkTestPrefs.HasKey("Sprk.Graphics.MSAA"))
			{
				num = ((SprkTestPrefs.GetInt("Sprk.Graphics.MSAA", 0) > 0) ? 1 : 0);
			}
			SprkTestPrefs.SetInt("Sprk.Graphics.Method", (num != 0) ? 4 : 0);
			SprkTestPrefs.Save();
		}
	}

	private static void ApplyPreset(int method)
	{
		EnsurePreset();
		method = Mathf.Clamp(method, 0, 4);
		SprkTestPrefs.SetInt("Sprk.Graphics.Method", method);
		SprkTestPrefs.SetInt("Sprk.Graphics.Preset", (method != 0) ? 3 : 0);
		SprkTestPrefs.SetInt("Sprk.Graphics.PostAA", postModes[method]);
		SprkTestPrefs.SetInt("Sprk.Graphics.MSAA", (method == 3) ? Samples : 0);
		SprkTestPrefs.Save();
	}

	private static int[] SupportedSamples()
	{
		if (supportedSamples != null)
		{
			return supportedSamples;
		}
		RenderTextureDescriptor descriptor = new RenderTextureDescriptor(128, 128, RenderTextureFormat.ARGB32, 24);
		return supportedSamples = new int[3] { 2, 4, 8 }.Where(delegate(int n)
		{
			descriptor.msaaSamples = n;
			return SystemInfo.GetRenderTextureSupportedMSAASampleCount(descriptor) == n;
		}).ToArray();
	}

	private static int SelectedSamples()
	{
		int[] array = SupportedSamples();
		if (array.Length == 0)
		{
			return 1;
		}
		int num = SprkTestPrefs.GetInt("Sprk.Graphics.MSAASamples", 4);
		int result = array[0];
		int[] array2 = array;
		foreach (int num2 in array2)
		{
			if (num2 <= num)
			{
				result = num2;
			}
		}
		return result;
	}

	public static void ObserveParticles(Component root)
	{
		if (root == null)
		{
			return;
		}
		if (root is ParticleObject && (bool)distanceEmitter.GetValue(root))
		{
			ReleaseParticles(root);
			return;
		}
		SprkParticleDensity sprkParticleDensity = root.GetComponent<SprkParticleDensity>();
		if (sprkParticleDensity == null)
		{
			sprkParticleDensity = root.gameObject.AddComponent<SprkParticleDensity>();
			particleBudgets.Add(sprkParticleDensity);
		}
		sprkParticleDensity.Capture();
		ApplyParticleBudget(sprkParticleDensity);
	}

	public static void ReleaseParticles(Component root)
	{
		if (!(root == null))
		{
			SprkParticleDensity component = root.GetComponent<SprkParticleDensity>();
			if (component != null)
			{
				component.Clear();
			}
		}
	}

	public static bool ObserveAssetParticles(bool opened, object instance)
	{
		if (!opened)
		{
			return false;
		}
		GameObject gameObject = assetParticleRoot.GetValue(instance, null) as GameObject;
		if (gameObject != null)
		{
			ObserveParticles(gameObject.transform);
		}
		return true;
	}

	public static void ReleaseAssetParticles(object instance)
	{
		GameObject gameObject = assetParticleRoot.GetValue(instance, null) as GameObject;
		if (gameObject != null)
		{
			ReleaseParticles(gameObject.transform);
		}
	}

	private void UpdateParticleDensity()
	{
		UpdateBattleEffects();
	}

	public static bool ShouldUpdatePreview(Component root)
	{
		if (root == null || !root.gameObject.activeInHierarchy)
		{
			SkippedPreviewUpdates++;
			return false;
		}
		Transform transform = root.transform;
		if (transform.childCount == 0)
		{
			return true;
		}
		for (int i = 0; i < transform.childCount; i++)
		{
			if (transform.GetChild(i).gameObject.activeInHierarchy)
			{
				return true;
			}
		}
		SkippedPreviewUpdates++;
		return false;
	}

	public static void UpdatePreviewCreature(ICreature creature, RenderFacade facade)
	{
		if (facade != null && !facade.gameObject.activeInHierarchy && (facade.GetComponentInParent<HeroPreview>(includeInactive: true) != null || facade.GetComponentInParent<RenderTargetCinema>(includeInactive: true) != null || facade.GetComponentInParent<Cinema>(includeInactive: true) != null))
		{
			SkippedPreviewVisualUpdates++;
		}
		else
		{
			creature.ChainUpdate();
		}
	}

	private static bool ConfigureTarget(RenderTexture rt, TargetSize size, int samples)
	{
		int num = (size.world ? EffectiveWorldScale : PreviewScale);
		if (size.scale == num && size.samples == samples && rt.width == size.actualWidth && rt.height == size.actualHeight && rt.antiAliasing == size.actualSamples)
		{
			return false;
		}
		int num2 = Mathf.Min(SystemInfo.maxTextureSize, 8192);
		float num3 = Mathf.Min((float)num / 100f, Mathf.Min((float)num2 / (float)Mathf.Max(1, size.width), (float)num2 / (float)Mathf.Max(1, size.height)));
		int num4 = Mathf.Max(1, Mathf.RoundToInt((float)size.width * num3));
		int num5 = Mathf.Max(1, Mathf.RoundToInt((float)size.height * num3));
		RenderTextureDescriptor descriptor = rt.descriptor;
		descriptor.width = num4;
		descriptor.height = num5;
		descriptor.msaaSamples = samples;
		int renderTextureSupportedMSAASampleCount = SystemInfo.GetRenderTextureSupportedMSAASampleCount(descriptor);
		size.scale = num;
		size.samples = samples;
		size.actualWidth = num4;
		size.actualHeight = num5;
		size.actualSamples = renderTextureSupportedMSAASampleCount;
		if (rt.width == num4 && rt.height == num5 && rt.antiAliasing == renderTextureSupportedMSAASampleCount)
		{
			return false;
		}
		bool num6 = rt.IsCreated();
		if (num6)
		{
			rt.Release();
		}
		rt.width = num4;
		rt.height = num5;
		rt.antiAliasing = renderTextureSupportedMSAASampleCount;
		if (num6)
		{
			rt.Create();
		}
		return true;
	}

	public static RenderTexture CreateTarget(int width, int height, int depth)
	{
		return AllocateTarget(width, height, depth, isWorld: false);
	}

	private static RenderTexture CreateWorldTarget(int width, int height, int depth)
	{
		return AllocateTarget(width, height, depth, isWorld: true);
	}

	private static RenderTexture AllocateTarget(int width, int height, int depth, bool isWorld)
	{
		RenderTexture renderTexture = CreateNativeTarget(width, height, depth);
		TargetSize targetSize = new TargetSize(width, height)
		{
			world = isWorld
		};
		ownedTargets[renderTexture] = targetSize;
		ConfigureTarget(renderTexture, targetSize, Samples);
		return renderTexture;
	}

	public static RenderTexture CreateNativeTarget(int width, int height, int depth)
	{
		return new RenderTexture(Mathf.Max(1, width), Mathf.Max(1, height), depth)
		{
			filterMode = FilterMode.Bilinear
		};
	}

	public static RenderTexture CreateScreenTarget()
	{
		RenderTexture renderTexture = FilterBase.CreateScreenRenderTarget();
		TargetSize targetSize = new TargetSize(renderTexture.width, renderTexture.height);
		ownedTargets[renderTexture] = targetSize;
		ConfigureTarget(renderTexture, targetSize, Samples);
		return renderTexture;
	}

	public static void Ensure()
	{
		if (!(instance != null))
		{
			EnsurePreset();
			GameObject obj = new GameObject("SprkGraphicsOptions");
			UnityEngine.Object.DontDestroyOnLoad(obj);
			instance = obj.AddComponent<SprkGraphicsOptions>();
			if (CustomWindow && !Screen.fullScreen && SystemOptionData.Graphic_WindowControl)
			{
				ApplyCustomResolution();
			}
			if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_GRAPHICS_TEST")) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE")) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR")))
			{
				instance.StartCoroutine(instance.Probe());
			}
		}
	}

	public static void Setup(GameOptionWindow window)
	{
		Ensure();
		GameOptionWindow.TabUIInfo[] mainTabs = window.MainTabs;
		foreach (GameOptionWindow.TabUIInfo tabUIInfo in mainTabs)
		{
			if (tabUIInfo.GameOptionTab == GameOptionTab.Game)
			{
				Add(tabUIInfo, "Anti-Aliasing", "Preset", new string[5] { "Off", "FXAA", "SMAA", "MSAA", "TAA" });
				Add(tabUIInfo, "FXAA Quality", "FXAAQuality", new string[2] { "Fast", "Standard" });
				Add(tabUIInfo, "SMAA Quality", "SMAAQuality", new string[3] { "Low", "Medium", "High" });
				int[] array = SupportedSamples();
				Add(tabUIInfo, "MSAA Samples", "MSAASamples", (array.Length != 0) ? array.Select((int n) => n + "x").ToArray() : new string[1] { "Unavailable" });
				Add(tabUIInfo, "TAA Sharpness", "TAASharpness", new string[3] { "Low", "Medium", "High" });
				Add(tabUIInfo, "Render Scale", "RenderScale", new string[6] { "100%", "125%", "150%", "175%", "200%", "Custom" });
				Add(tabUIInfo, "World Scale", "Scale", scaleChoices);
				Add(tabUIInfo, "Preview Scale", "PreviewScale", scaleChoices);
				Add(tabUIInfo, "Particle Density", "ParticleDensity", new string[3] { "Full", "Reduced", "Minimal" });
				Add(tabUIInfo, "Battle Effects", "BattleEffects", new string[3] { "Full", "Balanced", "Minimal" });
				Add(tabUIInfo, "Effect Coverage", "EffectCoverage", new string[3] { "Full", "Reduced", "Minimal" });
				Add(tabUIInfo, "Adaptive Battle Scale", "AdaptiveBattleScale", new string[5] { "Off", "30 FPS", "60 FPS", "120 FPS", "144 FPS" });
				Add(tabUIInfo, "Lobby Camera Sensitivity", "LobbyCameraSensitivity", new string[5] { "50%", "75%", "100%", "125%", "150%" });
				Add(tabUIInfo, "Model Camera Sensitivity", "ModelCameraSensitivity", new string[5] { "50%", "75%", "100%", "125%", "150%" });
				AddGlobalFilterRow(tabUIInfo);
				GameOptionToggle4Component[] componentsInChildren = tabUIInfo.GameOptionGrid.GetComponentsInChildren<GameOptionToggle4Component>(includeInactive: true);
				for (int num = 0; num < componentsInChildren.Length; num++)
				{
					ConfigureFrameRow(componentsInChildren[num]);
				}
				GameOptionToggle3Component[] componentsInChildren2 = tabUIInfo.GameOptionGrid.GetComponentsInChildren<GameOptionToggle3Component>(includeInactive: true);
				for (int num = 0; num < componentsInChildren2.Length; num++)
				{
					CenterTextureChoices(componentsInChildren2[num]);
				}
				GameOptionBaseComponent[] componentsInChildren3 = tabUIInfo.GameOptionGrid.GetComponentsInChildren<GameOptionBaseComponent>(includeInactive: true);
				for (int num = 0; num < componentsInChildren3.Length; num++)
				{
					ConfigureGraphicsHelp(componentsInChildren3[num]);
				}
				RefreshRows(tabUIInfo);
			}
		}
	}

	private static void RefreshRows(GameOptionWindow.TabUIInfo tab)
	{
		GameOptionBaseComponent[] componentsInChildren = tab.GameOptionGrid.GetComponentsInChildren<GameOptionBaseComponent>(includeInactive: true);
		int i;
		for (i = 0; i < adjustmentNames.Length; i++)
		{
			GameOptionBaseComponent gameOptionBaseComponent = componentsInChildren.FirstOrDefault((GameOptionBaseComponent r) => r.name == "SPRK " + adjustmentNames[i]);
			if (gameOptionBaseComponent != null)
			{
				gameOptionBaseComponent.gameObject.SetActive(Method == i + 1);
			}
		}
		PlaceBelowScreen(tab);
		RefreshScaleChildren(tab);
		tab.GameOptionGrid.Grid.hideInactive = true;
		tab.GameOptionGrid.RepositionGrid();
	}

	private static void RefreshRows(GameOptionWindow window)
	{
		if (window == null)
		{
			return;
		}
		GameOptionWindow.TabUIInfo[] mainTabs = window.MainTabs;
		foreach (GameOptionWindow.TabUIInfo tabUIInfo in mainTabs)
		{
			if (tabUIInfo.GameOptionTab == GameOptionTab.Game)
			{
				RefreshRows(tabUIInfo);
			}
		}
	}

	public static bool IsFrameRow(GameOptionToggle4Component row)
	{
		if (row.gameOptionData != null)
		{
			if (!(row.gameOptionData.FieldName == "Graphic_FrameControl"))
			{
				return row.gameOptionData.FieldName == "Graphic_ResolutionControl";
			}
			return true;
		}
		return false;
	}

	public static bool IsResolutionRow(GameOptionToggle4Component row)
	{
		if (row.gameOptionData != null && row.gameOptionData.FieldName == "Graphic_ResolutionControl")
		{
			return !string.IsNullOrEmpty(row.gameOptionData.Value4);
		}
		return false;
	}

	public static bool ConfigureFrameRow(GameOptionToggle4Component row)
	{
		if (!IsFrameRow(row))
		{
			return false;
		}
		if (row.gameOptionData.FieldName == "Graphic_ResolutionControl")
		{
			ConfigureChoices(row, "Resolution", new string[5] { "HD", "FHD", "QHD", "UHD", "Custom" });
			return true;
		}
		ConfigureChoices(row, "Frame", new string[5] { "30", "60", "144", "Auto", "Auto+" });
		return true;
	}

	public static void RestoreResolutionChoices(GameOptionToggle4Component row)
	{
		if (!(row == null) && row.gameOptionData != null && !(row.gameOptionData.FieldName != "Graphic_ResolutionControl") && row.toggleInfo != null && row.toggleInfo.Toggle4 != null)
		{
			row.toggleInfo.Toggle4.gameObject.SetActive(!string.IsNullOrEmpty(row.gameOptionData.Value4));
			if (!string.IsNullOrEmpty(row.gameOptionData.Value4))
			{
				ConfigureFrameRow(row);
			}
		}
	}

	[DllImport("user32.dll")]
	private static extern bool EnumWindows(WindowVisitor visitor, IntPtr state);

	[DllImport("user32.dll")]
	private static extern uint GetWindowThreadProcessId(IntPtr window, out uint pid);

	[DllImport("user32.dll")]
	private static extern bool IsWindowVisible(IntPtr window);

	[DllImport("user32.dll")]
	private static extern bool IsWindow(IntPtr window);

	[DllImport("user32.dll")]
	private static extern bool IsIconic(IntPtr window);

	[DllImport("user32.dll")]
	private static extern bool IsZoomed(IntPtr window);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern int GetClassName(IntPtr window, StringBuilder name, int length);

	[DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
	private static extern int GetWindowLong(IntPtr window, int index);

	[DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
	private static extern int SetWindowLong(IntPtr window, int index, int value);

	[DllImport("user32.dll")]
	private static extern bool GetClientRect(IntPtr window, out WindowRect rect);

	[DllImport("user32.dll")]
	private static extern bool GetWindowRect(IntPtr window, out WindowRect rect);

	[DllImport("user32.dll")]
	private static extern bool AdjustWindowRectEx(ref WindowRect rect, int style, bool menu, int extendedStyle);

	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(IntPtr window, IntPtr after, int x, int y, int width, int height, uint flags);

	public static bool ApplyCustomResolution()
	{
		if (Application.platform == RuntimePlatform.WindowsPlayer && !SystemOptionData.Graphic_WindowControl)
		{
			return true;
		}
		if (!CustomWindow)
		{
			return false;
		}
		Ensure();
		if (SystemOptionData.Graphic_WindowControl)
		{
			int width = Mathf.Max(320, SprkTestPrefs.GetInt("Sprk.Window.Width", Screen.width));
			int height = Mathf.Max(240, SprkTestPrefs.GetInt("Sprk.Window.Height", Screen.height));
			Screen.SetResolution(width, height, FullScreenMode.Windowed);
		}
		return true;
	}

	private IntPtr FindGameWindow()
	{
		if (gameWindow != IntPtr.Zero && IsWindow(gameWindow))
		{
			return gameWindow;
		}
		gameWindow = IntPtr.Zero;
		resizeStyleCaptured = false;
		uint ownPid = (uint)Process.GetCurrentProcess().Id;
		EnumWindows(delegate(IntPtr window, IntPtr state)
		{
			GetWindowThreadProcessId(window, out var pid);
			if (pid != ownPid || !IsWindowVisible(window))
			{
				return true;
			}
			StringBuilder stringBuilder = new StringBuilder(64);
			GetClassName(window, stringBuilder, stringBuilder.Capacity);
			if (stringBuilder.ToString() != "UnityWndClass")
			{
				return true;
			}
			gameWindow = window;
			return false;
		}, IntPtr.Zero);
		return gameWindow;
	}

	private void UpdateCustomWindow()
	{
		if (Application.platform != RuntimePlatform.WindowsPlayer || Screen.fullScreen)
		{
			return;
		}
		bool customWindow = CustomWindow;
		bool flag = !resizeStyleCaptured || customWindow != lastCustomWindow || Time.realtimeSinceStartup >= nextWindowStyleCheck;
		if (!flag && !customWindow)
		{
			return;
		}
		IntPtr intPtr = FindGameWindow();
		if (intPtr == IntPtr.Zero)
		{
			return;
		}
		if (flag || !resizeStyleCaptured)
		{
			nextWindowStyleCheck = Time.realtimeSinceStartup + 0.25f;
			lastCustomWindow = customWindow;
			int windowLong = GetWindowLong(intPtr, -16);
			if (!resizeStyleCaptured)
			{
				originalResizeStyle = windowLong & 0x50000;
				resizeStyleCaptured = true;
			}
			int num = (windowLong & -327681) | (customWindow ? 327680 : originalResizeStyle);
			if (windowLong != num && GetClientRect(intPtr, out var rect) && GetWindowRect(intPtr, out var rect2))
			{
				SetWindowLong(intPtr, -16, num);
				AdjustWindowRectEx(ref rect, num, menu: false, GetWindowLong(intPtr, -20));
				SetWindowPos(intPtr, IntPtr.Zero, rect2.Left, rect2.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, 52u);
			}
		}
		if (customWindow && !IsIconic(intPtr) && !IsZoomed(intPtr) && Screen.width >= 320 && Screen.height >= 240)
		{
			if (savedWidth != Screen.width || savedHeight != Screen.height)
			{
				savedWidth = Screen.width;
				savedHeight = Screen.height;
				saveSizeAt = Time.realtimeSinceStartup + 0.5f;
			}
			else if (saveSizeAt > 0f && Time.realtimeSinceStartup >= saveSizeAt)
			{
				SaveCustomSize();
				saveSizeAt = 0f;
			}
		}
	}

	private void SaveCustomSize()
	{
		if (savedWidth >= 320 && savedHeight >= 240)
		{
			SprkTestPrefs.SetInt("Sprk.Window.Width", savedWidth);
			SprkTestPrefs.SetInt("Sprk.Window.Height", savedHeight);
			SprkTestPrefs.Save();
		}
	}

	private void OnApplicationQuit()
	{
		if (CustomWindow && !Screen.fullScreen)
		{
			SaveCustomSize();
		}
	}

	private static void PlaceBelowScreen(GameOptionWindow.TabUIInfo tab)
	{
		GameOptionBaseComponent[] componentsInChildren = tab.GameOptionGrid.GetComponentsInChildren<GameOptionBaseComponent>(includeInactive: true);
		GameOptionBaseComponent gameOptionBaseComponent = componentsInChildren.FirstOrDefault((GameOptionBaseComponent r) => r.gameOptionData != null && r.gameOptionData.FieldName == "Graphic_WindowControl");
		GameOptionBaseComponent gameOptionBaseComponent2 = componentsInChildren.FirstOrDefault((GameOptionBaseComponent r) => r.gameOptionData != null && r.gameOptionData.OptionName == "OPTION_GAME_RESOULUTION");
		GameOptionBaseComponent gameOptionBaseComponent3 = componentsInChildren.FirstOrDefault((GameOptionBaseComponent r) => r.name == "SPRK Anti-Aliasing");
		GameOptionBaseComponent gameOptionBaseComponent4 = componentsInChildren.FirstOrDefault((GameOptionBaseComponent r) => r.name == "SPRK Render Scale");
		UIGrid grid = tab.GameOptionGrid.Grid;
		if (gameOptionBaseComponent == null || gameOptionBaseComponent3 == null || gameOptionBaseComponent4 == null || grid == null)
		{
			return;
		}
		bool hideInactive = grid.hideInactive;
		grid.hideInactive = false;
		List<Transform> childList = grid.GetChildList();
		grid.hideInactive = hideInactive;
		List<Transform> list = new List<Transform> { gameOptionBaseComponent3.transform };
		string[] array = adjustmentNames;
		foreach (string title in array)
		{
			GameOptionBaseComponent gameOptionBaseComponent5 = componentsInChildren.FirstOrDefault((GameOptionBaseComponent r) => r.name == "SPRK " + title);
			if (gameOptionBaseComponent5 != null)
			{
				list.Add(gameOptionBaseComponent5.transform);
			}
		}
		list.Add(gameOptionBaseComponent4.transform);
		array = new string[6] { "World Scale", "Preview Scale", "Particle Density", "Battle Effects", "Effect Coverage", "Adaptive Battle Scale" };
		foreach (string title2 in array)
		{
			GameOptionBaseComponent gameOptionBaseComponent6 = componentsInChildren.FirstOrDefault((GameOptionBaseComponent r) => r.name == "SPRK " + title2);
			if (gameOptionBaseComponent6 != null)
			{
				list.Add(gameOptionBaseComponent6.transform);
			}
		}
		array = new string[3] { "Lobby Camera Sensitivity", "Model Camera Sensitivity", "Global Filter" };
		foreach (string title3 in array)
		{
			GameOptionBaseComponent gameOptionBaseComponent7 = componentsInChildren.FirstOrDefault((GameOptionBaseComponent r) => r.name == "SPRK " + title3);
			if (gameOptionBaseComponent7 != null)
			{
				list.Add(gameOptionBaseComponent7.transform);
			}
		}
		foreach (Transform item in list)
		{
			childList.Remove(item);
		}
		int num2 = childList.IndexOf(gameOptionBaseComponent.transform);
		if (num2 >= 0)
		{
			if (gameOptionBaseComponent2 != null && childList.Remove(gameOptionBaseComponent2.transform))
			{
				num2 = childList.IndexOf(gameOptionBaseComponent.transform);
				childList.Insert(num2 + 1, gameOptionBaseComponent2.transform);
				num2++;
			}
			childList.InsertRange(num2 + 1, list);
			GameOptionBaseComponent gameOptionBaseComponent8 = componentsInChildren.FirstOrDefault((GameOptionBaseComponent r) => r.gameOptionData != null && r.gameOptionData.FieldName == "Graphic_FrameControl");
			GameOptionBaseComponent gameOptionBaseComponent9 = componentsInChildren.FirstOrDefault((GameOptionBaseComponent r) => r.gameOptionData != null && r.gameOptionData.FieldName == "Graphic_VSyncControl");
			if (gameOptionBaseComponent8 != null && gameOptionBaseComponent9 != null && childList.Contains(gameOptionBaseComponent8.transform) && childList.Remove(gameOptionBaseComponent9.transform))
			{
				childList.Insert(childList.IndexOf(gameOptionBaseComponent8.transform) + 1, gameOptionBaseComponent9.transform);
			}
			grid.sorting = UIGrid.Sorting.None;
			typeof(UIGrid).GetField("sorted", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(grid, false);
			for (int num3 = 0; num3 < childList.Count; num3++)
			{
				childList[num3].SetSiblingIndex(num3);
			}
		}
	}

	private static void Add(GameOptionWindow.TabUIInfo tab, string title, string key, string[] choices)
	{
		string rowName = "SPRK " + title;
		GameOptionBaseComponent[] source = (from r in tab.GameOptionGrid.GetComponentsInChildren<GameOptionBaseComponent>(includeInactive: true)
			where r.name == rowName
			select r).ToArray();
		GameOptionToggle4Component row = source.OfType<GameOptionToggle4Component>().FirstOrDefault();
		foreach (GameOptionBaseComponent item in source.Where((GameOptionBaseComponent r) => r != row))
		{
			item.gameObject.SetActive(value: false);
			item.transform.SetParent(null, worldPositionStays: false);
			UnityEngine.Object.Destroy(item.gameObject);
		}
		if (row == null)
		{
			row = UIHelper.AddGridItem(tab.GameOptionGrid, GameOptionToggle4Component.PrefabAuto);
		}
		row.name = rowName;
		row.enabled = false;
		row.ParentRoot.Root.SetActive(value: true);
		row.ChildRoot.Root.SetActive(value: false);
		row.ParentRoot.Label_OptionName.text = title;
		ConfigureGraphicsHelp(row);
		ConfigureChoices(row, key, choices);
	}

	private static void ConfigureChoices(GameOptionToggle4Component row, string key, string[] choices)
	{
		List<UIToggle> toggles = new List<UIToggle>
		{
			row.toggleInfo.Toggle1,
			row.toggleInfo.Toggle2,
			row.toggleInfo.Toggle3,
			row.toggleInfo.Toggle4
		};
		for (int i = 4; i < 6; i++)
		{
			string n = "Toggle" + (i + 1);
			Transform transform = toggles[3].transform.parent.Find(n);
			if (i < choices.Length)
			{
				if (transform == null)
				{
					GameObject obj = NGUITools.AddChild(toggles[3].transform.parent.gameObject, toggles[3].gameObject);
					obj.name = n;
					transform = obj.transform;
				}
				toggles.Add(transform.GetComponent<UIToggle>());
			}
			else if (transform != null)
			{
				UIToggle component = transform.GetComponent<UIToggle>();
				component.onChange.Clear();
				component.Set(state: false);
				transform.gameObject.SetActive(value: false);
			}
		}
		for (int j = 0; j < toggles.Count; j++)
		{
			toggles[j].onChange.Clear();
			if (j >= choices.Length)
			{
				toggles[j].Set(state: false);
			}
			toggles[j].gameObject.SetActive(j < choices.Length);
		}
		toggles = toggles.Take(choices.Length).ToList();
		UIGrid component2 = toggles[0].transform.parent.GetComponent<UIGrid>();
		float num = ((choices.Length <= 3) ? 280f : (840f / (float)(choices.Length - 1)));
		if (component2 != null)
		{
			component2.arrangement = UIGrid.Arrangement.Horizontal;
			component2.pivot = UIWidget.Pivot.TopLeft;
			component2.cellWidth = num;
			component2.maxPerLine = 0;
			component2.hideInactive = true;
		}
		bool syncing = false;
		Action refresh = delegate
		{
			syncing = true;
			int num3 = ReadIndex(key);
			for (int k = 0; k < toggles.Count; k++)
			{
				toggles[k].startsActive = k == num3;
				toggles[k].Set(k == num3);
			}
			syncing = false;
		};
		if (IsScaleChild(key))
		{
			SprkScaleRow sprkScaleRow = row.GetComponent<SprkScaleRow>();
			if (sprkScaleRow == null)
			{
				sprkScaleRow = row.gameObject.AddComponent<SprkScaleRow>();
			}
			sprkScaleRow.Refresh = refresh;
		}
		for (int num2 = 0; num2 < toggles.Count; num2++)
		{
			int choice = num2;
			UIToggle toggle = toggles[num2];
			toggle.group = 0;
			toggle.onChange.Clear();
			UILabel componentInChildren = toggle.GetComponentInChildren<UILabel>(includeInactive: true);
			componentInChildren.text = choices[num2];
			componentInChildren.width = ((choices.Length == 6) ? 90 : ((choices.Length == 5) ? 130 : 180));
			componentInChildren.overflowMethod = UILabel.Overflow.ShrinkContent;
			toggle.transform.localPosition = new Vector3((float)((choices.Length == 2) ? (num2 + 1) : num2) * num, 0f, 0f);
			EventDelegate.Add(toggle.onChange, delegate
			{
				if (!syncing)
				{
					if (IsScaleChild(key) && !CustomScaleMode)
					{
						refresh();
					}
					else
					{
						if (toggle.value)
						{
							if (key == "Preset")
							{
								ApplyPreset(choice);
							}
							else if (key == "Frame")
							{
								SystemOptionData.Graphic_FrameControl = (EFrameControl)frameValues[choice];
								SprkTestPrefs.Save();
							}
							else if (key == "Resolution")
							{
								if (CustomWindow)
								{
									instance.SaveCustomSize();
								}
								SystemOptionData.Graphic_ResolutionControl = (EResolutionControl)resolutionValues[choice];
								SprkTestPrefs.Save();
							}
							else
							{
								int value = ((!(key == "RenderScale")) ? (IsScaleChild(key) ? (100 + 25 * choice) : ((key == "AdaptiveBattleScale") ? adaptivePreferenceValues[choice] : (IsSensitivityKey(key) ? (50 + 25 * choice) : ((!(key == "MSAASamples")) ? choice : ((SupportedSamples().Length == 0) ? 1 : SupportedSamples()[choice]))))) : ((choice != 5) ? (100 + 25 * choice) : 0));
								SprkTestPrefs.SetInt("Sprk.Graphics." + key, value);
								SprkTestPrefs.Save();
							}
							if (!IsSensitivityKey(key))
							{
								instance.ResetHistories();
							}
						}
						refresh();
						if (key != "Frame" && key != "Resolution" && !IsSensitivityKey(key))
						{
							RefreshRows(row.GetComponentInParent<GameOptionWindow>(includeInactive: true));
						}
					}
				}
			});
		}
		refresh();
		if (choices.Length <= 3)
		{
			CenterChoices(toggles);
		}
		else if (component2 != null)
		{
			component2.Reposition();
		}
	}

	private static void CenterChoices(IEnumerable<UIToggle> toggles, Transform row = null, float center = 420f)
	{
		UIToggle[] array = toggles.Where((UIToggle t) => t.gameObject.activeSelf).ToArray();
		if (array.Length == 0)
		{
			return;
		}
		UIGrid component = array[0].transform.parent.GetComponent<UIGrid>();
		if (component != null)
		{
			component.arrangement = UIGrid.Arrangement.CellSnap;
			component.cellWidth = 0f;
			component.cellHeight = 0f;
			component.pivot = UIWidget.Pivot.TopLeft;
			component.animateSmoothly = false;
		}
		for (int num = 0; num < array.Length; num++)
		{
			Transform transform = array[num].transform;
			if (row == null)
			{
				Vector3 localPosition = transform.localPosition;
				localPosition.x = center + ((float)num - (float)(array.Length - 1) * 0.5f) * 280f;
				transform.localPosition = localPosition;
			}
			else
			{
				Vector3 position = row.InverseTransformPoint(transform.position);
				position.x = center + ((float)num - (float)(array.Length - 1) * 0.5f) * 280f;
				transform.position = row.TransformPoint(position);
			}
		}
		if (component != null)
		{
			component.Reposition();
		}
	}

	public static void CenterTextureChoices(GameOptionToggle3Component row)
	{
		if (row.gameOptionData == null || row.gameOptionData.FieldName != "Spec")
		{
			return;
		}
		UIHelper.SetLabelText(row.Label_ToggleMiddle, "Medium");
		GameOptionWindow componentInParent = row.GetComponentInParent<GameOptionWindow>(includeInactive: true);
		if (!(componentInParent == null))
		{
			GameOptionToggle4Component gameOptionToggle4Component = componentInParent.GetComponentsInChildren<GameOptionToggle4Component>(includeInactive: true).FirstOrDefault((GameOptionToggle4Component r) => r.name == "SPRK Anti-Aliasing");
			if (!(gameOptionToggle4Component == null))
			{
				Transform transform = gameOptionToggle4Component.toggleInfo.Toggle4.transform.parent.Find("Toggle5");
				float center = (gameOptionToggle4Component.transform.InverseTransformPoint(gameOptionToggle4Component.toggleInfo.Toggle1.transform.position).x + gameOptionToggle4Component.transform.InverseTransformPoint((transform == null) ? gameOptionToggle4Component.toggleInfo.Toggle4.transform.position : transform.position).x) * 0.5f;
				CenterChoices(new UIToggle[3]
				{
					row.toggleInfo.Toggle1,
					row.toggleInfo.Toggle2,
					row.toggleInfo.Toggle3
				}, row.transform, center);
			}
		}
	}

	private static bool IsSensitivityKey(string key)
	{
		if (!(key == "LobbyCameraSensitivity"))
		{
			return key == "ModelCameraSensitivity";
		}
		return true;
	}

	private static int ReadIndex(string key)
	{
		switch (key)
		{
		case "Resolution":
			return Mathf.Max(0, Array.IndexOf(resolutionValues, (int)SystemOptionData.Graphic_ResolutionControl));
		case "Frame":
			return Mathf.Max(0, Array.IndexOf(frameValues, (int)SystemOptionData.Graphic_FrameControl));
		case "Preset":
			return Method;
		case "PostAA":
			return postModes[Method];
		default:
			if (IsSensitivityKey(key))
			{
				return Mathf.Clamp((SprkTestPrefs.GetInt("Sprk.Graphics." + key, SprkTestPrefs.GetInt("Sprk.Graphics.CameraSensitivity", 100)) - 50) / 25, 0, 4);
			}
			switch (key)
			{
			case "Scale":
				return Mathf.Clamp((Scale - 100) / 25, 0, 4);
			case "RenderScale":
				if (RenderScalePreset != 0)
				{
					return (RenderScalePreset - 100) / 25;
				}
				return 5;
			case "PreviewScale":
				return (PreviewScale - 100) / 25;
			case "ParticleDensity":
				return Mathf.Clamp(SprkTestPrefs.GetInt("Sprk.Graphics.ParticleDensity", 0), 0, 2);
			case "AdaptiveBattleScale":
				return Mathf.Max(0, Array.IndexOf(adaptivePreferenceValues, SprkTestPrefs.GetInt("Sprk.Graphics." + key, 0)));
			case "BattleEffects":
			case "EffectCoverage":
				return Mathf.Clamp(SprkTestPrefs.GetInt("Sprk.Graphics." + key, 0), 0, 2);
			case "MSAASamples":
				return Mathf.Max(0, Array.IndexOf(SupportedSamples(), SelectedSamples()));
			case "FXAAQuality":
				return Mathf.Clamp(SprkTestPrefs.GetInt("Sprk.Graphics.FXAAQuality", 1), 0, 1);
			case "SMAAQuality":
				return Mathf.Clamp(SprkTestPrefs.GetInt("Sprk.Graphics.SMAAQuality", 2), 0, 2);
			case "TAASharpness":
				return Mathf.Clamp(SprkTestPrefs.GetInt("Sprk.Graphics.TAASharpness", 1), 0, 2);
			default:
				return 0;
			}
		}
	}

	private IEnumerator CustomWindowProbe(string root)
	{
		Action<bool, string> check = delegate(bool ok, string message)
		{
			if (!ok)
			{
				throw new Exception(message);
			}
		};
		if (Environment.GetEnvironmentVariable("SPRK_CUSTOM_WINDOW_RESTART") == "1")
		{
			SystemOptionData.Graphic_WindowControl = true;
			yield return new WaitForSecondsRealtime(1f);
			check(CustomWindow && Screen.width == 1137 && Screen.height == 777, "Custom size survives restart: " + Screen.width + "x" + Screen.height);
			check((GetWindowLong(FindGameWindow(), -16) & 0x50000) == 327680, "Resizable border survives restart");
			File.WriteAllText(Path.Combine(root, "custom-restart.txt"), "PASS: saved Custom selection, 1137x777 client size and resizable border survive restart.");
			yield break;
		}
		SprkTestPrefs.DeleteKey("Sprk.Window.Width");
		SprkTestPrefs.DeleteKey("Sprk.Window.Height");
		SystemOptionData.Graphic_WindowControl = true;
		GameObject customHost = new GameObject("CustomWindowChoicesProbe");
		customHost.SetActive(value: false);
		GameObject original = Resources.Load<GameObject>("Prefab2/Component/GameOption/GameOptionToggle4Component");
		GameOptionToggle4Component row = UnityEngine.Object.Instantiate(original, customHost.transform, worldPositionStays: false).GetComponent<GameOptionToggle4Component>();
		row.gameOptionData = new GameOptionData
		{
			FieldName = "Graphic_ResolutionControl",
			Value1 = "HD",
			Value2 = "FHD",
			Value3 = "QHD",
			Value4 = "UHD"
		};
		row.SetToggle();
		UIToggle[] toggles = new UIToggle[5]
		{
			row.toggleInfo.Toggle1,
			row.toggleInfo.Toggle2,
			row.toggleInfo.Toggle3,
			row.toggleInfo.Toggle4,
			row.toggleInfo.Toggle4.transform.parent.Find("Toggle5").GetComponent<UIToggle>()
		};
		UIToggle[] array = toggles;
		foreach (UIToggle obj in array)
		{
			typeof(UIToggle).GetField("mStarted", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, true);
		}
		check(toggles.Select((UIToggle t) => t.GetComponentInChildren<UILabel>(includeInactive: true).text).SequenceEqual(new string[5] { "HD", "FHD", "QHD", "UHD", "Custom" }), "Resolution labels");
		toggles[0].Set(state: true);
		yield return new WaitForSecondsRealtime(1f);
		check(Screen.width == 1280 && Screen.height == 720, "Native HD preset");
		toggles[4].Set(state: true);
		yield return new WaitForSecondsRealtime(1f);
		check(CustomWindow && (GetWindowLong(FindGameWindow(), -16) & 0x50000) == 327680, "Custom choice enables native resize border");
		Vector2[] array2 = new Vector2[2]
		{
			new Vector2(1493f, 821f),
			new Vector2(1137f, 777f)
		};
		for (int num2 = 0; num2 < array2.Length; num2++)
		{
			Vector2 size = array2[num2];
			WindowRect rect = new WindowRect
			{
				Right = (int)size.x,
				Bottom = (int)size.y
			};
			int windowLong = GetWindowLong(gameWindow, -16);
			AdjustWindowRectEx(ref rect, windowLong, menu: false, GetWindowLong(gameWindow, -20));
			SetWindowPos(gameWindow, IntPtr.Zero, 0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top, 22u);
			yield return new WaitForSecondsRealtime(1f);
			check(Screen.width == (int)size.x && Screen.height == (int)size.y, "Unity follows arbitrary native resize: " + Screen.width + "x" + Screen.height);
			check(SprkTestPrefs.GetInt("Sprk.Window.Width", 0) == (int)size.x && SprkTestPrefs.GetInt("Sprk.Window.Height", 0) == (int)size.y, "Custom dimensions saved");
			for (int num3 = 0; num3 < 4; num3++)
			{
				row.Refresh();
				row.SetToggle();
				row.OnClickToggle();
			}
			check(CustomWindow && toggles[4].value && toggles.Count((UIToggle t) => t.value) == 1, "Custom selection survives refresh");
		}
		toggles[0].Set(state: true);
		yield return new WaitForSecondsRealtime(1f);
		check(Screen.width == 1280 && Screen.height == 720 && (GetWindowLong(gameWindow, -16) & 0x50000) == originalResizeStyle, "Fixed preset restores native size and border");
		toggles[4].Set(state: true);
		yield return new WaitForSecondsRealtime(1f);
		check(Screen.width == 1137 && Screen.height == 777, "Returning to Custom restores last size");
		SystemOptionData.Graphic_WindowControl = false;
		yield return new WaitForSecondsRealtime(1f);
		check(Screen.fullScreen, "Native fullscreen still works");
		SystemOptionData.Graphic_WindowControl = true;
		yield return new WaitForSecondsRealtime(1f);
		check(!Screen.fullScreen && Screen.width == 1137 && Screen.height == 777, "Fullscreen to Custom restores last size");
		check(new LocalStorageEnum<EResolutionControl>(EResolutionControl.FHD, "KingsRaid_SystemOption_graphicResolutionContorl").Value == EResolutionControl.Custom, "Native custom enum persistence");
		UnityEngine.Object.Destroy(customHost);
		File.WriteAllText(Path.Combine(root, "custom.txt"), "PASS: five resolution choices; native HD; custom resize border; arbitrary 1493x821 and 1137x777 native window resizing reflected in Unity and saved; repeated row refresh; fixed-preset and fullscreen round trips; native Custom preference persistence.");
	}

	private IEnumerator Probe()
	{
		string root = Environment.GetEnvironmentVariable("SPRK_GRAPHICS_TEST");
		Directory.CreateDirectory(root);
		yield return new WaitForSecondsRealtime(3f);
		int[] array;
		if (Environment.GetEnvironmentVariable("SPRK_GRAPHICS_FULLSCREEN_ONLY") == "1")
		{
			SystemOptionData.Graphic_WindowControl = false;
			yield return new WaitForSecondsRealtime(1f);
			int width = Screen.width;
			int height = Screen.height;
			if (!Screen.fullScreen || width != Screen.currentResolution.width || height != Screen.currentResolution.height)
			{
				throw new Exception("Desktop fullscreen initialization");
			}
			GameObject fullscreenHost = new GameObject("FullscreenSettingsResolutionProbe");
			fullscreenHost.SetActive(value: false);
			GameObject original = Resources.Load<GameObject>("Prefab2/Component/GameOption/GameOptionToggle4Component");
			GameOptionToggle4Component row = UnityEngine.Object.Instantiate(original, fullscreenHost.transform, worldPositionStays: false).GetComponent<GameOptionToggle4Component>();
			row.gameOptionData = new GameOptionData
			{
				FieldName = "Graphic_ResolutionControl",
				Value1 = "HD",
				Value2 = "FHD",
				Value3 = "QHD",
				Value4 = "UHD"
			};
			array = resolutionValues;
			for (int i = 0; i < array.Length; i++)
			{
				int preset = (int)(SystemOptionData.Graphic_ResolutionControl = (EResolutionControl)array[i]);
				for (int j = 0; j < 4; j++)
				{
					row.Refresh();
					row.SetToggle();
					row.SetValueByParent(false, new GameOptionValueInfo
					{
						Value = "False"
					});
					row.OnClickToggle();
				}
				yield return new WaitForSecondsRealtime(0.2f);
				if (!Screen.fullScreen || Screen.width != width || Screen.height != height)
				{
					throw new Exception("Settings reapplied a window preset in fullscreen: " + preset + " " + Screen.width + "x" + Screen.height);
				}
			}
			SystemOptionData.Graphic_WindowControl = true;
			SystemOptionData.Graphic_ResolutionControl = EResolutionControl.HD;
			yield return new WaitForSecondsRealtime(0.5f);
			if (Screen.fullScreen || Screen.width != 1280 || Screen.height != 720)
			{
				throw new Exception("Windowed HD preset changed");
			}
			SystemOptionData.Graphic_WindowControl = false;
			yield return new WaitForSecondsRealtime(0.5f);
			if (!Screen.fullScreen || Screen.width != width || Screen.height != height)
			{
				throw new Exception("Window/fullscreen round trip");
			}
			UnityEngine.Object.Destroy(fullscreenHost);
			File.WriteAllText(Path.Combine(root, "fullscreen.txt"), "PASS: desktop fullscreen " + width + "x" + height + " retained across all five resolution setters and repeated native settings callbacks; windowed HD and fullscreen round trip preserved.");
			Application.Quit();
			yield break;
		}
		if (Environment.GetEnvironmentVariable("SPRK_GRAPHICS_FILTER_ONLY") == "1")
		{
			List<string> filterReport = new List<string>();
			yield return StartCoroutine(GlobalFilterPanelProbe(filterReport, root));
			yield return StartCoroutine(GlobalFilterProbe(filterReport, root));
			File.WriteAllLines(Path.Combine(root, "filter.txt"), filterReport.ToArray());
			Application.Quit();
			yield break;
		}
		if (Environment.GetEnvironmentVariable("SPRK_GRAPHICS_CUSTOM_WINDOW_ONLY") == "1")
		{
			yield return StartCoroutine(CustomWindowProbe(root));
			Application.Quit();
			yield break;
		}
		List<string> report = new List<string>();
		string[] array2;
		if (Environment.GetEnvironmentVariable("SPRK_GRAPHICS_RESOLUTION_ONLY") == "1")
		{
			GameObject gameObject = new GameObject("ResolutionChoicesProbe");
			gameObject.SetActive(value: false);
			GameOptionToggle4Component component = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefab2/Component/GameOption/GameOptionToggle4Component"), gameObject.transform, worldPositionStays: false).GetComponent<GameOptionToggle4Component>();
			component.gameOptionData = new GameOptionData
			{
				FieldName = "Graphic_ResolutionControl",
				Value1 = "HD",
				Value2 = "FHD",
				Value3 = "QHD",
				Value4 = "UHD"
			};
			component.SetToggle();
			for (int k = 0; k < 4; k++)
			{
				array2 = new string[2] { "False", "True" };
				foreach (string text in array2)
				{
					component.toggleInfo.Toggle4.gameObject.SetActive(value: false);
					component.SetValueByParent(text == "True", new GameOptionValueInfo
					{
						Value = text
					});
					if (!component.toggleInfo.Toggle4.gameObject.activeSelf)
					{
						throw new Exception("UHD lost after parent mode " + text);
					}
					component.toggleInfo.Toggle4.gameObject.SetActive(value: false);
					component.SetToggle();
					if (!component.toggleInfo.Toggle4.gameObject.activeSelf)
					{
						throw new Exception("UHD lost after row rebuild");
					}
				}
			}
			component.gameOptionData.FieldName = "OtherSetting";
			component.toggleInfo.Toggle4.gameObject.SetActive(value: false);
			RestoreResolutionChoices(component);
			if (component.toggleInfo.Toggle4.gameObject.activeSelf)
			{
				throw new Exception("Unrelated row changed");
			}
			component.gameOptionData.FieldName = "Graphic_ResolutionControl";
			component.gameOptionData.Value4 = "";
			RestoreResolutionChoices(component);
			if (component.toggleInfo.Toggle4.gameObject.activeSelf)
			{
				throw new Exception("Undefined fourth choice exposed");
			}
			UnityEngine.Object.Destroy(gameObject);
			File.WriteAllText(Path.Combine(root, "resolution.txt"), "PASS: real resolution prefab, repeated native Full/Window parent callbacks and row rebuilds restore UHD; unrelated rows and undefined choices preserved.");
			Application.Quit();
			yield break;
		}
		if (Environment.GetEnvironmentVariable("SPRK_GRAPHICS_RESTART_TEST") == "1")
		{
			bool flag = Method == 4 && Scale == 100 && ReadIndex("FXAAQuality") == 0 && ReadIndex("SMAAQuality") == 1 && SprkTestPrefs.GetInt("Sprk.Graphics.MSAASamples", 0) == 8 && ReadIndex("TAASharpness") == 2 && ReadIndex("LobbyCameraSensitivity") == 1 && ReadIndex("ModelCameraSensitivity") == 3 && CustomScaleMode && PreviewScale == 125 && ReadIndex("ParticleDensity") == 1 && ReadIndex("BattleEffects") == 1 && ReadIndex("EffectCoverage") == 1 && AdaptiveBattleTarget == 144;
			File.WriteAllText(Path.Combine(root, "restart.txt"), "preferencesSurviveRestart=" + flag);
			Application.Quit();
			yield break;
		}
		report.Add("resources=" + FindResources());
		GameObject uiHost = new GameObject("GraphicsSettingsLifecycleProbe");
		uiHost.SetActive(value: false);
		UISimpleGrid wrapper = uiHost.AddComponent<UISimpleGrid>();
		GameObject gridHost = new GameObject("Grid");
		gridHost.transform.SetParent(uiHost.transform, worldPositionStays: false);
		wrapper.Grid = gridHost.AddComponent<UIGrid>();
		GameOptionWindow window = uiHost.AddComponent<GameOptionWindow>();
		window.MainTabs = new GameOptionWindow.TabUIInfo[1]
		{
			new GameOptionWindow.TabUIInfo
			{
				GameOptionTab = GameOptionTab.Game,
				GameOptionGrid = wrapper
			}
		};
		for (int m = 0; m < 8; m++)
		{
			Setup(window);
		}
		GameOptionToggle4Component[] rows = wrapper.GetComponentsInChildren<GameOptionToggle4Component>(includeInactive: true);
		report.Add("settingsRows=" + rows.Length);
		GameOptionToggle4Component presetRow = rows.Single((GameOptionToggle4Component r) => r.name == "SPRK Anti-Aliasing");
		UIToggle[] componentsInChildren = wrapper.GetComponentsInChildren<UIToggle>(includeInactive: true);
		foreach (UIToggle obj in componentsInChildren)
		{
			typeof(UIToggle).GetField("mStarted", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, true);
		}
		UIToggle component2 = presetRow.toggleInfo.Toggle4.transform.parent.Find("Toggle5").GetComponent<UIToggle>();
		UIToggle[] aaChoices = new UIToggle[5]
		{
			presetRow.toggleInfo.Toggle1,
			presetRow.toggleInfo.Toggle2,
			presetRow.toggleInfo.Toggle3,
			presetRow.toggleInfo.Toggle4,
			component2
		};
		GameOptionToggle4Component gameOptionToggle4Component = rows.Single((GameOptionToggle4Component r) => r.name == "SPRK Render Scale");
		UIToggle[] array3 = new UIToggle[5]
		{
			gameOptionToggle4Component.toggleInfo.Toggle1,
			gameOptionToggle4Component.toggleInfo.Toggle2,
			gameOptionToggle4Component.toggleInfo.Toggle3,
			gameOptionToggle4Component.toggleInfo.Toggle4,
			gameOptionToggle4Component.toggleInfo.Toggle4.transform.parent.Find("Toggle5").GetComponent<UIToggle>()
		};
		bool allChoices = aaChoices.Select((UIToggle t) => t.GetComponentInChildren<UILabel>(includeInactive: true).text).SequenceEqual(new string[5] { "Off", "FXAA", "SMAA", "MSAA", "TAA" });
		bool conditional = true;
		bool remembered = true;
		for (int num = 0; num < 3; num++)
		{
			int method;
			for (method = 0; method < 5; method++)
			{
				aaChoices[method].Set(state: true);
				allChoices &= Method == method && aaChoices.Count((UIToggle t) => t.value) == 1;
				int i2;
				for (i2 = 0; i2 < adjustmentNames.Length; i2++)
				{
					conditional &= rows.Single((GameOptionToggle4Component r) => r.name == "SPRK " + adjustmentNames[i2]).gameObject.activeSelf == (method == i2 + 1);
				}
				if (method > 0)
				{
					GameOptionToggle4Component gameOptionToggle4Component2 = rows.Single((GameOptionToggle4Component r) => r.name == "SPRK " + adjustmentNames[method - 1]);
					UIToggle[] array4 = new UIToggle[4]
					{
						gameOptionToggle4Component2.toggleInfo.Toggle1,
						gameOptionToggle4Component2.toggleInfo.Toggle2,
						gameOptionToggle4Component2.toggleInfo.Toggle3,
						gameOptionToggle4Component2.toggleInfo.Toggle4
					}.Where((UIToggle t) => t.gameObject.activeSelf).ToArray();
					for (int num2 = 0; num2 < array4.Length; num2++)
					{
						array4[num2].Set(state: true);
						allChoices &= array4.Count((UIToggle t) => t.value) == 1;
					}
					ApplyPreset((method + 1) % 5);
					ApplyPreset(method);
					int num3 = ReadIndex((new string[4] { "FXAAQuality", "SMAAQuality", "MSAASamples", "TAASharpness" })[method - 1]);
					remembered &= num3 == array4.Length - 1;
				}
				Setup(window);
			}
		}
		for (int num4 = 0; num4 < array3.Length; num4++)
		{
			array3[num4].Set(state: true);
			allChoices &= Scale == 100 + 25 * num4 && array3.Count((UIToggle t) => t.value) == 1;
		}
		yield return StartCoroutine(RenderScaleSettingsProbe(report, window));
		SprkTestPrefs.DeleteKey("Sprk.Graphics.CameraSensitivity");
		array2 = new string[2] { "LobbyCameraSensitivity", "ModelCameraSensitivity" };
		foreach (string text2 in array2)
		{
			SprkTestPrefs.DeleteKey("Sprk.Graphics." + text2);
		}
		Setup(window);
		bool flag2 = ReadIndex("LobbyCameraSensitivity") == 2 && ReadIndex("ModelCameraSensitivity") == 2;
		SprkTestPrefs.SetInt("Sprk.Graphics.CameraSensitivity", 75);
		Setup(window);
		flag2 &= ReadIndex("LobbyCameraSensitivity") == 1 && ReadIndex("ModelCameraSensitivity") == 1;
		array2 = new string[2] { "Lobby", "Model" };
		foreach (string kind in array2)
		{
			GameOptionToggle4Component gameOptionToggle4Component3 = rows.Single((GameOptionToggle4Component r) => r.name == "SPRK " + kind + " Camera Sensitivity");
			UIToggle[] array5 = new UIToggle[5]
			{
				gameOptionToggle4Component3.toggleInfo.Toggle1,
				gameOptionToggle4Component3.toggleInfo.Toggle2,
				gameOptionToggle4Component3.toggleInfo.Toggle3,
				gameOptionToggle4Component3.toggleInfo.Toggle4,
				gameOptionToggle4Component3.toggleInfo.Toggle4.transform.parent.Find("Toggle5").GetComponent<UIToggle>()
			};
			string key = ((kind == "Lobby") ? "ModelCameraSensitivity" : "LobbyCameraSensitivity");
			int num5 = ReadIndex(key);
			for (int num6 = 0; num6 < array5.Length; num6++)
			{
				array5[num6].Set(state: true);
				Setup(window);
				flag2 &= SprkTestPrefs.GetInt("Sprk.Graphics." + kind + "CameraSensitivity", 0) == 50 + 25 * num6 && array5[num6].value && array5.Count((UIToggle t) => t.value) == 1 && ReadIndex(key) == num5;
			}
		}
		report.Add("cameraSensitivityChoices=" + flag2);
		UIHelper.AddGridItem(wrapper, GameOptionButtonComponent.PrefabAuto).name = "SPRK Anti-Aliasing";
		Setup(window);
		allChoices &= wrapper.GetComponentsInChildren<GameOptionBaseComponent>(includeInactive: true).Count((GameOptionBaseComponent r) => r.name.StartsWith("SPRK ")) == 15;
		bool flag3 = true;
		array2 = new string[5] { "Preview Scale", "Particle Density", "Battle Effects", "Effect Coverage", "Adaptive Battle Scale" };
		foreach (string title in array2)
		{
			UIToggle[] array6 = (from t in rows.Single((GameOptionToggle4Component r) => r.name == "SPRK " + title).GetComponentsInChildren<UIToggle>(includeInactive: true)
				where t.gameObject.activeSelf
				select t).ToArray();
			componentsInChildren = array6;
			foreach (UIToggle obj2 in componentsInChildren)
			{
				typeof(UIToggle).GetField("mStarted", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj2, true);
			}
			string text3 = title.Replace(" ", "");
			if (text3 == "AdaptiveBattleScale")
			{
				flag3 &= array6.Length == 5;
			}
			for (int num8 = 0; num8 < array6.Length; num8++)
			{
				array6[num8].Set(state: true);
				flag3 &= ReadIndex(text3) == num8 && array6.Count((UIToggle t) => t.value) == 1;
			}
		}
		SprkTestPrefs.SetInt("Sprk.Graphics.PreviewScale", 100);
		SprkTestPrefs.SetInt("Sprk.Graphics.ParticleDensity", 0);
		array2 = new string[3] { "BattleEffects", "EffectCoverage", "AdaptiveBattleScale" };
		foreach (string text4 in array2)
		{
			SprkTestPrefs.SetInt("Sprk.Graphics." + text4, 0);
		}
		report.Add("performanceSettingChoices=" + flag3);
		report.Add("settingsAllChoices=" + allChoices);
		report.Add("conditionalAARows=" + conditional);
		report.Add("rememberedAdjustments=" + remembered);
		GameOptionToggle3Component textureRow = UIHelper.AddGridItem(wrapper, GameOptionToggle3Component.PrefabAuto);
		textureRow.gameOptionData = new GameOptionData
		{
			FieldName = "Spec"
		};
		UIToggle[] array7 = new UIToggle[3]
		{
			textureRow.toggleInfo.Toggle1,
			textureRow.toggleInfo.Toggle2,
			textureRow.toggleInfo.Toggle3
		};
		componentsInChildren = array7;
		for (int l = 0; l < componentsInChildren.Length; l++)
		{
			componentsInChildren[l].gameObject.SetActive(value: true);
		}
		bool flag4 = true;
		for (int num9 = 0; num9 < 8; num9++)
		{
			CenterTextureChoices(textureRow);
			Setup(window);
			aaChoices[0].transform.parent.GetComponent<UIGrid>().Reposition();
			array7[0].transform.parent.GetComponent<UIGrid>().Reposition();
			flag4 &= Mathf.Approximately(aaChoices[0].transform.localPosition.x, 0f) && Mathf.Approximately(aaChoices[4].transform.localPosition.x, 840f);
			float num10 = (presetRow.transform.InverseTransformPoint(aaChoices[0].transform.position).x + presetRow.transform.InverseTransformPoint(aaChoices[4].transform.position).x) * 0.5f;
			for (int num11 = 0; num11 < array7.Length; num11++)
			{
				float x = textureRow.transform.InverseTransformPoint(array7[num11].transform.position).x;
				flag4 &= Mathf.Abs(x - (num10 + (float)((num11 - 1) * 280))) < 0.1f;
			}
		}
		report.Add("textureCheckboxRowX=" + string.Join(",", array7.Select((UIToggle t) => textureRow.transform.InverseTransformPoint(t.transform.position).x.ToString("F1")).ToArray()));
		report.Add("aaCheckboxRowX=" + string.Join(",", aaChoices.Select((UIToggle t) => presetRow.transform.InverseTransformPoint(t.transform.position).x.ToString("F1")).ToArray()));
		report.Add("centeredGraphicsChoices=" + flag4);
		textureRow.gameOptionData = null;
		textureRow.transform.SetParent(null, worldPositionStays: false);
		UnityEngine.Object.Destroy(textureRow.gameObject);
		GameOptionButtonComponent gameOptionButtonComponent = UIHelper.AddGridItem(wrapper, GameOptionButtonComponent.PrefabAuto);
		gameOptionButtonComponent.name = "Screen";
		gameOptionButtonComponent.gameOptionData = new GameOptionData
		{
			FieldName = "Graphic_WindowControl"
		};
		UIHelper.AddGridItem(wrapper, GameOptionButtonComponent.PrefabAuto).name = "ZZAfter";
		GameOptionButtonComponent gameOptionButtonComponent2 = UIHelper.AddGridItem(wrapper, GameOptionButtonComponent.PrefabAuto);
		gameOptionButtonComponent2.name = "Resolution";
		gameOptionButtonComponent2.gameOptionData = new GameOptionData
		{
			OptionName = "OPTION_GAME_RESOULUTION"
		};
		wrapper.Grid.sorting = UIGrid.Sorting.Alphabetic;
		for (int num12 = 0; num12 < 8; num12++)
		{
			Setup(window);
		}
		GameObject gameObject2 = new GameObject("ActiveGraphicsLayoutProbe");
		UISimpleGrid uISimpleGrid = gameObject2.AddComponent<UISimpleGrid>();
		uISimpleGrid.Grid = wrapper.Grid;
		gridHost.transform.SetParent(gameObject2.transform, worldPositionStays: false);
		window.MainTabs[0].GameOptionGrid = uISimpleGrid;
		UIGrid grid = uISimpleGrid.Grid;
		grid.arrangement = UIGrid.Arrangement.Vertical;
		grid.cellHeight = 110f;
		grid.pivot = UIWidget.Pivot.TopLeft;
		grid.animateSmoothly = false;
		bool flag5 = true;
		for (int num13 = 0; num13 < 5; num13++)
		{
			ApplyPreset(num13);
			RefreshRows(window);
			List<Transform> childList = grid.GetChildList();
			List<string> list = new List<string> { "Screen", "Resolution", "SPRK Anti-Aliasing" };
			if (num13 > 0)
			{
				list.Add("SPRK " + adjustmentNames[num13 - 1]);
			}
			list.Add("SPRK Render Scale");
			list.Add("SPRK World Scale");
			list.Add("SPRK Preview Scale");
			list.Add("SPRK Particle Density");
			list.Add("SPRK Battle Effects");
			list.Add("SPRK Effect Coverage");
			list.Add("SPRK Adaptive Battle Scale");
			list.Add("SPRK Lobby Camera Sensitivity");
			list.Add("SPRK Model Camera Sensitivity");
			list.Add("SPRK Global Filter");
			list.Add("ZZAfter");
			flag5 &= childList.Select((Transform t) => t.name).SequenceEqual(list);
			for (int num14 = 1; num14 < childList.Count; num14++)
			{
				flag5 &= Mathf.Abs(childList[num14].localPosition.y - childList[num14 - 1].localPosition.y + 110f) < 0.1f;
			}
		}
		report.Add("compactConditionalLayout=" + flag5);
		gridHost.transform.SetParent(uiHost.transform, worldPositionStays: false);
		window.MainTabs[0].GameOptionGrid = wrapper;
		UnityEngine.Object.Destroy(gameObject2);
		GameOptionToggle4Component gameOptionToggle4Component4 = UIHelper.AddGridItem(wrapper, GameOptionToggle4Component.PrefabAuto);
		gameOptionToggle4Component4.gameOptionData = new GameOptionData
		{
			FieldName = "Graphic_FrameControl"
		};
		ConfigureFrameRow(gameOptionToggle4Component4);
		UIToggle[] array8 = new UIToggle[5]
		{
			gameOptionToggle4Component4.toggleInfo.Toggle1,
			gameOptionToggle4Component4.toggleInfo.Toggle2,
			gameOptionToggle4Component4.toggleInfo.Toggle3,
			gameOptionToggle4Component4.toggleInfo.Toggle4,
			gameOptionToggle4Component4.toggleInfo.Toggle4.transform.parent.Find("Toggle5").GetComponent<UIToggle>()
		};
		componentsInChildren = array8;
		foreach (UIToggle obj3 in componentsInChildren)
		{
			typeof(UIToggle).GetField("mStarted", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj3, true);
		}
		bool flag6 = true;
		int[] array9 = new int[6] { 0, 1, 2, 3, 4, 2 };
		foreach (int num15 in array9)
		{
			array8[num15].Set(state: true);
			flag6 &= SystemOptionData.Graphic_FrameControl == (EFrameControl)frameValues[num15];
			if (num15 < 3)
			{
				flag6 &= Application.targetFrameRate == frameValues[num15] && QualitySettings.vSyncCount == 0;
			}
			if (num15 == 3)
			{
				flag6 &= Application.targetFrameRate == -1 && QualitySettings.vSyncCount == 1;
			}
			if (num15 == 4)
			{
				flag6 &= Application.targetFrameRate > 0;
			}
			for (int num16 = 0; num16 < 8; num16++)
			{
				gameOptionToggle4Component4.Refresh();
			}
			gameOptionToggle4Component4.OnClickToggle();
			flag6 &= array8[num15].value && array8.Count((UIToggle t) => t.value) == 1 && SystemOptionData.Graphic_FrameControl == (EFrameControl)frameValues[num15];
		}
		flag6 &= array8.Select((UIToggle t) => t.GetComponentInChildren<UILabel>(includeInactive: true).text).SequenceEqual(new string[5] { "30", "60", "144", "Auto", "Auto+" });
		flag6 &= new LocalStorageEnum<EFrameControl>(EFrameControl.Manual_30, "KingsRaid_SystemOption_graphicFrameContorl").Value == EFrameControl.Manual_144;
		report.Add("frameChoicesAnd144=" + flag6 + " target=" + Application.targetFrameRate + " saved=" + SprkTestPrefs.GetString("KingsRaid_SystemOption_graphicFrameContorl", "missing"));
		UnityEngine.Object.Destroy(uiHost);
		bool flag7 = true;
		for (int num17 = 0; num17 < 4; num17++)
		{
			SprkTestPrefs.DeleteKey("Sprk.Graphics.Method");
			SprkTestPrefs.SetInt("Sprk.Graphics.Preset", num17);
			preferencesReady = false;
			EnsurePreset();
			flag7 &= Method == ((num17 != 0) ? 4 : 0);
		}
		SprkTestPrefs.DeleteKey("Sprk.Graphics.Method");
		SprkTestPrefs.DeleteKey("Sprk.Graphics.Preset");
		SprkTestPrefs.DeleteKey("Sprk.Graphics.PostAA");
		SprkTestPrefs.DeleteKey("Sprk.Graphics.MSAA");
		preferencesReady = false;
		EnsurePreset();
		flag7 &= Method == 4 && Scale == 200;
		SprkTestPrefs.SetInt("Sprk.Graphics.FXAAQuality", 1);
		SprkTestPrefs.SetInt("Sprk.Graphics.SMAAQuality", 2);
		SprkTestPrefs.SetInt("Sprk.Graphics.MSAASamples", 4);
		SprkTestPrefs.SetInt("Sprk.Graphics.TAASharpness", 1);
		report.Add("aaMethodMigration=" + flag7);
		GameObject host = new GameObject("GraphicsProbeCamera");
		Camera camera = host.AddComponent<Camera>();
		camera.cullingMask = 1073741824;
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = Color.gray;
		camera.transform.position = new Vector3(0f, 0f, -4f);
		camera.depth = 0f;
		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.layer = 30;
		cube.transform.rotation = Quaternion.Euler(23f, 31f, 17f);
		Material material = new Material(Shader.Find("Unlit/Texture"))
		{
			mainTexture = Texture2D.whiteTexture
		};
		cube.GetComponent<Renderer>().material = material;
		for (int height = 0; height < 5; height++)
		{
			for (int width = 100; width <= 200; width += 25)
			{
				ApplyPreset(height);
				SprkTestPrefs.SetInt("Sprk.Graphics.Scale", width);
				SprkTestPrefs.SetInt("Sprk.Graphics.PreviewScale", width);
				RenderTexture target = CreateTarget(320, 240, 24);
				target.Create();
				camera.targetTexture = target;
				for (int i = 0; i < 12; i++)
				{
					camera.enabled = true;
					camera.targetTexture = target;
					camera.Render();
					yield return new WaitForEndOfFrame();
				}
				camera.targetTexture = target;
				camera.Render();
				RenderTexture active = RenderTexture.active;
				RenderTexture temporary = RenderTexture.GetTemporary(target.width, target.height, 0);
				Graphics.Blit(target, temporary);
				RenderTexture.active = temporary;
				Texture2D texture2D = new Texture2D(target.width, target.height, TextureFormat.RGB24, mipChain: false);
				texture2D.ReadPixels(new Rect(0f, 0f, target.width, target.height), 0, 0);
				texture2D.Apply();
				File.WriteAllBytes(Path.Combine(root, "aa-" + height + "-" + width + ".png"), texture2D.EncodeToPNG());
				RenderTexture.active = active;
				RenderTexture.ReleaseTemporary(temporary);
				PostProcessLayer component3 = camera.GetComponent<PostProcessLayer>();
				report.Add("mode=" + height + " scale=" + width + " size=" + target.width + "x" + target.height + " samples=" + target.antiAliasing + " applied=" + ((component3 == null) ? "none" : component3.antialiasingMode.ToString()) + " status=" + status);
				camera.targetTexture = null;
				target.Release();
				UnityEngine.Object.Destroy(target);
				UnityEngine.Object.Destroy(texture2D);
			}
		}
		ApplyPreset(2);
		SprkTestPrefs.SetInt("Sprk.Graphics.Scale", 100);
		RenderTexture liveTarget = CreateTarget(320, 240, 24);
		liveTarget.Create();
		camera.targetTexture = liveTarget;
		camera.enabled = true;
		Vector3 nativePosition = camera.transform.position;
		Quaternion nativeRotation = camera.transform.rotation;
		bool liveOk = true;
		bool[] array10 = new bool[2] { false, true };
		foreach (bool orthographic in array10)
		{
			camera.orthographic = orthographic;
			array = new int[3] { 200, 125, 100 };
			foreach (int i in array)
			{
				int[] array11 = new int[6] { 4, 3, 2, 1, 0, 4 };
				foreach (int preset2 in array11)
				{
					ApplyPreset(preset2);
					SprkTestPrefs.SetInt("Sprk.Graphics.Scale", i);
					SprkTestPrefs.SetInt("Sprk.Graphics.PreviewScale", i);
					for (int frame = 0; frame < 3; frame++)
					{
						yield return new WaitForEndOfFrame();
					}
					int samples = Samples;
					RenderTextureDescriptor descriptor = liveTarget.descriptor;
					descriptor.msaaSamples = samples;
					samples = SystemInfo.GetRenderTextureSupportedMSAASampleCount(descriptor);
					PostProcessLayer component4 = camera.GetComponent<PostProcessLayer>();
					liveOk &= camera.targetTexture == liveTarget && liveTarget.IsCreated() && liveTarget.width == 320 * i / 100 && liveTarget.height == 240 * i / 100 && liveTarget.antiAliasing == samples;
					liveOk &= component4.antialiasingMode == (PostProcessLayer.Antialiasing)postModes[preset2];
					liveOk &= camera.transform.position == nativePosition && camera.transform.rotation == nativeRotation;
				}
			}
		}
		bool appliedAdjustments = true;
		array = new int[4] { 1, 2, 3, 4 };
		foreach (int width in array)
		{
			ApplyPreset(width);
			for (int i = 0; i < ((width == 1) ? 2 : 3); i++)
			{
				string text5 = (new string[4] { "FXAAQuality", "SMAAQuality", "MSAASamples", "TAASharpness" })[width - 1];
				int value = ((width == 3) ? (new int[3] { 2, 4, 8 })[i] : i);
				SprkTestPrefs.SetInt("Sprk.Graphics." + text5, value);
				yield return new WaitForEndOfFrame();
				PostProcessLayer component5 = camera.GetComponent<PostProcessLayer>();
				if (width == 1)
				{
					appliedAdjustments &= component5.fastApproximateAntialiasing.fastMode == (i == 0);
				}
				if (width == 2)
				{
					appliedAdjustments &= component5.subpixelMorphologicalAntialiasing.quality == (SubpixelMorphologicalAntialiasing.Quality)i;
				}
				if (width == 3)
				{
					RenderTextureDescriptor descriptor2 = liveTarget.descriptor;
					descriptor2.msaaSamples = Samples;
					appliedAdjustments &= liveTarget.antiAliasing == SystemInfo.GetRenderTextureSupportedMSAASampleCount(descriptor2) && component5.antialiasingMode == PostProcessLayer.Antialiasing.None;
				}
				if (width == 4)
				{
					appliedAdjustments &= component5.temporalAntialiasing.sharpness == (new float[3] { 0f, 0.5f, 1f })[i];
				}
			}
		}
		report.Add("liveAdjustmentApplication=" + appliedAdjustments);
		report.Add("livePreviewSwitching=" + liveOk);
		yield return StartCoroutine(PerformanceControlsProbe(report));
		yield return StartCoroutine(BattleControlsProbe(report));
		camera.orthographic = false;
		camera.targetTexture = null;
		liveTarget.Release();
		UnityEngine.Object.Destroy(liveTarget);
		ApplyPreset(1);
		SprkTestPrefs.SetInt("Sprk.Graphics.Scale", 150);
		camera.targetTexture = null;
		float[] array12 = (from c in Camera.allCameras
			where c.GetComponent<UICamera>() != null
			select c.depth).ToArray();
		camera.depth = ((array12.Length == 0) ? 0f : (array12.Min() - 0.5f));
		for (int height = 0; height < 12; height++)
		{
			camera.enabled = true;
			yield return new WaitForEndOfFrame();
		}
		report.Add("nativeWorldCameraClassification=" + (GlobalCameraManager.HasMainRenderTarget(camera) && GlobalCameraManager.CollectAllCameras().Any((GlobalCameraManager.CameraInfo c) => c.Cam == camera)));
		report.Add("screenWorld=" + ((world == null) ? "missing" : (world.width + "x" + world.height)) + " stack=" + redirected.Count);
		camera.enabled = false;
		Camera[] nativeCameras = Camera.allCameras;
		Camera[] array13 = nativeCameras;
		for (int l = 0; l < array13.Length; l++)
		{
			array13[l].enabled = false;
		}
		GameObject lobbyHost = new GameObject("LobbyGraphicsProbe");
		GameObject gameObject3 = new GameObject("GraphicsProbeUI");
		gameObject3.transform.SetParent(lobbyHost.transform, worldPositionStays: false);
		Camera emptyUi = gameObject3.AddComponent<Camera>();
		emptyUi.depth = -2f;
		emptyUi.orthographic = true;
		emptyUi.cullingMask = 0;
		emptyUi.clearFlags = CameraClearFlags.Depth;
		gameObject3.AddComponent<UICamera>();
		RenderTexture screenTarget = new RenderTexture(Screen.width, Screen.height, 0);
		screenTarget.Create();
		CommandBuffer captureCommand = new CommandBuffer
		{
			name = "SPRK probe screen capture"
		};
		captureCommand.Blit(BuiltinRenderTextureType.CameraTarget, screenTarget);
		emptyUi.AddCommandBuffer(CameraEvent.AfterEverything, captureCommand);
		GameObject gameObject4 = new GameObject("BottomUI Camera");
		gameObject4.transform.SetParent(lobbyHost.transform, worldPositionStays: false);
		Camera background = gameObject4.AddComponent<Camera>();
		background.orthographic = true;
		background.depth = -5f;
		background.cullingMask = 1 << LayerMask.NameToLayer("BottomUI");
		background.clearFlags = CameraClearFlags.Color;
		background.backgroundColor = Color.gray;
		gameObject4.AddComponent<UICamera>();
		GameObject gameObject5 = new GameObject("Bottom3D Camera");
		gameObject5.transform.SetParent(lobbyHost.transform, worldPositionStays: false);
		Camera model = gameObject5.AddComponent<Camera>();
		model.orthographic = true;
		model.orthographicSize = 2f;
		model.depth = -4f;
		model.transform.position = new Vector3(0f, 0f, -4f);
		model.clearFlags = CameraClearFlags.Depth;
		model.cullingMask = 1 << LayerMask.NameToLayer("Bottom3D");
		gameObject5.AddComponent<UICamera>();
		cube.layer = LayerMask.NameToLayer("Bottom3D");
		bool lobbyOk = true;
		array = new int[7] { 1, 0, 3, 4, 2, 0, 4 };
		for (int height = 0; height < array.Length; height++)
		{
			int width = array[height];
			ApplyPreset(width);
			SprkTestPrefs.SetInt("Sprk.Graphics.Scale", 100);
			for (int i = 0; i < 12; i++)
			{
				yield return new WaitForEndOfFrame();
			}
			PostProcessLayer component6 = model.GetComponent<PostProcessLayer>();
			int num18 = (int)((!(component6 == null)) ? component6.antialiasingMode : PostProcessLayer.Antialiasing.None);
			bool flag8 = width == 0;
			lobbyOk &= ((!flag8) ? (redirected.ContainsKey(background) && redirected.ContainsKey(model) && world != null && world.width == Screen.width && world.height == Screen.height && presentationCamera != null) : (world == null && redirected.Count == 0 && background.targetTexture == null && model.targetTexture == null));
			lobbyOk &= num18 == postModes[width] && QualitySettings.antiAliasing == ((Samples != 1) ? Samples : 0);
			if (component6 != null)
			{
				lobbyOk &= component6.enabled == (width != 0 && width != 3);
			}
			if (!flag8)
			{
				RenderTextureDescriptor descriptor3 = world.descriptor;
				descriptor3.msaaSamples = Samples;
				lobbyOk &= world.antiAliasing == SystemInfo.GetRenderTextureSupportedMSAASampleCount(descriptor3);
			}
			lobbyOk &= Camera.allCameras.Where((Camera c) => c.GetComponent<UICamera>() != null && !IsLobbyBackground(c)).All((Camera c) => c.targetTexture == null && !redirected.ContainsKey(c));
			lobbyOk &= !emptyUi.allowMSAA;
			report.Add("lobbyPreset=" + width + " applied=" + num18 + " samples=" + ((!(world == null)) ? world.antiAliasing : 0));
			presentationFrame = -1;
			background.Render();
			model.Render();
			emptyUi.Render();
			RenderTexture active2 = RenderTexture.active;
			RenderTexture.active = screenTarget;
			Texture2D texture2D2 = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, mipChain: false);
			texture2D2.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
			texture2D2.Apply();
			lobbyOk &= texture2D2.GetPixels32().Any((Color32 pixel) => pixel.r > 200);
			File.WriteAllBytes(Path.Combine(root, "lobby-screen-" + width + ".png"), texture2D2.EncodeToPNG());
			UnityEngine.Object.Destroy(texture2D2);
			int num19 = ((world == null) ? Screen.width : world.width);
			int num20 = ((world == null) ? Screen.height : world.height);
			RenderTexture renderTexture = ((world == null) ? null : RenderTexture.GetTemporary(world.width, world.height, 0));
			if (world != null)
			{
				Graphics.Blit(world, renderTexture);
			}
			RenderTexture.active = ((world == null) ? screenTarget : renderTexture);
			Texture2D texture2D3 = new Texture2D(num19, num20, TextureFormat.RGB24, mipChain: false);
			texture2D3.ReadPixels(new Rect(0f, 0f, num19, num20), 0, 0);
			texture2D3.Apply();
			File.WriteAllBytes(Path.Combine(root, "lobby-" + width + ".png"), texture2D3.EncodeToPNG());
			UnityEngine.Object.Destroy(texture2D3);
			RenderTexture.active = active2;
			if (renderTexture != null)
			{
				RenderTexture.ReleaseTemporary(renderTexture);
			}
		}
		GameObject gameObject6 = new GameObject("Inactive trailing lobby camera");
		gameObject6.transform.SetParent(lobbyHost.transform, worldPositionStays: false);
		Camera trailing = gameObject6.AddComponent<Camera>();
		trailing.orthographic = true;
		trailing.depth = -3f;
		trailing.cullingMask = 1 << LayerMask.NameToLayer("Bottom3DUI");
		trailing.clearFlags = CameraClearFlags.Depth;
		gameObject6.AddComponent<UICamera>();
		yield return new WaitForEndOfFrame();
		bool trailingOk = lastWorld == model && !redirected.ContainsKey(trailing) && presentationCamera != null;
		trailing.enabled = false;
		yield return new WaitForEndOfFrame();
		presentationFrame = -1;
		background.Render();
		model.Render();
		emptyUi.Render();
		RenderTexture active3 = RenderTexture.active;
		RenderTexture.active = screenTarget;
		Texture2D texture2D4 = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, mipChain: false);
		texture2D4.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
		texture2D4.Apply();
		trailingOk &= texture2D4.GetPixels32().Any((Color32 pixel) => pixel.r > 200);
		UnityEngine.Object.Destroy(texture2D4);
		RenderTexture.active = active3;
		report.Add("disabledTrailingCameraPresentation=" + trailingOk);
		report.Add("lobbyCameraStack=" + lobbyOk);
		RecordFrameCost(report, "lobbyTAA100");
		emptyUi.RemoveCommandBuffer(CameraEvent.AfterEverything, captureCommand);
		UnityEngine.Object.Destroy(lobbyHost);
		yield return null;
		GameObject battleHost = new GameObject("BattleGraphicsProbe");
		GameObject gameObject7 = new GameObject("BattleBackgroundUI Camera");
		gameObject7.transform.SetParent(battleHost.transform, worldPositionStays: false);
		Camera battleBackground = gameObject7.AddComponent<Camera>();
		battleBackground.depth = -7f;
		battleBackground.orthographic = true;
		battleBackground.cullingMask = 1 << LayerMask.NameToLayer("BattleBackgroundUI");
		battleBackground.clearFlags = CameraClearFlags.Color;
		battleBackground.backgroundColor = Color.gray;
		gameObject7.AddComponent<UICamera>();
		GameObject gameObject8 = new GameObject("Default Camera");
		gameObject8.transform.SetParent(battleHost.transform, worldPositionStays: false);
		Camera battleModel = gameObject8.AddComponent<Camera>();
		battleModel.depth = -6f;
		battleModel.transform.position = new Vector3(0f, 0f, -4f);
		battleModel.cullingMask = 1 << LayerMask.NameToLayer("Default");
		battleModel.clearFlags = CameraClearFlags.Depth;
		cube.layer = LayerMask.NameToLayer("Default");
		GameObject gameObject9 = new GameObject("BattleUICamera");
		gameObject9.transform.SetParent(battleHost.transform, worldPositionStays: false);
		Camera battleOverlay = gameObject9.AddComponent<Camera>();
		battleOverlay.depth = -5f;
		battleOverlay.cullingMask = 1 << LayerMask.NameToLayer("BattleUI");
		battleOverlay.clearFlags = CameraClearFlags.Color;
		battleOverlay.backgroundColor = Color.black;
		battleOverlay.AddCommandBuffer(CameraEvent.AfterEverything, captureCommand);
		bool battleOk = true;
		for (int height = 0; height < 5; height++)
		{
			ApplyPreset(height);
			SprkTestPrefs.SetInt("Sprk.Graphics.Scale", 125);
			for (int width = 0; width < 12; width++)
			{
				yield return new WaitForEndOfFrame();
			}
			presentationFrame = -1;
			battleBackground.Render();
			battleModel.Render();
			battleOverlay.Render();
			battleOk &= redirected.ContainsKey(battleBackground) && redirected.ContainsKey(battleModel) && !redirected.ContainsKey(battleOverlay) && presentationCamera == battleOverlay;
			battleOk &= GlobalCameraManager.HasMainRenderTarget(battleModel);
			PostProcessLayer component7 = battleModel.GetComponent<PostProcessLayer>();
			int num21 = (int)((!(component7 == null)) ? component7.antialiasingMode : PostProcessLayer.Antialiasing.None);
			battleOk &= num21 == postModes[height] && battleOverlay.GetComponent<PostProcessLayer>() == null;
			RenderTexture active4 = RenderTexture.active;
			RenderTexture.active = screenTarget;
			Texture2D texture2D5 = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, mipChain: false);
			texture2D5.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
			texture2D5.Apply();
			battleOk &= texture2D5.GetPixels32().Any((Color32 pixel) => pixel.r > 200);
			File.WriteAllBytes(Path.Combine(root, "battle-" + height + ".png"), texture2D5.EncodeToPNG());
			UnityEngine.Object.Destroy(texture2D5);
			RenderTexture.active = active4;
			report.Add("battlePreset=" + height + " applied=" + num21 + " hudNative=" + !redirected.ContainsKey(battleOverlay));
		}
		report.Add("battleCameraStack=" + battleOk);
		ApplyPreset(3);
		SprkTestPrefs.SetInt("Sprk.Graphics.Scale", 200);
		SprkTestPrefs.SetInt("Sprk.Graphics.MSAASamples", 8);
		yield return new WaitForEndOfFrame();
		RecordFrameCost(report, "battleMSAA8Scale200");
		RenderTexture renderTexture2 = world;
		RenderTexture targetTexture = battleModel.targetTexture;
		Camera camera2 = presentationCamera;
		for (int num22 = 0; num22 < 20; num22++)
		{
			LateUpdate();
		}
		report.Add("stableRenderTargets=" + (world == renderTexture2 && battleModel.targetTexture == targetTexture && presentationCamera == camera2));
		yield return StartCoroutine(BattleStackControlsProbe(report, battleModel, battleOverlay));
		RenderTexture nativeTarget = FilterBase.CreateRenderTarget(137, 91);
		nativeTarget.Create();
		camera.targetTexture = nativeTarget;
		camera.enabled = true;
		GameObject nativeHost = new GameObject("NativePostProcessProbe");
		Camera nativeCameraProbe = nativeHost.AddComponent<Camera>();
		nativeCameraProbe.targetTexture = nativeTarget;
		nativeCameraProbe.cullingMask = 0;
		PostProcessLayer nativeLayer = nativeHost.AddComponent<PostProcessLayer>();
		nativeLayer.Init(resources);
		nativeLayer.volumeLayer = 0;
		nativeLayer.fog.enabled = false;
		nativeLayer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
		yield return new WaitForEndOfFrame();
		report.Add("nativeFilterTargetsAndEffects=" + (nativeTarget.width == 137 && nativeTarget.height == 91 && nativeTarget.antiAliasing == 1 && !ownedTargets.ContainsKey(nativeTarget) && nativeLayer.enabled && nativeLayer.antialiasingMode == PostProcessLayer.Antialiasing.FastApproximateAntialiasing));
		camera.enabled = false;
		camera.targetTexture = null;
		nativeCameraProbe.targetTexture = null;
		nativeTarget.Release();
		UnityEngine.Object.Destroy(nativeTarget);
		UnityEngine.Object.Destroy(nativeHost);
		battleOverlay.RemoveCommandBuffer(CameraEvent.AfterEverything, captureCommand);
		captureCommand.Release();
		screenTarget.Release();
		UnityEngine.Object.Destroy(screenTarget);
		UnityEngine.Object.Destroy(battleHost);
		array13 = nativeCameras;
		foreach (Camera camera3 in array13)
		{
			if (camera3 != null)
			{
				camera3.enabled = true;
			}
		}
		UnityEngine.Object.Destroy(cube);
		UnityEngine.Object.Destroy(material);
		UnityEngine.Object.Destroy(host);
		yield return StartCoroutine(GraphicsHelpProbe(report, root));
		ApplyPreset(4);
		SprkTestPrefs.SetInt("Sprk.Graphics.RenderScale", 0);
		SprkTestPrefs.SetInt("Sprk.Graphics.Scale", 100);
		SprkTestPrefs.SetInt("Sprk.Graphics.FXAAQuality", 0);
		SprkTestPrefs.SetInt("Sprk.Graphics.SMAAQuality", 1);
		SprkTestPrefs.SetInt("Sprk.Graphics.MSAASamples", 8);
		SprkTestPrefs.SetInt("Sprk.Graphics.TAASharpness", 2);
		SprkTestPrefs.SetInt("Sprk.Graphics.LobbyCameraSensitivity", 75);
		SprkTestPrefs.SetInt("Sprk.Graphics.ModelCameraSensitivity", 125);
		SprkTestPrefs.SetInt("Sprk.Graphics.PreviewScale", 125);
		SprkTestPrefs.SetInt("Sprk.Graphics.ParticleDensity", 1);
		array2 = new string[3] { "BattleEffects", "EffectCoverage", "AdaptiveBattleScale" };
		foreach (string text6 in array2)
		{
			SprkTestPrefs.SetInt("Sprk.Graphics." + text6, 1);
		}
		SprkTestPrefs.SetInt("Sprk.Graphics.AdaptiveBattleScale", 4);
		SprkTestPrefs.Save();
		File.WriteAllLines(Path.Combine(root, "report.txt"), report.ToArray());
		Application.Quit();
	}

	private static int HistoryTextures(PostProcessLayer layer)
	{
		RenderTexture[][] array = typeof(TemporalAntialiasing).GetField("m_HistoryTextures", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(layer.temporalAntialiasing) as RenderTexture[][];
		int num = 0;
		if (array != null)
		{
			RenderTexture[][] array2 = array;
			foreach (RenderTexture[] array3 in array2)
			{
				if (array3 == null)
				{
					continue;
				}
				RenderTexture[] array4 = array3;
				foreach (RenderTexture renderTexture in array4)
				{
					if (renderTexture != null && renderTexture.IsCreated())
					{
						num++;
					}
				}
			}
		}
		return num;
	}

	private static ParticleSystem ProbeEmitter(Transform root, string name, int capacity = 512, bool burst = false)
	{
		GameObject obj = new GameObject(name);
		obj.transform.SetParent(root, worldPositionStays: false);
		ParticleSystem particleSystem = obj.AddComponent<ParticleSystem>();
		particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
		ParticleSystem.MainModule main = particleSystem.main;
		main.maxParticles = capacity;
		main.startLifetime = 10f;
		main.startSpeed = 0f;
		main.startSize = 0.05f;
		ParticleSystem.EmissionModule emission = particleSystem.emission;
		emission.enabled = true;
		emission.rateOverTime = 80f;
		if (burst)
		{
			emission.SetBursts(new ParticleSystem.Burst[1]
			{
				new ParticleSystem.Burst(0f, 10)
			});
		}
		particleSystem.useAutoRandomSeed = false;
		particleSystem.randomSeed = 17u;
		return particleSystem;
	}

	private IEnumerator PerformanceControlsProbe(List<string> report)
	{
		int method = Method;
		int scale = Scale;
		int value = SprkTestPrefs.GetInt("Sprk.Graphics.PreviewScale", 0);
		int value2 = ReadIndex("ParticleDensity");
		SprkTestPrefs.DeleteKey("Sprk.Graphics.CacheProbe");
		bool flag = SprkTestPrefs.GetInt("Sprk.Graphics.CacheProbe", 7) == 7 && SprkTestPrefs.GetInt("Sprk.Graphics.CacheProbe", 9) == 9 && !SprkTestPrefs.HasKey("Sprk.Graphics.CacheProbe");
		SprkTestPrefs.SetInt("Sprk.Graphics.CacheProbe", 31);
		flag &= SprkTestPrefs.GetInt("Sprk.Graphics.CacheProbe", 7) == 31 && SprkTestPrefs.HasKey("Sprk.Graphics.CacheProbe");
		SprkTestPrefs.SetInt("Sprk.Graphics.CacheProbe", 47);
		flag &= SprkTestPrefs.GetInt("Sprk.Graphics.CacheProbe", 7) == 47;
		string environmentVariable = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
		try
		{
			Environment.SetEnvironmentVariable("SPRK_AUTOMATION_PROFILE", environmentVariable + "-cache");
			flag &= !SprkTestPrefs.HasKey("Sprk.Graphics.CacheProbe");
			SprkTestPrefs.SetInt("Sprk.Graphics.CacheProbe", 83);
			flag &= SprkTestPrefs.GetInt("Sprk.Graphics.CacheProbe", 7) == 83;
		}
		finally
		{
			Environment.SetEnvironmentVariable("SPRK_AUTOMATION_PROFILE", environmentVariable);
		}
		flag &= SprkTestPrefs.GetInt("Sprk.Graphics.CacheProbe", 7) == 47;
		SprkTestPrefs.SetString("Sprk.Graphics.CacheProbe", "cached");
		flag &= SprkTestPrefs.GetString("Sprk.Graphics.CacheProbe", "") == "cached";
		SprkTestPrefs.DeleteKey("Sprk.Graphics.CacheProbe");
		flag &= !SprkTestPrefs.HasKey("Sprk.Graphics.CacheProbe") && SprkTestPrefs.GetString("Sprk.Graphics.CacheProbe", "missing") == "missing";
		OnApplicationFocus(focused: true);
		for (int i = 0; i < 3; i++)
		{
			LateUpdate();
		}
		int graphicsStorageReads = SprkTestPrefs.GraphicsStorageReads;
		for (int j = 0; j < 100; j++)
		{
			LateUpdate();
		}
		report.Add("graphicsPreferenceCache=" + (flag & (SprkTestPrefs.GraphicsStorageReads == graphicsStorageReads)) + " steadyStorageReads=" + (SprkTestPrefs.GraphicsStorageReads - graphicsStorageReads));
		GameObject gameObject = new GameObject("VisibilityProbe");
		HeroPreview heroPreview = gameObject.AddComponent<HeroPreview>();
		GameObject obj = new GameObject("Preview content");
		obj.transform.SetParent(gameObject.transform, worldPositionStays: false);
		MethodInfo method2 = typeof(HeroPreview).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		heroPreview.Show(show: false);
		int skippedPreviewUpdates = SkippedPreviewUpdates;
		method2.Invoke(heroPreview, null);
		bool flag2 = SkippedPreviewUpdates == skippedPreviewUpdates + 1 && !ShouldUpdatePreview(heroPreview);
		heroPreview.Show(show: true);
		skippedPreviewUpdates = SkippedPreviewUpdates;
		method2.Invoke(heroPreview, null);
		flag2 &= SkippedPreviewUpdates == skippedPreviewUpdates && ShouldUpdatePreview(heroPreview);
		RenderFacade facade = obj.AddComponent<RenderFacade>();
		ICreature creature = (ICreature)FormatterServices.GetUninitializedObject(typeof(HeroPreview).Assembly.GetType("NGameAsset.NCreature.CreatureProxy"));
		heroPreview.Show(show: false);
		int skippedPreviewVisualUpdates = SkippedPreviewVisualUpdates;
		UpdatePreviewCreature(creature, facade);
		flag2 &= SkippedPreviewVisualUpdates == skippedPreviewVisualUpdates + 1;
		heroPreview.Show(show: true);
		skippedPreviewVisualUpdates = SkippedPreviewVisualUpdates;
		UpdatePreviewCreature(creature, facade);
		flag2 &= SkippedPreviewVisualUpdates == skippedPreviewVisualUpdates;
		GameObject obj2 = new GameObject("HiddenBattleFacadeProbe");
		RenderFacade facade2 = obj2.AddComponent<RenderFacade>();
		obj2.SetActive(value: false);
		UpdatePreviewCreature(creature, facade2);
		flag2 &= SkippedPreviewVisualUpdates == skippedPreviewVisualUpdates;
		UnityEngine.Object.Destroy(obj2);
		UnityEngine.Object.Destroy(gameObject);
		report.Add("nativePreviewVisibility=" + flag2);
		GameObject gameObject2 = new GameObject("ParticleDensityProbe");
		ParticleObject particleObject = gameObject2.AddComponent<ParticleObject>();
		ParticleSystem particleSystem = ProbeEmitter(gameObject2.transform, "Decorative sparks");
		ParticleSystem particleSystem2 = ProbeEmitter(gameObject2.transform, "Warning ring");
		ParticleSystem particleSystem3 = ProbeEmitter(gameObject2.transform, "Burst flash", 512, burst: true);
		ParticleSystem particleSystem4 = ProbeEmitter(gameObject2.transform, "Small emitter", 16);
		SprkTestPrefs.SetInt("Sprk.Graphics.ParticleDensity", 0);
		ObserveParticles(particleObject);
		particleSystem.Simulate(1f, withChildren: true, restart: true, fixedTimeStep: false);
		int particleCount = particleSystem.particleCount;
		SprkTestPrefs.SetInt("Sprk.Graphics.ParticleDensity", 2);
		UpdateParticleDensity();
		bool flag3 = Mathf.Approximately(particleSystem.emission.rateOverTimeMultiplier, 40f) && Mathf.Approximately(particleSystem2.emission.rateOverTimeMultiplier, 80f) && Mathf.Approximately(particleSystem3.emission.rateOverTimeMultiplier, 80f) && Mathf.Approximately(particleSystem4.emission.rateOverTimeMultiplier, 80f);
		particleSystem.Simulate(1f, withChildren: true, restart: true, fixedTimeStep: false);
		int particleCount2 = particleSystem.particleCount;
		flag3 = flag3 && particleCount2 > 0 && particleCount2 < particleCount;
		SprkTestPrefs.SetInt("Sprk.Graphics.ParticleDensity", 1);
		UpdateParticleDensity();
		flag3 &= Mathf.Approximately(particleSystem.emission.rateOverTimeMultiplier, 60f);
		gameObject2.SetActive(value: false);
		flag3 &= Mathf.Approximately(particleSystem.emission.rateOverTimeMultiplier, 80f);
		gameObject2.SetActive(value: true);
		flag3 &= Mathf.Approximately(particleSystem.emission.rateOverTimeMultiplier, 60f);
		ObserveParticles(particleObject);
		flag3 &= Mathf.Approximately(particleSystem.emission.rateOverTimeMultiplier, 60f);
		particleObject.Play = true;
		typeof(ParticleObject).GetMethod("FinalizeParticle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(particleObject, null);
		flag3 &= !particleObject.Play && Mathf.Approximately(particleSystem.emission.rateOverTimeMultiplier, 80f) && particleSystem3.emission.burstCount == 1;
		distanceEmitter.SetValue(particleObject, true);
		ObserveParticles(particleObject);
		flag3 &= Mathf.Approximately(particleSystem.emission.rateOverTimeMultiplier, 80f);
		distanceEmitter.SetValue(particleObject, false);
		Type declaringType = assetParticleRoot.DeclaringType;
		object obj3 = Activator.CreateInstance(declaringType, new object[1]);
		assetParticleRoot.SetValue(obj3, gameObject2, null);
		flag3 &= !ObserveAssetParticles(opened: false, obj3) && Mathf.Approximately(particleSystem.emission.rateOverTimeMultiplier, 80f);
		flag3 &= ObserveAssetParticles(opened: true, obj3) && Mathf.Approximately(particleSystem.emission.rateOverTimeMultiplier, 60f);
		declaringType.GetMethod("Close", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(obj3, new object[1] { false });
		flag3 &= Mathf.Approximately(particleSystem.emission.rateOverTimeMultiplier, 80f) && (bool)declaringType.GetProperty("IsEnd").GetValue(obj3, null);
		report.Add("particleDensityAndPooling=" + flag3 + " particlesFull=" + particleCount + " particlesMinimal=" + particleCount2);
		UnityEngine.Object.Destroy(gameObject2);
		ApplyPreset(0);
		SprkTestPrefs.SetInt("Sprk.Graphics.Scale", 200);
		SprkTestPrefs.SetInt("Sprk.Graphics.PreviewScale", 125);
		RenderTexture renderTexture = CreateTarget(320, 240, 24);
		renderTexture.Create();
		RenderTexture renderTexture2 = CreateWorldTarget(320, 240, 24);
		renderTexture2.Create();
		bool flag4 = renderTexture.width == 400 && renderTexture.height == 300 && renderTexture2.width == 640 && renderTexture2.height == 480;
		int[] array = new int[5] { 100, 125, 150, 175, 200 };
		foreach (int num in array)
		{
			SprkTestPrefs.SetInt("Sprk.Graphics.PreviewScale", num);
			ConfigureTarget(renderTexture, ownedTargets[renderTexture], 1);
			int num2 = num;
			flag4 &= renderTexture.width == 320 * num2 / 100 && renderTexture.height == 240 * num2 / 100 && renderTexture2.width == 640;
		}
		report.Add("independentPreviewScale=" + flag4);
		renderTexture2.Release();
		ownedTargets.Remove(renderTexture2);
		UnityEngine.Object.Destroy(renderTexture2);
		GameObject gameObject3 = new GameObject("TemporalResourceProbe");
		Camera camera = gameObject3.AddComponent<Camera>();
		camera.targetTexture = renderTexture;
		camera.cullingMask = 1073741824;
		bool flag5 = true;
		DepthTextureMode[] array2 = new DepthTextureMode[2]
		{
			DepthTextureMode.None,
			DepthTextureMode.Depth | DepthTextureMode.DepthNormals
		};
		for (int k = 0; k < array2.Length; k++)
		{
			DepthTextureMode depthTextureMode = (camera.depthTextureMode = array2[k]);
			array = new int[8] { 4, 1, 4, 2, 4, 3, 4, 0 };
			foreach (int num3 in array)
			{
				ApplyPreset(num3);
				LateUpdate();
				camera.Render();
				PostProcessLayer component = camera.GetComponent<PostProcessLayer>();
				flag5 = ((num3 != 4) ? (flag5 & (HistoryTextures(component) == 0 && camera.depthTextureMode == depthTextureMode)) : (flag5 & (HistoryTextures(component) == 2 && (camera.depthTextureMode & DepthTextureMode.MotionVectors) != 0)));
			}
		}
		report.Add("temporalResourceCleanup=" + flag5);
		camera.enabled = false;
		camera.targetTexture = null;
		renderTexture.Release();
		ownedTargets.Remove(renderTexture);
		UnityEngine.Object.Destroy(renderTexture);
		UnityEngine.Object.Destroy(gameObject3);
		ApplyPreset(method);
		SprkTestPrefs.SetInt("Sprk.Graphics.Scale", scale);
		SprkTestPrefs.SetInt("Sprk.Graphics.PreviewScale", value);
		SprkTestPrefs.SetInt("Sprk.Graphics.ParticleDensity", value2);
		yield return null;
	}

	private void RecordFrameCost(List<string> report, string label)
	{
		for (int i = 0; i < 20; i++)
		{
			LateUpdate();
		}
		MethodInfo method = typeof(GC).GetMethod("GetAllocatedBytesForCurrentThread", BindingFlags.Static | BindingFlags.Public);
		Func<long> func = ((method == null) ? null : ((Func<long>)Delegate.CreateDelegate(typeof(Func<long>), method)));
		Stopwatch stopwatch = new Stopwatch();
		long num = func?.Invoke() ?? 0;
		stopwatch.Start();
		for (int j = 0; j < 500; j++)
		{
			LateUpdate();
		}
		stopwatch.Stop();
		long num2 = ((func == null) ? (-1) : ((func() - num) / 500));
		report.Add("graphicsTick=" + label + " cpuUs=" + (stopwatch.Elapsed.TotalMilliseconds * 1000.0 / 500.0).ToString("F2", CultureInfo.InvariantCulture) + " allocatedBytes=" + num2);
	}

	private static bool OnlyLayers(Camera camera, int mask)
	{
		if (camera.cullingMask != 0)
		{
			return (camera.cullingMask & ~mask) == 0;
		}
		return false;
	}

	private static bool IsLobbyBackground(Camera camera)
	{
		return OnlyLayers(camera, lobbyMask);
	}

	private static bool IsModelCamera(Camera camera)
	{
		if (!(camera.GetComponent<UICamera>() == null) || camera.orthographic || OnlyLayers(camera, battleUiMask))
		{
			return OnlyLayers(camera, modelMask);
		}
		return true;
	}

	private static bool IsOverlayCamera(Camera camera)
	{
		if (!(camera.GetComponent<UICamera>() != null))
		{
			return OnlyLayers(camera, battleUiMask);
		}
		return true;
	}

	private bool IsUI(Camera camera)
	{
		if (!uiCameras.TryGetValue(camera, out var value) || !value)
		{
			value = (uiCameras[camera] = camera.GetComponent<UICamera>() != null);
		}
		return value;
	}

	private RenderTexture NativeTarget(Camera camera)
	{
		if (!redirected.TryGetValue(camera, out var value) || !(camera.targetTexture == world))
		{
			return camera.targetTexture;
		}
		return value;
	}

	private void CollectCameras()
	{
		int allCamerasCount = Camera.allCamerasCount;
		if (cameraBuffer.Length < allCamerasCount)
		{
			Array.Resize(ref cameraBuffer, Mathf.NextPowerOfTwo(allCamerasCount));
		}
		allCamerasCount = Camera.GetAllCameras(cameraBuffer);
		cameras.Clear();
		onscreen.Clear();
		worldCameras.Clear();
		for (int i = 0; i < allCamerasCount; i++)
		{
			if (cameraBuffer[i] != null && cameraBuffer[i].isActiveAndEnabled && !filterCameraSet.Contains(cameraBuffer[i]))
			{
				cameras.Add(cameraBuffer[i]);
			}
		}
		Array.Clear(cameraBuffer, 0, cameraBuffer.Length);
		for (int j = 1; j < cameras.Count; j++)
		{
			Camera camera = cameras[j];
			int num = j - 1;
			while (num >= 0 && cameras[num].depth > camera.depth)
			{
				cameras[num + 1] = cameras[num];
				num--;
			}
			cameras[num + 1] = camera;
		}
		float num2 = float.PositiveInfinity;
		foreach (Camera camera2 in cameras)
		{
			if (NativeTarget(camera2) == null && IsUI(camera2) && !IsLobbyBackground(camera2))
			{
				num2 = camera2.depth;
				break;
			}
		}
		foreach (Camera camera3 in cameras)
		{
			if (!(NativeTarget(camera3) != null) && !(camera3.depth >= num2) && (!IsUI(camera3) || IsLobbyBackground(camera3)))
			{
				onscreen.Add(camera3);
				if (IsModelCamera(camera3))
				{
					worldCameras.Add(camera3);
				}
			}
		}
		if (worldCameras.Count > 0)
		{
			float depth = worldCameras[worldCameras.Count - 1].depth;
			while (onscreen.Count > 0 && onscreen[onscreen.Count - 1].depth > depth)
			{
				onscreen.RemoveAt(onscreen.Count - 1);
			}
		}
	}

	private void PruneCaches()
	{
		removeCameras.Clear();
		foreach (KeyValuePair<Camera, CommandBuffer> presentationCommand in presentationCommands)
		{
			if (presentationCommand.Key == null)
			{
				presentationCommand.Value.Release();
				removeCameras.Add(presentationCommand.Key);
			}
		}
		foreach (Camera removeCamera in removeCameras)
		{
			presentationCommands.Remove(removeCamera);
		}
		removeTargets.Clear();
		foreach (RenderTexture key in ownedTargets.Keys)
		{
			if (key == null)
			{
				removeTargets.Add(key);
			}
		}
		foreach (RenderTexture removeTarget in removeTargets)
		{
			ownedTargets.Remove(removeTarget);
		}
		removeCameras.Clear();
		foreach (Camera key2 in layers.Keys)
		{
			if (key2 == null)
			{
				removeCameras.Add(key2);
			}
		}
		foreach (Camera removeCamera2 in removeCameras)
		{
			layers.Remove(removeCamera2);
			signatures.Remove(removeCamera2);
			positions.Remove(removeCamera2);
			rotations.Remove(removeCamera2);
			temporalResources.Remove(removeCamera2);
		}
		removeCameras.Clear();
		foreach (Camera key3 in uiCameras.Keys)
		{
			if (key3 == null)
			{
				removeCameras.Add(key3);
			}
		}
		foreach (Camera removeCamera3 in removeCameras)
		{
			uiCameras.Remove(removeCamera3);
		}
		helperLayers.RemoveWhere((PostProcessLayer layer) => layer == null);
		particleBudgets.RemoveAll((SprkParticleDensity budget) => budget == null);
	}

	private void RestoreFinalBlits()
	{
		foreach (KeyValuePair<PostProcessLayer, bool> finalBlitDefault in finalBlitDefaults)
		{
			if (finalBlitDefault.Key != null)
			{
				finalBlitDefault.Key.finalBlitToCameraTarget = finalBlitDefault.Value;
			}
		}
		finalBlitDefaults.Clear();
	}

	private void LateUpdate()
	{
		UpdateGlobalFilter(LateUpdateGraphics());
	}

	private bool LateUpdateGraphics()
	{
		UpdateCustomWindow();
		presentationFrame = -1;
		int num = ReadIndex("PostAA");
		int num2 = ReadIndex("SMAAQuality");
		int num3 = ReadIndex("FXAAQuality");
		int num4 = ReadIndex("TAASharpness");
		int samples = Samples;
		if (!SprkTestPrefs.HasKey("Sprk.Graphics.PostAA") && !SprkTestPrefs.HasKey("Sprk.Graphics.MSAA") && !SprkTestPrefs.HasKey("Sprk.Graphics.RenderScale") && !SprkTestPrefs.HasKey("Sprk.Graphics.Scale") && !SprkTestPrefs.HasKey("Sprk.Graphics.ParticleDensity") && !SprkTestPrefs.HasKey("Sprk.Graphics.BattleEffects") && !SprkTestPrefs.HasKey("Sprk.Graphics.EffectCoverage") && !SprkTestPrefs.HasKey("Sprk.Graphics.AdaptiveBattleScale") && !GlobalFilterActive)
		{
			return false;
		}
		int num5 = ((samples != 1) ? samples : 0);
		if (QualitySettings.antiAliasing != num5)
		{
			QualitySettings.antiAliasing = num5;
		}
		CollectCameras();
		UpdateBattleControls();
		UpdateParticleDensity();
		int effectiveWorldScale = EffectiveWorldScale;
		bool flag = true;
		foreach (Camera item in onscreen)
		{
			if (item.rect != new Rect(0f, 0f, 1f, 1f))
			{
				flag = false;
				break;
			}
		}
		bool flag2 = worldCameras.Count > 0 && flag && (effectiveWorldScale != 100 || num != 0 || samples > 1 || GlobalFilterActive);
		removeCameras.Clear();
		foreach (KeyValuePair<Camera, RenderTexture> item2 in redirected)
		{
			if (!flag2 || !onscreen.Contains(item2.Key))
			{
				if (item2.Key != null && item2.Key.targetTexture == world)
				{
					item2.Key.targetTexture = item2.Value;
				}
				removeCameras.Add(item2.Key);
			}
		}
		foreach (Camera removeCamera in removeCameras)
		{
			redirected.Remove(removeCamera);
		}
		if (flag2)
		{
			int num6 = samples;
			int num7 = (Screen.width * 397) ^ (Screen.height * 17) ^ effectiveWorldScale ^ (num6 * 31);
			TargetSize value = null;
			if (world == null || !world.IsCreated() || !ownedTargets.TryGetValue(world, out value) || value.width != Screen.width || value.height != Screen.height || value.scale != effectiveWorldScale || value.samples != num6)
			{
				RestoreTargets();
				bool flag3 = battleActive && AdaptiveBattleTarget != 0;
				RetireWorld(flag3 && world != null && value != null && value.width == Screen.width && value.height == Screen.height && value.samples == num6);
				if (!flag3)
				{
					ReleaseRetainedWorlds();
				}
				world = (flag3 ? TakeRetainedWorld(Screen.width, Screen.height, effectiveWorldScale, num6) : null);
				if (world == null)
				{
					world = CreateWorldTarget(Screen.width, Screen.height, 24);
					world.name = "SPRK World";
					world.Create();
				}
				ResetHistories();
			}
			foreach (Camera item3 in onscreen)
			{
				if (!redirected.ContainsKey(item3))
				{
					redirected[item3] = item3.targetTexture;
				}
				if (item3.targetTexture != world)
				{
					item3.targetTexture = world;
				}
				item3.allowMSAA = world.antiAliasing > 1;
			}
			lastWorld = onscreen[onscreen.Count - 1];
			Camera camera = null;
			foreach (Camera camera2 in cameras)
			{
				if (camera2.depth > lastWorld.depth && camera2.targetTexture == null && IsOverlayCamera(camera2))
				{
					camera = camera2;
					break;
				}
			}
			if (presentationCamera != camera)
			{
				foreach (CommandBuffer value4 in presentationCommands.Values)
				{
					value4.Clear();
				}
				presentationCamera = camera;
			}
			if (lastLogged != num7 + num * 101)
			{
				lastLogged = num7 + num * 101;
				UnityEngine.Debug.Log("SPRK graphics: mode=" + num + " target=" + world.width + "x" + world.height + " samples=" + world.antiAliasing + " worldCameras=" + onscreen.Count);
			}
		}
		else if (world != null || presentationCamera != null)
		{
			RestoreTargets();
			ReleaseWorld();
		}
		RestoreFinalBlits();
		bool flag4 = num == 0 || FindResources();
		status = (flag4 ? "Ready" : "Native post-processing resources unavailable");
		foreach (Camera camera3 in cameras)
		{
			bool flag5 = onscreen.Contains(camera3);
			if ((IsUI(camera3) || OnlyLayers(camera3, battleUiMask)) && !flag5)
			{
				if (camera3.targetTexture == null)
				{
					camera3.allowMSAA = false;
				}
				continue;
			}
			RenderTexture targetTexture = camera3.targetTexture;
			if (!flag5 && targetTexture != null && ownedTargets.TryGetValue(targetTexture, out var value2) && ConfigureTarget(targetTexture, value2, samples))
			{
				ResetHistories();
			}
			camera3.allowMSAA = camera3.targetTexture != null && camera3.targetTexture.antiAliasing > 1;
			if (!flag5 && (camera3.targetTexture == null || !ownedTargets.ContainsKey(camera3.targetTexture)))
			{
				continue;
			}
			bool flag6 = num == 3 && worldCameras.Count > 0;
			bool flag7 = !flag5 || (flag6 ? (camera3 == worldCameras[0]) : (camera3 == lastWorld));
			if (!layers.TryGetValue(camera3, out var value3) || value3 == null)
			{
				value3 = camera3.GetComponent<PostProcessLayer>();
			}
			if (value3 == null && num != 0 && flag7 && flag4)
			{
				value3 = camera3.gameObject.AddComponent<PostProcessLayer>();
				value3.Init(resources);
				value3.volumeTrigger = camera3.transform;
				value3.volumeLayer = 0;
				value3.fog.enabled = false;
				helperLayers.Add(value3);
			}
			if (!(value3 == null))
			{
				layers[camera3] = value3;
				if (flag5 && flag2)
				{
					finalBlitDefaults[value3] = value3.finalBlitToCameraTarget;
					value3.finalBlitToCameraTarget = false;
				}
				int num8 = ((flag7 && flag4) ? num : 0);
				RenderTexture targetTexture2 = camera3.targetTexture;
				if (num8 == 3 && (!SystemInfo.supportsMotionVectors || !value3.temporalAntialiasing.IsSupported() || (targetTexture2 != null && targetTexture2.antiAliasing > 1)))
				{
					num8 = 0;
					status = "TAA unavailable for camera " + camera3.name;
				}
				UpdateTemporalResources(camera3, value3, num8);
				if (helperLayers.Contains(value3) && value3.enabled != (num8 != 0))
				{
					value3.enabled = num8 != 0;
				}
				int num9 = num8 * 100000 + num2 * 1000 + num3 * 101 + num4 * 103 + ((targetTexture2 == null) ? Screen.width : targetTexture2.width) * 31 + ((targetTexture2 == null) ? Screen.height : targetTexture2.height);
				bool flag8 = num8 == 3 && positions.ContainsKey(camera3) && (Vector3.Distance(positions[camera3], camera3.transform.position) > 3f || Quaternion.Angle(rotations[camera3], camera3.transform.rotation) > 35f);
				bool num10 = !signatures.ContainsKey(camera3) || signatures[camera3] != num9;
				if (num10 || flag8)
				{
					value3.ResetHistory();
				}
				signatures[camera3] = num9;
				positions[camera3] = camera3.transform.position;
				rotations[camera3] = camera3.transform.rotation;
				if (num10)
				{
					value3.antialiasingMode = (PostProcessLayer.Antialiasing)num8;
					value3.fastApproximateAntialiasing.fastMode = num3 == 0;
					value3.fastApproximateAntialiasing.keepAlpha = true;
					value3.subpixelMorphologicalAntialiasing.quality = (SubpixelMorphologicalAntialiasing.Quality)num2;
					value3.temporalAntialiasing.jitterSpread = 0.75f;
					value3.temporalAntialiasing.sharpness = (float)num4 * 0.5f;
					value3.temporalAntialiasing.stationaryBlending = 0.9f;
					value3.temporalAntialiasing.motionBlending = 0.8f;
				}
			}
		}
		foreach (KeyValuePair<Camera, PostProcessLayer> layer in layers)
		{
			if (layer.Key != null && layer.Value != null && (!cameras.Contains(layer.Key) || (!onscreen.Contains(layer.Key) && (layer.Key.targetTexture == null || !ownedTargets.ContainsKey(layer.Key.targetTexture)))))
			{
				UpdateTemporalResources(layer.Key, layer.Value, 0);
				if (helperLayers.Contains(layer.Value))
				{
					layer.Value.enabled = false;
				}
			}
		}
		if (Time.frameCount % 120 == 0)
		{
			PruneCaches();
		}
		return true;
	}

	public static void ApplyParticleBudget(SprkParticleDensity budget)
	{
		budget.Apply(ParticleDensity, CurrentEffectCoverage, battleDensityFactor, (instance != null && instance.battleActive) ? instance.battleEffectMask : 0);
	}

	private void ResetBattleControls()
	{
		battleActive = false;
		battleEffectMask = 0;
		battleDensityFactor = 1f;
		effectSettings = -1;
		nextEffectBudget = 0f;
		adaptiveBattle.Configure(Scale, 0);
		lastBattleTimestamp = 0uL;
		nextBattleTiming = (lastValidBattleTiming = 0f);
		ReleaseRetainedWorlds();
	}

	private bool HasBattleCameraStack()
	{
		if (worldCameras.Count == 0 || battleUiMask == 0)
		{
			return false;
		}
		foreach (Camera camera in cameras)
		{
			if (NativeTarget(camera) == null && OnlyLayers(camera, battleUiMask))
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateBattleControls()
	{
		bool flag = HasBattleCameraStack();
		if (flag != battleActive)
		{
			ResetBattleControls();
			battleActive = flag;
		}
		int num = 0;
		if (flag)
		{
			foreach (Camera worldCamera in worldCameras)
			{
				num |= worldCamera.cullingMask;
			}
		}
		if (num != battleEffectMask)
		{
			battleEffectMask = num;
			effectSettings = -1;
		}
		int num2 = (battleActive ? AdaptiveBattleTarget : 0);
		if (num2 == 0 && retainedWorlds.Count > 0)
		{
			ReleaseRetainedWorlds();
		}
		adaptiveBattle.Configure(Scale, num2);
		if (num2 == 0 || !Application.isFocused || !FrameTimingManager.IsFeatureEnabled())
		{
			adaptiveBattle.Configure(Scale, 0);
			lastBattleTimestamp = 0uL;
			lastValidBattleTiming = Time.realtimeSinceStartup;
			return;
		}
		FrameTimingManager.CaptureFrameTimings();
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (realtimeSinceStartup < nextBattleTiming)
		{
			return;
		}
		nextBattleTiming = realtimeSinceStartup + 0.25f;
		if (FrameTimingManager.GetLatestTimings(1u, battleTiming) != 0)
		{
			FrameTiming frameTiming = battleTiming[0];
			if (frameTiming.frameStartTimestamp != 0L && frameTiming.frameStartTimestamp != lastBattleTimestamp && frameTiming.gpuFrameTime > 0.0 && frameTiming.gpuFrameTime < 250.0)
			{
				lastBattleTimestamp = frameTiming.frameStartTimestamp;
				lastValidBattleTiming = realtimeSinceStartup;
				double cpu = System.Math.Max(0.0, System.Math.Max(frameTiming.cpuMainThreadFrameTime - frameTiming.cpuMainThreadPresentWaitTime, frameTiming.cpuRenderThreadFrameTime));
				adaptiveBattle.Submit(frameTiming.gpuFrameTime, cpu);
				return;
			}
		}
		if (realtimeSinceStartup - lastValidBattleTiming > 2f)
		{
			adaptiveBattle.Restore();
		}
	}

	private void UpdateBattleEffects()
	{
		int num = ReadIndex("ParticleDensity");
		int num2 = (battleActive ? ReadIndex("BattleEffects") : 0);
		int currentEffectCoverage = CurrentEffectCoverage;
		int num3 = num + num2 * 4 + currentEffectCoverage * 16 + (battleActive ? 64 : 0);
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (num3 == effectSettings && (num2 == 0 || realtimeSinceStartup < nextEffectBudget))
		{
			return;
		}
		effectSettings = num3;
		nextEffectBudget = realtimeSinceStartup + 0.25f;
		float num4 = 0f;
		float num5 = 0f;
		if (num2 != 0)
		{
			foreach (SprkParticleDensity particleBudget in particleBudgets)
			{
				if (particleBudget != null)
				{
					num4 += particleBudget.DemandForMask(battleEffectMask, ParticleDensity, out var uncapped);
					num5 += uncapped;
				}
			}
		}
		float num6 = ((num2 == 1) ? 4800 : 2400);
		battleDensityFactor = ((num2 == 0 || num4 <= num6) ? 1f : (num6 / num5));
		for (int num7 = particleBudgets.Count - 1; num7 >= 0; num7--)
		{
			SprkParticleDensity sprkParticleDensity = particleBudgets[num7];
			if (sprkParticleDensity == null)
			{
				particleBudgets.RemoveAt(num7);
			}
			else
			{
				ApplyParticleBudget(sprkParticleDensity);
			}
		}
	}

	public static void UpdateCircleShadow(Transform pivot, Component cache)
	{
		if (!(pivot == null) && !(cache == null))
		{
			Transform transform = cache.transform;
			Vector3 position = transform.position;
			Vector3 position2 = pivot.position;
			if (position.x != position2.x || position.z != position2.z)
			{
				position.x = position2.x;
				position.z = position2.z;
				transform.position = position;
			}
			position = transform.localPosition;
			if (position.y != 0f)
			{
				position.y = 0f;
				transform.localPosition = position;
			}
			Quaternion rotation = transform.rotation;
			if (rotation.x != 0f || rotation.y != 0f || rotation.z != 0f || rotation.w != 1f)
			{
				transform.rotation = Quaternion.identity;
			}
		}
	}

	private IEnumerator BattleStackControlsProbe(List<string> report, Camera model, Camera hud)
	{
		int saved = SprkTestPrefs.GetInt("Sprk.Graphics.AdaptiveBattleScale", 0);
		int savedPreview = SprkTestPrefs.GetInt("Sprk.Graphics.PreviewScale", 0);
		SprkTestPrefs.SetInt("Sprk.Graphics.AdaptiveBattleScale", 1);
		SprkTestPrefs.SetInt("Sprk.Graphics.PreviewScale", 200);
		bool flag = HasBattleCameraStack() && battleActive;
		adaptiveBattle.Configure(200, 60);
		for (int i = 0; i < 6; i++)
		{
			adaptiveBattle.Submit(30.0, 5.0);
		}
		RenderTexture renderTexture = CreateWorldTarget(320, 240, 24);
		RenderTexture renderTexture2 = CreateTarget(320, 240, 24);
		flag &= EffectiveWorldScale == 175 && renderTexture.width == 560 && renderTexture.height == 420 && renderTexture2.width == 640 && renderTexture2.height == 480 && hud.targetTexture == null;
		ownedTargets.Remove(renderTexture);
		ownedTargets.Remove(renderTexture2);
		UnityEngine.Object.Destroy(renderTexture);
		UnityEngine.Object.Destroy(renderTexture2);
		SprkTestPrefs.SetInt("Sprk.Graphics.AdaptiveBattleScale", 0);
		LateUpdate();
		report.Add("battleScaleTargetRouting=" + (flag & (EffectiveWorldScale == 200 && model.targetTexture == world && world.width == Screen.width * 2 && hud.targetTexture == null)));
		SprkTestPrefs.SetInt("Sprk.Graphics.AdaptiveBattleScale", 1);
		LateUpdate();
		RenderTexture full = world;
		int reusesBefore = RetainedWorldReuses;
		Stopwatch timer = new Stopwatch();
		bool reuse = full != null && EffectiveWorldScale == 200 && full.width == Screen.width * 2;
		for (int j = 0; j < 6; j++)
		{
			adaptiveBattle.Submit(30.0, 5.0);
		}
		timer.Start();
		LateUpdate();
		timer.Stop();
		double allocateMs = timer.Elapsed.TotalMilliseconds;
		RenderTexture reduced = world;
		reuse &= EffectiveWorldScale == 175 && reduced != null && reduced != full && reduced.width == Mathf.RoundToInt((float)Screen.width * 1.75f) && retainedWorlds.Contains(full) && model.targetTexture == reduced;
		yield return new WaitForEndOfFrame();
		for (int k = 0; k < 12; k++)
		{
			adaptiveBattle.Submit(8.0, 4.0);
		}
		timer.Reset();
		timer.Start();
		LateUpdate();
		timer.Stop();
		double reuseMs = timer.Elapsed.TotalMilliseconds;
		reuse &= EffectiveWorldScale == 200 && world == full && full.IsCreated() && RetainedWorldReuses == reusesBefore + 1 && retainedWorlds.Contains(reduced) && model.targetTexture == full && hud.targetTexture == null;
		yield return new WaitForEndOfFrame();
		SprkTestPrefs.SetInt("Sprk.Graphics.AdaptiveBattleScale", 0);
		LateUpdate();
		reuse &= retainedWorlds.Count == 0 && world == full && model.targetTexture == full;
		report.Add("adaptiveWorldReuse=" + reuse + " allocateSwitchMs=" + allocateMs.ToString("F3") + " reuseSwitchMs=" + reuseMs.ToString("F3") + " budgetMB=" + RetainedWorldBudget / 1048576);
		int readings = 0;
		int available = 0;
		double gpu = 0.0;
		double cpu = 0.0;
		ulong stamp = 0uL;
		for (int l = 0; l < 24; l++)
		{
			FrameTimingManager.CaptureFrameTimings();
			yield return new WaitForEndOfFrame();
			uint latestTimings = FrameTimingManager.GetLatestTimings(1u, battleTiming);
			available += (int)latestTimings;
			if (latestTimings != 0 && battleTiming[0].frameStartTimestamp != stamp && battleTiming[0].gpuFrameTime > 0.0)
			{
				stamp = battleTiming[0].frameStartTimestamp;
				readings++;
				gpu += battleTiming[0].gpuFrameTime;
				cpu += battleTiming[0].cpuFrameTime;
			}
		}
		report.Add("nativeBattleFrameTimings=" + readings + " available=" + available + " timestamp=" + battleTiming[0].frameStartTimestamp + " rawGpu=" + battleTiming[0].gpuFrameTime + " rawCpu=" + battleTiming[0].cpuFrameTime + " gpuSupport=" + SystemInfo.supportsGpuRecorder + " gpuFrequency=" + FrameTimingManager.GetGpuTimerFrequency() + " cpuFrequency=" + FrameTimingManager.GetCpuTimerFrequency() + " gpuMs=" + ((readings == 0) ? 0.0 : (gpu / (double)readings)) + " cpuMs=" + ((readings == 0) ? 0.0 : (cpu / (double)readings)));
		report.Add("nativeBattleGpuTimingsAvailable=" + (readings > 0));
		SprkTestPrefs.SetInt("Sprk.Graphics.AdaptiveBattleScale", saved);
		SprkTestPrefs.SetInt("Sprk.Graphics.PreviewScale", savedPreview);
		LateUpdate();
	}

	private IEnumerator BattleControlsProbe(List<string> report)
	{
		SprkBattleResolution sprkBattleResolution = new SprkBattleResolution();
		sprkBattleResolution.Configure(200, 60);
		bool flag = sprkBattleResolution.Current == 200;
		for (int i = 0; i < 5; i++)
		{
			sprkBattleResolution.Submit(30.0, 5.0);
		}
		flag &= sprkBattleResolution.Current == 200;
		sprkBattleResolution.Submit(30.0, 5.0);
		flag &= sprkBattleResolution.Current == 175;
		for (int j = 0; j < 60; j++)
		{
			sprkBattleResolution.Submit(35.0, 5.0);
		}
		flag &= sprkBattleResolution.Current == 75;
		for (int k = 0; k < 12; k++)
		{
			sprkBattleResolution.Submit(8.0, 4.0);
		}
		flag &= sprkBattleResolution.Current == 100;
		sprkBattleResolution.Configure(200, 60);
		sprkBattleResolution.Restore();
		for (int l = 0; l < 24; l++)
		{
			sprkBattleResolution.Submit(30.0, 45.0);
		}
		flag &= sprkBattleResolution.Current == 200;
		for (int m = 0; m < 24; m++)
		{
			sprkBattleResolution.Submit(0.0, 1.0);
		}
		flag &= sprkBattleResolution.Current == 200;
		sprkBattleResolution.Configure(125, 120);
		flag &= sprkBattleResolution.Current == 125;
		for (int n = 0; n < 24; n++)
		{
			sprkBattleResolution.Submit(18.0, 2.0);
		}
		sprkBattleResolution.Configure(125, 0);
		report.Add("adaptiveBattleController=" + (flag & (sprkBattleResolution.Current == 125)));
		SprkBattleResolution sprkBattleResolution2 = new SprkBattleResolution();
		sprkBattleResolution2.Configure(200, 60);
		for (int num = 0; num < 6; num++)
		{
			sprkBattleResolution2.Submit(30.0, 5.0);
		}
		bool flag2 = sprkBattleResolution2.Current == 175;
		for (int num2 = 0; num2 < 12; num2++)
		{
			sprkBattleResolution2.Submit(8.0, 4.0);
		}
		flag2 &= sprkBattleResolution2.Current == 200;
		for (int num3 = 0; num3 < 6; num3++)
		{
			sprkBattleResolution2.Submit(30.0, 5.0);
		}
		flag2 &= sprkBattleResolution2.Current == 175 && sprkBattleResolution2.RaiseAfter == 24;
		for (int num4 = 0; num4 < 23; num4++)
		{
			sprkBattleResolution2.Submit(8.0, 4.0);
		}
		flag2 &= sprkBattleResolution2.Current == 175;
		sprkBattleResolution2.Submit(8.0, 4.0);
		flag2 &= sprkBattleResolution2.Current == 200;
		for (int num5 = 0; num5 < 41; num5++)
		{
			sprkBattleResolution2.Submit(14.0, 4.0);
		}
		flag2 &= sprkBattleResolution2.Current == 200 && sprkBattleResolution2.RaiseAfter == 12;
		for (int num6 = 0; num6 < 5; num6++)
		{
			for (int num7 = 0; num7 < 6; num7++)
			{
				sprkBattleResolution2.Submit(30.0, 5.0);
			}
			for (int num8 = 0; num8 < sprkBattleResolution2.RaiseAfter; num8++)
			{
				sprkBattleResolution2.Submit(8.0, 4.0);
			}
		}
		flag2 &= sprkBattleResolution2.RaiseAfter == 96;
		sprkBattleResolution2.Restore();
		report.Add("adaptiveBattleOscillation=" + (flag2 & (sprkBattleResolution2.RaiseAfter == 12 && sprkBattleResolution2.Current == 200)));
		int value = SprkTestPrefs.GetInt("Sprk.Graphics.AdaptiveBattleScale", 0);
		bool flag3 = true;
		int[] array = new int[5] { 0, 1, 2, 3, 4 };
		foreach (int num10 in array)
		{
			SprkTestPrefs.SetInt("Sprk.Graphics.AdaptiveBattleScale", num10);
			SprkTestPrefs.ClearGraphicsCache();
			int num11 = num10 switch
			{
				4 => 144, 
				3 => 30, 
				2 => 120, 
				1 => 60, 
				_ => 0, 
			};
			flag3 &= AdaptiveBattleTarget == num11;
			sprkBattleResolution.Configure(200, AdaptiveBattleTarget);
			if (num11 == 0)
			{
				sprkBattleResolution.Submit(100.0, 1.0);
				flag3 &= sprkBattleResolution.Current == 200;
				continue;
			}
			double num12 = 1000.0 / (double)num11;
			for (int num13 = 0; num13 < 6; num13++)
			{
				sprkBattleResolution.Submit(num12 * 0.9, 1.0);
			}
			flag3 &= sprkBattleResolution.Current == 200;
			for (int num14 = 0; num14 < 6; num14++)
			{
				sprkBattleResolution.Submit(num12 * 1.2, 1.0);
			}
			flag3 &= sprkBattleResolution.Current == 175;
			for (int num15 = 0; num15 < 12; num15++)
			{
				sprkBattleResolution.Submit(num12 * 0.5, 1.0);
			}
			flag3 &= sprkBattleResolution.Current == 200;
		}
		SprkTestPrefs.SetInt("Sprk.Graphics.AdaptiveBattleScale", value);
		report.Add("adaptiveBattleTargetsAndCompatibility=" + flag3);
		report.Add("nativeFrameTimingEnabled=" + FrameTimingManager.IsFeatureEnabled());
		int savedDensity = ReadIndex("ParticleDensity");
		int savedEffects = ReadIndex("BattleEffects");
		int savedCoverage = ReadIndex("EffectCoverage");
		int savedAdaptive = SprkTestPrefs.GetInt("Sprk.Graphics.AdaptiveBattleScale", 0);
		bool wasBattle = battleActive;
		int savedMask = battleEffectMask;
		SprkTestPrefs.SetInt("Sprk.Graphics.ParticleDensity", 0);
		SprkTestPrefs.SetInt("Sprk.Graphics.BattleEffects", 2);
		SprkTestPrefs.SetInt("Sprk.Graphics.EffectCoverage", 2);
		GameObject gameObject = new GameObject("CrowdedCosmeticEffectsProbe");
		List<SprkParticleDensity> list = new List<SprkParticleDensity>();
		List<ParticleSystem> list2 = new List<ParticleSystem>();
		for (int num16 = 0; num16 < 12; num16++)
		{
			GameObject gameObject2 = new GameObject("Cosmetic emitter " + num16);
			gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
			ParticleSystem item = ProbeEmitter(gameObject2.transform, "Decorative sparks");
			ObserveParticles(gameObject2.transform);
			list.Add(gameObject2.GetComponent<SprkParticleDensity>());
			list2.Add(item);
		}
		ParticleSystem particleSystem = ProbeEmitter(gameObject.transform, "Warning telegraph");
		ObserveParticles(gameObject.transform);
		SprkParticleDensity component = gameObject.GetComponent<SprkParticleDensity>();
		bool flag4 = component.Demand == 0f;
		component.Clear();
		GameObject gameObject3 = new GameObject("Preview effects probe");
		gameObject3.layer = LayerMask.NameToLayer("Bottom3D");
		ParticleSystem particleSystem2 = ProbeEmitter(gameObject3.transform, "Preview sparks");
		particleSystem2.gameObject.layer = gameObject3.layer;
		ObserveParticles(gameObject3.transform);
		float maxParticleSize = list2[0].GetComponent<ParticleSystemRenderer>().maxParticleSize;
		battleActive = true;
		battleEffectMask = 1;
		effectSettings = -1;
		UpdateBattleEffects();
		float num17 = 0f;
		foreach (SprkParticleDensity item2 in list)
		{
			item2.DemandForMask(1, 1f, out var uncapped);
			num17 += uncapped;
		}
		float num18 = 80f * Mathf.Min(1f, 2400f / num17);
		bool flag5 = flag4 && num18 < 80f && Mathf.Approximately(list2[0].emission.rateOverTimeMultiplier, num18) && Mathf.Approximately(particleSystem.emission.rateOverTimeMultiplier, 80f);
		flag5 &= Mathf.Approximately(particleSystem2.emission.rateOverTimeMultiplier, 80f);
		bool flag6 = Mathf.Approximately(list2[0].GetComponent<ParticleSystemRenderer>().maxParticleSize, Mathf.Min(maxParticleSize, 0.08f));
		for (int num19 = 0; num19 < 8; num19++)
		{
			list[0].Clear();
			list[0].Capture();
			ApplyParticleBudget(list[0]);
		}
		flag5 &= Mathf.Approximately(list2[0].emission.rateOverTimeMultiplier, num18);
		for (int num20 = 4; num20 < 12; num20++)
		{
			list[num20].gameObject.SetActive(value: false);
		}
		nextEffectBudget = 0f;
		UpdateBattleEffects();
		flag5 &= Mathf.Approximately(list2[0].emission.rateOverTimeMultiplier, 80f);
		for (int num21 = 4; num21 < 12; num21++)
		{
			list[num21].gameObject.SetActive(value: true);
		}
		nextEffectBudget = 0f;
		UpdateBattleEffects();
		flag5 &= Mathf.Approximately(list2[0].emission.rateOverTimeMultiplier, num18);
		battleActive = false;
		UpdateBattleEffects();
		flag5 &= Mathf.Approximately(list2[0].emission.rateOverTimeMultiplier, 80f);
		flag6 &= Mathf.Approximately(list2[0].GetComponent<ParticleSystemRenderer>().maxParticleSize, maxParticleSize);
		gameObject.SetActive(value: false);
		foreach (SprkParticleDensity item3 in list)
		{
			item3.Clear();
		}
		UnityEngine.Object.Destroy(gameObject);
		ReleaseParticles(gameObject3.transform);
		UnityEngine.Object.Destroy(gameObject3);
		report.Add("crowdedBattleEffectBudget=" + flag5 + " fullRate=80 budgetedRate=" + num18);
		report.Add("battleEffectCoverageAndRestore=" + flag6);
		GameObject gameObject4 = new GameObject("ShadowParentProbe");
		gameObject4.transform.position = new Vector3(3f, 5f, 7f);
		gameObject4.transform.rotation = Quaternion.Euler(20f, 30f, 10f);
		gameObject4.transform.localScale = new Vector3(2f, 3f, 4f);
		GameObject gameObject5 = new GameObject("ShadowProbe");
		gameObject5.transform.SetParent(gameObject4.transform, worldPositionStays: false);
		GameObject gameObject6 = new GameObject("ShadowPivotProbe");
		gameObject6.transform.position = new Vector3(9f, 8f, 6f);
		GameObject gameObject7 = new GameObject("NativeShadowReference");
		gameObject7.transform.SetParent(gameObject4.transform, worldPositionStays: false);
		Type type = typeof(HeroPreview).Assembly.GetType("NGameAsset.NCreature.RenderCache");
		Component value2 = gameObject5.AddComponent(type);
		Type type2 = type.Assembly.GetType("NGameAsset.NCreature.CircleShadowInstance");
		object uninitializedObject = FormatterServices.GetUninitializedObject(type2);
		type2.GetField("_pivotTm", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(uninitializedObject, gameObject6.transform);
		type2.GetField("_renderCache", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(uninitializedObject, value2);
		MethodInfo method = type2.GetMethod("NVespa.NSingleton.ILateUpdater.ChainLateUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
		bool flag7 = true;
		for (int num22 = 0; num22 < 6; num22++)
		{
			gameObject6.transform.position += new Vector3(1f, 0.1f, -0.5f);
			Vector3 position = gameObject7.transform.position;
			position.x = gameObject6.transform.position.x;
			position.z = gameObject6.transform.position.z;
			gameObject7.transform.position = position;
			position = gameObject7.transform.localPosition;
			position.y = 0f;
			gameObject7.transform.localPosition = position;
			gameObject7.transform.rotation = Quaternion.identity;
			method.Invoke(uninitializedObject, null);
			flag7 &= (gameObject5.transform.position - gameObject7.transform.position).sqrMagnitude < 1E-06f && Quaternion.Angle(gameObject5.transform.rotation, gameObject7.transform.rotation) < 0.01f;
		}
		report.Add("nativeShadowPosePreserved=" + flag7);
		gameObject4.transform.position = Vector3.zero;
		gameObject4.transform.rotation = Quaternion.identity;
		gameObject4.transform.localScale = Vector3.one;
		method.Invoke(uninitializedObject, null);
		gameObject5.transform.hasChanged = false;
		method.Invoke(uninitializedObject, null);
		report.Add("nativeShadowStableWrites=" + !gameObject5.transform.hasChanged);
		UnityEngine.Object.Destroy(gameObject4);
		UnityEngine.Object.Destroy(gameObject6);
		GameObject containerHost = new GameObject("PoolReuseProbe");
		Container container = containerHost.AddComponent<Container>();
		ParticleObject particleObject = new GameObject("Destroyed pooled object").AddComponent<ParticleObject>();
		ParticleObject live = new GameObject("Reusable pooled object").AddComponent<ParticleObject>();
		live.gameObject.SetActive(value: false);
		Queue<ObjectBase> queue = new Queue<ObjectBase>();
		queue.Enqueue(particleObject);
		queue.Enqueue(live);
		((Dictionary<Type, Queue<ObjectBase>>)typeof(Container).GetField("_unusedDic", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(container))[typeof(ParticleObject)] = queue;
		UnityEngine.Object.Destroy(particleObject.gameObject);
		yield return null;
		MethodInfo methodInfo = typeof(Container).GetMethod("DequeueUnused", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(typeof(ParticleObject));
		report.Add("nativePoolSkipsDestroyedHead=" + (methodInfo.Invoke(container, null) == live && live.gameObject.activeSelf && queue.Count == 0 && methodInfo.Invoke(container, null) == null));
		UnityEngine.Object.Destroy(live.gameObject);
		UnityEngine.Object.Destroy(containerHost);
		SprkTestPrefs.SetInt("Sprk.Graphics.ParticleDensity", savedDensity);
		SprkTestPrefs.SetInt("Sprk.Graphics.BattleEffects", savedEffects);
		SprkTestPrefs.SetInt("Sprk.Graphics.EffectCoverage", savedCoverage);
		SprkTestPrefs.SetInt("Sprk.Graphics.AdaptiveBattleScale", savedAdaptive);
		battleActive = wasBattle;
		battleEffectMask = savedMask;
		effectSettings = -1;
		UpdateBattleEffects();
	}

	private static GraphicsHelpInfo HelpFor(GameOptionBaseComponent row)
	{
		if (row == null)
		{
			return null;
		}
		string text = (row.name.StartsWith("SPRK ", StringComparison.Ordinal) ? row.name.Substring(5) : ((row.gameOptionData == null) ? null : row.gameOptionData.FieldName));
		if (text == null || !graphicsHelp.TryGetValue(text, out var value))
		{
			return null;
		}
		return value;
	}

	public static void ConfigureGraphicsHelp(GameOptionBaseComponent row)
	{
		if (HelpFor(row) == null)
		{
			return;
		}
		GameOptionBaseComponent.GameOptionRootInfo[] array = new GameOptionBaseComponent.GameOptionRootInfo[2] { row.ParentRoot, row.ChildRoot };
		foreach (GameOptionBaseComponent.GameOptionRootInfo gameOptionRootInfo in array)
		{
			if (gameOptionRootInfo != null && gameOptionRootInfo.ToolTipImage != null)
			{
				gameOptionRootInfo.ToolTipImage.SetActive(value: true);
			}
		}
		if (row.Collider_Tooltip == null)
		{
			return;
		}
		Collider[] collider_Tooltip = row.Collider_Tooltip;
		foreach (Collider collider in collider_Tooltip)
		{
			if (collider != null)
			{
				collider.enabled = true;
			}
		}
	}

	public static bool ShowGraphicsHelp(GameOptionBaseComponent row)
	{
		GraphicsHelpInfo graphicsHelpInfo = HelpFor(row);
		if (graphicsHelpInfo == null)
		{
			return false;
		}
		UIWidget component = row.GetComponent<UIWidget>();
		if (component == null)
		{
			return true;
		}
		GameOptionTooltip.Open(component, new TooltipTextInfo
		{
			Name = new StringProxy
			{
				Value = graphicsHelpInfo.title
			},
			Desc = new StringProxy
			{
				Value = graphicsHelpInfo.description
			}
		});
		return true;
	}

	public static bool HideGraphicsHelp(GameOptionBaseComponent row)
	{
		if (HelpFor(row) == null)
		{
			return false;
		}
		UIWidget component = row.GetComponent<UIWidget>();
		if (component != null)
		{
			GameOptionTooltip.Hide(component);
		}
		return true;
	}

	private IEnumerator GraphicsHelpProbe(List<string> report, string output)
	{
		LoginManagement manager = SceneAutoBehaviour<LoginManagement>.instance;
		LoginAccountInfo loginAccountInfo = manager.GetInstance<LoginAccountInfo>();
		SprkLoginSettings.Configure(loginAccountInfo);
		loginAccountInfo.GetComponent<SprkLoginSettings>().OpenSettings();
		yield return new WaitForSecondsRealtime(0.5f);
		GameOptionWindow settings = manager.GetInstance<GameOptionWindow>();
		Camera[] allCameras = Camera.allCameras;
		foreach (Camera camera in allCameras)
		{
			if (camera.name == "BI UI Camera")
			{
				camera.gameObject.SetActive(value: false);
			}
		}
		GameOptionBaseComponent[] rows = settings.GetComponentsInChildren<GameOptionBaseComponent>(includeInactive: true);
		bool helpOk = true;
		bool isolated = true;
		bool events = true;
		int checkedRows = 0;
		GameOptionBaseComponent frameRow = rows.Single((GameOptionBaseComponent r) => r.gameOptionData != null && r.gameOptionData.FieldName == "Graphic_FrameControl");
		GameOptionBaseComponent vsyncRow = rows.Single((GameOptionBaseComponent r) => r.gameOptionData != null && r.gameOptionData.FieldName == "Graphic_VSyncControl");
		helpOk &= frameRow.transform.parent == vsyncRow.transform.parent && vsyncRow.transform.GetSiblingIndex() == frameRow.transform.GetSiblingIndex() + 1;
		MethodInfo onShow = typeof(GameOptionBaseComponent).GetMethod("OnShowTooltip", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		MethodInfo onHide = typeof(GameOptionBaseComponent).GetMethod("OnHideTooltip", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		BindingFlags labelFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		List<string> records = new List<string>();
		GameOptionBaseComponent[] array = rows;
		foreach (GameOptionBaseComponent row in array)
		{
			GraphicsHelpInfo help = HelpFor(row);
			if (help == null)
			{
				continue;
			}
			checkedRows++;
			if (row.name == "SPRK FXAA Quality")
			{
				ApplyPreset(1);
			}
			else if (row.name == "SPRK SMAA Quality")
			{
				ApplyPreset(2);
			}
			else if (row.name == "SPRK MSAA Samples")
			{
				ApplyPreset(3);
			}
			else if (row.name == "SPRK TAA Sharpness")
			{
				ApplyPreset(4);
			}
			Setup(settings);
			ConfigureGraphicsHelp(row);
			helpOk &= row.ParentRoot.ToolTipImage != null && row.ParentRoot.ToolTipImage.activeSelf;
			if (row.gameOptionData != null && row.gameOptionData.Index != 0)
			{
				GameOptionData data = GameRoot.Instance.GameOptionDataContainer.GetData(row.gameOptionData.Index);
				isolated &= data == row.gameOptionData && data.ToolTipDesc != help.description;
			}
			UIScrollView componentInParent = row.GetComponentInParent<UIScrollView>();
			if (componentInParent != null)
			{
				UIPanel component = componentInParent.GetComponent<UIPanel>();
				Vector4 finalClipRegion = component.finalClipRegion;
				Vector3 vector = component.transform.InverseTransformPoint(row.transform.position);
				componentInParent.MoveRelative(new Vector3(0f, finalClipRegion.y - vector.y, 0f));
				componentInParent.RestrictWithinBounds(instant: true);
			}
			onShow.Invoke(row, null);
			yield return null;
			UIWidget owner = row.GetComponent<UIWidget>();
			GameOptionTooltip gameOptionTooltip = Resources.FindObjectsOfTypeAll<GameOptionTooltip>().FirstOrDefault((GameOptionTooltip t) => t.gameObject.activeInHierarchy);
			events &= owner != null && GameOptionTooltip.IsVisible(owner) && gameOptionTooltip != null;
			if (gameOptionTooltip != null)
			{
				UILabel uILabel = (UILabel)gameOptionTooltip.GetType().BaseType.GetField("DescLabel", labelFlags).GetValue(gameOptionTooltip);
				UILabel uILabel2 = (UILabel)gameOptionTooltip.GetType().BaseType.GetField("NameLabel", labelFlags).GetValue(gameOptionTooltip);
				helpOk &= uILabel.text == help.description && uILabel2.text == help.title;
				records.Add(help.title + "\n" + uILabel.text + "\n");
				if (help.title == "Max Frame" || help.title == "Adaptive Battle Scale" || help.title == "Render Scale")
				{
					yield return new WaitForEndOfFrame();
					Texture2D texture2D = ScreenCapture.CaptureScreenshotAsTexture();
					File.WriteAllBytes(Path.Combine(output, "help-" + help.title.Replace(" ", "-") + ".png"), texture2D.EncodeToPNG());
					UnityEngine.Object.Destroy(texture2D);
				}
			}
			onHide.Invoke(row, null);
			yield return null;
			events &= !GameOptionTooltip.IsVisible(owner);
			if (help.title == "Max Frame")
			{
				yield return new WaitForEndOfFrame();
				Texture2D texture2D2 = ScreenCapture.CaptureScreenshotAsTexture();
				File.WriteAllBytes(Path.Combine(output, "frame-vsync-order.png"), texture2D2.EncodeToPNG());
				UnityEngine.Object.Destroy(texture2D2);
			}
			if (!(help.title == "Render Scale"))
			{
				continue;
			}
			int savedPreset = RenderScalePreset;
			UIToggle[] presets = (from t in row.GetComponentsInChildren<UIToggle>(includeInactive: true)
				where t.gameObject.activeSelf
				select t).ToArray();
			GameOptionBaseComponent world = rows.Single((GameOptionBaseComponent r) => r.name == "SPRK World Scale");
			GameOptionBaseComponent preview = rows.Single((GameOptionBaseComponent r) => r.name == "SPRK Preview Scale");
			bool layout = true;
			int[] array2 = new int[2] { 2, 5 };
			foreach (int choice in array2)
			{
				presets[choice].Set(state: true);
				yield return null;
				bool custom = choice == 5;
				GameOptionBaseComponent[] array3 = new GameOptionBaseComponent[2] { world, preview };
				foreach (GameOptionBaseComponent gameOptionBaseComponent in array3)
				{
					layout &= gameOptionBaseComponent.ChildRoot.Root.activeSelf && !gameOptionBaseComponent.ParentRoot.Root.activeSelf && gameOptionBaseComponent.GetComponent<UIWidget>().isGrayscale == !custom;
					foreach (UIToggle item in from t in gameOptionBaseComponent.GetComponentsInChildren<UIToggle>(includeInactive: true)
						where t.gameObject.activeSelf
						select t)
					{
						Collider component2 = item.GetComponent<Collider>();
						layout &= component2 != null && component2.enabled == custom;
					}
				}
				yield return new WaitForEndOfFrame();
				Texture2D texture2D3 = ScreenCapture.CaptureScreenshotAsTexture();
				File.WriteAllBytes(Path.Combine(output, custom ? "scale-hierarchy-custom.png" : "scale-hierarchy-preset.png"), texture2D3.EncodeToPNG());
				UnityEngine.Object.Destroy(texture2D3);
			}
			report.Add("renderScaleNativeHierarchy=" + layout);
			SprkTestPrefs.SetInt("Sprk.Graphics.RenderScale", savedPreset);
			Setup(settings);
		}
		report.Add("graphicsHelpRows=" + checkedRows + " correctText=" + helpOk + " nativeShowHide=" + events + " tableIsolation=" + isolated);
		File.WriteAllLines(Path.Combine(output, "help-text.txt"), records.ToArray());
		settings.ReOpen();
		yield return null;
		helpOk &= vsyncRow.transform.GetSiblingIndex() == frameRow.transform.GetSiblingIndex() + 1;
		helpOk &= (from r in settings.GetComponentsInChildren<GameOptionBaseComponent>(includeInactive: true)
			where HelpFor(r) != null
			select r).All((GameOptionBaseComponent r) => r.ParentRoot.ToolTipImage != null && r.ParentRoot.ToolTipImage.activeSelf);
		report.Add("graphicsHelpAndPooling=" + (checkedRows == graphicsHelp.Count && helpOk && isolated && events));
		settings.Close();
	}

	private static int NormalizeScale(int value)
	{
		return Mathf.Clamp(Mathf.RoundToInt((float)(value - 100) / 25f) * 25 + 100, 100, 200);
	}

	private static void EnsureScaleSettings()
	{
		if (!SprkTestPrefs.HasKey("Sprk.Graphics.RenderScale"))
		{
			int num = NormalizeScale(SprkTestPrefs.GetInt("Sprk.Graphics.Scale", 100));
			int num2 = SprkTestPrefs.GetInt("Sprk.Graphics.PreviewScale", 0);
			int num3 = ((num2 == 0) ? num : NormalizeScale(num2));
			SprkTestPrefs.SetInt("Sprk.Graphics.Scale", num);
			SprkTestPrefs.SetInt("Sprk.Graphics.PreviewScale", num3);
			SprkTestPrefs.SetInt("Sprk.Graphics.RenderScale", (num == num3) ? num : 0);
			SprkTestPrefs.Save();
		}
	}

	private static bool IsScaleChild(string key)
	{
		if (!(key == "Scale"))
		{
			return key == "PreviewScale";
		}
		return true;
	}

	private static void RefreshScaleChildren(GameOptionWindow.TabUIInfo tab)
	{
		string[] array = new string[2] { "World Scale", "Preview Scale" };
		foreach (string title in array)
		{
			GameOptionToggle4Component gameOptionToggle4Component = tab.GameOptionGrid.GetComponentsInChildren<GameOptionToggle4Component>(includeInactive: true).FirstOrDefault((GameOptionToggle4Component r) => r.name == "SPRK " + title);
			if (!(gameOptionToggle4Component == null))
			{
				SprkScaleRow component = gameOptionToggle4Component.GetComponent<SprkScaleRow>();
				if (component != null && component.Refresh != null)
				{
					component.Refresh();
				}
				gameOptionToggle4Component.ParentRoot.Root.SetActive(value: false);
				gameOptionToggle4Component.ChildRoot.Root.SetActive(value: true);
				gameOptionToggle4Component.ChildRoot.Label_OptionName.text = title;
				gameOptionToggle4Component.SetUIState(CustomScaleMode);
				gameOptionToggle4Component.SetGrayScale(CustomScaleMode);
				ConfigureGraphicsHelp(gameOptionToggle4Component);
			}
		}
	}

	private IEnumerator RenderScaleSettingsProbe(List<string> report, GameOptionWindow window)
	{
		string environmentVariable = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
		bool flag = true;
		try
		{
			int[,] array = new int[4, 4]
			{
				{ 150, 0, 150, 150 },
				{ 200, 125, 0, 125 },
				{ 100, 75, 100, 100 },
				{ 175, 150, 0, 150 }
			};
			for (int i = 0; i < array.GetLength(0); i++)
			{
				Environment.SetEnvironmentVariable("SPRK_AUTOMATION_PROFILE", environmentVariable + "-scale-migration-" + i);
				SprkTestPrefs.DeleteKey("Sprk.Graphics.RenderScale");
				SprkTestPrefs.SetInt("Sprk.Graphics.Scale", array[i, 0]);
				SprkTestPrefs.SetInt("Sprk.Graphics.PreviewScale", array[i, 1]);
				EnsureScaleSettings();
				flag &= RenderScalePreset == array[i, 2] && Scale == array[i, 0] && PreviewScale == array[i, 3];
			}
		}
		finally
		{
			Environment.SetEnvironmentVariable("SPRK_AUTOMATION_PROFILE", environmentVariable);
		}
		report.Add("renderScaleMigration=" + flag);
		GameOptionToggle4Component[] componentsInChildren = window.GetComponentsInChildren<GameOptionToggle4Component>(includeInactive: true);
		GameOptionToggle4Component arg = componentsInChildren.Single((GameOptionToggle4Component r) => r.name == "SPRK Render Scale");
		GameOptionToggle4Component gameOptionToggle4Component = componentsInChildren.Single((GameOptionToggle4Component r) => r.name == "SPRK World Scale");
		GameOptionToggle4Component gameOptionToggle4Component2 = componentsInChildren.Single((GameOptionToggle4Component r) => r.name == "SPRK Preview Scale");
		Func<GameOptionToggle4Component, UIToggle[]> func = (GameOptionToggle4Component r) => (from t in r.GetComponentsInChildren<UIToggle>(includeInactive: true)
			where t.gameObject.activeSelf
			select t).ToArray();
		UIToggle[] array2 = func(arg);
		bool flag2 = array2.Length == 6 && func(gameOptionToggle4Component).Length == 5 && func(gameOptionToggle4Component2).Length == 5;
		flag2 &= gameOptionToggle4Component.ChildRoot.Root.activeSelf && !gameOptionToggle4Component.ParentRoot.Root.activeSelf && gameOptionToggle4Component2.ChildRoot.Root.activeSelf && !gameOptionToggle4Component2.ParentRoot.Root.activeSelf;
		SprkTestPrefs.SetInt("Sprk.Graphics.RenderScale", 0);
		SprkTestPrefs.SetInt("Sprk.Graphics.Scale", 150);
		SprkTestPrefs.SetInt("Sprk.Graphics.PreviewScale", 175);
		Setup(window);
		for (int num = 0; num < 5; num++)
		{
			array2[num].Set(state: true);
			flag2 &= Scale == 100 + 25 * num && PreviewScale == Scale && gameOptionToggle4Component.GetComponent<UIWidget>().isGrayscale && gameOptionToggle4Component2.GetComponent<UIWidget>().isGrayscale;
			flag2 &= func(gameOptionToggle4Component)[num].value && func(gameOptionToggle4Component2)[num].value;
			func(gameOptionToggle4Component)[(num + 1) % 5].Set(state: true);
			func(gameOptionToggle4Component2)[(num + 2) % 5].Set(state: true);
			flag2 &= Scale == 100 + 25 * num && PreviewScale == Scale && SprkTestPrefs.GetInt("Sprk.Graphics.Scale", 0) == 150 && SprkTestPrefs.GetInt("Sprk.Graphics.PreviewScale", 0) == 175;
		}
		array2[5].Set(state: true);
		flag2 &= CustomScaleMode && Scale == 150 && PreviewScale == 175 && !gameOptionToggle4Component.GetComponent<UIWidget>().isGrayscale && !gameOptionToggle4Component2.GetComponent<UIWidget>().isGrayscale;
		for (int num2 = 0; num2 < 5; num2++)
		{
			func(gameOptionToggle4Component)[num2].Set(state: true);
			func(gameOptionToggle4Component2)[4 - num2].Set(state: true);
			flag2 &= Scale == 100 + 25 * num2 && PreviewScale == 200 - 25 * num2;
		}
		RenderTexture renderTexture = CreateWorldTarget(320, 240, 24);
		RenderTexture renderTexture2 = CreateTarget(320, 240, 24);
		array2[2].Set(state: true);
		ConfigureTarget(renderTexture, ownedTargets[renderTexture], 1);
		ConfigureTarget(renderTexture2, ownedTargets[renderTexture2], 1);
		flag2 &= renderTexture.width == 480 && renderTexture2.width == 480;
		array2[5].Set(state: true);
		ConfigureTarget(renderTexture, ownedTargets[renderTexture], 1);
		ConfigureTarget(renderTexture2, ownedTargets[renderTexture2], 1);
		flag2 &= renderTexture.width == 640 && renderTexture2.width == 320;
		ownedTargets.Remove(renderTexture);
		ownedTargets.Remove(renderTexture2);
		UnityEngine.Object.Destroy(renderTexture);
		UnityEngine.Object.Destroy(renderTexture2);
		report.Add("renderScaleParentChildren=" + flag2);
		SprkTestPrefs.SetInt("Sprk.Graphics.RenderScale", 0);
		Setup(window);
		yield break;
	}

	public static FilterValues FilterPresetDefaults(int preset)
	{
		int[] array = new int[FilterFields.Length];
		array[12] = ((Mathf.Clamp(preset, 0, FilterPresetNames.Length - 1) == 0) ? 100 : 50);
		return FilterValues.FromArray(array);
	}

	public static float[] EffectiveFilter(int preset, FilterValues v)
	{
		preset = Mathf.Clamp(preset, 0, FilterPresetNames.Length - 1);
		int[] array = filterPresetLooks[preset];
		float num = ((preset == 0) ? 0f : ((float)v.Intensity / 50f * (float)array[12] / 100f));
		float[] array2 = new float[12];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = Mathf.Clamp((float)array[i] * num + (float)v.Get(i), (float)FilterFields[i].Min * 2f, (float)FilterFields[i].Max * 2f);
		}
		return array2;
	}

	public static bool FilterHasEffect(int preset, FilterValues v)
	{
		return EffectiveFilter(preset, v).Any((float e) => Mathf.Abs(e) > 0.001f);
	}

	private static string FilterKey(int preset)
	{
		return "Sprk.Graphics.Filter.v2." + FilterPresetNames[preset];
	}

	private static string LegacyFilterKey(int preset)
	{
		return "Sprk.Graphics.Filter." + FilterPresetNames[preset];
	}

	public static FilterValues SavedFilter(int preset)
	{
		preset = Mathf.Clamp(preset, 0, FilterPresetNames.Length - 1);
		FilterValues fallback = FilterPresetDefaults(preset);
		FilterValues filterValues = FilterValues.Parse(SprkTestPrefs.GetString(FilterKey(preset), ""), fallback);
		if (preset != 0)
		{
			return filterValues;
		}
		return filterValues.With(12, 100);
	}

	public static void SaveFilter(int preset, FilterValues values)
	{
		if (preset >= 0 && preset < FilterPresetNames.Length)
		{
			if (preset == 0)
			{
				values = values.With(12, 100);
			}
			if (values.SameAs(FilterPresetDefaults(preset)))
			{
				SprkTestPrefs.DeleteKey(FilterKey(preset));
			}
			else
			{
				SprkTestPrefs.SetString(FilterKey(preset), values.Serialize());
			}
			SprkTestPrefs.Save();
		}
	}

	public static void ResetFilter(int preset)
	{
		if (preset >= 0 && preset < FilterPresetNames.Length)
		{
			SprkTestPrefs.DeleteKey(FilterKey(preset));
			SprkTestPrefs.Save();
		}
	}

	public static void RestoreDefaultFilters()
	{
		for (int i = 0; i < FilterPresetNames.Length; i++)
		{
			SprkTestPrefs.DeleteKey(FilterKey(i));
			SprkTestPrefs.DeleteKey(LegacyFilterKey(i));
		}
		SprkTestPrefs.DeleteKey("Sprk.Graphics.Filter.Preset");
		SprkTestPrefs.Save();
	}

	public static void PreviewFilter(int preset, FilterValues values)
	{
		previewFilterPreset = Mathf.Clamp(preset, 0, FilterPresetNames.Length - 1);
		previewFilterValues = values?.Clone();
	}

	public static void EndFilterPreview()
	{
		previewFilterPreset = -1;
		previewFilterValues = null;
	}

	public static string FilterValueLabel(int field, int value)
	{
		if (field == 2)
		{
			return ((value > 0) ? "+" : "") + (value * 40).ToString(CultureInfo.InvariantCulture) + "K";
		}
		return value.ToString(CultureInfo.InvariantCulture);
	}

	public static bool IsFilterCamera(Camera camera)
	{
		if (instance != null && camera != null)
		{
			return instance.filterCameraSet.Contains(camera);
		}
		return false;
	}

	private static bool EnsureFilterLayers()
	{
		if (worldFilterLayer >= 0 && previewFilterLayer >= 0 && thumbFilterLayer >= 0)
		{
			return true;
		}
		List<int> list = new List<int>();
		int num = 31;
		while (num >= 3 && list.Count < 3)
		{
			if (string.IsNullOrEmpty(LayerMask.LayerToName(num)))
			{
				list.Add(num);
			}
			num--;
		}
		if (list.Count < 3)
		{
			UnityEngine.Debug.LogWarning("SPRK global filter: no free volume layers");
			return false;
		}
		worldFilterLayer = list[0];
		previewFilterLayer = list[1];
		thumbFilterLayer = list[2];
		UnityEngine.Debug.Log("SPRK global filter: volume layers world=" + worldFilterLayer + " preview=" + previewFilterLayer + " thumbnail=" + thumbFilterLayer);
		return true;
	}

	private static T AddEffect<T>(PostProcessProfile profile) where T : PostProcessEffectSettings
	{
		T val = ScriptableObject.CreateInstance<T>();
		val.hideFlags = HideFlags.DontSave;
		val.enabled.Override(x: false);
		profile.AddSettings(val);
		return val;
	}

	private FilterStack CreateFilterStack(string name, int layer, bool world)
	{
		FilterStack filterStack = new FilterStack();
		filterStack.host = new GameObject(name);
		filterStack.host.layer = layer;
		filterStack.host.transform.SetParent(base.transform, worldPositionStays: false);
		filterStack.profile = ScriptableObject.CreateInstance<PostProcessProfile>();
		filterStack.profile.hideFlags = HideFlags.DontSave;
		filterStack.profile.name = name;
		filterStack.grading = AddEffect<ColorGrading>(filterStack.profile);
		filterStack.grading.gradingMode.Override(GradingMode.LowDefinitionRange);
		if (world)
		{
			filterStack.bloom = AddEffect<Bloom>(filterStack.profile);
			filterStack.bloom.threshold.Override(0.8f);
			filterStack.bloom.softKnee.Override(0.5f);
			filterStack.bloom.diffusion.Override(7f);
			filterStack.vignette = AddEffect<Vignette>(filterStack.profile);
			filterStack.vignette.mode.Override(VignetteMode.Classic);
			filterStack.vignette.color.Override(Color.black);
			filterStack.vignette.smoothness.Override(0.4f);
			filterStack.vignette.roundness.Override(1f);
			filterStack.vignette.rounded.Override(x: false);
			filterStack.grain = AddEffect<Grain>(filterStack.profile);
			filterStack.grain.colored.Override(x: false);
			filterStack.grain.size.Override(1f);
			filterStack.grain.lumContrib.Override(0.8f);
			filterStack.sharpen = AddEffect<Sharpen>(filterStack.profile);
		}
		filterStack.volume = filterStack.host.AddComponent<PostProcessVolume>();
		filterStack.volume.isGlobal = true;
		filterStack.volume.priority = 1000f;
		filterStack.volume.sharedProfile = filterStack.profile;
		filterStack.volume.weight = 0f;
		return filterStack;
	}

	private static float PaletteSaturation(float x)
	{
		if (!(x > 0f))
		{
			return x * 8f;
		}
		return x * 4f;
	}

	private static void ApplyFilterValues(FilterStack stack, int preset, FilterValues values)
	{
		if (stack != null)
		{
			float[] array = EffectiveFilter(preset, values);
			float x = array[0];
			float num = array[1];
			float num2 = array[2];
			float num3 = array[3];
			float value = array[4];
			float value2 = array[5];
			float num4 = array[6];
			float num5 = array[7];
			float num6 = array[8];
			float num7 = array[9];
			float num8 = array[10];
			float num9 = array[11];
			stack.volume.weight = 1f;
			ColorGrading grading = stack.grading;
			grading.enabled.Override(x: true);
			grading.temperature.Override(Mathf.Clamp(num2 * 1.2f, -100f, 100f));
			grading.brightness.Override(Mathf.Clamp(value, -100f, 100f));
			grading.contrast.Override(Mathf.Clamp(value2, -100f, 100f));
			float num10 = num4 * 1.2f + PaletteSaturation(x);
			if (num5 < 0f)
			{
				num10 += (0f - num5) * 0.8f;
			}
			num10 = Mathf.Clamp(num10, -100f, 100f);
			if (num5 > 0f)
			{
				num10 = Mathf.Lerp(num10, -100f, num5 / 50f);
			}
			grading.saturation.Override(num10);
			grading.lift.Override(new Vector4(0f, 0f, 0f, num6 / 50f * 0.3f + num * 0.05f));
			grading.gamma.Override(new Vector4(0f, 0f, 0f, num * 0.04f));
			grading.gain.Override(new Vector4(0f, 0f, 0f, (num7 < 0f) ? (num7 / 50f * 0.25f) : 0f));
			if (stack.bloom != null)
			{
				stack.bloom.enabled.Override(num7 > 0f);
				stack.bloom.intensity.Override(Mathf.Max(0f, num7) / 50f * 4f);
			}
			if (stack.vignette != null)
			{
				stack.vignette.enabled.Override(num8 > 0f);
				stack.vignette.intensity.Override(Mathf.Min(1f, num8 / 50f * 0.45f));
			}
			if (stack.grain != null)
			{
				stack.grain.enabled.Override(num9 > 0f);
				stack.grain.intensity.Override(Mathf.Min(1f, num9 / 50f * 0.6f));
			}
			if (stack.sharpen != null)
			{
				stack.sharpen.enabled.Override(num3 > 0f);
				stack.sharpen.Intensity.Override(num3 / 50f);
			}
		}
	}

	private Camera CreateFilterCamera(Camera anchor, bool world)
	{
		GameObject gameObject = new GameObject("SPRK Filter Camera (" + anchor.name + ")");
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		gameObject.SetActive(value: false);
		Camera camera = gameObject.AddComponent<Camera>();
		camera.clearFlags = CameraClearFlags.Nothing;
		camera.cullingMask = 0;
		camera.orthographic = true;
		camera.orthographicSize = 1f;
		camera.nearClipPlane = 0.01f;
		camera.farClipPlane = 1f;
		camera.useOcclusionCulling = false;
		camera.allowHDR = false;
		camera.allowDynamicResolution = false;
		PostProcessLayer postProcessLayer = gameObject.AddComponent<PostProcessLayer>();
		postProcessLayer.Init(resources);
		postProcessLayer.volumeTrigger = gameObject.transform;
		postProcessLayer.volumeLayer = (world ? WorldFilterMask : PreviewFilterMask);
		postProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
		postProcessLayer.fog.enabled = false;
		postProcessLayer.stopNaNPropagation = false;
		if (world)
		{
			camera.AddCommandBuffer(CameraEvent.AfterImageEffects, OpaqueAlphaCommand());
		}
		filterCameraSet.Add(camera);
		gameObject.SetActive(value: true);
		return camera;
	}

	private CommandBuffer OpaqueAlphaCommand()
	{
		if (opaqueAlpha != null)
		{
			return opaqueAlpha;
		}
		PropertySheetFactory propertySheetFactory = new PropertySheetFactory();
		if (discardAlpha == null)
		{
			discardAlpha = propertySheetFactory.Get(resources.shaders.discardAlpha);
		}
		if (copySheet == null)
		{
			copySheet = propertySheetFactory.Get(resources.shaders.copy);
		}
		opaqueAlpha = new CommandBuffer
		{
			name = "SPRK Global Filter opaque alpha"
		};
		int num = Shader.PropertyToID("_SprkGlobalFilterAlpha");
		opaqueAlpha.GetTemporaryRT(num, -1, -1, 0, FilterMode.Point, RenderTextureFormat.ARGB32);
		opaqueAlpha.BlitFullscreenTriangle(BuiltinRenderTextureType.CameraTarget, num, discardAlpha, 0);
		opaqueAlpha.BlitFullscreenTriangle(num, BuiltinRenderTextureType.CameraTarget, copySheet, 0);
		opaqueAlpha.ReleaseTemporaryRT(num);
		return opaqueAlpha;
	}

	private void DestroyFilterCamera(Camera camera)
	{
		if (!(camera == null))
		{
			filterCameraSet.Remove(camera);
			camera.targetTexture = null;
			camera.gameObject.SetActive(value: false);
			UnityEngine.Object.Destroy(camera.gameObject);
		}
	}

	public static void ReleaseGlobalFilterThumbnails()
	{
		if (instance != null)
		{
			instance.ReleaseFilterThumbnails();
		}
	}

	private void ReleaseFilterCameras()
	{
		ReleaseFilterThumbnails();
		foreach (KeyValuePair<Camera, Camera> filterCamera in filterCameras)
		{
			DestroyFilterCamera(filterCamera.Value);
		}
		filterCameras.Clear();
		filterCameraSet.RemoveWhere((Camera c) => c == null);
	}

	private void SetFilterCamerasEnabled(bool enabled)
	{
		foreach (KeyValuePair<Camera, Camera> filterCamera in filterCameras)
		{
			if (filterCamera.Value != null && filterCamera.Value.enabled != enabled)
			{
				filterCamera.Value.enabled = enabled;
			}
		}
	}

	private float NextCameraDepth(float depth)
	{
		float num = float.PositiveInfinity;
		foreach (Camera camera in cameras)
		{
			if (camera.depth > depth && camera.depth < num)
			{
				num = camera.depth;
			}
		}
		if (!float.IsPositiveInfinity(num))
		{
			return (depth + num) * 0.5f;
		}
		return depth + 1f;
	}

	private void StripFilterLayers(Camera camera)
	{
		int num = WorldFilterMask | PreviewFilterMask | ThumbFilterMask;
		PostProcessLayer component = camera.GetComponent<PostProcessLayer>();
		if (component != null && (component.volumeLayer.value & num) != 0)
		{
			component.volumeLayer = component.volumeLayer.value & ~num;
		}
	}

	private void UpdateGlobalFilter(bool collected)
	{
		if (!GlobalFilterActive)
		{
			SetFilterCamerasEnabled(enabled: false);
			ReleaseFilterSnapshotCommand();
			filterStatus = "Inactive";
			return;
		}
		if (!FindResources())
		{
			SetFilterCamerasEnabled(enabled: false);
			filterStatus = "Post-processing resources unavailable";
			return;
		}
		if (!EnsureFilterLayers())
		{
			SetFilterCamerasEnabled(enabled: false);
			filterStatus = "No free volume layers";
			return;
		}
		if (worldFilter == null)
		{
			worldFilter = CreateFilterStack("SPRK Global Filter (World)", worldFilterLayer, world: true);
		}
		if (previewFilter == null)
		{
			previewFilter = CreateFilterStack("SPRK Global Filter (Preview)", previewFilterLayer, world: false);
		}
		FilterValues activeFilterValues = ActiveFilterValues;
		string text = ActiveFilterPreset + ":" + activeFilterValues.Serialize();
		if (appliedFilter != text)
		{
			ApplyFilterValues(worldFilter, ActiveFilterPreset, activeFilterValues);
			ApplyFilterValues(previewFilter, ActiveFilterPreset, activeFilterValues);
			appliedFilter = text;
		}
		if (!collected)
		{
			CollectCameras();
		}
		filterAnchors.Clear();
		previewAnchors.Clear();
		if (world != null && lastWorld != null && lastWorld.targetTexture == world && lastWorld.isActiveAndEnabled)
		{
			filterAnchors[lastWorld] = true;
		}
		foreach (Camera camera in cameras)
		{
			StripFilterLayers(camera);
			RenderTexture targetTexture = camera.targetTexture;
			if (!(targetTexture == null) && !(targetTexture == world) && camera.cullingMask != 0 && !IsUI(camera) && ownedTargets.TryGetValue(targetTexture, out var value) && !value.world && (!previewAnchors.TryGetValue(targetTexture, out var value2) || value2.depth <= camera.depth))
			{
				previewAnchors[targetTexture] = camera;
			}
		}
		foreach (Camera value4 in previewAnchors.Values)
		{
			filterAnchors[value4] = false;
		}
		removeFilterCameras.Clear();
		foreach (KeyValuePair<Camera, Camera> filterCamera in filterCameras)
		{
			if (filterCamera.Key == null || filterCamera.Value == null || !filterAnchors.ContainsKey(filterCamera.Key) || filterAnchors[filterCamera.Key] != (filterCamera.Value.GetComponent<PostProcessLayer>().volumeLayer.value == WorldFilterMask))
			{
				removeFilterCameras.Add(filterCamera.Key);
			}
		}
		foreach (Camera removeFilterCamera in removeFilterCameras)
		{
			DestroyFilterCamera(filterCameras[removeFilterCamera]);
			filterCameras.Remove(removeFilterCamera);
		}
		int num = 0;
		int num2 = 0;
		Camera worldCompanion = null;
		foreach (KeyValuePair<Camera, bool> filterAnchor in filterAnchors)
		{
			if (!filterCameras.TryGetValue(filterAnchor.Key, out var value3))
			{
				value3 = CreateFilterCamera(filterAnchor.Key, filterAnchor.Value);
				filterCameras[filterAnchor.Key] = value3;
			}
			RenderTexture targetTexture2 = filterAnchor.Key.targetTexture;
			if (value3.targetTexture != targetTexture2)
			{
				value3.targetTexture = targetTexture2;
			}
			float num3 = NextCameraDepth(filterAnchor.Key.depth);
			if (value3.depth != num3)
			{
				value3.depth = num3;
			}
			value3.allowMSAA = targetTexture2 != null && targetTexture2.antiAliasing > 1;
			if (!value3.enabled)
			{
				value3.enabled = true;
			}
			if (filterAnchor.Value)
			{
				num++;
				worldCompanion = value3;
			}
			else
			{
				num2++;
			}
		}
		UpdateFilterSnapshot(worldCompanion);
		filterStatus = "Active " + FilterPresetNames[ActiveFilterPreset] + " world=" + num + " previews=" + num2;
	}

	private static void AddGlobalFilterRow(GameOptionWindow.TabUIInfo tab)
	{
		GameOptionBaseComponent[] source = (from r in tab.GameOptionGrid.GetComponentsInChildren<GameOptionBaseComponent>(includeInactive: true)
			where r.name == "SPRK Global Filter"
			select r).ToArray();
		GameOptionButtonComponent row = source.OfType<GameOptionButtonComponent>().FirstOrDefault();
		foreach (GameOptionBaseComponent item in source.Where((GameOptionBaseComponent r) => r != row))
		{
			item.gameObject.SetActive(value: false);
			item.transform.SetParent(null, worldPositionStays: false);
			UnityEngine.Object.Destroy(item.gameObject);
		}
		if (row == null)
		{
			row = UIHelper.AddGridItem(tab.GameOptionGrid, GameOptionButtonComponent.PrefabAuto);
		}
		row.name = "SPRK Global Filter";
		row.enabled = false;
		if (row.ParentRoot != null)
		{
			if (row.ParentRoot.Root != null)
			{
				row.ParentRoot.Root.SetActive(value: true);
			}
			if (row.ParentRoot.Label_OptionName != null)
			{
				row.ParentRoot.Label_OptionName.text = "Global Filter";
			}
		}
		if (row.ChildRoot != null && row.ChildRoot.Root != null && row.ChildRoot.Root != ((row.ParentRoot == null) ? null : row.ParentRoot.Root))
		{
			row.ChildRoot.Root.SetActive(value: false);
		}
		RefreshGlobalFilterRow(row);
		if (row.Button != null)
		{
			row.Button.onClick.Clear();
			GameOptionButtonComponent target = row;
			EventDelegate.Add(row.Button.onClick, delegate
			{
				SprkGlobalFilterPanel.Open(target.GetComponentInParent<GameOptionWindow>(includeInactive: true), target);
			});
		}
		ConfigureGraphicsHelp(row);
	}

	public static void RefreshGlobalFilterRow(GameOptionButtonComponent row)
	{
		if (row != null && row.Label_Button != null)
		{
			row.Label_Button.text = FilterPresetNames[FilterPreset];
		}
	}

	private void UpdateFilterSnapshot(Camera worldCompanion)
	{
		if (!FilterPanelOpen || worldCompanion == null)
		{
			ReleaseFilterSnapshotCommand();
			return;
		}
		int num = Mathf.Max(64, Screen.width / 4);
		int num2 = Mathf.Max(36, Screen.height / 4);
		if (filterSnapshot == null || filterSnapshot.width != num || filterSnapshot.height != num2)
		{
			ReleaseFilterSnapshotCommand();
			if (filterSnapshot != null)
			{
				filterSnapshot.Release();
				UnityEngine.Object.Destroy(filterSnapshot);
			}
			filterSnapshot = new RenderTexture(num, num2, 0, RenderTextureFormat.ARGB32)
			{
				name = "SPRK Global Filter Snapshot",
				filterMode = FilterMode.Bilinear
			};
			filterSnapshot.Create();
		}
		if (snapshotOwner != worldCompanion)
		{
			ReleaseFilterSnapshotCommand();
			snapshotCommand = new CommandBuffer
			{
				name = "SPRK Global Filter snapshot"
			};
			snapshotCommand.Blit(BuiltinRenderTextureType.CameraTarget, filterSnapshot);
			worldCompanion.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, snapshotCommand);
			snapshotOwner = worldCompanion;
		}
		snapshotFrame = Time.frameCount;
	}

	private void ReleaseFilterSnapshotCommand()
	{
		if (snapshotCommand != null)
		{
			if (snapshotOwner != null)
			{
				snapshotOwner.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, snapshotCommand);
			}
			snapshotCommand.Release();
		}
		snapshotCommand = null;
		snapshotOwner = null;
	}

	private void ReleaseFilterThumbnails()
	{
		ReleaseFilterSnapshotCommand();
		if (filterSnapshot != null)
		{
			filterSnapshot.Release();
			UnityEngine.Object.Destroy(filterSnapshot);
			filterSnapshot = null;
		}
		snapshotFrame = -100;
	}

	private Texture FallbackSnapshot()
	{
		if (fallbackSnapshot != null)
		{
			return fallbackSnapshot;
		}
		fallbackSnapshot = new Texture2D(256, 144, TextureFormat.RGBA32, mipChain: false)
		{
			wrapMode = TextureWrapMode.Clamp,
			hideFlags = HideFlags.DontSave
		};
		for (int i = 0; i < 144; i++)
		{
			for (int j = 0; j < 256; j++)
			{
				float num = (float)j / 255f;
				float num2 = (float)i / 143f;
				Color color = Color.Lerp(new Color(0.95f, 0.78f, 0.55f), new Color(0.32f, 0.58f, 0.86f), Mathf.SmoothStep(0.35f, 1f, num2));
				float num3 = 0.46f + 0.06f * Mathf.Sin(num * 9f + 1.3f) + 0.03f * Mathf.Sin(num * 23f);
				float num4 = 0.3f + 0.07f * Mathf.Sin(num * 5f + 0.4f) + 0.025f * Mathf.Sin(num * 17f + 2f);
				Color a = color;
				a = Color.Lerp(a, new Color(0.42f, 0.5f, 0.62f), Mathf.Clamp01((num3 - num2) * 144f * 0.5f));
				a = Color.Lerp(a, Color.Lerp(new Color(0.18f, 0.3f, 0.17f), new Color(0.4f, 0.56f, 0.3f), num2 / 0.35f), Mathf.Clamp01((num4 - num2) * 144f * 0.5f));
				fallbackSnapshot.SetPixel(j, i, a);
			}
		}
		fallbackSnapshot.Apply();
		return fallbackSnapshot;
	}

	public static void RenderFilterThumbnail(RenderTexture target, int preset, FilterValues values)
	{
		if (instance != null)
		{
			instance.RenderThumbnail(target, preset, values);
		}
	}

	private void RenderThumbnail(RenderTexture target, int preset, FilterValues values)
	{
		if (!(target == null) && FindResources() && EnsureFilterLayers())
		{
			if (thumbFilter == null)
			{
				thumbFilter = CreateFilterStack("SPRK Global Filter (Thumbnail)", thumbFilterLayer, world: true);
			}
			if (thumbCamera == null)
			{
				GameObject gameObject = new GameObject("SPRK Global Filter Thumbnail Camera");
				gameObject.transform.SetParent(base.transform, worldPositionStays: false);
				gameObject.SetActive(value: false);
				thumbCamera = gameObject.AddComponent<Camera>();
				thumbCamera.enabled = false;
				thumbCamera.clearFlags = CameraClearFlags.Nothing;
				thumbCamera.cullingMask = 0;
				thumbCamera.orthographic = true;
				thumbCamera.nearClipPlane = 0.01f;
				thumbCamera.farClipPlane = 1f;
				thumbCamera.allowHDR = false;
				thumbCamera.allowMSAA = false;
				thumbCamera.useOcclusionCulling = false;
				PostProcessLayer postProcessLayer = gameObject.AddComponent<PostProcessLayer>();
				postProcessLayer.Init(resources);
				postProcessLayer.volumeTrigger = gameObject.transform;
				postProcessLayer.volumeLayer = ThumbFilterMask;
				postProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
				postProcessLayer.fog.enabled = false;
				postProcessLayer.stopNaNPropagation = false;
				thumbCamera.AddCommandBuffer(CameraEvent.AfterImageEffects, OpaqueAlphaCommand());
				filterCameraSet.Add(thumbCamera);
				gameObject.SetActive(value: true);
			}
			Graphics.Blit(HasFilterSnapshot ? filterSnapshot : FallbackSnapshot(), target);
			ApplyFilterValues(thumbFilter, preset, values);
			thumbCamera.targetTexture = target;
			thumbCamera.Render();
			thumbCamera.targetTexture = null;
		}
	}

	private static Material ProbeSolid(Color color)
	{
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false);
		texture2D.SetPixel(0, 0, color);
		texture2D.Apply();
		return new Material(Shader.Find("Unlit/Texture"))
		{
			mainTexture = texture2D
		};
	}

	private static Texture2D ReadTarget(RenderTexture target)
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture temporary = RenderTexture.GetTemporary(target.width, target.height, 0, RenderTextureFormat.ARGB32);
		Graphics.Blit(target, temporary);
		RenderTexture.active = temporary;
		Texture2D texture2D = new Texture2D(target.width, target.height, TextureFormat.RGBA32, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, target.width, target.height), 0, 0);
		texture2D.Apply();
		RenderTexture.active = active;
		RenderTexture.ReleaseTemporary(temporary);
		return texture2D;
	}

	private static bool Gray(Color32 c, int tolerance)
	{
		if (Mathf.Abs(c.r - c.g) <= tolerance)
		{
			return Mathf.Abs(c.g - c.b) <= tolerance;
		}
		return false;
	}

	private static bool Near(Color32 a, Color32 b, int tolerance)
	{
		if (Mathf.Abs(a.r - b.r) <= tolerance && Mathf.Abs(a.g - b.g) <= tolerance)
		{
			return Mathf.Abs(a.b - b.b) <= tolerance;
		}
		return false;
	}

	private static string Describe(Color32 c)
	{
		return "(" + c.r + "," + c.g + "," + c.b + "," + c.a + ")";
	}

	private IEnumerator GlobalFilterProbe(List<string> report, string output)
	{
		Camera[] allCameras = Camera.allCameras;
		for (int i = 0; i < allCameras.Length; i++)
		{
			allCameras[i].enabled = false;
		}
		ApplyPreset(0);
		SprkTestPrefs.SetInt("Sprk.Graphics.RenderScale", 100);
		SprkTestPrefs.SetInt("Sprk.Graphics.Scale", 100);
		SprkTestPrefs.SetInt("Sprk.Graphics.PreviewScale", 100);
		RestoreDefaultFilters();
		EndFilterPreview();
		bool flag = FilterPreset == 0 && !GlobalFilterActive && SavedFilter(1).SameAs(FilterPresetDefaults(1));
		FilterValues filterValues = SavedFilter(1).With(5, -7).With(0, 3);
		SaveFilter(1, filterValues);
		FilterValues filterValues2 = SavedFilter(3).With(11, 4);
		SaveFilter(3, filterValues2);
		SprkTestPrefs.ClearGraphicsCache();
		flag &= SavedFilter(1).SameAs(filterValues) && SavedFilter(3).SameAs(filterValues2) && SavedFilter(2).SameAs(FilterPresetDefaults(2));
		ResetFilter(1);
		flag &= SavedFilter(1).SameAs(FilterPresetDefaults(1)) && SavedFilter(3).SameAs(filterValues2);
		SprkTestPrefs.SetString("Sprk.Graphics.Filter.v2.Soft", "1,2,garbage");
		flag &= SavedFilter(2).SameAs(FilterPresetDefaults(2));
		SprkTestPrefs.SetString("Sprk.Graphics.Filter.v2.Soft", "9,9,0,0,0,0,0,0,0,0,0,0,500");
		flag &= SavedFilter(2).X == 5 && SavedFilter(2).Y == 5 && SavedFilter(2).Intensity == 100;
		FilterPreset = 5;
		flag &= FilterPreset == 5 && ActiveFilterPreset == 5 && GlobalFilterActive;
		PreviewFilter(2, FilterPresetDefaults(2).With(12, 0));
		flag &= ActiveFilterPreset == 2 && !GlobalFilterActive;
		PreviewFilter(2, FilterPresetDefaults(2).With(12, 0).With(5, 10));
		flag &= GlobalFilterActive;
		EndFilterPreview();
		flag &= ActiveFilterPreset == 5;
		RestoreDefaultFilters();
		flag &= FilterPreset == 0 && SavedFilter(3).SameAs(FilterPresetDefaults(3));
		SaveFilter(0, FilterPresetDefaults(0).With(5, 30).With(12, 10));
		SprkTestPrefs.ClearGraphicsCache();
		flag &= SavedFilter(0).Contrast == 30 && SavedFilter(0).Intensity == 100 && FilterPreset == 0 && GlobalFilterActive;
		ResetFilter(0);
		flag &= SavedFilter(0).SameAs(FilterPresetDefaults(0)) && !GlobalFilterActive;
		SaveFilter(0, FilterPresetDefaults(0).With(0, 2));
		RestoreDefaultFilters();
		report.Add("globalFilterPersistence=" + (flag & (SavedFilter(0).Neutral && !GlobalFilterActive)));
		bool flag2 = Enumerable.Range(0, FilterPresetNames.Length).All((int num6) => FilterPresetDefaults(num6).Neutral && FilterPresetDefaults(num6).Intensity == ((num6 == 0) ? 100 : 50) && SavedFilter(num6).SameAs(FilterPresetDefaults(num6)));
		flag2 &= !FilterHasEffect(0, FilterPresetDefaults(0));
		for (int num = 1; num < FilterPresetNames.Length; num++)
		{
			float[] half = EffectiveFilter(num, FilterPresetDefaults(num));
			float[] full = EffectiveFilter(num, FilterPresetDefaults(num).With(12, 100));
			flag2 &= FilterHasEffect(num, FilterPresetDefaults(num)) && !FilterHasEffect(num, FilterPresetDefaults(num).With(12, 0));
			flag2 &= Enumerable.Range(0, half.Length).All((int f) => Mathf.Abs(full[f]) >= Mathf.Abs(half[f]));
		}
		SprkTestPrefs.SetString("Sprk.Graphics.Filter.Vivid", "0,0,0,10,0,30,40,-15,-20,0,0,0,60");
		SprkTestPrefs.ClearGraphicsCache();
		flag2 &= SavedFilter(1).SameAs(FilterPresetDefaults(1));
		RestoreDefaultFilters();
		report.Add("globalFilterPresetLooks=" + (flag2 & !SprkTestPrefs.HasKey("Sprk.Graphics.Filter.Vivid")));
		GameObject host = new GameObject("GlobalFilterProbe");
		Camera camera = new GameObject("BottomUI Camera").AddComponent<Camera>();
		camera.transform.SetParent(host.transform, worldPositionStays: false);
		camera.orthographic = true;
		camera.depth = -5f;
		camera.cullingMask = 1 << LayerMask.NameToLayer("BottomUI");
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1f);
		camera.gameObject.AddComponent<UICamera>();
		int num2 = LayerMask.NameToLayer("Bottom3D");
		Camera camera2 = new GameObject("Bottom3D Camera").AddComponent<Camera>();
		camera2.transform.SetParent(host.transform, worldPositionStays: false);
		camera2.orthographic = true;
		camera2.orthographicSize = 2f;
		camera2.depth = -4f;
		camera2.transform.position = new Vector3(0f, 0f, -4f);
		camera2.clearFlags = CameraClearFlags.Depth;
		camera2.cullingMask = 1 << num2;
		camera2.gameObject.AddComponent<UICamera>();
		GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
		obj.transform.SetParent(host.transform, worldPositionStays: false);
		obj.layer = num2;
		obj.GetComponent<Renderer>().material = ProbeSolid(new Color(0.85f, 0.2f, 0.15f, 1f));
		int num3 = LayerMask.NameToLayer("UI");
		if (num3 < 0)
		{
			num3 = 5;
		}
		Camera ui = new GameObject("GlobalFilterProbe UI").AddComponent<Camera>();
		ui.transform.SetParent(host.transform, worldPositionStays: false);
		ui.orthographic = true;
		ui.orthographicSize = 1f;
		ui.depth = -2f;
		ui.cullingMask = 1 << num3;
		ui.clearFlags = CameraClearFlags.Depth;
		ui.transform.position = new Vector3(100f, 0f, -4f);
		ui.gameObject.AddComponent<UICamera>();
		GameObject obj2 = GameObject.CreatePrimitive(PrimitiveType.Quad);
		obj2.transform.SetParent(host.transform, worldPositionStays: false);
		obj2.layer = num3;
		obj2.transform.position = new Vector3(98.9f, 0f, 0f);
		obj2.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
		obj2.GetComponent<Renderer>().material = ProbeSolid(new Color(0.1f, 0.8f, 0.2f, 1f));
		RenderTexture screenTarget = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
		screenTarget.Create();
		CommandBuffer capture = new CommandBuffer
		{
			name = "SPRK filter probe capture"
		};
		capture.Blit(BuiltinRenderTextureType.CameraTarget, screenTarget);
		ui.AddCommandBuffer(CameraEvent.AfterEverything, capture);
		PostProcessLayer nativeLayer = camera.gameObject.AddComponent<PostProcessLayer>();
		nativeLayer.Init(resources);
		nativeLayer.volumeLayer = -1;
		nativeLayer.enabled = false;
		int num4 = 30;
		Camera previewCamera = new GameObject("HeroPreview Camera").AddComponent<Camera>();
		previewCamera.transform.SetParent(host.transform, worldPositionStays: false);
		previewCamera.transform.position = new Vector3(0f, 50f, -4f);
		previewCamera.orthographic = true;
		previewCamera.orthographicSize = 2f;
		previewCamera.depth = -10f;
		previewCamera.cullingMask = 1 << num4;
		previewCamera.clearFlags = CameraClearFlags.Color;
		previewCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
		RenderTexture previewTarget = CreateTarget(160, 120, 24);
		previewTarget.Create();
		previewCamera.targetTexture = previewTarget;
		GameObject obj3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
		obj3.transform.SetParent(host.transform, worldPositionStays: false);
		obj3.layer = num4;
		obj3.transform.position = new Vector3(0f, 50f, 0f);
		obj3.GetComponent<Renderer>().material = ProbeSolid(new Color(0.85f, 0.2f, 0.15f, 1f));
		Color32 worldDefault = default(Color32);
		Color32 uiDefault = default(Color32);
		Color32 previewDefault = default(Color32);
		bool worldOk = true;
		bool uiOk = true;
		bool previewOk = true;
		bool defaultCost = true;
		bool isolation = true;
		var array = new[]
		{
			new
			{
				preset = 0,
				aa = 0,
				name = "default"
			},
			new
			{
				preset = 5,
				aa = 0,
				name = "noir"
			},
			new
			{
				preset = 1,
				aa = 0,
				name = "vivid"
			},
			new
			{
				preset = 3,
				aa = 0,
				name = "film"
			},
			new
			{
				preset = 5,
				aa = 1,
				name = "noir-fxaa"
			},
			new
			{
				preset = 5,
				aa = 3,
				name = "noir-msaa"
			},
			new
			{
				preset = 5,
				aa = 4,
				name = "noir-taa"
			},
			new
			{
				preset = 0,
				aa = 0,
				name = "default-again"
			}
		};
		var array2 = array;
		foreach (var c in array2)
		{
			ApplyPreset(c.aa);
			FilterPreset = c.preset;
			for (int frame = 0; frame < 8; frame++)
			{
				yield return new WaitForEndOfFrame();
			}
			Texture2D texture2D = ReadTarget(screenTarget);
			Texture2D texture2D2 = ReadTarget(previewTarget);
			File.WriteAllBytes(Path.Combine(output, "filter-" + c.name + ".png"), texture2D.EncodeToPNG());
			File.WriteAllBytes(Path.Combine(output, "filter-preview-" + c.name + ".png"), texture2D2.EncodeToPNG());
			int x = Mathf.RoundToInt((1.1f / (2f * (float)Screen.width / (float)Screen.height) * -1f + 0.5f) * (float)Screen.width);
			Color32 color = texture2D.GetPixel(Screen.width / 2, Screen.height / 2);
			Color32 color2 = texture2D.GetPixel(x, Screen.height / 2);
			Color32 c2 = texture2D.GetPixel(Screen.width - 8, 8);
			Color32 color3 = texture2D2.GetPixel(texture2D2.width / 2, texture2D2.height / 2);
			Color32 c3 = texture2D2.GetPixel(2, 2);
			report.Add("filterCase=" + c.name + " cube=" + Describe(color) + " ui=" + Describe(color2) + " back=" + Describe(c2) + " preview=" + Describe(color3) + " previewCorner=" + Describe(c3) + " cameras=" + FilterCameraCount + " status=" + FilterStatus + " world=" + ((world == null) ? "none" : (world.width + "x" + world.height + "x" + world.antiAliasing)));
			if (c.name == "default")
			{
				worldDefault = color;
				uiDefault = color2;
				previewDefault = color3;
			}
			if (c.preset == 0)
			{
				defaultCost &= FilterCameraCount == 0 && (c.aa != 0 || world == null);
				worldOk &= Near(color, worldDefault, 3) && color.r > 150 && color.g < 100;
				previewOk &= Near(color3, previewDefault, 3) && color3.r > 150;
			}
			else
			{
				worldOk &= FilterCameraCount == 2;
				if (c.preset == 5)
				{
					worldOk &= Gray(color, 6);
					previewOk &= Gray(color3, 6);
				}
				else
				{
					worldOk &= !Near(color, worldDefault, 4);
					previewOk &= !Near(color3, previewDefault, 4);
				}
			}
			worldOk &= color.a == byte.MaxValue && c2.a == byte.MaxValue;
			uiOk &= Near(color2, uiDefault, 2) && color2.g > 150;
			previewOk &= c3.a == 0;
			isolation &= (nativeLayer.volumeLayer.value & (WorldFilterMask | PreviewFilterMask)) == 0 || (c.preset == 0 && FilterCameraCount == 0);
			UnityEngine.Object.Destroy(texture2D);
			UnityEngine.Object.Destroy(texture2D2);
		}
		report.Add("globalFilterWorld=" + worldOk);
		report.Add("globalFilterUiUntouched=" + uiOk);
		report.Add("globalFilterPreview=" + previewOk);
		report.Add("globalFilterDefaultCost=" + defaultCost);
		report.Add("globalFilterNativeIsolation=" + isolation);
		FilterPreset = 5;
		FilterPanelOpen = true;
		for (int num5 = 0; num5 < 6; num5++)
		{
			yield return new WaitForEndOfFrame();
		}
		bool snapshot = HasFilterSnapshot;
		if (snapshot)
		{
			Texture2D texture2D3 = ReadTarget(filterSnapshot);
			Color32 color4 = texture2D3.GetPixel(texture2D3.width / 2, texture2D3.height / 2);
			UnityEngine.Object.Destroy(texture2D3);
			RenderTexture renderTexture = new RenderTexture(160, 120, 0, RenderTextureFormat.ARGB32);
			renderTexture.Create();
			RenderFilterThumbnail(renderTexture, 5, SavedFilter(5));
			Texture2D texture2D4 = ReadTarget(renderTexture);
			Color32 c4 = texture2D4.GetPixel(texture2D4.width / 2, texture2D4.height / 2);
			RenderFilterThumbnail(renderTexture, 0, SavedFilter(0));
			Texture2D texture2D5 = ReadTarget(renderTexture);
			Color32 color5 = texture2D5.GetPixel(texture2D5.width / 2, texture2D5.height / 2);
			snapshot &= color4.r > 150 && color4.g < 100 && Gray(c4, 6) && c4.a == byte.MaxValue && Near(color5, color4, 4);
			report.Add("globalFilterSnapshotPixels raw=" + Describe(color4) + " noir=" + Describe(c4) + " default=" + Describe(color5));
			UnityEngine.Object.Destroy(texture2D4);
			UnityEngine.Object.Destroy(texture2D5);
			renderTexture.Release();
			UnityEngine.Object.Destroy(renderTexture);
		}
		FilterPanelOpen = false;
		FilterPreset = 0;
		yield return new WaitForEndOfFrame();
		report.Add("globalFilterSnapshot=" + snapshot);
		FilterPreset = 5;
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		previewCamera.enabled = false;
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		bool lifecycle = FilterCameraCount == 1;
		FilterPreset = 0;
		yield return new WaitForEndOfFrame();
		report.Add("globalFilterLifecycle=" + (lifecycle & (FilterCameraCount == 0)));
		ui.RemoveCommandBuffer(CameraEvent.AfterEverything, capture);
		capture.Release();
		previewCamera.targetTexture = null;
		ownedTargets.Remove(previewTarget);
		previewTarget.Release();
		UnityEngine.Object.Destroy(previewTarget);
		screenTarget.Release();
		UnityEngine.Object.Destroy(screenTarget);
		UnityEngine.Object.Destroy(host);
		ApplyPreset(0);
		RestoreDefaultFilters();
		yield return null;
	}

	private static float Brightness(Texture texture)
	{
		RenderTexture renderTexture = texture as RenderTexture;
		if (renderTexture == null)
		{
			return 0f;
		}
		Texture2D texture2D = ReadTarget(renderTexture);
		float num = 0f;
		Color32[] pixels = texture2D.GetPixels32();
		Color32[] array = pixels;
		for (int i = 0; i < array.Length; i++)
		{
			Color32 color = array[i];
			num += (float)(color.r + color.g + color.b);
		}
		UnityEngine.Object.Destroy(texture2D);
		return num / ((float)pixels.Length * 3f);
	}

	private static void Screenshot(string output, string name)
	{
		Texture2D texture2D = ScreenCapture.CaptureScreenshotAsTexture();
		File.WriteAllBytes(Path.Combine(output, name), texture2D.EncodeToPNG());
		UnityEngine.Object.Destroy(texture2D);
	}

	private IEnumerator GlobalFilterPanelProbe(List<string> report, string output)
	{
		RestoreDefaultFilters();
		EndFilterPreview();
		LoginManagement manager = SceneAutoBehaviour<LoginManagement>.instance;
		LoginAccountInfo loginAccountInfo = manager.GetInstance<LoginAccountInfo>();
		SprkLoginSettings.Configure(loginAccountInfo);
		loginAccountInfo.GetComponent<SprkLoginSettings>().OpenSettings();
		yield return new WaitForSecondsRealtime(0.5f);
		GameOptionWindow settings = manager.GetInstance<GameOptionWindow>();
		Camera[] allCameras = Camera.allCameras;
		foreach (Camera camera in allCameras)
		{
			if (camera.name == "BI UI Camera")
			{
				camera.gameObject.SetActive(value: false);
			}
		}
		GameOptionButtonComponent[] array = (from r in settings.GetComponentsInChildren<GameOptionButtonComponent>(includeInactive: true)
			where r.name == "SPRK Global Filter"
			select r).ToArray();
		GameOptionButtonComponent row = array.FirstOrDefault();
		bool rowOk = array.Length == 1 && row.Label_Button != null && row.Label_Button.text == "Default" && row.Button != null;
		rowOk &= row.ParentRoot != null && row.ParentRoot.Label_OptionName != null && row.ParentRoot.Label_OptionName.text == "Global Filter";
		rowOk &= HelpFor(row) != null && row.ParentRoot.ToolTipImage != null && row.ParentRoot.ToolTipImage.activeSelf;
		UIScrollView componentInParent = row.GetComponentInParent<UIScrollView>();
		if (componentInParent != null)
		{
			UIPanel component = componentInParent.GetComponent<UIPanel>();
			Vector3 vector = component.transform.InverseTransformPoint(row.transform.position);
			componentInParent.MoveRelative(new Vector3(0f, component.finalClipRegion.y - vector.y, 0f));
			componentInParent.RestrictWithinBounds(instant: true);
		}
		yield return new WaitForEndOfFrame();
		Screenshot(output, "filter-settings-row.png");
		report.Add("globalFilterRow=" + rowOk);
		EventDelegate.Execute(row.Button.onClick);
		for (int i2 = 0; i2 < 20; i2++)
		{
			yield return new WaitForEndOfFrame();
		}
		SprkGlobalFilterPanel panel = SprkGlobalFilterPanel.Current;
		bool panelOk = panel != null && FilterPanelOpen && panel.Selected == 0 && !panel.Dirty;
		panelOk &= settings.GetComponentsInChildren<UIPanel>(includeInactive: true).All((UIPanel p) => p.alpha == 0f);
		yield return new WaitForEndOfFrame();
		Screenshot(output, "filter-panel-default.png");
		report.Add("globalFilterNativeSkin=" + (panel.MissingSprites == "") + " missing=" + panel.MissingSprites);
		Transform transform = panel.GetComponentsInChildren<Transform>(includeInactive: true).FirstOrDefault((Transform t) => t.name == "Intensity");
		bool flag = transform != null && !transform.gameObject.activeInHierarchy;
		panel.SetPalette(2, -1);
		panel.SetField(5, 20);
		panel.SetField(12, 0);
		flag &= panel.Dirty && ActiveFilterPreset == 0 && ActiveFilterValues.X == 2 && ActiveFilterValues.Contrast == 20 && ActiveFilterValues.Intensity == 100;
		panel.ResetSelected();
		flag &= !panel.Dirty && ActiveFilterValues.Neutral;
		panel.Select(1);
		flag &= transform != null && transform.gameObject.activeInHierarchy;
		panel.Select(0);
		report.Add("globalFilterDefaultEditable=" + flag);
		panel.Select(1);
		for (int i2 = 0; i2 < 20; i2++)
		{
			yield return new WaitForEndOfFrame();
		}
		panelOk &= ActiveFilterPreset == 1 && FilterPreset == 0 && panel.Dirty;
		panel.SetPalette(-5, 5);
		panel.SetField(5, 33);
		panelOk &= ActiveFilterValues.X == -5 && ActiveFilterValues.Y == 5 && ActiveFilterValues.Contrast == 33;
		yield return new WaitForEndOfFrame();
		Screenshot(output, "filter-panel-vivid-palette.png");
		for (int i2 = 0; i2 < 3; i2++)
		{
			yield return new WaitForEndOfFrame();
		}
		Transform transform2 = panel.GetComponentsInChildren<Transform>(includeInactive: true).FirstOrDefault((Transform t) => t.name == "Advanced Tab Input");
		bool flag2 = false;
		if (transform2 != null)
		{
			Camera camera2 = NGUITools.FindCameraForLayer(transform2.gameObject.layer);
			if (camera2 != null && UICamera.Raycast(camera2.WorldToScreenPoint(transform2.position)) && UICamera.hoveredObject == transform2.gameObject)
			{
				UIEventListener component2 = transform2.GetComponent<UIEventListener>();
				if (component2 != null && component2.onClick != null)
				{
					component2.onClick(transform2.gameObject);
				}
				flag2 = panel.AdvancedTab;
			}
		}
		report.Add("globalFilterTabClick=" + flag2);
		if (!panel.AdvancedTab)
		{
			panel.ShowAdvanced(value: true);
		}
		for (int i2 = 0; i2 < 5; i2++)
		{
			yield return new WaitForEndOfFrame();
		}
		Screenshot(output, "filter-panel-vivid-advanced.png");
		bool flag3 = Enumerable.Range(0, 3).All((int index) => Brightness(panel.Thumbnail(index)) > 4f);
		report.Add("globalFilterThumbnails=" + flag3 + " snapshot=" + HasFilterSnapshot + " brightness=" + string.Join(",", (from index in Enumerable.Range(0, 3)
			select Brightness(panel.Thumbnail(index)).ToString("0")).ToArray()));
		panel.Step(1);
		panelOk &= panel.Selected == 2 && ActiveFilterPreset == 2;
		panel.Step(-1);
		panelOk &= panel.Selected == 1 && panel.Working.X == -5 && panel.Working.Contrast == 33;
		panel.Step(-1);
		panel.Step(-1);
		panelOk &= panel.Selected == FilterPresetNames.Length - 1;
		panel.Select(1);
		panel.SetField(0, 0);
		panel.SetField(1, 0);
		panel.SetPalette(-5, 5);
		panel.Apply();
		panelOk &= FilterPreset == 1 && SavedFilter(1).X == -5 && SavedFilter(1).Contrast == 33 && !panel.Dirty && row.Label_Button.text == "Vivid";
		panel.ResetSelected();
		panelOk &= panel.Working.SameAs(FilterPresetDefaults(1)) && panel.Dirty;
		panel.ToggleUi();
		panelOk &= panel.UiHidden;
		yield return new WaitForEndOfFrame();
		Screenshot(output, "filter-panel-hidden-ui.png");
		panel.ToggleUi();
		panel.Close();
		yield return null;
		panelOk &= SprkGlobalFilterPanel.Current == null && !FilterPanelOpen && ActiveFilterPreset == 1 && SavedFilter(1).X == -5;
		report.Add("globalFilterPanelState=" + (panelOk & settings.GetComponentsInChildren<UIPanel>(includeInactive: true).Any((UIPanel p) => p.alpha > 0f)));
		EventDelegate.Execute(row.Button.onClick);
		yield return null;
		panel = SprkGlobalFilterPanel.Current;
		bool restore = panel != null && panel.Selected == 1 && panel.Working.X == -5;
		panel.Select(5);
		panel.Close();
		yield return null;
		restore &= FilterPreset == 1 && ActiveFilterPreset == 1;
		EventDelegate.Execute(row.Button.onClick);
		yield return null;
		panel = SprkGlobalFilterPanel.Current;
		panel.RestoreDefaults();
		restore &= panel.Selected == 0 && panel.Dirty;
		panel.Apply();
		restore &= FilterPreset == 0 && SavedFilter(1).SameAs(FilterPresetDefaults(1)) && row.Label_Button.text == "Default";
		report.Add("globalFilterRestoreSteps=" + restore + " preset=" + FilterPreset + " label=" + row.Label_Button.text);
		EventDelegate.Execute(row.Button.onClick);
		yield return null;
		restore &= SprkGlobalFilterPanel.Current != null;
		settings.gameObject.SetActive(value: false);
		for (int i2 = 0; i2 < 3; i2++)
		{
			yield return null;
		}
		restore &= SprkGlobalFilterPanel.Current == null && !FilterPanelOpen && ActiveFilterPreset == 0;
		restore &= settings.GetComponentsInChildren<UIPanel>(includeInactive: true).All((UIPanel p) => p.alpha > 0f);
		settings.gameObject.SetActive(value: true);
		settings.Close();
		report.Add("globalFilterRestoreAndDiscard=" + restore);
		RestoreDefaultFilters();
	}
}
