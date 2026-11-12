using System;
using System.Collections.Generic;
using System.Linq;
using NGame2;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using UnityEngine;

public sealed class SprkGlobalFilterPanel : MonoBehaviour
{
	private sealed class Slider
	{
		public int field;

		public float width;

		public UIWidget track;

		public UIWidget fill;

		public UIWidget knob;

		public UILabel name;

		public UILabel value;

		public UIWidget hit;

		public bool bipolar;
	}

	public static SprkGlobalFilterPanel Current;

	private const float DesignHeight = 1080f;

	private const float PadY = -118f;

	private const float DotSpacing = 30f;

	private const float ListY = -118f;

	private const float ListHeight = 350f;

	private const int PadSize = 340;

	private const string FrameFill = "Common_Box_Text_2";

	private const string TitleSprite = "PayShop_Title_Name";

	private const string CloseSprite = "Btn_PayShop_Close";

	private const string BoxSprite = "Common_Box_3";

	private const string TabOn = "Common_Button_Tab_Enable";

	private const string TabOff = "Common_Button_Tab_Disable";

	private const string SelectSprite = "Common_Select_Frame";

	private const string CheckSprite = "Common_Check";

	private const string TrackSprite = "soundbar_bg";

	private const string FillSprite = "soundbar_on";

	private const string KnobSprite = "soundbar_bt";

	private const string ButtonSprite = "Common_Button6_Normal";

	private const string EmphasisSprite = "Common_Button6_Emphasis";

	private const string DisabledSprite = "Common_Button6_Disable";

	private const string CancelSprite = "Common_Button7_Cancel";

	private const string EyeBoxSprite = "Common_Box_3";

	private static Texture2D white;

	private static Texture2D circle;

	private static Texture2D ring;

	private static Texture2D padGradient;

	private static Texture2D temperatureTrack;

	private static Texture2D eyeIcon;

	private GameOptionWindow window;

	private GameOptionButtonComponent row;

	private readonly List<KeyValuePair<UIPanel, float>> hiddenPanels = new List<KeyValuePair<UIPanel, float>>();

	private readonly List<BaseWindow> worldMapHiders = new List<BaseWindow>();

	private UIPanel panel;

	private UIPanel listPanel;

	private UIScrollView list;

	private GameObject content;

	private GameObject paletteRoot;

	private GameObject advancedRoot;

	private GameObject intensityRoot;

	private UILabel title;

	private UILabel presetName;

	private UILabel coordinate;

	private UILabel intensityValue;

	private UILabel applyLabel;

	private UILabel paletteTabLabel;

	private UILabel advancedTabLabel;

	private UITexture centerThumb;

	private UITexture leftThumb;

	private UITexture rightThumb;

	private UISprite checkMark;

	private UISprite applyButton;

	private UISprite paletteTab;

	private UISprite advancedTab;

	private readonly List<UIAtlas> atlases = new List<UIAtlas>();

	private readonly List<string> missingSprites = new List<string>();

	private UILabel titleStyle;

	private UILabel nameStyle;

	private UILabel valueStyle;

	private UILabel tabOnStyle;

	private UILabel tabOffStyle;

	private UILabel buttonStyle;

	private UIWidget blocker;

	private readonly UITexture[,] dots = new UITexture[11, 11];

	private UITexture originRing;

	private readonly List<Slider> sliders = new List<Slider>();

	private Slider intensity;

	private readonly RenderTexture[] thumbs = new RenderTexture[3];

	private UIFont bitmapFont;

	private Font trueTypeFont;

	private float halfWidth;

	private int layer;

	private int windowLayer;

	private Vector2 screenSize;

	private int savedPreset;

	private int selected;

	private SprkGraphicsOptions.FilterValues[] working;

	private SprkGraphicsOptions.FilterValues[] saved;

	private bool advanced;

	private bool hiddenUi;

	private int thumbTimer;

	public static bool IsOpen => Current != null;

	public int Selected => selected;

	public bool Dirty
	{
		get
		{
			if (selected != savedPreset)
			{
				return true;
			}
			for (int i = 0; i < working.Length; i++)
			{
				if (!working[i].SameAs(saved[i]))
				{
					return true;
				}
			}
			return false;
		}
	}

	public SprkGraphicsOptions.FilterValues Working => working[selected];

	public bool AdvancedTab => advanced;

	public bool UiHidden => hiddenUi;

	public string HiddenPanelState => string.Join(",", hiddenPanels.Select((KeyValuePair<UIPanel, float> p) => (!(p.Key == null)) ? p.Value.ToString("0.##") : "null").ToArray());

	public string MissingSprites => string.Join(",", missingSprites.ToArray());

