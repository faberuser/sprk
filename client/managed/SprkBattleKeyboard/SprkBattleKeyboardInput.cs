using System.Collections.Generic;
using System.Linq;
using NGame2.NBattleField;
using NGame2.NUI.NComponent2.NBattle;
using NGame2.NUI.NWindow.NBattle;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

public sealed class SprkBattleKeyboardInput : MonoBehaviour
{
	private const string HintName = "SPRK Key Hint";
	private const float HintRefreshSeconds = 0.25f;
	private const float BadgeScale = 0.55f;
	private static readonly Color HintText = new Color(1f, 0.95f, 0.8f);
	private static UIAtlas hintAtlas;
	private static bool hintAtlasSearched;
	private HeroParty party;
	private readonly List<PartySlotComponent2> slots = new List<PartySlotComponent2>(4);
	private readonly List<BaseSkillComponent> skills = new List<BaseSkillComponent>(8);
	private float nextHintRefresh;
	private int hintRevision = -1;
	private static int keyboardFires;
	private static int keyboardFiresBeforeStart;

	public static void Attach(HeroParty owner)
	{
		SprkBattleKeyboardInput input = owner.GetComponent<SprkBattleKeyboardInput>();
		if (input == null)
		{
			input = owner.gameObject.AddComponent<SprkBattleKeyboardInput>();
		}
		input.party = owner;
		input.hintRevision = -1;
	}

	// HeroParty's normal UI update is throttled to 0.1s; key-down edges need every frame.
	private void LateUpdate()
	{
		if (party == null)
		{
			return;
		}
		RefreshHints();
		if (!SprkBattleKeyboard.ControlsEnabled || !Input.anyKeyDown || GlobalTimer.pause || !ActivePartyVisible())
		{
			return;
		}
		BattleFacade facade = SingletonAuto<BattleFacade>.instance;
		if (!facade.IsValidBattle())
		{
			return;
		}
		BattleFacade.BattleCacheData battle = facade.GetBattleCache();
		// Like tapping the icons, keys may reserve skills before the fight starts (wave intro);
		// the native click/reserve path decides what is allowed.
		if (battle == null || battle.IsBattleEnded)
		{
			return;
		}
		OrderedSlots(ActiveSlots(), slots);
		for (int hero = 0; hero < slots.Count && hero < 4; hero++)
		{
			PartySlotComponent2 slot = slots[hero];
			if (slot.CreatureCache.Uid == 0 || slot.CreatureCache.IsDeadOrResting())
			{
				continue;
			}
			for (int action = 0; action < 4; action++)
			{
				if (!SprkBattleKeyboard.IsPressed(hero, action))
				{
					continue;
				}
				BaseSkillComponent skill = FindAction(slot, action);
				if (skill != null && skill.gameObject.activeInHierarchy && Visible(skill.WidgetThis))
				{
					// Preserve native locking, disabled-slot, next-skill and reservation behavior.
					keyboardFires++;
					if (!battle.IsBattleStarted) keyboardFiresBeforeStart++;
					skill.OnClickSkill();
				}
			}
		}
	}

	// The battle HUD shows HeroParty through its panel while the window itself stays in
	// OpenedHide, so BaseWindow.IsShow is false for the whole fight. Use IsOpen plus the
	// rendered alpha of whichever party panel (main or swap) is in front.
	private bool ActivePartyVisible()
	{
		return party.IsOpen() && Visible(UseSubParty() ? party.Panel_SubParty : party.Panel_Party);
	}

	private bool UseSubParty()
	{
		return party.EnabledSwapParty && !party.IsSelectedMain;
	}

	private IncreaseDic<int, PartySlotComponent2> ActiveSlots()
	{
		return UseSubParty() ? party.SubPartySlotDic : party.PartySlotDic;
	}

	private static void OrderedSlots(IncreaseDic<int, PartySlotComponent2> source, List<PartySlotComponent2> result)
	{
		result.Clear();
		if (source == null)
		{
			return;
		}
		for (int i = 0; i < source.Count; i++)
		{
			PartySlotComponent2 slot = source.GetValueByIndex(i);
			if (slot != null && slot.gameObject.activeInHierarchy && Visible(slot.RootWidget))
			{
				result.Add(slot);
			}
		}
		// Dictionary/wave order is not necessarily the left-to-right rendered order.
		result.Sort(ComparePosition);
	}

	private void RefreshHints()
	{
		int revision = SprkBattleKeyboard.Revision;
		if (revision == hintRevision && Time.unscaledTime < nextHintRefresh)
		{
			return;
		}
		hintRevision = revision;
		nextHintRefresh = Time.unscaledTime + HintRefreshSeconds;
		bool show = SprkBattleKeyboard.ControlsEnabled && SprkBattleKeyboard.KeyHintsEnabled;
		RefreshHints(party.PartySlotDic, show);
		if (party.EnabledSwapParty)
		{
			RefreshHints(party.SubPartySlotDic, show);
		}
	}

