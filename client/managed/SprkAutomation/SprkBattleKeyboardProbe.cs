using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NBattle;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

internal static class SprkBattleKeyboardProbe
{
	private const string Prefix = "Sprk.BattleKeyboard.";
	private const string RowName = "SPRK Battle Keyboard";
	private const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
	private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
	private static GameOptionWindow window;
	private static Type keyboard;
	private static Type binding;

	internal static void CheckContext(int target, string mode)
	{
		string profile = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE") ?? "";
		if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR")) ||
			UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login" ||
			!profile.StartsWith("battle-keyboard-", StringComparison.Ordinal) || profile.Length == "battle-keyboard-".Length)
			throw new InvalidOperationException("Battle keyboard probe requires isolated Login and a battle-keyboard-* automation profile");
		if (target != 1 || (mode != "init" && mode != "run"))
			throw new InvalidOperationException("Opt in with battle-keyboard-probe target=1 text=init or text=run");
	}

	internal static List<object> Run(string mode)
	{
		CheckContext(1, mode);
		keyboard = Assembly.Load("SprkBattleKeyboard").GetType("SprkBattleKeyboard", true);
		binding = keyboard.GetNestedType("Binding", BindingFlags.NonPublic);
		var report = new List<object>();
		GameOptionButtonComponent row = Fixture();
		if (mode == "init")
		{
			report.Add("READY: native GameOptionWindow Battle tab and keyboard row visible; no account-dependent settings.Open called. Send target=1 text=run to verify and open the editor, or click the row for a screenshot.");
			return report;
		}

		var saved = new Preferences();
		try
		{
			foreach (string key in new[] { "Enabled", "Preset", "Custom.v1" }) SprkTestPrefs.DeleteKey(Prefix + key);
			SprkTestPrefs.Save();
			Reload();
			Check(!(bool)Get("ControlsEnabled"), "controls must default off");
			Check((int)Get("SelectedPreset") == 0, "default preset must be A");
			report.Add("PASS: absent preferences default to OFF and Preset A");

			KeyCode[][] keys = {
				new[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
					KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8,
					KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I },
				new[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
					KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
					KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
					KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 },
				new[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
					KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F,
					KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V }
			};
			string[][] labels = {
				"1,2,3,4,5,6,7,8,Q,W,E,R,T,Y,U,I".Split(','),
				"1,2,3,4,Alt+1,Alt+2,Alt+3,Alt+4,Shift+1,Shift+2,Shift+3,Shift+4,Ctrl+1,Ctrl+2,Ctrl+3,Ctrl+4".Split(','),
				"1,2,3,4,Q,W,E,R,A,S,D,F,Z,X,C,V".Split(',')
			};
			for (int p = 0; p < 3; p++)
			{
				Set("SelectedPreset", p);
				for (int slot = 0; slot < 16; slot++)
				{
					int modifiers = p == 1 ? new[] { 0, 2, 4, 1 }[slot / 4] : 0;
					CheckBinding(slot, keys[p][slot], modifiers, "preset " + p);
					Check((string)Call("GetBindingLabel", slot / 4, slot % 4) == labels[p][slot], "preset label " + p + "/" + slot);
				}
				report.Add("PASS: Preset " + (char)('A' + p) + " all 16 exact keys/modifiers/labels: " + string.Join(", ", labels[p]));
			}

			Set("SelectedPreset", 3);
			for (int slot = 0; slot < 16; slot++) CheckBinding(slot, keys[0][slot], 0, "initial custom clones A");
			var expectedKeys = (KeyCode[])keys[0].Clone();
			var expectedModifiers = new int[16];
			Assign(0, KeyCode.F9, 15);
			expectedKeys[0] = KeyCode.F9; expectedModifiers[0] = 15;
			Assign(1, KeyCode.F9, 1);
			expectedKeys[1] = KeyCode.F9; expectedModifiers[1] = 1;
			Check((int)Call("FindDuplicate", 7, NewBinding(KeyCode.F9, 15)) == 0, "duplicate must match exact chord");
			Check((int)Call("FindDuplicate", 0, NewBinding(KeyCode.F9, 15)) == -1, "duplicate excludes own slot");
			Check((int)Call("FindDuplicate", 7, NewBinding(KeyCode.F9, 2)) == -1, "different modifiers are not duplicates");
			Assign(7, KeyCode.F9, 15);
			expectedKeys[0] = KeyCode.None; expectedModifiers[0] = 0;
			expectedKeys[7] = KeyCode.F9; expectedModifiers[7] = 15;
			Assign(15, KeyCode.None, 0);
			expectedKeys[15] = KeyCode.None;
			Check((int)Call("FindDuplicate", 15, NewBinding(KeyCode.None, 0)) == -1, "unbound slots are not duplicates");
			CheckCustom(expectedKeys, expectedModifiers);
			Check((string)Call("GetBindingLabel", 0, 0) == "Unbound" &&
				(string)Call("GetBindingLabel", 1, 3) == "Ctrl+Alt+Shift+Meta+F9", "unbound and combined modifier labels");
			string serialized = SprkTestPrefs.GetString(Prefix + "Custom.v1", "");
			Check(serialized.Split(';').Length == 16, "all custom slots persisted");
			for (int p = 0; p < 3; p++)
			{
				Set("SelectedPreset", p);
				Reload();
				Check((int)Get("SelectedPreset") == p, "preset selection reload " + p);
				for (int slot = 0; slot < 16; slot++)
					CheckBinding(slot, keys[p][slot], p == 1 ? new[] { 0, 2, 4, 1 }[slot / 4] : 0, "preset unchanged after custom edits " + p);
				Set("SelectedPreset", 3);
				CheckCustom(expectedKeys, expectedModifiers);
				Check(SprkTestPrefs.GetString(Prefix + "Custom.v1", "") == serialized, "preset switching overwrote custom");
			}
			Set("ControlsEnabled", true);
			Reload();
			Check((bool)Get("ControlsEnabled") && (int)Get("SelectedPreset") == 3, "enabled/custom selection persisted on reload");
			CheckCustom(expectedKeys, expectedModifiers);
			Set("ControlsEnabled", false);
			Reload();
			Check(!(bool)Get("ControlsEnabled"), "disabled preference persisted on reload");
			report.Add("PASS: Custom starts as A; exact-chord duplicates reassign/unbind only the old slot; different modifiers coexist; explicit unbind; all 16 custom slots survive A/B/C switching and cold managed-cache reload; ON/OFF and selected preset persist");
		}
		finally
		{
			saved.Restore();
			Reload();
		}
		report.Add("PASS: original profile preferences restored, including absent keys, and feature cache reloaded");
		TestRuntimeHelpers(report);
		row = Fixture();
		EventDelegate.Execute(row.Button.onClick);
		Type panelType = keyboard.Assembly.GetType("SprkBattleKeyboardPanel", true);
		var editor = (MonoBehaviour)panelType.GetField("Current", Static).GetValue(null);
		Check(editor != null && editor.gameObject.activeInHierarchy, "row UIButton callback did not open editor");
		Check((bool)Get("SettingsOpen"), "editor must suppress battle keyboard input");
		Check(editor.GetComponentsInChildren<UILabel>(true).Any(l => l.text == "Hero Skill Keyboard Controls"), "editor title missing");
		var bindingLabels = (UILabel[])panelType.GetField("bindingLabels", Instance).GetValue(editor);
		Check(bindingLabels.Length == 16, "editor must have 16 binding labels");
		for (int slot = 0; slot < 16; slot++)
			Check(bindingLabels[slot] != null && bindingLabels[slot].gameObject.activeInHierarchy &&
				bindingLabels[slot].text == (string)Call("GetBindingLabel", slot / 4, slot % 4), "editor binding label " + slot);
		report.Add("PASS: eight Setup refreshes keep one visible native row and one UIButton callback; native row callback opens active keyboard editor; SettingsOpen suppresses input. Editor left open with restored preferences for a subsequent screenshot command.");
		TestEditorBack(editor, panelType, report);
		report.Add("PASS: battle-keyboard regression complete: default OFF, 48 exact preset bindings, custom persistence/duplicates/unbinding/reload, preference restoration, native row/editor and 16 displayed binding labels; synchronous runtime helper and capture/back results listed above (see any SKIP entries)");
		report.Add("CONCERNS: reload resets the feature's managed cache and rereads saved PlayerPrefs, not a process restart. Runtime fixtures test private helpers and native click delegation in isolation, not LateUpdate, live HeroParty dictionary population, battle lifecycle/dead-party guards, disabled-slot battle synchronization, or actual skill execution. No battle manager/cache is created or mutated and no native skill.Open is called. Widget rejection is tested synchronously; positive rendering, clipping, occlusion, artwork and pointer hit testing still need the screenshot/live client. One direct native back call does not reproduce OS Escape down/up timing or the former double-dispatch bug; RestoredPortal shortcut suppression and real focus/modifier/key edges remain live-input checks. Full account-dependent settings.Open is bypassed; installed-asset preparation is process-local fixture setup and is not reverted.");
		return report;
	}

	private static void TestRuntimeHelpers(List<object> report)
	{
		Type input = keyboard.Assembly.GetType("SprkBattleKeyboardInput", true);
		GameObject root = InactiveObject("BattleKeyboard Runtime Probe", null);
		GameObject visual = null;
		try
		{
			var slot = InactiveObject("Action Slot", root.transform).AddComponent<PartySlotComponent2>();
			var normals = new NormalSkillComponent[3];
			int[] nativeSlots = { 0, 4, 9 };
			for (int i = 0; i < normals.Length; i++)
			{
				normals[i] = InactiveObject("Normal " + i, root.transform).AddComponent<NormalSkillComponent>();
				typeof(BaseSkillComponent).GetField("<SkillSlotIndex>k__BackingField", Instance).SetValue(normals[i], nativeSlots[i]);
			}
			var soul = InactiveObject("Soul Weapon", root.transform).AddComponent<SoulWeaponSkillComponent>();
			typeof(BaseSkillComponent).GetField("<SkillSlotIndex>k__BackingField", Instance).SetValue(soul, 12);
			var skills = new List<BaseSkillComponent> { normals[0], soul, null, normals[1], normals[2] };
			typeof(PartySlotComponent2).GetField("_skillList", Instance).SetValue(slot, skills);
			MethodInfo find = input.GetMethod("FindAction", Static);
			for (int i = 0; i < 3; i++)
				Check(ReferenceEquals(find.Invoke(null, new object[] { slot, i }), normals[i]), "FindAction normal order " + i);
			Check(ReferenceEquals(find.Invoke(null, new object[] { slot, 3 }), soul), "FindAction Soul Weapon with interleaved normals");
			skills.Clear();
			skills.Add(soul);
			for (int i = 0; i < 3; i++)
				Check(find.Invoke(null, new object[] { slot, i }) == null, "missing normal must not resolve Soul Weapon " + i);
			Check(ReferenceEquals(find.Invoke(null, new object[] { slot, 3 }), soul), "Soul Weapon independent of missing normals");
			skills.Clear();
			Check(find.Invoke(null, new object[] { slot, 3 }) == null, "missing Soul Weapon must return null");
			report.Add("PASS: runtime FindAction follows normal component/display-list order for native slots 0/4/9, ignores interleaved Soul Weapon/null, and resolves Soul Weapon independently with all normals missing; empty actions return null");

			foreach (BaseSkillComponent skill in normals.Cast<BaseSkillComponent>().Concat(new[] { soul })) TestNativeClick(skill);
			report.Add("PASS: native OnClickSkill on three NormalSkillComponents and SoulWeaponSkillComponent delegates the fixture creature uid/native slot exactly once; locked SkillInfo blocks the callback and reservation flag; no Open or battle context used");
			TestPosition(input, root.transform, report);

			// Combat objects above never become active. Only this separate, empty
			// NGUI hierarchy is enabled briefly to exercise native visibility guards.
			visual = InactiveObject("BattleKeyboard Visibility Probe", null);
			var panel = visual.AddComponent<UIPanel>();
			var widgetObject = InactiveObject("Widget", visual.transform);
			var widget = widgetObject.AddComponent<UIWidget>();
			MethodInfo panelVisible = input.GetMethod("Visible", Static, null, new[] { typeof(UIPanel) }, null);
			MethodInfo widgetVisible = input.GetMethod("Visible", Static, null, new[] { typeof(UIWidget) }, null);
			Check(!Visible(panelVisible, null) && !Visible(widgetVisible, null), "Visible rejects null panel/widget");
			Check(!Visible(panelVisible, panel) && !Visible(widgetVisible, widget), "Visible rejects inactive panel/widget");
			visual.SetActive(true);
			panel.alpha = 1f;
			Check(Visible(panelVisible, panel), "active standalone opaque panel should pass Visible");
			panel.enabled = false;
			Check(!Visible(panelVisible, panel), "Visible rejects disabled panel");
			panel.enabled = true;
			panel.alpha = 0f;
			Check(panel.CalculateFinalAlpha(Time.frameCount) == 0f && !Visible(panelVisible, panel), "Visible rejects alpha-zero panel");
			panel.alpha = 1f;
			widgetObject.SetActive(true);
			widget.alpha = 1f;
			widget.Invalidate(true);
			if (Visible(widgetVisible, widget))
				report.Add("PASS: native opaque active UIWidget currently passes Visible (not proof of pixels rendered)");
			else
				report.Add("SKIP: positive rendered UIWidget visibility is not guaranteed before an NGUI render/update; no visible baseline is fabricated");
			widget.enabled = false;
			Check(!Visible(widgetVisible, widget), "Visible rejects disabled widget");
			widget.enabled = true;
			widget.alpha = 0f;
			widget.Invalidate(true);
			Check(widget.CalculateFinalAlpha(Time.frameCount) == 0f && !Visible(widgetVisible, widget), "Visible rejects alpha-zero widget");
			widget.alpha = 1f;
			widgetObject.SetActive(false);
			Check(!Visible(widgetVisible, widget), "Visible rejects inactive widget with active panel");
			visual.SetActive(false);
			Check(!Visible(panelVisible, panel), "Visible rejects inactive panel after activation");
			report.Add("PASS: native Visible overloads reject null, inactive, disabled and alpha-zero UIPanel/UIWidget; opaque standalone panel passes; widget rendering baseline reported separately");
		}
		finally
		{
			if (visual != null) { visual.SetActive(false); UnityEngine.Object.Destroy(visual); }
			root.SetActive(false);
			UnityEngine.Object.Destroy(root);
		}
	}

	private static GameObject InactiveObject(string name, Transform parent)
	{
		var go = new GameObject(name);
		go.SetActive(false);
		if (parent != null) go.transform.SetParent(parent, false);
		return go;
	}

	private static bool Visible(MethodInfo method, object component) => (bool)method.Invoke(null, new[] { component });

	private static void TestNativeClick(BaseSkillComponent skill)
	{
		const int creatureUid = 73129;
		int expectedSlot = skill.SkillSlotIndex;
		int calls = 0, actualUid = 0, actualSlot = -1;
		BaseSkillComponent.ClickIconCallback callback = (uid, index) => { calls++; actualUid = uid; actualSlot = index; };
		Type type = typeof(BaseSkillComponent);
		type.GetField("_creatureUid", Instance).SetValue(skill, creatureUid);
		type.GetField("_onClickAction", Instance).SetValue(skill, callback);
		FieldInfo info = type.GetField("<SkillInfo>k__BackingField", Instance);
		info.SetValue(skill, new NGame2.NAccount.Helper.SkillBasicInfo { IsLocked = false });
		skill.OnClickSkill();
		Check(calls == 1 && actualUid == creatureUid && actualSlot == expectedSlot, "native click callback uid/slot/count " + skill.name);
		Check((bool)type.GetField("_isReservedActive", Instance).GetValue(skill), "native click sets reservation flag " + skill.name);
		type.GetField("_isReservedActive", Instance).SetValue(skill, false);
		info.SetValue(skill, new NGame2.NAccount.Helper.SkillBasicInfo { IsLocked = true });
		skill.OnClickSkill();
		Check(calls == 1 && !(bool)type.GetField("_isReservedActive", Instance).GetValue(skill), "locked skill invoked callback/reservation " + skill.name);
		type.GetField("_onClickAction", Instance).SetValue(skill, null);
	}

	private static void TestPosition(Type input, Transform parent, List<object> report)
	{
		int layer = window.gameObject.layer;
		Camera camera = NGUITools.FindCameraForLayer(layer);
		if (camera == null || camera.pixelRect.width <= 0f || camera.pixelRect.height <= 0f)
		{
			report.Add("SKIP: ComparePosition screen-space fixture has no usable native UI camera; world-X fallback is not claimed as screen-space coverage");
			return;
		}
		var ordered = new PartySlotComponent2[3];
		float distance = Mathf.Max(camera.nearClipPlane + 1f, 10f);
		for (int i = 0; i < ordered.Length; i++)
		{
			var go = InactiveObject("Position " + i, parent);
			go.layer = layer;
			go.transform.position = camera.ViewportToWorldPoint(new Vector3(0.25f + i * 0.25f, 0.5f, distance));
			ordered[i] = go.AddComponent<PartySlotComponent2>();
		}
		MethodInfo compare = input.GetMethod("ComparePosition", Static);
		var dictionary = new Dictionary<int, PartySlotComponent2> { { 10, ordered[2] }, { 20, ordered[0] }, { 30, ordered[1] } };
		var shuffled = dictionary.OrderBy(entry => entry.Key).Select(entry => entry.Value).ToList();
		Check(!shuffled.SequenceEqual(ordered), "position fixture must start outside screen order");
		for (int left = 0; left < 3; left++)
			for (int right = 0; right < 3; right++)
			{
				float x1 = camera.WorldToScreenPoint(ordered[left].transform.position).x;
				float x2 = camera.WorldToScreenPoint(ordered[right].transform.position).x;
				int actual = (int)compare.Invoke(null, new object[] { ordered[left], ordered[right] });
				Check(Math.Sign(actual) == Math.Sign(x1.CompareTo(x2)), "ComparePosition projected X " + left + "/" + right);
			}
		shuffled.Sort((left, right) => (int)compare.Invoke(null, new object[] { left, right }));
		Check(shuffled.SequenceEqual(ordered), "ComparePosition must replace dictionary/key order with screen order");
		report.Add("PASS: ComparePosition matches native camera screen-X comparisons (including equality/reverse) and sorts three inactive slot fixtures out of dictionary/key order; camera=" + camera.name);
	}

	private static void TestEditorBack(MonoBehaviour editor, Type panelType, List<object> report)
	{
		var saved = new Preferences();
		try
		{
			Set("SelectedPreset", 3);
			string[] before = Enumerable.Range(0, 16).Select(slot => (string)Call("GetBindingLabel", slot / 4, slot % 4)).ToArray();
			bool hadCustom = SprkTestPrefs.HasKey(Prefix + "Custom.v1");
			string custom = SprkTestPrefs.GetString(Prefix + "Custom.v1");
			panelType.GetMethod("BeginCapture", Instance).Invoke(editor, new object[] { 0 });
			Check((int)panelType.GetField("captureSlot", Instance).GetValue(editor) == 0, "editor BeginCapture must enter capture");
			window.OnSystemBackButton();
			Check((int)panelType.GetField("captureSlot", Instance).GetValue(editor) == -1, "native back must cancel capture");
			Check(editor.gameObject.activeInHierarchy && window.IsOpen() &&
				ReferenceEquals(panelType.GetField("Current", Static).GetValue(null), editor), "one native back must not close editor/window while capturing");
			Check(before.SequenceEqual(Enumerable.Range(0, 16).Select(slot => (string)Call("GetBindingLabel", slot / 4, slot % 4))) &&
				SprkTestPrefs.HasKey(Prefix + "Custom.v1") == hadCustom && SprkTestPrefs.GetString(Prefix + "Custom.v1") == custom,
				"capture/back must leave all bindings and saved custom data unchanged");
		}
		finally
		{
			saved.Restore();
			Reload();
			panelType.GetMethod("Sync", Instance).Invoke(editor, null);
		}
		report.Add("PASS: BeginCapture then exactly one window.OnSystemBackButton cancels capture, keeps the same editor/window active, and leaves all 16 bindings/custom preference unchanged; original preset/preferences restored and editor resynced");
	}

	private static GameOptionButtonComponent Fixture()
	{
		NVespa.SingletonAuto<NClientData.LocalizationLoader>.instance.SetEnableLoadFromPatch(true);
		if (window == null)
		{
			var manager = SceneAutoBehaviour<LoginManagement>.instance;
			Check(manager != null, "native LoginManagement missing");
			Check(manager.GetInstance<GameOptionWindow>() == null, "refusing to reuse a non-probe settings window");
			manager.AddInstance<GameOptionWindow>(GameOptionWindow.PrefabPath);
			window = manager.GetInstance<GameOptionWindow>();
			Check(window != null, "native GameOptionWindow prefab missing");
			// Only establish the native visual fixture. Open/ReOpen builds unrelated
			// account-dependent options and must not run at isolated Login.
			window.enabled = false;
			window.gameObject.SetActive(true);
			window.ShowRoot(true);
			typeof(BaseWindow).GetField("_windowState", Instance).SetValue(window, BaseWindow.EState.OpenedShow);
			var battle = window.MainTabs.Single(t => t.GameOptionTab == GameOptionTab.Battle);
			typeof(GameOptionWindow).GetField("CurrentTab", Instance).SetValue(window, battle);
			window.SetWidgetRoot();
			foreach (var panel in window.GetComponentsInChildren<UIPanel>(true)) panel.alpha = 1f;
		}
		Type editorType = keyboard.Assembly.GetType("SprkBattleKeyboardPanel", true);
		var editor = (MonoBehaviour)editorType.GetField("Current", Static).GetValue(null);
		if (editor != null) editorType.GetMethod("Close", Instance).Invoke(editor, null);
		for (int i = 0; i < 8; i++) Call("Setup", window);
		var rows = window.GetComponentsInChildren<GameOptionButtonComponent>(true).Where(r => r.name == RowName).ToArray();
		Check(rows.Length == 1, "Setup duplicated/missed keyboard row: " + rows.Length);
		var row = rows[0];
		Check(row.gameObject.activeInHierarchy && row.Button != null && row.Button.gameObject.activeInHierarchy, "keyboard row/button hidden");
		Check(row.Button.onClick.Count == 1, "keyboard row must have exactly one callback");
		Check(row.ParentRoot != null && row.ParentRoot.Label_OptionName != null &&
			row.ParentRoot.Label_OptionName.text == "Hero Skill Keyboard Controls", "keyboard row caption");
		Check(row.Label_Button != null && row.Label_Button.text == ((bool)Get("ControlsEnabled") ? "On - Configure" : "Off - Configure"), "keyboard row enabled caption");
		Check(NGUITools.FindInParents<UIRoot>(window.gameObject) != null, "native settings UIRoot missing");
		return row;
	}

	private static object Call(string name, params object[] args) => keyboard.GetMethod(name, Static).Invoke(null, args);
	private static object Get(string name) => keyboard.GetProperty(name, Static).GetValue(null, null);
	private static void Set(string name, object value) => keyboard.GetProperty(name, Static).SetValue(null, value, null);
	private static object NewBinding(KeyCode key, int modifiers) => Activator.CreateInstance(binding, Instance, null, new object[] { key, modifiers }, null);
	private static void Assign(int slot, KeyCode key, int modifiers) => Call("AssignCustom", slot, NewBinding(key, modifiers));

	private static void Reload()
	{
		keyboard.GetField("loaded", Static).SetValue(null, false);
		keyboard.GetField("custom", Static).SetValue(null, null);
		Call("EnsureLoaded");
	}

	private static void CheckBinding(int slot, KeyCode key, int modifiers, string context)
	{
		object value = Call("GetBinding", slot);
		Check((KeyCode)binding.GetField("Key", Instance).GetValue(value) == key &&
			(int)binding.GetField("Modifiers", Instance).GetValue(value) == modifiers,
			context + " slot " + slot + " expected " + key + ":" + modifiers + " got " + Call("GetBindingLabel", slot / 4, slot % 4));
	}

	private static void CheckCustom(KeyCode[] keys, int[] modifiers)
	{
		Check((int)Get("SelectedPreset") == 3, "expected Custom selected");
		for (int slot = 0; slot < 16; slot++) CheckBinding(slot, keys[slot], modifiers[slot], "custom persistence");
	}

	private static void Check(bool condition, string message)
	{
		if (!condition) throw new InvalidOperationException("FAIL battle-keyboard: " + message);
	}

	private sealed class Preferences
	{
		private readonly string[] keys = { Prefix + "Enabled", Prefix + "Preset", Prefix + "Custom.v1" };
		private readonly bool[] present;
		private readonly int enabled;
		private readonly int preset;
		private readonly string custom;

		internal Preferences()
		{
			present = keys.Select(SprkTestPrefs.HasKey).ToArray();
			enabled = SprkTestPrefs.GetInt(keys[0]);
			preset = SprkTestPrefs.GetInt(keys[1]);
			custom = SprkTestPrefs.GetString(keys[2]);
		}

		internal void Restore()
		{
			SprkTestPrefs.SetInt(keys[0], enabled);
			SprkTestPrefs.SetInt(keys[1], preset);
			SprkTestPrefs.SetString(keys[2], custom);
			for (int i = 0; i < keys.Length; i++) if (!present[i]) SprkTestPrefs.DeleteKey(keys[i]);
			SprkTestPrefs.Save();
			for (int i = 0; i < keys.Length; i++) Check(SprkTestPrefs.HasKey(keys[i]) == present[i], "restore preference presence " + keys[i]);
			Check(SprkTestPrefs.GetInt(keys[0]) == enabled && SprkTestPrefs.GetInt(keys[1]) == preset &&
				SprkTestPrefs.GetString(keys[2]) == custom, "restore original preference values");
		}
	}
}
