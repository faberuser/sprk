using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUI.NWindow;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

/// Automation probe for Technomagic "reforge until": renders the Technomagic
/// Reforge window from its prefab on the Login screen (through the
/// ticket-search probe's `show:` mode), fills its stat panels with fixture rows
/// and drives the hooks.
public static class SprkTmReforgeUntilProbe
{
	private static ValanceOptionRenewWindow window;

	private static StatGaugeComponent stats;

	private static StatGaugeComponent skills;

	private const int FixtureSlot = 987655;

	// An identified Technomagic weapon, as used by the server tests.
	private const int FixtureItem = 911301;

	private static EquipItemInfo item;

	private static void Check(bool value, string message)
	{
		if (!value)
		{
			throw new Exception("TM reforge until: " + message);
		}
	}

	private static StatGaugeComponent Create(PrefabAuto<StatGaugeComponent> prefab, string path, Transform parent)
	{
		StatGaugeComponent component = null;
		try
		{
			component = prefab.Factory.Create(parent);
		}
		catch (Exception)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(VespaResource.Load<GameObject>(path), parent, worldPositionStays: false);
			component = gameObject.GetComponent<StatGaugeComponent>();
		}
		component.transform.localPosition = Vector3.zero;
		component.transform.localRotation = Quaternion.identity;
		component.transform.localScale = Vector3.one;
		return component;
	}

	private static string Show()
	{
		string text = (string)Assembly.Load("SprkTicketSearch").GetType("SprkTicketSearchProbe").GetMethod("Run")
			.Invoke(null, new object[1] { "show:ValanceOptionRenewWindow" });
		GameObject gameObject = GameObject.Find("TicketSearchShow_ValanceOptionRenewWindow");
		Check(gameObject != null, "window not shown: " + text);
		foreach (UITweener tweener in gameObject.GetComponentsInChildren<UITweener>(includeInactive: true))
		{
			tweener.enabled = false;
		}
		foreach (UIPanel panel in gameObject.GetComponentsInChildren<UIPanel>(includeInactive: true))
		{
			panel.alpha = 1f;
		}
		window = gameObject.GetComponent<ValanceOptionRenewWindow>();
		Check(window != null, "no ValanceOptionRenewWindow");
		// The fixture's two lines are the first two stats of its pool.
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(FixtureItem);
		Check(data != null, "no item data for " + FixtureItem);
		List<EquipOptionData> pool = new List<EquipOptionData>();
		List<int> indices = new List<int>(data.SubData.OptionIndex ?? new int[0]);
		foreach (int groupIndex in data.SubData.OptionGroupIndex ?? new int[0])
		{
			EquipOptionGroupData group = GameRoot.Instance.EquipOptionGroupDataContainer.GetData(groupIndex);
			if (group != null && group.OptionIndex != null)
			{
				indices.AddRange(group.OptionIndex);
			}
		}
		foreach (int index in indices)
		{
			EquipOptionData option = GameRoot.Instance.EquipOptionDataContainer.GetData(index);
			if (option != null && option.Ratio > 0 && !pool.Exists((EquipOptionData o) => o.OptionType == option.OptionType))
			{
				pool.Add(option);
			}
		}
		Check(pool.Count >= 4, "fixture pool too small: " + pool.Count);
		item = new EquipItemInfo
		{
			SlotIndex = FixtureSlot,
			ItemIndex = FixtureItem,
			Star = 5,
			Identified = 1,
			OptionIndex1 = pool[0].Index,
			OptionStep1 = 1,
			OptionIndex2 = pool[1].Index,
			OptionStep2 = pool[1].OptionValueStep
		};
		typeof(ValanceEnhanceBase).GetProperty("EquipItemInfo").SetValue(window, item, null);
		stats = Create(StatGaugeComponent.ValanceButtonPrefabAuto, StatGaugeComponent.ValanceButtonPrefabPath, window.EquipOptionTable.transform);
		stats.SetTitle("Additional Options");
		for (int i = 0; i < 2; i++)
		{
			stats.AddStat(pool[i].OptionType, pool[i].GetOptionValue(5, (i == 0) ? 1 : pool[i].OptionValueStep), pool[i].GetOptionValueMax(5), 1, 4, "/", delegate { });
		}
		skills = Create(StatGaugeComponent.ValanceSkillButtonPrefabAuto, StatGaugeComponent.ValanceButtonPrefabPath2, window.EquipOptionTable.transform);
		skills.SetTitle("Additional Skill");
		skills.AddExtraSkill("Level of [3rd Skill] +7", delegate { });
		window.EquipOptionTable.Reposition();
		// The material icons and counts, laid out like the window's own (largest counts seen in game).
		EquipItemDetailData detail = GameRoot.Instance.EquipItemDetailDataContainer.GetData(data.SubData.DetailIndex);
		Check(detail != null, "no detail data");
		int[] materialItems = new int[2] { detail.RenewItemIndex1, detail.RenewItemIndex2 };
		Transform[] roots = new Transform[2] { window.MaterialItemRoot1, window.MaterialItemRoot2 };
		UILabel[] labels = new UILabel[2] { window.LabelReqMaterialCount1, window.LabelReqMaterialCount2 };
		string[] counts = new string[2] { "9776 / 1", "5004790 / 300" };
		for (int i = 0; i < 2; i++)
		{
			ItemData material = GameRoot.Instance.ItemDataContainer.GetData(materialItems[i]);
			if (material != null && roots[i].GetComponentInChildren<ItemIconComponent>() == null)
			{
				ItemIconComponent icon = ItemIconComponent.PrefabAuto.Factory.Create(roots[i]);
				icon.transform.localPosition = Vector3.zero;
				icon.transform.localScale = Vector3.one;
				try
				{
					icon.Open(material);
				}
				catch (NullReferenceException)
				{
					// The fixture account has no item counts; the icon's frame is what the layout needs.
				}
			}
			UIHelper.SetLabelText(labels[i], counts[i]);
		}
		UIHelper.EnableGameObject(window.MaterialWidget, true);
		UIHelper.EnableGameObject(window.LabelEmptyTarget, false);
		// Natively disabled until a line is selected.
		UIHelper.EnableButton(window.Button_Renew, false);
		SprkReforgeUntil.SkipExplanations();
		SprkReforgeUntil.TechnomagicBuilt(window, stats, skills);
		return text;
	}

	private static Bounds BoundsOf(Transform t)
	{
		return NGUIMath.CalculateRelativeWidgetBounds(window.transform, t, considerInactive: false);
	}

	private static bool Overlap(Bounds a, Bounds b)
	{
		return a.min.x < b.max.x - 1f && b.min.x < a.max.x - 1f && a.min.y < b.max.y - 1f && b.min.y < a.max.y - 1f;
	}

	private static string ButtonText()
	{
		UILabel label = SprkReforgeUntil.TechnomagicButton.GetComponentInChildren<UILabel>(includeInactive: true);
		return (label == null) ? null : label.text;
	}

	private static OperationType TypeOf(StatGaugeInfo row)
	{
		return (row != null && row.StatType is OperationType) ? (OperationType)row.StatType : OperationType.None;
	}

	private static StatGaugeInfo PoolRow(OperationType type)
	{
		StatGaugeComponent pool = SprkReforgeUntil.TechnomagicPool;
		for (int i = 0; pool != null; i++)
		{
			StatGaugeInfo row = pool.GetStatGaugeInfo(i);
			if (row == null || TypeOf(row) == type)
			{
				return row;
			}
		}
		return null;
	}

	/// The first pool stat that is on neither fixture line.
	private static OperationType FreeStat()
	{
		StatGaugeComponent pool = SprkReforgeUntil.TechnomagicPool;
		for (int i = 0; pool != null; i++)
		{
			StatGaugeInfo row = pool.GetStatGaugeInfo(i);
			Check(row != null, "no free stat");
			if (TypeOf(row) != TypeOf(stats.GetStatGaugeInfo(0)) && TypeOf(row) != TypeOf(stats.GetStatGaugeInfo(1)))
			{
				return TypeOf(row);
			}
		}
		return OperationType.None;
	}

	private static string Test()
	{
		StringBuilder sb = new StringBuilder();
		Show();
		UIButton target = SprkReforgeUntil.TechnomagicButton;
		Check(target != null && target.gameObject.activeInHierarchy, "no Target Stat button");
		Check(target.isEnabled && target.GetComponent<Collider>().enabled, "Target Stat button disabled like Change Option");
		Check(ButtonText() == "Target Stat", "button text " + ButtonText());
		Check(target.onClick.Count == 1, "button keeps Change Option's handler");
		Bounds renew = BoundsOf(window.Button_Renew.transform);
		Bounds button = BoundsOf(target.transform);
		sb.Append("button=").Append(B(button)).Append(" renew=").Append(B(renew));
		Check(!Overlap(button, renew), "buttons overlap");
		// The material container is wider than its contents; check the icons and counts.
		foreach (Transform part in new Transform[4] { window.MaterialItemRoot1, window.MaterialItemRoot2, window.LabelReqMaterialCount1.transform, window.LabelReqMaterialCount2.transform })
		{
			Bounds bounds = BoundsOf(part);
			UILabel label = part.GetComponent<UILabel>();
			if (label != null)
			{
				// The printed text, not the label box.
				Vector2 size = label.printedSize;
				Vector2 offset = label.pivotOffset;
				Vector3 min = window.transform.InverseTransformPoint(label.transform.TransformPoint(new Vector3(-offset.x * size.x, -offset.y * size.y, 0f)));
				Vector3 max = window.transform.InverseTransformPoint(label.transform.TransformPoint(new Vector3((1f - offset.x) * size.x, (1f - offset.y) * size.y, 0f)));
				bounds = new Bounds((min + max) / 2f, new Vector3(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y), 0f));
			}
			sb.Append(' ').Append(part.name).Append('=').Append(B(bounds));
			Check(bounds.size.x > 0f, part.name + " empty");
			Check(!Overlap(button, bounds), "button overlaps " + part.name + " " + B(bounds));
		}
		// Stacked on Change Option: same column and size, just above it.
		Check(Mathf.Abs(button.center.x - renew.center.x) < 1f && Mathf.Abs(button.size.x - renew.size.x) < 1f && Mathf.Abs(button.size.y - renew.size.y) < 1f, "button not in Change Option's column " + B(button) + " " + B(renew));
		Check(button.min.y >= renew.max.y && button.min.y - renew.max.y < 40f, "button not just above Change Option " + B(button) + " " + B(renew));
		Check(!Overlap(button, BoundsOf(skills.transform)) && !Overlap(button, BoundsOf(stats.transform)), "button overlaps the option panels");
		// Lit like an enabled Change Option, although Change Option itself is disabled (gray) here.
		UIWidget face = target.GetComponent<UIWidget>();
		Check(target.defaultColor == window.Button_Renew.defaultColor, "button default color " + target.defaultColor + " vs " + window.Button_Renew.defaultColor);
		Check(face != null && face.color == window.Button_Renew.defaultColor, "button face color " + (face == null ? "none" : face.color.ToString()));
		// This button shows "disabled" by swapping its sprite, not by tinting.
		UISprite faceSprite = face as UISprite;
		UISprite renewSprite = window.Button_Renew.GetComponent<UISprite>();
		Check(target.normalSprite == window.Button_Renew.normalSprite && faceSprite != null && faceSprite.spriteName == window.Button_Renew.normalSprite, "button sprite " + (faceSprite == null ? "none" : faceSprite.spriteName) + " vs " + window.Button_Renew.normalSprite);
		Check(renewSprite != null && renewSprite.spriteName != faceSprite.spriteName, "fixture Change Option should show its disabled sprite, has " + (renewSprite == null ? "none" : renewSprite.spriteName));
		sb.Append(" sprite=").Append(faceSprite.spriteName).Append(" renew=").Append(renewSprite.spriteName);
		Check(!window.Button_Renew.isEnabled, "fixture Change Option should be disabled");
		sb.Append(" face=").Append(face.color.ToString("0.00")).Append(" renewDisabled=").Append(window.Button_Renew.GetComponent<UIWidget>().color.ToString("0.00"));
		int pool = SprkReforgeUntil.PoolCount;
		Check(pool >= 4, "pool " + pool);
		Bounds statsTitle = BoundsOf(stats.Label_Title.transform);
		Bounds skillsBounds = BoundsOf(skills.transform);
		// Pool view: a "Stat Pool" panel in the option list; the item's panels hidden.
		SprkReforgeUntil.ToggleTargetView();
		StatGaugeComponent poolPanel = SprkReforgeUntil.TechnomagicPool;
		Check(poolPanel != null && poolPanel.gameObject.activeInHierarchy, "pool not shown");
		Check(poolPanel.transform.parent == window.EquipOptionTable.transform, "pool outside the option list");
		Check(!stats.gameObject.activeSelf && !skills.gameObject.activeSelf, "item panels still shown");
		Check(poolPanel.Label_Title.text == "Stat Pool", "pool title " + poolPanel.Label_Title.text);
		Check(ButtonText() == "Back", "pool button text " + ButtonText());
		Check(poolPanel.GetStatGaugeInfo(pool - 1) != null && poolPanel.GetStatGaugeInfo(pool) == null, "pool rows != " + pool);
		Bounds title = BoundsOf(poolPanel.Label_Title.transform);
		Bounds first = BoundsOf(poolPanel.GetStatGaugeInfo(0).transform);
		Check(Mathf.Abs(title.max.y - statsTitle.max.y) < 2f, "pool title moved " + B(title) + " " + B(statsTitle));
		Check(first.max.y <= title.min.y + 1f, "first row overlaps the title " + B(first) + " " + B(title));
		UIScrollView scrollView = NGUITools.FindInParents<UIScrollView>(window.EquipOptionTable.gameObject);
		Collider rowCollider = poolPanel.GetStatGaugeInfo(pool - 1).GetComponentInChildren<Collider>();
		Check(rowCollider != null && rowCollider.GetComponent<UIDragScrollView>() != null && rowCollider.GetComponent<UIDragScrollView>().scrollView == scrollView, "rows do not drag the list");
		sb.Append(" pool=").Append(pool).Append(" rows=").Append(B(BoundsOf(poolPanel.transform))).Append(" clip=").Append(scrollView.panel.finalClipRegion.ToString("0"));
		// Picking a stat the item lacks makes it the target and closes the pool.
		OperationType wanted = FreeStat();
		PoolRow(wanted).OnClickSelect();
		Check(SprkReforgeUntil.Target == wanted && SprkReforgeUntil.IsTechnomagicArmed(FixtureSlot), "target not armed");
		Check(!SprkReforgeUntil.IsTechnomagicArmed(FixtureSlot + 1), "other items armed");
		Check(SprkReforgeUntil.TechnomagicPool == null && stats.gameObject.activeSelf && skills.gameObject.activeSelf, "pool not closed");
		string armed = ButtonText();
		Check(armed == "Target: " + SprkReforgeUntil.StatName(wanted), "armed text " + armed);
		// The pool reopens with the target selected; picking it again clears it.
		SprkReforgeUntil.ToggleTargetView();
		StatGaugeInfo wantedRow = PoolRow(wanted);
		Check(wantedRow.StateRoot != null && Array.Exists(wantedRow.StateRoot, (StatGaugeInfo.StateRootInfo s) => s.Type == StatGaugeInfo.StateType.Selected && s.Root != null && s.Root.gameObject.activeSelf), "target row not selected");
		wantedRow.OnClickSelect();
		Check(SprkReforgeUntil.Target == OperationType.None && !SprkReforgeUntil.IsTechnomagicArmed(FixtureSlot), "target not cleared");
		Check(ButtonText() == "Target Stat", "cleared text " + ButtonText());
		// "Back" closes the pool without changing anything.
		SprkReforgeUntil.ToggleTargetView();
		SprkReforgeUntil.ToggleTargetView();
		Check(SprkReforgeUntil.TechnomagicPool == null && stats.gameObject.activeSelf && SprkReforgeUntil.Target == OperationType.None, "back did not close the pool");
		Bounds statsTitle2 = BoundsOf(stats.Label_Title.transform);
		Check(Vector3.Distance(statsTitle2.center, statsTitle.center) < 0.5f, "native panel moved " + B(statsTitle2) + " " + B(statsTitle));
		Check(Vector3.Distance(BoundsOf(skills.transform).center, skillsBounds.center) < 0.5f, "skill panel moved");
		// The window rebuilding its panels hands them back visible and keeps the target for the same item.
		SprkReforgeUntil.ToggleTargetView();
		PoolRow(wanted).OnClickSelect();
		SprkReforgeUntil.ToggleTargetView();
		SprkReforgeUntil.TechnomagicReleasing(window);
		Check(SprkReforgeUntil.TechnomagicPool == null && stats.gameObject.activeSelf && skills.gameObject.activeSelf, "released panels hidden");
		Check(!target.gameObject.activeSelf, "button shown without panels");
		SprkReforgeUntil.TechnomagicBuilt(window, stats, skills);
		Check(SprkReforgeUntil.IsTechnomagicArmed(FixtureSlot) && target.gameObject.activeSelf, "target lost on rebuild");
		// Another item drops it.
		item.SlotIndex = FixtureSlot + 1;
		SprkReforgeUntil.TechnomagicBuilt(window, stats, skills);
		Check(SprkReforgeUntil.Target == OperationType.None, "target kept for another item");
		item.SlotIndex = FixtureSlot;
		SprkReforgeUntil.TechnomagicBuilt(window, stats, skills);
		// Additional skills: a "Skill Pool" panel below the stat pool; picking a skill targets it.
		int skillCount = SprkReforgeUntil.SkillPoolCount;
		Check(skillCount >= 2, "skill pool " + skillCount);
		SprkReforgeUntil.ToggleTargetView();
		PoolRow(wanted).OnClickSelect();
		Check(SprkReforgeUntil.IsTechnomagicArmed(FixtureSlot), "stat target before skills");
		SprkReforgeUntil.ToggleTargetView();
		StatGaugeComponent skillPool = SprkReforgeUntil.TechnomagicSkillPool;
		Check(skillPool != null && skillPool.gameObject.activeInHierarchy && skillPool.transform.parent == window.EquipOptionTable.transform, "skill pool not shown in the option list");
		Check(skillPool.Label_Title.text == "Skill Pool", "skill pool title " + skillPool.Label_Title.text);
		Check(skillPool.GetStatGaugeInfo(skillCount - 1) != null && skillPool.GetStatGaugeInfo(skillCount) == null, "skill rows != " + skillCount);
		Bounds statPoolBounds = BoundsOf(SprkReforgeUntil.TechnomagicPool.transform);
		Bounds skillTitle = BoundsOf(skillPool.Label_Title.transform);
		Check(skillTitle.max.y <= statPoolBounds.min.y + 1f, "skill pool not below the stat pool " + B(skillTitle) + " " + B(statPoolBounds));
		Check(!skills.gameObject.activeSelf, "item skill panel still shown");
		Collider skillCollider = skillPool.GetStatGaugeInfo(skillCount - 1).GetComponentInChildren<Collider>();
		Check(skillCollider != null && skillCollider.GetComponent<UIDragScrollView>() != null, "skill rows do not drag the list");
		UILabel skillLabel = skillPool.GetStatGaugeInfo(1).GetComponentInChildren<UILabel>(includeInactive: true);
		string skillText = skillLabel == null ? "" : skillLabel.text;
		skillPool.GetStatGaugeInfo(1).OnClickSelect();
		int skillTarget = SprkReforgeUntil.SkillTarget;
		Check(skillTarget != 0 && SprkReforgeUntil.IsTechnomagicSkillArmed(FixtureSlot) && !SprkReforgeUntil.IsTechnomagicSkillArmed(FixtureSlot + 1), "skill target not armed");
		Check(SprkReforgeUntil.IsTechnomagicArmed(FixtureSlot), "skill pick dropped the stat target");
		Check(SprkReforgeUntil.TechnomagicSkillPool == null && skills.gameObject.activeSelf, "skill pool not closed");
		Check(ButtonText() == "Targets: 2", "both-targets text " + ButtonText());
		sb.Append(" skills=").Append(skillCount).Append(" skillTarget=").Append(SprkReforgeUntil.SkillName(skillTarget));
		// The skill target alone names the skill; it survives a rebuild for the same item.
		SprkReforgeUntil.ToggleTargetView();
		PoolRow(wanted).OnClickSelect();
		Check(ButtonText() == "Target: " + SprkReforgeUntil.SkillName(skillTarget), "skill-only text " + ButtonText());
		Check(SprkReforgeUntil.SkillName(skillTarget) == skillText, "skill name " + SprkReforgeUntil.SkillName(skillTarget) + " vs row " + skillText);
		SprkReforgeUntil.TechnomagicBuilt(window, stats, skills);
		Check(SprkReforgeUntil.IsTechnomagicSkillArmed(FixtureSlot), "skill target lost on rebuild");
		// The pool reopens with the skill selected; picking it again clears it.
		SprkReforgeUntil.ToggleTargetView();
		StatGaugeInfo selectedSkill = SprkReforgeUntil.TechnomagicSkillPool.GetStatGaugeInfo(1);
		Check(selectedSkill.StateRoot != null && Array.Exists(selectedSkill.StateRoot, (StatGaugeInfo.StateRootInfo s) => s.Type == StatGaugeInfo.StateType.Selected && s.Root != null && s.Root.gameObject.activeSelf), "skill target row not selected");
		selectedSkill.OnClickSelect();
		Check(SprkReforgeUntil.SkillTarget == 0 && !SprkReforgeUntil.IsTechnomagicSkillArmed(FixtureSlot), "skill target not cleared");
		Check(ButtonText() == "Target Stat", "cleared skill text " + ButtonText());
		// Another item drops the skill target.
		SprkReforgeUntil.ToggleTargetView();
		SprkReforgeUntil.TechnomagicSkillPool.GetStatGaugeInfo(1).OnClickSelect();
		item.SlotIndex = FixtureSlot + 1;
		SprkReforgeUntil.TechnomagicBuilt(window, stats, skills);
		Check(SprkReforgeUntil.SkillTarget == 0, "skill target kept for another item");
		item.SlotIndex = FixtureSlot;
		SprkReforgeUntil.TechnomagicBuilt(window, stats, skills);
		// Closing the window drops the targets and the host.
		SprkReforgeUntil.ToggleTargetView();
		PoolRow(wanted).OnClickSelect();
		SprkReforgeUntil.ToggleTargetView();
		SprkReforgeUntil.TechnomagicSkillPool.GetStatGaugeInfo(1).OnClickSelect();
		Check(SprkReforgeUntil.IsTechnomagicArmed(FixtureSlot) && SprkReforgeUntil.IsTechnomagicSkillArmed(FixtureSlot), "re-armed");
		SprkReforgeUntil.TechnomagicClosing(window);
		Check(!SprkReforgeUntil.IsTechnomagicArmed(FixtureSlot) && SprkReforgeUntil.Target == OperationType.None, "closing kept the target");
		Check(!SprkReforgeUntil.IsTechnomagicSkillArmed(FixtureSlot) && SprkReforgeUntil.SkillTarget == 0, "closing kept the skill target");
		Check(!SprkReforgeUntil.TryStartTechnomagicSkill(SingletonAuto<ValanceWebManager>.instance, FixtureSlot, 1), "unarmed skill start");
		Check(!SprkReforgeUntil.TryStartTechnomagic(SingletonAuto<ValanceWebManager>.instance, FixtureSlot, 1), "unarmed start");
		sb.Append(" target=").Append(armed);
		return "PASS: " + sb;
	}

	private static string B(Bounds b)
	{
		return "[" + b.min.x.ToString("0") + ".." + b.max.x.ToString("0") + " x " + b.min.y.ToString("0") + ".." + b.max.y.ToString("0") + "]";
	}

	private static void Dump(StringBuilder sb, Transform root, Transform t, string path, int depth)
	{
		if (depth > 9)
		{
			return;
		}
		List<string> names = new List<string>();
		foreach (Component component in t.GetComponents<Component>())
		{
			if (component != null && !(component is Transform))
			{
				names.Add(component.GetType().Name);
			}
		}
		string extra = string.Empty;
		UIWidget widget = t.GetComponent<UIWidget>();
		if (widget != null)
		{
			extra = " w=" + widget.width + "x" + widget.height + " d=" + widget.depth;
			UILabel label = widget as UILabel;
			if (label != null)
			{
				extra += " text=\"" + label.text.Replace("\n", "\\n") + "\" fs=" + label.fontSize;
			}
			UISprite sprite = widget as UISprite;
			if (sprite != null)
			{
				extra += " sprite=" + sprite.spriteName;
			}
		}
		sb.Append('\n').Append(path).Append(t.gameObject.activeSelf ? "" : " (off)").Append(' ')
			.Append(B(NGUIMath.CalculateRelativeWidgetBounds(root, t, considerInactive: false)))
			.Append(" {").Append(string.Join(",", names.ToArray())).Append('}')
			.Append(extra);
		foreach (Transform child in t)
		{
			Dump(sb, root, child, path + "/" + child.name, depth + 1);
		}
	}

	public static string Run(string mode)
	{
		if (mode == "dump" || mode == "dump-table")
		{
			string text = Show();
			StringBuilder sb = new StringBuilder(text);
			Transform root = window.transform;
			if (mode == "dump-table")
			{
				Dump(sb, root, window.EquipOptionTable.transform.parent, window.EquipOptionTable.transform.parent.name, 0);
				Dump(sb, root, window.Button_Renew.transform.parent, window.Button_Renew.transform.parent.name, 0);
			}
			else
			{
				Dump(sb, root, root, root.name, 0);
			}
			return sb.ToString();
		}
		if (mode == "test")
		{
			return Test();
		}
		if (mode == "show-skill-pool")
		{
			string text = Show();
			SprkReforgeUntil.ToggleTargetView();
			// Scroll the option list to the skill pool for the screenshot.
			UIScrollView scroll = NGUITools.FindInParents<UIScrollView>(window.EquipOptionTable.gameObject);
			StatGaugeComponent statPool = SprkReforgeUntil.TechnomagicPool;
			if (scroll != null && statPool != null)
			{
				scroll.MoveRelative(new Vector3(0f, BoundsOf(statPool.transform).size.y - 20f, 0f));
				scroll.RestrictWithinBounds(instant: true);
			}
			return text + " skills=" + SprkReforgeUntil.SkillPoolCount;
		}
		if (mode == "show" || mode == "show-armed" || mode == "show-pool")
		{
			string text = Show();
			if (mode != "show")
			{
				SprkReforgeUntil.ToggleTargetView();
				PoolRow(FreeStat()).OnClickSelect();
				if (mode == "show-pool")
				{
					SprkReforgeUntil.ToggleTargetView();
				}
			}
			return text + " target=" + SprkReforgeUntil.Target;
		}
		if (mode == "hide")
		{
			if (window != null)
			{
				SprkReforgeUntil.TechnomagicClosing(window);
				UnityEngine.Object.DestroyImmediate(window.gameObject);
			}
			return "hidden";
		}
		return "unknown mode " + mode;
	}
}