	private void RefreshHints(IncreaseDic<int, PartySlotComponent2> source, bool show)
	{
		OrderedSlots(source, slots);
		for (int hero = 0; hero < slots.Count; hero++)
		{
			PartySlotComponent2 slot = slots[hero];
			skills.Clear();
			slot.FindSkillComponent(skill => { skills.Add(skill); return false; });
			for (int i = 0; i < skills.Count; i++)
			{
				int action = hero < 4 ? ActionIndex(slot, skills[i]) : -1;
				string text = show && action >= 0 ? SprkBattleKeyboard.GetBinding(hero * 4 + action).ShortLabel : "";
				SetHint(skills[i], text);
			}
		}
	}

	private static int ActionIndex(PartySlotComponent2 slot, BaseSkillComponent skill)
	{
		for (int action = 0; action < 4; action++)
		{
			if (FindAction(slot, action) == skill)
			{
				return action;
			}
		}
		return -1;
	}

	private static void SetHint(BaseSkillComponent skill, string text)
	{
		UIWidget anchor = skill.WidgetThis;
		if (anchor == null)
		{
			return;
		}
		Transform hint = anchor.transform.Find(HintName);
		if (string.IsNullOrEmpty(text))
		{
			if (hint != null && hint.gameObject.activeSelf)
			{
				hint.gameObject.SetActive(false);
			}
			return;
		}
		if (hint == null)
		{
			hint = CreateHint(skill, anchor).transform;
		}
		UILabel label = hint.GetComponentInChildren<UILabel>(true);
		UIWidget back = hint.GetComponent<UIWidget>();
		if (label == null || back == null)
		{
			return;
		}
		if (label.text != text)
		{
			label.text = text;
		}
		// Size the key cap to the icon and keep it in the icon's top-left corner.
		// Built at 1/BadgeScale and scaled down so the sliced gold border renders thinner.
		float onIcon = Mathf.Max(24f, Mathf.Min(anchor.width, anchor.height) * 0.32f);
		int size = Mathf.RoundToInt(onIcon / BadgeScale);
		label.fontSize = Mathf.RoundToInt(size * 0.6f);
		label.UpdateNGUIText();
		int width = Mathf.Max(size, Mathf.CeilToInt(NGUIText.CalculatePrintedSize(text).x) + size / 2);
		back.width = width;
		back.height = size;
		label.width = width - 8;
		label.height = size;
		hint.localScale = new Vector3(BadgeScale, BadgeScale, 1f);
		Vector3[] corners = anchor.localCorners;
		hint.localPosition = new Vector3(corners[1].x + width * BadgeScale * 0.5f - 2f, corners[1].y - size * BadgeScale * 0.5f + 2f, 0f);
		if (!hint.gameObject.activeSelf)
		{
			hint.gameObject.SetActive(true);
		}
	}

	private static GameObject CreateHint(BaseSkillComponent skill, UIWidget anchor)
	{
		int depth = 0;
		foreach (UIWidget widget in skill.GetComponentsInChildren<UIWidget>(true))
		{
			depth = Mathf.Max(depth, widget.depth);
		}
		GameObject go = new GameObject(HintName);
		go.layer = anchor.gameObject.layer;
		go.transform.SetParent(anchor.transform, false);
		UIWidget back;
		UIAtlas atlas = HintAtlas();
		if (atlas != null)
		{
			UISprite sprite = go.AddComponent<UISprite>();
			sprite.atlas = atlas;
			sprite.spriteName = "Common_CheckBox";
			sprite.type = UIBasicSprite.Type.Sliced;
			back = sprite;
		}
		else
		{
			UITexture texture = go.AddComponent<UITexture>();
			texture.mainTexture = Texture2D.whiteTexture;
			texture.color = new Color(0.1f, 0.07f, 0.04f, 0.8f);
			back = texture;
		}
		back.depth = depth + 10;
		GameObject text = new GameObject("Key");
		text.layer = go.layer;
		text.transform.SetParent(go.transform, false);
		UILabel label = text.AddComponent<UILabel>();
		UILabel style = skill.LabelCoolTime != null ? skill.LabelCoolTime : skill.LabelStatus;
		if (style != null && style.bitmapFont != null)
		{
			label.bitmapFont = style.bitmapFont;
		}
		else if (style != null && style.trueTypeFont != null)
		{
			label.trueTypeFont = style.trueTypeFont;
		}
		else
		{
			UILabel any = skill.GetComponentsInChildren<UILabel>(true).FirstOrDefault(l => l.bitmapFont != null || l.trueTypeFont != null);
			if (any != null)
			{
				label.bitmapFont = any.bitmapFont;
				label.trueTypeFont = any.bitmapFont == null ? any.trueTypeFont : null;
			}
		}
		label.supportEncoding = false;
		label.overflowMethod = UILabel.Overflow.ShrinkContent;
		label.alignment = NGUIText.Alignment.Center;
		label.color = HintText;
		label.effectStyle = UILabel.Effect.Outline;
		label.effectColor = new Color(0.12f, 0.08f, 0.03f, 0.9f);
		label.effectDistance = new Vector2(1.6f, 1.6f);
		label.depth = depth + 11;
		return go;
	}