	public static SprkGlobalFilterPanel Open(GameOptionWindow window, GameOptionButtonComponent row)
	{
		if (Current != null)
		{
			return Current;
		}
		if (window == null)
		{
			return null;
		}
		UIRoot uIRoot = NGUITools.FindInParents<UIRoot>(window.gameObject);
		if (uIRoot == null)
		{
			return null;
		}
		int num = RenderedLayer(window);
		int num2 = num;
		Transform parent = uIRoot.transform;
		if (GlobalUIRoot.isValidInstance)
		{
			GlobalUIRoot instance = GlobalUIRoot.instance;
			if (instance.UIRoot != null && instance.ContentsGroup != null && NGUITools.FindCameraForLayer(instance.gameObject.layer) != null)
			{
				uIRoot = instance.UIRoot;
				parent = instance.ContentsGroup.transform;
				num2 = instance.gameObject.layer;
			}
		}
		GameObject obj = new GameObject("SPRK Global Filter Panel");
		obj.layer = num2;
		obj.transform.SetParent(parent, worldPositionStays: false);
		SprkGlobalFilterPanel sprkGlobalFilterPanel = obj.AddComponent<SprkGlobalFilterPanel>();
		sprkGlobalFilterPanel.window = window;
		sprkGlobalFilterPanel.row = row;
		sprkGlobalFilterPanel.layer = num2;
		sprkGlobalFilterPanel.windowLayer = num;
		sprkGlobalFilterPanel.Build(uIRoot);
		Current = sprkGlobalFilterPanel;
		return sprkGlobalFilterPanel;
	}

	private static int RenderedLayer(GameOptionWindow window)
	{
		UIWidget[] componentsInChildren = window.GetComponentsInChildren<UIWidget>(includeInactive: false);
		foreach (UIWidget uIWidget in componentsInChildren)
		{
			if (NGUITools.FindCameraForLayer(uIWidget.gameObject.layer) != null)
			{
				return uIWidget.gameObject.layer;
			}
		}
		componentsInChildren = window.GetComponentsInChildren<UIWidget>(includeInactive: true);
		foreach (UIWidget uIWidget2 in componentsInChildren)
		{
			if (NGUITools.FindCameraForLayer(uIWidget2.gameObject.layer) != null)
			{
				return uIWidget2.gameObject.layer;
			}
		}
		return window.gameObject.layer;
	}

	private void HideSceneUi()
	{
		Transform transform = ((window.transform.parent != null) ? window.transform.parent : window.transform);
		UIPanel[] componentsInChildren = transform.GetComponentsInChildren<UIPanel>(includeInactive: true);
		foreach (UIPanel uIPanel in componentsInChildren)
		{
			if (uIPanel.transform.IsChildOf(window.transform) || (uIPanel.gameObject.layer == windowLayer && !(uIPanel.GetComponentInParent<LoginBackground>() != null)))
			{
				hiddenPanels.Add(new KeyValuePair<UIPanel, float>(uIPanel, uIPanel.alpha));
				uIPanel.alpha = 0f;
			}
		}
		BaseWindow[] componentsInChildren2 = transform.GetComponentsInChildren<BaseWindow>(includeInactive: true);
		foreach (BaseWindow baseWindow in componentsInChildren2)
		{
			if (baseWindow.HideWorldMap)
			{
				baseWindow.HideWorldMap = false;
				worldMapHiders.Add(baseWindow);
			}
		}
	}

	private void RestoreSceneUi()
	{
		foreach (KeyValuePair<UIPanel, float> hiddenPanel in hiddenPanels)
		{
			if (hiddenPanel.Key != null)
			{
				hiddenPanel.Key.alpha = hiddenPanel.Value;
			}
		}
		hiddenPanels.Clear();
		foreach (BaseWindow worldMapHider in worldMapHiders)
		{
			if (worldMapHider != null)
			{
				worldMapHider.HideWorldMap = true;
			}
		}
		worldMapHiders.Clear();
	}

	public void Close()
	{
		Close(applied: false);
	}

	public void Close(bool applied)
	{
		if (Current == this)
		{
			Current = null;
		}
		SprkGraphicsOptions.EndFilterPreview();
		SprkGraphicsOptions.FilterPanelOpen = false;
		RestoreSceneUi();
		RenderTexture[] array = thumbs;
		foreach (RenderTexture renderTexture in array)
		{
			if (renderTexture != null)
			{
				renderTexture.Release();
				UnityEngine.Object.Destroy(renderTexture);
			}
		}
		SprkGraphicsOptions.ReleaseGlobalFilterThumbnails();
		SprkGraphicsOptions.RefreshGlobalFilterRow(row);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		if (Current == this)
		{
			Current = null;
			SprkGraphicsOptions.EndFilterPreview();
			SprkGraphicsOptions.FilterPanelOpen = false;
			RestoreSceneUi();
		}
	}

	public void Select(int preset)
	{
		int num = SprkGraphicsOptions.FilterPresetNames.Length;
		selected = (preset % num + num) % num;
		Sync(thumbnails: true);
	}

	public void Step(int delta)
	{
		Select(selected + delta);
	}

	public void SetField(int field, int value)
	{
		if (selected != 0 || field != 12)
		{
			working[selected] = working[selected].With(field, value);
			Sync(thumbnails: false);
		}
	}

	public void SetPalette(int x, int y)
	{
		working[selected] = working[selected].With(0, x).With(1, y);
		Sync(thumbnails: false);
	}

