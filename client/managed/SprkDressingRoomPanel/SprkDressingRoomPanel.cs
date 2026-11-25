using System.Collections.Generic;
using NGame2.NUI.NAtlasAssignComponent;
using NGame2.NUI.NWindow;
using UnityEngine;

// Adds an arrow tab to the Dressing Room that slides the left panel (category column, costume
// lists, hero list) out to the left and back, so the model and the room can be seen unobstructed.
//
// Those widgets are NGUI rects anchored every frame to the left edge of NormalMenu. Moving their
// transforms would be undone by the anchors, so the slide shifts the anchor offsets instead.
public static class SprkDressingRoomPanel
{
	private const float SlideSeconds = 0.3f;

	// Right edge of the category column and lists, measured from NormalMenu's left edge.
	private const int PanelWidth = 825;

	private const float ButtonGap = 32f;

	// Gold chevron (66x158) from NguiAtlas_UI_Prefab_Quest; it points left, so it reads as "hide".
	private const string ChevronAtlas = "Quest";

	private const string ChevronSprite = "Quest_BigArrow_Left";

	private const float ChevronScale = 0.75f;

	// Vertical centre of the left panel in NormalMenu space.
	private const float ButtonY = 42f;

	private sealed class Slider
	{
		public UIRect Rect;

		public int Left;

		public int Right;
	}

	private sealed class State
	{
		public HeroCostumeView View;

		public UIWidget Menu;

		public readonly List<Slider> Sliders = new List<Slider>();

		public UIButton Button;

		public bool Hidden;

		// 0 = panel shown, 1 = panel hidden.
		public float Progress;
	}

	private static State state;

	// Called from HeroCostumeView.UpdateDerived.
	public static void Update(HeroCostumeView view)
	{
		State current = Ensure(view);
		if (current == null)
		{
			return;
		}
		float target = current.Hidden ? 1f : 0f;
		if (current.Progress != target)
		{
			current.Progress = Mathf.MoveTowards(current.Progress, target, Time.unscaledDeltaTime / SlideSeconds);
		}
		Apply(current);
	}

	// Called when the Dressing Room is reset; it always reopens with the panel shown.
	public static void Reset(HeroCostumeView view)
	{
		if (state != null && state.View == view)
		{
			state.Hidden = false;
			state.Progress = 0f;
			Apply(state);
		}
	}

	private static void Toggle()
	{
		if (state != null)
		{
			state.Hidden = !state.Hidden;
		}
	}

	private static State Ensure(HeroCostumeView view)
	{
		if (view == null)
		{
			return null;
		}
		if (state != null && state.View == view && state.Menu != null && state.Button != null)
		{
			return state;
		}
		if (state == null || state.View != view || state.Menu == null)
		{
			UIWidget menu = FindMenu(view);
			if (menu == null)
			{
				return null;
			}
			if (state != null && state.View != null && state.View != view)
			{
				state.Progress = 0f;
				Apply(state);
			}
			state = new State
			{
				View = view,
				Menu = menu
			};
			Transform menuTransform = menu.transform;
			UIRect[] rects = menu.GetComponentsInChildren<UIRect>(includeInactive: true);
			for (int i = 0; i < rects.Length; i++)
			{
				UIRect rect = rects[i];
				if (rect.leftAnchor.target == menuTransform && rect.rightAnchor.target == menuTransform && rect.leftAnchor.relative == 0f && rect.rightAnchor.relative == 0f)
				{
					state.Sliders.Add(new Slider
					{
						Rect = rect,
						Left = rect.leftAnchor.absolute,
						Right = rect.rightAnchor.absolute
					});
				}
			}
		}
		state.Button = CreateButton(view, state.Menu);
		return (state.Button != null) ? state : null;
	}

	private static UIWidget FindMenu(HeroCostumeView view)
	{
		Transform found = view.transform.Find("Panel/Root/NormalMenu");
		UIWidget menu = (found != null) ? found.GetComponent<UIWidget>() : null;
		return (menu != null) ? menu : view.NormalMenuWidget;
	}

