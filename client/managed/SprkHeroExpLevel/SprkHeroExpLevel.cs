using System;
using System.Reflection;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NGlobal;
using UnityEngine;

public sealed class SprkHeroExpLevel : MonoBehaviour
{
	private static readonly FieldInfo YesField = typeof(LevelUpMaxPopup).GetField("_onYes", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly int[] Steps = new int[4] { -10, -1, 1, 10 };

	private static SprkHeroExpLevel runner;

	private HeroGetExpComponent pendingSlot;

	private int pendingPerPotion;

	private float pendingUntil;

	private LevelUpMaxPopup popup;

	private Action boundYes;

	private object closure;

	private FieldInfo countField;

	private FieldInfo nextTotalField;

	private SprkHeroExpLevelPlan plan;

	private int level;

	private long perPotion;

	private string itemName = "";

	private GameObject[] buttons;

	public static void Attach(HeroGetExpComponent slot, int perPotion)
	{
		if (!(slot == null) && perPotion > 0)
		{
			if (runner == null)
			{
				GameObject obj = new GameObject("SPRK Hero EXP Level");
				UnityEngine.Object.DontDestroyOnLoad(obj);
				runner = obj.AddComponent<SprkHeroExpLevel>();
			}
			runner.pendingSlot = slot;
			runner.pendingPerPotion = perPotion;
			runner.pendingUntil = Time.realtimeSinceStartup + 3f;
			runner.TryBind();
		}
	}

	private void Update()
	{
		if (pendingSlot != null)
		{
			if (Time.realtimeSinceStartup > pendingUntil)
			{
				pendingSlot = null;
			}
			else
			{
				TryBind();
			}
		}
		if (popup != null && (!popup.gameObject.activeInHierarchy || YesField.GetValue(popup) != boundYes))
		{
			Unbind();
		}
	}

	private void TryBind()
	{
		HeroGetExpComponent heroGetExpComponent = pendingSlot;
		if (heroGetExpComponent == null || YesField == null)
		{
			return;
		}
		LevelUpMaxPopup[] array = Resources.FindObjectsOfTypeAll<LevelUpMaxPopup>();
		foreach (LevelUpMaxPopup levelUpMaxPopup in array)
		{
			if (!(levelUpMaxPopup == null) && levelUpMaxPopup.gameObject.activeInHierarchy && YesField.GetValue(levelUpMaxPopup) is Action { Target: not null } action)
			{
				FieldInfo field = action.Target.GetType().GetField("<>4__this");
				if (!(field == null) && field.GetValue(action.Target) == heroGetExpComponent)
				{
					pendingSlot = null;
					Bind(levelUpMaxPopup, action, heroGetExpComponent);
					break;
				}
			}
		}
	}

	private void Bind(LevelUpMaxPopup target, Action yes, HeroGetExpComponent slot)
	{
		Unbind();
		Type type = yes.Target.GetType();
		countField = type.GetField("usablePotion");
		nextTotalField = type.GetField("nextHeroTotalExp");
		FieldInfo field = type.GetField("curHeroTotalExp");
		FieldInfo field2 = type.GetField("MaxHeroExp");
		FieldInfo field3 = type.GetField("itemInfo");
		CachedHeroInfo heroInfo = slot.HeroInfo;
		CreatureLevelDataContainer levels = ((GameRoot.Instance == null) ? null : GameRoot.Instance.CreatureLevelDataContainer);
		if (!(countField == null) && !(nextTotalField == null) && !(field == null) && !(field2 == null) && !(field3 == null) && heroInfo != null && levels != null)
		{
			perPotion = pendingPerPotion;
			plan = new SprkHeroExpLevelPlan((long)field.GetValue(yes.Target), (long)field2.GetValue(yes.Target), perPotion, (int)countField.GetValue(yes.Target), heroInfo.Level, heroInfo.GetMaxLevel(), (int l) => levels.GetTotalExp(l, 0L));
			if (!plan.CanChoose)
			{
				plan = null;
				return;
			}
			ItemData itemData = ((!(field3.GetValue(yes.Target) is ItemInfo itemInfo) || GameRoot.Instance.ItemDataContainer == null) ? null : GameRoot.Instance.ItemDataContainer.GetData(itemInfo.ItemIndex));
			itemName = ((itemData == null) ? "" : itemData.Name);
			popup = target;
			boundYes = yes;
			closure = yes.Target;
			BuildButtons();
			level = plan.TopLevel;
			Apply();
		}
	}

	private void Unbind()
	{
		if (buttons != null)
		{
			GameObject[] array = buttons;
			foreach (GameObject gameObject in array)
			{
				if (gameObject != null)
				{
					UnityEngine.Object.Destroy(gameObject);
				}
			}
		}
		buttons = null;
		popup = null;
		boundYes = null;
		closure = null;
		plan = null;
	}

	private void BuildButtons()
	{
		Transform root = popup.transform;
		Transform transform = Find(root, "Button_No");
		Transform transform2 = Find(root, "Button_Yes");
		UILabel label_AfterLevel = popup.Label_AfterLevel;
		UILabel label_BeforeLevel = popup.Label_BeforeLevel;
		if (transform == null || transform2 == null || label_AfterLevel == null || label_BeforeLevel == null)
		{
			return;
		}
		Transform parent = transform.parent;
		Transform transform3 = ((label_AfterLevel.transform.parent != null) ? label_AfterLevel.transform.parent : label_AfterLevel.transform);
		Transform transform4 = ((label_BeforeLevel.transform.parent != null) ? label_BeforeLevel.transform.parent : label_BeforeLevel.transform);
		Vector3 vector = parent.InverseTransformPoint(Middle(transform3));
		float num = Mathf.Abs(vector.x - parent.InverseTransformPoint(Middle(transform4)).x);
		if (num < 1f)
		{
			num = 240f;
		}
		buttons = new GameObject[Steps.Length];
		for (int i = 0; i < Steps.Length; i++)
		{
			int delta = Steps[i];
			GameObject gameObject = UnityEngine.Object.Instantiate(((delta < 0) ? transform : transform2).gameObject, parent, worldPositionStays: false);
			gameObject.name = "SprkLevelStep" + ((delta < 0) ? "Minus" : "Plus") + Math.Abs(delta);
			MonoBehaviour[] componentsInChildren = gameObject.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
			foreach (MonoBehaviour monoBehaviour in componentsInChildren)
			{
				if (monoBehaviour is UIButtonMessage || monoBehaviour is UIEventTrigger || monoBehaviour is UIEventListener || monoBehaviour is UIForwardEvents || monoBehaviour.GetType().Name.IndexOf("Locali", StringComparison.Ordinal) >= 0)
				{
					UnityEngine.Object.DestroyImmediate(monoBehaviour);
				}
			}
			UIButton component = gameObject.GetComponent<UIButton>();
			if (component != null)
			{
				component.onClick.Clear();
				EventDelegate.Add(component.onClick, delegate
				{
					Change(delta);
				});
			}
			float num2 = ((Math.Abs(delta) == 1) ? 0.36f : 0.64f) * num * (float)Math.Sign(delta);
			gameObject.transform.localPosition = new Vector3(vector.x + num2, vector.y, gameObject.transform.localPosition.z);
			UIWidget component2 = gameObject.GetComponent<UIWidget>();
			if (component2 != null)
			{
				component2.width = Mathf.RoundToInt(num * 0.25f);
				component2.height = Mathf.RoundToInt(num * 0.16f);
			}
			UILabel componentInChildren = gameObject.GetComponentInChildren<UILabel>(includeInactive: true);
			if (componentInChildren != null)
			{
				componentInChildren.text = ((delta > 0) ? "+" : "−") + Math.Abs(delta);
				componentInChildren.overflowMethod = UILabel.Overflow.ShrinkContent;
				componentInChildren.width = Mathf.RoundToInt(num * 0.22f);
				componentInChildren.height = Mathf.RoundToInt(num * 0.13f);
				componentInChildren.transform.localPosition = Vector3.zero;
			}
			NGUITools.UpdateWidgetCollider(gameObject);
			gameObject.SetActive(value: true);
			buttons[i] = gameObject;
		}
	}

	private static Vector3 Middle(Transform group)
	{
		UILabel[] componentsInChildren = group.GetComponentsInChildren<UILabel>(includeInactive: true);
		if (componentsInChildren.Length == 0)
		{
			return group.position;
		}
		Vector3 zero = Vector3.zero;
		UILabel[] array = componentsInChildren;
		foreach (UILabel uILabel in array)
		{
			zero += uILabel.transform.position;
		}
		return zero / componentsInChildren.Length;
	}

	private static Transform Find(Transform root, string name)
	{
		Transform[] componentsInChildren = root.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name == name)
			{
				return transform;
			}
		}
		return null;
	}

	private void Change(int delta)
	{
		if (plan != null)
		{
			level = plan.Step(level, delta);
			Apply();
		}
	}

	private void Apply()
	{
		if (plan == null || closure == null)
		{
			return;
		}
		int num = plan.Potions(level);
		countField.SetValue(closure, num);
		nextTotalField.SetValue(closure, plan.TotalAfter(num));
		UIHelper.SetLabelText(popup.Label_AfterLevel, plan.LevelAfter(num));
		UIHelper.SetLabelText(popup.Label_Content1, LocalizationManager.GetStringDirect("EXP_USE_POPUP_MESSAGE_1", itemName, num));
		UIHelper.SetLabelText(popup.Label_Content2, LocalizationManager.GetStringDirect("EXP_USE_POPUP_MESSAGE_2", perPotion * num));
		if (buttons == null)
		{
			return;
		}
		for (int i = 0; i < buttons.Length; i++)
		{
			UIButton uIButton = ((buttons[i] == null) ? null : buttons[i].GetComponent<UIButton>());
			if (uIButton != null)
			{
				uIButton.isEnabled = ((Steps[i] < 0) ? (level > plan.MinLevel) : (level < plan.TopLevel));
			}
		}
	}
}