	public void ShowAdvanced(bool value)
	{
		advanced = value;
		Sync(thumbnails: false);
	}

	public void ResetSelected()
	{
		working[selected] = SprkGraphicsOptions.FilterPresetDefaults(selected);
		Sync(thumbnails: true);
	}

	public void RestoreDefaults()
	{
		for (int i = 0; i < working.Length; i++)
		{
			working[i] = SprkGraphicsOptions.FilterPresetDefaults(i);
		}
		selected = 0;
		Sync(thumbnails: true);
	}

	public void Apply()
	{
		if (Dirty)
		{
			for (int i = 0; i < working.Length; i++)
			{
				SprkGraphicsOptions.SaveFilter(i, working[i]);
			}
			SprkGraphicsOptions.FilterPreset = selected;
			savedPreset = selected;
			for (int j = 0; j < working.Length; j++)
			{
				saved[j] = SprkGraphicsOptions.SavedFilter(j);
			}
			SprkGraphicsOptions.RefreshGlobalFilterRow(row);
			Sync(thumbnails: false);
		}
	}

	public Texture Thumbnail(int index)
	{
		return thumbs[index];
	}

	public void ToggleUi()
	{
		hiddenUi = !hiddenUi;
		content.SetActive(!hiddenUi);
	}

	private void Build(UIRoot root)
	{
		EnsureTextures();
		UILabel uILabel = window.GetComponentsInChildren<UILabel>(includeInactive: true).FirstOrDefault((UILabel l) => l.bitmapFont != null || l.trueTypeFont != null);
		if (uILabel != null)
		{
			bitmapFont = uILabel.bitmapFont;
			trueTypeFont = uILabel.trueTypeFont;
		}
		UISprite[] componentsInChildren = window.GetComponentsInChildren<UISprite>(includeInactive: true);
		foreach (UISprite uISprite in componentsInChildren)
		{
			if (uISprite.atlas != null && !atlases.Contains(uISprite.atlas))
			{
				atlases.Add(uISprite.atlas);
			}
		}
		UILabel[] labels = window.GetComponentsInChildren<UILabel>(includeInactive: true);
		Func<string, UILabel> func = (string part) => labels.FirstOrDefault((UILabel l) => Path(l.transform).Contains(part));
		titleStyle = func("Sprite_Title/");
		nameStyle = labels.FirstOrDefault((UILabel l) => l.name == "OptionName");
		valueStyle = labels.FirstOrDefault((UILabel l) => l.name.StartsWith("Label_Toggle"));
		tabOnStyle = func("Toggle_Game/");
		tabOffStyle = func("Toggle_Battle/");
		buttonStyle = func("Button_Logout/");
		int num2 = 0;
		foreach (UIPanel item in UIPanel.list)
		{
			if (item != null && item.depth > num2)
			{
				num2 = item.depth;
			}
		}
		panel = base.gameObject.AddComponent<UIPanel>();
		panel.depth = num2 + 20;
		HideSceneUi();
		savedPreset = SprkGraphicsOptions.FilterPreset;
		selected = savedPreset;
		int num3 = SprkGraphicsOptions.FilterPresetNames.Length;
		working = new SprkGraphicsOptions.FilterValues[num3];
		saved = new SprkGraphicsOptions.FilterValues[num3];
		for (int num4 = 0; num4 < num3; num4++)
		{
			saved[num4] = SprkGraphicsOptions.SavedFilter(num4);
			working[num4] = saved[num4].Clone();
		}
		SprkGraphicsOptions.FilterPanelOpen = true;
		Layout(root);
		Sync(thumbnails: true);
	}