	private static UIButton CreateButton(HeroCostumeView view, UIWidget menu)
	{
		// The game's own back arrow, reused so the tab matches the window's style.
		UIButton source = view.NormalCloseButton;
		if (source == null)
		{
			return null;
		}
		GameObject go = Object.Instantiate(source.gameObject, menu.transform, worldPositionStays: false);
		go.name = "SprkPanelToggle";
		UIRect[] rects = go.GetComponentsInChildren<UIRect>(includeInactive: true);
		for (int i = 0; i < rects.Length; i++)
		{
			UIRect rect = rects[i];
			rect.leftAnchor.target = null;
			rect.rightAnchor.target = null;
			rect.bottomAnchor.target = null;
			rect.topAnchor.target = null;
			rect.ResetAnchors();
		}
		UIButton button = go.GetComponent<UIButton>();
		button.onClick.Clear();
		EventDelegate.Add(button.onClick, Toggle);
		UseChevron(go, button);
		go.SetActive(value: true);
		return button;
	}

	// Swaps the back arrow for the gold chevron of the Quest window's page arrows. If that atlas
	// cannot be loaded the tab keeps the back arrow, which still works.
	private static void UseChevron(GameObject go, UIButton button)
	{
		UISprite sprite = go.GetComponent<UISprite>();
		UISpriteAtlasAssigner assigner = go.GetComponent<UISpriteAtlasAssigner>();
		if (sprite == null || assigner == null)
		{
			return;
		}
		UIAtlas previousAtlas = sprite.atlas;
		string previousSprite = sprite.spriteName;
		assigner.Target = sprite;
		assigner.AtlasName = ChevronAtlas;
		assigner.SpriteName = ChevronSprite;
		sprite.atlas = null;
		assigner.Assign();
		if (sprite.atlas == null || sprite.GetAtlasSprite() == null)
		{
			assigner.AtlasName = (previousAtlas != null) ? previousAtlas.name : assigner.AtlasName;
			assigner.SpriteName = previousSprite;
			sprite.atlas = previousAtlas;
			sprite.spriteName = previousSprite;
			Debug.LogWarning("SprkDressingRoomPanel: could not load " + ChevronAtlas + "/" + ChevronSprite + "; keeping the back arrow");
			return;
		}
		// Plain button: no hover/press sprite swaps back to the original arrow.
		button.hoverSprite = null;
		button.pressedSprite = null;
		button.disabledSprite = null;
		button.normalSprite = ChevronSprite;
		sprite.type = UIBasicSprite.Type.Simple;
		sprite.MakePixelPerfect();
		sprite.width = Mathf.RoundToInt(sprite.width * ChevronScale);
		sprite.height = Mathf.RoundToInt(sprite.height * ChevronScale);
		NGUITools.UpdateWidgetCollider(go);
	}

	private static void Apply(State current)
	{
		float eased = Mathf.SmoothStep(0f, 1f, current.Progress);
		int offset = Mathf.RoundToInt(eased * PanelWidth);
		for (int i = 0; i < current.Sliders.Count; i++)
		{
			Slider slider = current.Sliders[i];
			if (slider.Rect != null)
			{
				slider.Rect.leftAnchor.absolute = slider.Left - offset;
				slider.Rect.rightAnchor.absolute = slider.Right - offset;
			}
		}
		if (current.Button != null && current.Menu != null)
		{
			float left = current.Menu.localCorners[0].x;
			Transform transform = current.Button.transform;
			transform.localPosition = new Vector3(left + PanelWidth + ButtonGap - offset, ButtonY, 0f);
			// The arrow points left ("hide") while the panel is out and turns to point right ("show").
			transform.localRotation = Quaternion.Euler(0f, 0f, 180f * eased);
		}
	}
}