	private static UIAtlas HintAtlas()
	{
		if (hintAtlas == null && !hintAtlasSearched)
		{
			hintAtlasSearched = true;
			hintAtlas = Resources.FindObjectsOfTypeAll<UIAtlas>().FirstOrDefault(a => a != null && a.GetSprite("Common_CheckBox") != null);
		}
		return hintAtlas;
	}

	private static BaseSkillComponent FindAction(PartySlotComponent2 slot, int action)
	{
		if (action == 3)
		{
			return slot.FindSkillComponent(skill => skill is SoulWeaponSkillComponent);
		}
		int index = 0;
		return slot.FindSkillComponent(skill => skill is NormalSkillComponent && index++ == action);
	}

	private static int ComparePosition(PartySlotComponent2 left, PartySlotComponent2 right)
	{
		return ScreenX(left).CompareTo(ScreenX(right));
	}

	private static float ScreenX(PartySlotComponent2 slot)
	{
		Camera camera = NGUITools.FindCameraForLayer(slot.gameObject.layer);
		return camera != null ? camera.WorldToScreenPoint(slot.transform.position).x : slot.transform.position.x;
	}

	private static bool Visible(UIPanel panel)
	{
		return panel != null && panel.enabled && panel.gameObject.activeInHierarchy
			&& panel.CalculateFinalAlpha(Time.frameCount) > 0.01f;
	}

	private static bool Visible(UIWidget widget)
	{
		return widget != null && widget.enabled && widget.gameObject.activeInHierarchy
			&& widget.isVisible && widget.CalculateFinalAlpha(Time.frameCount) > 0.01f;
	}

	// Automation (battle-keyboard-live): the gate the battle hook evaluates each frame,
	// plus hint badges and how many skills keyboard input has fired so far.
	public static string Diagnose()
	{
		var sb = new System.Text.StringBuilder();
		sb.Append("enabled=").Append(SprkBattleKeyboard.ControlsEnabled)
			.Append(" hints=").Append(SprkBattleKeyboard.KeyHintsEnabled)
			.Append(" settingsOpen=").Append(SprkBattleKeyboard.SettingsOpen)
			.Append(" pause=").Append(GlobalTimer.pause)
			.Append(" fires=").Append(keyboardFires)
			.Append(" firesBeforeStart=").Append(keyboardFiresBeforeStart);
		foreach (SprkBattleKeyboardInput input in FindObjectsOfType<SprkBattleKeyboardInput>())
		{
			HeroParty p = input.party;
			if (p == null)
			{
				continue;
			}
			sb.Append(" || party open=").Append(p.IsOpen()).Append(" state=").Append(p.WindowState)
				.Append(" partyVisible=").Append(input.ActivePartyVisible());
			BattleFacade facade = SingletonAuto<BattleFacade>.instance;
			if (facade.IsValidBattle() && facade.GetBattleCache() != null)
			{
				BattleFacade.BattleCacheData b = facade.GetBattleCache();
				sb.Append(" started=").Append(b.IsBattleStarted).Append(" ended=").Append(b.IsBattleEnded).Append(" battleEnabled=").Append(b.IsBattleEnabled);
			}
			OrderedSlots(input.ActiveSlots(), input.slots);
			for (int hero = 0; hero < input.slots.Count; hero++)
			{
				sb.Append(" || hero ").Append(hero + 1).Append(':');
				for (int action = 0; action < 4; action++)
				{
					BaseSkillComponent skill = FindAction(input.slots[hero], action);
					Transform hint = skill != null && skill.WidgetThis != null ? skill.WidgetThis.transform.Find(HintName) : null;
					sb.Append(' ').Append(skill == null ? "-" : (hint != null && hint.gameObject.activeInHierarchy
						? hint.GetComponentInChildren<UILabel>(true).text : "?"));
				}
			}
		}
		return sb.ToString();
	}
}