	private void Layout(UIRoot root)
	{
		for (int num = base.transform.childCount - 1; num >= 0; num--)
		{
			GameObject obj = base.transform.GetChild(num).gameObject;
			obj.SetActive(value: false);
			UnityEngine.Object.Destroy(obj);
		}
		sliders.Clear();
		missingSprites.Clear();
		float num2 = (float)root.activeHeight / 1080f;
		screenSize = new Vector2(Screen.width, Screen.height);
		halfWidth = 540f * (float)Screen.width / Mathf.Max(1f, Screen.height);
		GameObject gameObject = Child(base.transform, "Scaled");
		gameObject.transform.localScale = Vector3.one * num2;
		blocker = Widget<UIWidget>(gameObject.transform, "Blocker", Mathf.CeilToInt(halfWidth * 2f), 1080, Vector2.zero, 0);
		NGUITools.AddWidgetCollider(blocker.gameObject);
		UIEventListener.Get(blocker.gameObject).onClick = delegate
		{
			if (hiddenUi)
			{
				ToggleUi();
			}
		};
		content = Child(gameObject.transform, "Content");
		float num3 = 0f - halfWidth + 40f + 380f;
		Sprite(content.transform, "Frame Fill", "Common_Box_Text_2", 730, 1030, new Vector2(num3, 0f), 1);
		Sprite(content.transform, "Title Bar", "PayShop_Title_Name", 650, 86, new Vector2(num3, 474f), 6);
		title = Label(content.transform, "Global Filter", 40, new Vector2(num3, 474f), 7, UIWidget.Pivot.Center, titleStyle);
		UISprite uISprite = Sprite(content.transform, "Close", "Btn_PayShop_Close", 46, 46, new Vector2(num3 + 380f - 138f, 474f), 8, sliced: false);
		Clickable(uISprite.gameObject, Close, 10);
		Sprite(content.transform, "Previous Frame", "Common_Box_3", 146, 186, new Vector2(num3 - 252f, 300f), 3);
		Sprite(content.transform, "Next Frame", "Common_Box_3", 146, 186, new Vector2(num3 + 252f, 300f), 3);
		leftThumb = Tex(content.transform, "Previous", null, 130, 170, new Vector2(num3 - 252f, 300f), new Color(0.6f, 0.6f, 0.6f, 1f), 4);
		rightThumb = Tex(content.transform, "Next", null, 130, 170, new Vector2(num3 + 252f, 300f), new Color(0.6f, 0.6f, 0.6f, 1f), 4);
		centerThumb = Tex(content.transform, "Current", null, 360, 203, new Vector2(num3, 300f), Color.white, 6);
		Sprite(content.transform, "Current Frame", "Common_Select_Frame", 384, 227, new Vector2(num3, 300f), 4);
		Clickable(leftThumb.gameObject, delegate
		{
			Step(-1);
		});
		Clickable(rightThumb.gameObject, delegate
		{
			Step(1);
		});
		checkMark = Sprite(content.transform, "Applied", "Common_Check", 52, 44, new Vector2(num3 + 158f, 378f), 8, sliced: false);
		presetName = Label(content.transform, "", 36, new Vector2(num3, 168f), 7, UIWidget.Pivot.Center, titleStyle);
		paletteTab = Sprite(content.transform, "Palette Tab", "Common_Button_Tab_Enable", 300, 84, new Vector2(num3 - 152f, 105f), 4);
		advancedTab = Sprite(content.transform, "Advanced Tab", "Common_Button_Tab_Disable", 300, 84, new Vector2(num3 + 152f, 105f), 4);
		paletteTabLabel = Label(content.transform, "Color Palette", 30, new Vector2(num3 - 152f, 105f), 5, UIWidget.Pivot.Center, tabOnStyle);
		advancedTabLabel = Label(content.transform, "Advanced", 30, new Vector2(num3 + 152f, 105f), 5, UIWidget.Pivot.Center, tabOffStyle);
		Clickable(Widget<UIWidget>(content.transform, "Palette Tab Input", 300, 84, new Vector2(num3 - 152f, 105f), 9).gameObject, delegate
		{
			ShowAdvanced(value: false);
		});
		Clickable(Widget<UIWidget>(content.transform, "Advanced Tab Input", 300, 84, new Vector2(num3 + 152f, 105f), 9).gameObject, delegate
		{
			ShowAdvanced(value: true);
		});
		paletteRoot = Child(content.transform, "Palette");
		Sprite(paletteRoot.transform, "Pad Frame", "Common_Box_3", 368, 368, new Vector2(num3, -118f), 3);
		UITexture pad = Tex(paletteRoot.transform, "Pad", padGradient, 340, 340, new Vector2(num3, -118f), Color.white, 4);
		for (int num4 = 0; num4 < 11; num4++)
		{
			for (int num5 = 0; num5 < 11; num5++)
			{
				dots[num4, num5] = Tex(paletteRoot.transform, "Dot " + (num4 - 5) + "," + (num5 - 5), circle, 10, 10, new Vector2(num3 + (float)(num4 - 5) * 30f, -118f + (float)(num5 - 5) * 30f), Color.white, 6);
			}
		}
		originRing = Tex(paletteRoot.transform, "Origin", ring, 16, 16, new Vector2(num3, -118f), Color.white, 7);
		NGUITools.AddWidgetCollider(pad.gameObject);
		UIEventListener uIEventListener = UIEventListener.Get(pad.gameObject);
		uIEventListener.onPress = delegate(GameObject g, bool down)
		{
			if (down)
			{
				PickPalette(pad);
			}
		};
		uIEventListener.onDrag = delegate
		{
			PickPalette(pad);
		};
		coordinate = Label(paletteRoot.transform, "(0,0)", 28, new Vector2(num3 + 170f + 70f, -272f), 6, UIWidget.Pivot.Center, valueStyle);
		advancedRoot = Child(content.transform, "Advanced");
		listPanel = advancedRoot.AddComponent<UIPanel>();
		listPanel.depth = panel.depth + 1;
		listPanel.clipping = UIDrawCall.Clipping.SoftClip;
		listPanel.baseClipRegion = new Vector4(num3, -118f, 640f, 350f);
		listPanel.clipSoftness = new Vector2(0f, 16f);
		list = advancedRoot.AddComponent<UIScrollView>();
		list.movement = UIScrollView.Movement.Vertical;
		list.dragEffect = UIScrollView.DragEffect.MomentumAndSpring;
		list.scrollWheelFactor = 0.5f;
		for (int num6 = 2; num6 < 12; num6++)
		{
			sliders.Add(CreateSlider(advancedRoot.transform, num6, new Vector2(num3, 9f - (float)(num6 - 2) * 96f), 520f, scroll: true));
		}
		intensityRoot = Child(content.transform, "Intensity");
		intensity = CreateSlider(intensityRoot.transform, 12, new Vector2(num3, -362f), 520f, scroll: false);
		UISprite uISprite2 = Sprite(content.transform, "Reset", "Common_Button6_Normal", 300, 80, new Vector2(num3 - 160f, -462f), 4);
		Label(content.transform, "Reset", 32, new Vector2(num3 - 160f, -462f), 5, UIWidget.Pivot.Center, buttonStyle);
		Clickable(uISprite2.gameObject, ResetSelected);
		applyButton = Sprite(content.transform, "Apply", "Common_Button6_Emphasis", 300, 80, new Vector2(num3 + 160f, -462f), 4);
		applyLabel = Label(content.transform, "Apply", 32, new Vector2(num3 + 160f, -462f), 5, UIWidget.Pivot.Center, buttonStyle);
		Clickable(applyButton.gameObject, Apply);
		UISprite uISprite3 = Sprite(content.transform, "Restore Default", "Common_Button7_Cancel", 340, 84, new Vector2(halfWidth - 290f, -480f), 4);
		Label(content.transform, "Restore Default", 30, new Vector2(halfWidth - 290f, -480f), 5, UIWidget.Pivot.Center, buttonStyle);
		Clickable(uISprite3.gameObject, RestoreDefaults);
		UISprite uISprite4 = Sprite(content.transform, "Hide UI", "Common_Box_3", 68, 68, new Vector2(halfWidth - 85f, -475f), 4);
		Tex(content.transform, "Eye", eyeIcon, 42, 27, new Vector2(halfWidth - 85f, -475f), new Color(0.98f, 0.93f, 0.78f, 1f), 5);
		Clickable(uISprite4.gameObject, ToggleUi);
		for (int num7 = 0; num7 < thumbs.Length; num7++)
		{
			if (thumbs[num7] == null)
			{
				int width = 320;
				int height = Mathf.Max(90, Mathf.RoundToInt(320f / SprkGraphicsOptions.SnapshotAspect));
				thumbs[num7] = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
				{
					name = "SPRK Global Filter Thumbnail " + num7
				};
				thumbs[num7].Create();
			}
		}
		leftThumb.mainTexture = thumbs[0];
		centerThumb.mainTexture = thumbs[1];
		rightThumb.mainTexture = thumbs[2];
		Crop(leftThumb);
		Crop(centerThumb);
		Crop(rightThumb);
		content.SetActive(!hiddenUi);
		list.ResetPosition();
	}

	private void Crop(UITexture texture)
	{
		float snapshotAspect = SprkGraphicsOptions.SnapshotAspect;
		float num = (float)texture.width / (float)texture.height;
		texture.uvRect = ((snapshotAspect > num) ? new Rect(0.5f - num / snapshotAspect * 0.5f, 0f, num / snapshotAspect, 1f) : new Rect(0f, 0.5f - snapshotAspect / num * 0.5f, 1f, snapshotAspect / num));
	}

	private void Sync(bool thumbnails)
	{
		SprkGraphicsOptions.FilterValues filterValues = working[selected];
		SprkGraphicsOptions.PreviewFilter(selected, filterValues);
		presetName.text = SprkGraphicsOptions.FilterPresetNames[selected];
		checkMark.alpha = ((selected == savedPreset) ? 1f : 0f);
		bool active = selected != 0;
		paletteTab.spriteName = (advanced ? "Common_Button_Tab_Disable" : "Common_Button_Tab_Enable");
		advancedTab.spriteName = (advanced ? "Common_Button_Tab_Enable" : "Common_Button_Tab_Disable");
		Style(paletteTabLabel, advanced ? tabOffStyle : tabOnStyle);
		Style(advancedTabLabel, advanced ? tabOnStyle : tabOffStyle);
		paletteRoot.SetActive(!advanced);
		advancedRoot.SetActive(advanced);
		intensityRoot.SetActive(active);
		coordinate.text = "(" + filterValues.X + "," + filterValues.Y + ")";
		for (int i = 0; i < 11; i++)
		{
			for (int j = 0; j < 11; j++)
			{
				int num = i - 5 - filterValues.X;
				int num2 = j - 5 - filterValues.Y;
				bool flag = num == 0 && num2 == 0;
				bool flag2 = Mathf.Abs(num) + Mathf.Abs(num2) == 1;
				bool flag3 = num == 0 || num2 == 0;
				int num3 = (flag ? 26 : (flag2 ? 16 : (flag3 ? 12 : 10)));
				UITexture uITexture = dots[i, j];
				uITexture.width = num3;
				uITexture.height = num3;
				uITexture.color = new Color(1f, 1f, 1f, (flag || flag2) ? 1f : (flag3 ? 0.85f : 0.45f));
			}
		}
		originRing.alpha = ((filterValues.X == 0 && filterValues.Y == 0) ? 0f : 0.9f);
		foreach (Slider slider in sliders)
		{
			UpdateSlider(slider, filterValues, enabled: true);
		}
		UpdateSlider(intensity, filterValues, active);
		bool dirty = Dirty;
		applyButton.spriteName = (dirty ? "Common_Button6_Emphasis" : "Common_Button6_Disable");
		applyLabel.alpha = (dirty ? 1f : 0.7f);
		if (thumbnails)
		{
			thumbTimer = 0;
		}
	}

	private void RenderThumbs()
	{
		int num = SprkGraphicsOptions.FilterPresetNames.Length;
		for (int i = 0; i < 3; i++)
		{
			int num2 = ((selected + i - 1) % num + num) % num;
			SprkGraphicsOptions.RenderFilterThumbnail(thumbs[i], num2, working[num2]);
		}
	}

	private void Update()
	{
		if (window == null || !window.gameObject.activeInHierarchy || !window.IsOpen())
		{
			Close();
			return;
		}
		if (Screen.width != (int)screenSize.x || Screen.height != (int)screenSize.y)
		{
			UIRoot uIRoot = NGUITools.FindInParents<UIRoot>(base.gameObject);
			if (uIRoot != null)
			{
				Layout(uIRoot);
				Sync(thumbnails: true);
			}
		}
		foreach (KeyValuePair<UIPanel, float> hiddenPanel in hiddenPanels)
		{
			if (hiddenPanel.Key != null && hiddenPanel.Key.alpha > 0f)
			{
				hiddenPanel.Key.alpha = 0f;
			}
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Close();
			return;
		}
		if (Input.GetKeyDown(KeyCode.A))
		{
			Step(-1);
		}
		if (Input.GetKeyDown(KeyCode.D))
		{
			Step(1);
		}
		if (thumbTimer-- <= 0)
		{
			RenderThumbs();
			thumbTimer = 15;
		}
	}

	private Slider CreateSlider(Transform parent, int field, Vector2 center, float width, bool scroll)
	{
		SprkGraphicsOptions.FilterField filterField = SprkGraphicsOptions.FilterFields[field];
		Slider s = new Slider
		{
			field = field,
			width = width,
			bipolar = (filterField.Min < 0)
		};
		Sprite(parent, filterField.Name + " Box", "Common_Box_3", (int)width + 90, 88, center, 3);
		s.name = Label(parent, filterField.Name, 28, center + new Vector2((0f - width) * 0.5f, 20f), 8, UIWidget.Pivot.Left, nameStyle);
		s.value = Label(parent, "0", 28, center + new Vector2(width * 0.5f, 20f), 8, UIWidget.Pivot.Right, valueStyle);
		bool num = field == 2;
		Vector2 position = center + new Vector2(0f, -18f);
		s.track = Sprite(parent, filterField.Name + " Track", "soundbar_bg", (int)width, 24, position, 4);
		if (num)
		{
			Tex(parent, filterField.Name + " Gradient", temperatureTrack, (int)width - 10, 14, position, Color.white, 5);
		}
		UISprite uISprite = Sprite(parent, filterField.Name + " Fill", "soundbar_on", 10, 16, position, 5);
		uISprite.pivot = UIWidget.Pivot.Left;
		s.fill = uISprite;
		if (num)
		{
			s.fill.alpha = 0f;
		}
		s.knob = Sprite(parent, filterField.Name + " Knob", "soundbar_bt", 32, 44, position, 6, sliced: false);
		s.hit = Widget<UIWidget>(parent, filterField.Name + " Input", (int)width + 30, 54, position, 9);
		NGUITools.AddWidgetCollider(s.hit.gameObject);
		UIEventListener uIEventListener = UIEventListener.Get(s.hit.gameObject);
		uIEventListener.onPress = delegate(GameObject g, bool down)
		{
			if (down)
			{
				Drag(s);
			}
		};
		uIEventListener.onDrag = delegate
		{
			Drag(s);
		};
		if (scroll)
		{
			uIEventListener.onScroll = delegate(GameObject g, float delta)
			{
				if (list != null)
				{
					list.Scroll(delta);
				}
			};
			UIWidget uIWidget = Widget<UIWidget>(parent, filterField.Name + " Row", (int)width + 90, 88, center, 2);
			NGUITools.AddWidgetCollider(uIWidget.gameObject);
			uIWidget.gameObject.AddComponent<UIDragScrollView>().scrollView = list;
		}
		return s;
	}

	private void Drag(Slider s)
	{
		if (!(UICamera.currentCamera == null))
		{
			SprkGraphicsOptions.FilterField filterField = SprkGraphicsOptions.FilterFields[s.field];
			Vector3 position = UICamera.currentCamera.ScreenToWorldPoint(UICamera.lastTouchPosition);
			float t = Mathf.Clamp01(s.track.transform.InverseTransformPoint(position).x / s.width + 0.5f);
			SetField(s.field, Mathf.RoundToInt(Mathf.Lerp(filterField.Min, filterField.Max, t)));
		}
	}

	private void UpdateSlider(Slider s, SprkGraphicsOptions.FilterValues values, bool enabled)
	{
		SprkGraphicsOptions.FilterField filterField = SprkGraphicsOptions.FilterFields[s.field];
		int num = values.Get(s.field);
		float num2 = (float)(num - filterField.Min) / (float)(filterField.Max - filterField.Min);
		s.value.text = SprkGraphicsOptions.FilterValueLabel(s.field, num);
		Vector3 localPosition = s.track.transform.localPosition;
		float num3 = (s.bipolar ? Mathf.Min(num2, 0.5f) : 0f);
		float num4 = (s.bipolar ? Mathf.Max(num2, 0.5f) : num2);
		bool flag = num4 - num3 > 0.001f;
		s.fill.width = Mathf.Max(10, Mathf.RoundToInt((num4 - num3) * (s.width - 8f)));
		s.fill.transform.localPosition = new Vector3(localPosition.x - s.width * 0.5f + 4f + num3 * (s.width - 8f), localPosition.y, 0f);
		s.knob.transform.localPosition = new Vector3(localPosition.x - s.width * 0.5f + num2 * s.width, localPosition.y, 0f);
		float num5 = (enabled ? 1f : 0.35f);
		s.name.alpha = num5;
		s.value.alpha = num5;
		s.knob.alpha = num5;
		s.track.alpha = num5;
		if (s.field != 2)
		{
			s.fill.alpha = (flag ? num5 : 0f);
		}
		Collider component = s.hit.GetComponent<Collider>();
		if (component != null)
		{
			component.enabled = enabled;
		}
	}

	private void PickPalette(UITexture pad)
	{
		if (!(UICamera.currentCamera == null))
		{
			Vector3 vector = pad.transform.InverseTransformPoint(UICamera.currentCamera.ScreenToWorldPoint(UICamera.lastTouchPosition));
			SetPalette(Mathf.Clamp(Mathf.RoundToInt(vector.x / 30f), -5, 5), Mathf.Clamp(Mathf.RoundToInt(vector.y / 30f), -5, 5));
		}
	}

	private GameObject Child(Transform parent, string name)
	{
		GameObject obj = new GameObject(name);
		obj.layer = layer;
		obj.transform.SetParent(parent, worldPositionStays: false);
		return obj;
	}

	private T Widget<T>(Transform parent, string name, int width, int height, Vector2 position, int depth) where T : UIWidget
	{
		GameObject obj = Child(parent, name);
		obj.transform.localPosition = position;
		T val = obj.AddComponent<T>();
		val.width = width;
		val.height = height;
		val.depth = depth;
		return val;
	}

	private UITexture Tex(Transform parent, string name, Texture texture, int width, int height, Vector2 position, Color color, int depth)
	{
		UITexture uITexture = Widget<UITexture>(parent, name, width, height, position, depth);
		uITexture.mainTexture = texture;
		uITexture.color = color;
		return uITexture;
	}

	private UISprite Sprite(Transform parent, string name, string sprite, int width, int height, Vector2 position, int depth, bool sliced = true)
	{
		UISprite uISprite = Widget<UISprite>(parent, name, width, height, position, depth);
		UIAtlas uIAtlas = atlases.FirstOrDefault((UIAtlas a) => a.GetSprite(sprite) != null);
		if (uIAtlas == null)
		{
			if (!missingSprites.Contains(sprite))
			{
				missingSprites.Add(sprite);
			}
		}
		else
		{
			uISprite.atlas = uIAtlas;
			uISprite.spriteName = sprite;
		}
		uISprite.type = (sliced ? UIBasicSprite.Type.Sliced : UIBasicSprite.Type.Simple);
		uISprite.width = width;
		uISprite.height = height;
		return uISprite;
	}

	private static string Path(Transform t)
	{
		string text = t.name;
		t = t.parent;
		while (t != null)
		{
			text = t.name + "/" + text;
			t = t.parent;
		}
		return text;
	}

	private static void Style(UILabel label, UILabel source)
	{
		if (!(label == null))
		{
			if (source == null)
			{
				label.color = Color.white;
				label.effectStyle = UILabel.Effect.Outline8;
				label.effectColor = Color.black;
				label.applyGradient = false;
				return;
			}
			label.color = source.color;
			label.applyGradient = source.applyGradient;
			label.gradientTop = source.gradientTop;
			label.gradientBottom = source.gradientBottom;
			label.effectStyle = source.effectStyle;
			label.effectColor = source.effectColor;
			label.effectDistance = source.effectDistance;
			label.spacingX = source.spacingX;
			label.fontStyle = source.fontStyle;
		}
	}

	private UILabel Label(Transform parent, string text, int size, Vector2 position, int depth, UIWidget.Pivot pivot, UILabel style)
	{
		GameObject obj = Child(parent, "Label " + text);
		obj.transform.localPosition = position;
		UILabel uILabel = obj.AddComponent<UILabel>();
		UILabel uILabel2 = ((style != null && (style.bitmapFont != null || style.trueTypeFont != null)) ? style : null);
		if (uILabel2 != null)
		{
			uILabel.bitmapFont = uILabel2.bitmapFont;
			if (uILabel2.bitmapFont == null)
			{
				uILabel.trueTypeFont = uILabel2.trueTypeFont;
			}
		}
		else if (bitmapFont != null)
		{
			uILabel.bitmapFont = bitmapFont;
		}
		else if (trueTypeFont != null)
		{
			uILabel.trueTypeFont = trueTypeFont;
		}
		uILabel.fontSize = size;
		uILabel.overflowMethod = UILabel.Overflow.ResizeFreely;
		uILabel.pivot = pivot;
		uILabel.depth = depth;
		uILabel.alignment = pivot switch
		{
			UIWidget.Pivot.Right => NGUIText.Alignment.Right, 
			UIWidget.Pivot.Left => NGUIText.Alignment.Left, 
			_ => NGUIText.Alignment.Center, 
		};
		Style(uILabel, style);
		uILabel.text = text;
		obj.transform.localPosition = position;
		return uILabel;
	}

	private void Clickable(GameObject go, Action action, int padding = 0)
	{
		go.GetComponent<UIWidget>();
		NGUITools.AddWidgetCollider(go);
		BoxCollider component = go.GetComponent<BoxCollider>();
		if (padding > 0 && component != null)
		{
			component.size += new Vector3(padding * 2, padding * 2, 0f);
		}
		UIEventListener.Get(go).onClick = delegate
		{
			action();
		};
	}

	private static Texture2D NewTexture(int w, int h)
	{
		return new Texture2D(w, h, TextureFormat.RGBA32, mipChain: false)
		{
			wrapMode = TextureWrapMode.Clamp,
			filterMode = FilterMode.Bilinear,
			hideFlags = HideFlags.DontSave
		};
	}

	private static void EnsureTextures()
	{
		if (white != null)
		{
			return;
		}
		white = NewTexture(4, 4);
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				white.SetPixel(j, i, Color.white);
			}
		}
		white.Apply();
		circle = Disc(64, 0f);
		ring = Disc(64, 0.72f);
		eyeIcon = NewTexture(96, 60);
		for (int k = 0; k < 60; k++)
		{
			for (int l = 0; l < 96; l++)
			{
				float num = ((float)l - 47.5f) / 46f;
				float f = ((float)k - 29.5f) / 29f;
				float num2 = (1f - num * num) * 0.92f;
				float num3 = Mathf.Abs(Mathf.Abs(f) - num2) * 29f;
				float a = ((Mathf.Abs(num) <= 1f) ? Mathf.Clamp01(3.2f - num3) : 0f);
				float num4 = Vector2.Distance(new Vector2(l, k), new Vector2(47.5f, 29.5f));
				a = Mathf.Max(a, Mathf.Clamp01(15.5f - num4));
				if (num4 < 6f)
				{
					a = Mathf.Min(a, Mathf.Clamp01(num4 - 4.5f));
				}
				eyeIcon.SetPixel(l, k, new Color(1f, 1f, 1f, a));
			}
		}
		eyeIcon.Apply();
		padGradient = NewTexture(64, 64);
		Color a2 = new Color32(175, 185, 189, byte.MaxValue);
		Color b = new Color32(119, 185, 207, byte.MaxValue);
		Color a3 = new Color32(34, 38, 39, byte.MaxValue);
		Color b2 = new Color32(27, 43, 63, byte.MaxValue);
		for (int m = 0; m < 64; m++)
		{
			for (int n = 0; n < 64; n++)
			{
				float t = (float)n / 63f;
				float t2 = (float)m / 63f;
				padGradient.SetPixel(n, m, Color.Lerp(Color.Lerp(a3, b2, t), Color.Lerp(a2, b, t), t2));
			}
		}
		padGradient.Apply();
		temperatureTrack = NewTexture(256, 4);
		for (int num5 = 0; num5 < 256; num5++)
		{
			float num6 = (float)num5 / 255f;
			Color color = ((num6 < 0.5f) ? Color.Lerp(new Color(0.36f, 0.66f, 0.86f), new Color(0.6f, 0.8f, 0.75f), num6 * 2f) : Color.Lerp(new Color(0.6f, 0.8f, 0.75f), new Color(0.88f, 0.6f, 0.24f), (num6 - 0.5f) * 2f));
			for (int num7 = 0; num7 < 4; num7++)
			{
				temperatureTrack.SetPixel(num5, num7, color);
			}
		}
		temperatureTrack.Apply();
	}

	private static Texture2D Disc(int size, float hole)
	{
		Texture2D texture2D = NewTexture(size, size);
		float num = (float)(size - 1) * 0.5f;
		float num2 = (float)size * 0.5f;
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				float num3 = Vector2.Distance(new Vector2(j, i), new Vector2(num, num));
				float a = Mathf.Clamp01(num2 - num3);
				if (hole > 0f)
				{
					a = Mathf.Min(a, Mathf.Clamp01(num3 - num2 * hole));
				}
				texture2D.SetPixel(j, i, new Color(1f, 1f, 1f, a));
			}
		}
		texture2D.Apply();
		return texture2D;
	}
}
