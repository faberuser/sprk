using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NJit;
using NVespa.NSingleton;
using UnityEngine;

public sealed class SprkHeroRuneFilter : MonoBehaviour, RuneSelectItemFilterOptionList.IListener
{
	private sealed class ProbeSlots : HeroRuneListPanel.IDataProvider
	{
		private readonly RuneSlotType slot;

		public ProbeSlots(RuneSlotType value)
		{
			slot = value;
		}

		public List<RuneSlotType> GetActivedRuneSlotTypes()
		{
			return new List<RuneSlotType> { slot };
		}
	}

	private HeroRuneListPanel panel;

	private readonly List<Enum> selected = new List<Enum>();

	private UIButton button;

	private UILabel buttonLabel;

	private RuneSelectItemFilterOptionList popup;

	private bool opened;

	public static void Open(HeroRuneListPanel panel)
	{
		SprkHeroRuneFilter sprkHeroRuneFilter = panel.GetComponent<SprkHeroRuneFilter>();
		if (sprkHeroRuneFilter == null)
		{
			sprkHeroRuneFilter = panel.gameObject.AddComponent<SprkHeroRuneFilter>();
		}
		sprkHeroRuneFilter.panel = panel;
		sprkHeroRuneFilter.selected.Clear();
		sprkHeroRuneFilter.opened = true;
		sprkHeroRuneFilter.CreateButton();
		sprkHeroRuneFilter.UpdateLabel();
	}

	private void CreateButton()
	{
		if (!(button != null))
		{
			UIButton componentInParent = panel.Label_SelectedCategory.GetComponentInParent<UIButton>(includeInactive: true);
			GameObject gameObject = UnityEngine.Object.Instantiate(componentInParent.gameObject, componentInParent.transform.parent, worldPositionStays: false);
			gameObject.name = "SprkRuneSortOption";
			button = gameObject.GetComponent<UIButton>();
			button.onClick.Clear();
			EventDelegate.Add(button.onClick, OnClick);
			Layout(componentInParent.gameObject, 432, -120f, sort: false);
			Layout(gameObject, 220, 226f, sort: true);
			buttonLabel = gameObject.GetComponentInChildren<UILabel>(includeInactive: true);
			buttonLabel.fontSize = 26;
			buttonLabel.text = SprkText.Get("Sort Option");
			gameObject.SetActive(value: true);
		}
	}

	private static void Layout(GameObject go, int width, float x, bool sort)
	{
		UIWidget[] componentsInChildren = go.GetComponentsInChildren<UIWidget>(includeInactive: true);
		foreach (UIWidget uIWidget in componentsInChildren)
		{
			uIWidget.leftAnchor.target = null;
			uIWidget.rightAnchor.target = null;
			uIWidget.topAnchor.target = null;
			uIWidget.bottomAnchor.target = null;
			if (uIWidget.transform == go.transform)
			{
				uIWidget.width = width;
			}
			else if (uIWidget is UILabel)
			{
				uIWidget.width = width - (sort ? 16 : 84);
				uIWidget.transform.localPosition = new Vector3((!sort) ? 28 : 0, 0f, 0f);
			}
			else if (uIWidget.name == "Sprite_Bg")
			{
				uIWidget.width = width + 16;
			}
			else if (uIWidget.name == "Sprite_Arrow")
			{
				if (sort)
				{
					uIWidget.gameObject.SetActive(value: false);
				}
				else
				{
					uIWidget.transform.localPosition = new Vector3((float)(-width) * 0.5f + 60f, 0f, 0f);
				}
			}
		}
		go.transform.localPosition = new Vector3(x, 0f, 0f);
		NGUITools.AddWidgetCollider(go);
	}

	private void UpdateLabel()
	{
		if (buttonLabel != null)
		{
			buttonLabel.text = SprkText.Get(((selected.Count == 0) ? "Sort Option" : (SprkText.Format("Sort Option ({0})", selected.Count))));
		}
	}

	public void OnClick()
	{
		if (opened && !(panel == null) && panel.gameObject.activeInHierarchy)
		{
			MyHeroManagement instance = SceneAutoBehaviour<MyHeroManagement>.instance;
			popup = instance.GetInstance<RuneSelectItemFilterOptionList>();
			if (popup == null)
			{
				instance.AddInstance<RuneSelectItemFilterOptionList>("Resources/Prefab2/Window/Inventory/RuneSelectItemFilterOptionList");
				popup = instance.GetInstance<RuneSelectItemFilterOptionList>();
			}
			if (popup != null)
			{
				popup.Open(this, new List<Enum>(selected), 2);
			}
		}
	}

	public static void Close(HeroRuneListPanel panel)
	{
		SprkHeroRuneFilter component = panel.GetComponent<SprkHeroRuneFilter>();
		if (!(component == null))
		{
			component.opened = false;
			component.selected.Clear();
			component.UpdateLabel();
			if (component.popup != null && component.popup.IsOpen())
			{
				component.popup.Close();
			}
			component.popup = null;
		}
	}

	public static bool Matches(HeroRuneListPanel panel, IItemInfo item)
	{
		SprkHeroRuneFilter component = panel.GetComponent<SprkHeroRuneFilter>();
		if (component == null || component.selected.Count == 0)
		{
			return true;
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(item.ItemIndex);
		if (data == null)
		{
			return false;
		}
		return ContainsAll(RuneHelper.GetRuneStat(data, (item as EquipItemInfo)?.PunishmentRuneOption), component.selected);
	}

	public static bool ContainsAll(Dictionary<Enum, long> stats, List<Enum> filters)
	{
		return filters?.All((Enum stat) => stats != null && stats.ContainsKey(stat)) ?? true;
	}

	public void OnClickConfirm(List<Enum> filters, Action complete)
	{
		if (!opened)
		{
			complete?.Invoke();
			return;
		}
		selected.Clear();
		if (filters != null)
		{
			selected.AddRange(filters);
		}
		UpdateLabel();
		complete?.Invoke();
		if (panel != null && panel.gameObject.activeInHierarchy)
		{
			panel.Refresh();
			panel.ResetScroll();
		}
	}

	public static string Probe()
	{
		TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
		GameObject original = VespaResource.Load<GameObject>("Resources/Prefab2/Window/Hero/HeroRunehenge");
		GameObject gameObject = new GameObject("RuneFilterProbe");
		gameObject.SetActive(value: false);
		HeroRuneListPanel runeListPanel = UnityEngine.Object.Instantiate(original, gameObject.transform, worldPositionStays: false).GetComponent<HeroRunehengeWindow>().RuneListPanel;
		List<string> list = new List<string>();
		Open(runeListPanel);
		SprkHeroRuneFilter component = runeListPanel.GetComponent<SprkHeroRuneFilter>();
		UIButton componentInParent = runeListPanel.Label_SelectedCategory.GetComponentInParent<UIButton>(includeInactive: true);
		Check(component.button != null && component.button.onClick.Count == 1, "native button binding");
		Check(componentInParent.onClick.Count > 0, "native category dropdown preserved");
		UIWidget component2 = componentInParent.GetComponent<UIWidget>();
		UIWidget component3 = component.button.GetComponent<UIWidget>();
		Check(componentInParent.transform.localPosition.x + (float)component2.width * 0.5f < component.button.transform.localPosition.x - (float)component3.width * 0.5f, "separate header hit areas");
		Open(runeListPanel);
		Check(runeListPanel.GetComponentsInChildren<UIButton>(includeInactive: true).Count((UIButton b) => b.name == "SprkRuneSortOption") == 1, "reopen does not duplicate button");
		RuneSelectItemListWindow component4 = UnityEngine.Object.Instantiate(VespaResource.Load<GameObject>("Resources/Prefab2/Window/Inventory/RuneSelectItemListWindow"), gameObject.transform, worldPositionStays: false).GetComponent<RuneSelectItemListWindow>();
		MethodInfo method = typeof(RuneSelectItemListWindow).GetMethod("IsSatisfyCondition", BindingFlags.Instance | BindingFlags.NonPublic);
		MethodInfo method2 = typeof(HeroRuneListPanel).GetMethod("Selector", BindingFlags.Instance | BindingFlags.NonPublic);
		ItemData[] array = (from d in GameRoot.Instance.ItemDataContainer.GetAllData()
			where d.Type == ItemType.Rune
			select d).ToArray();
		Check(array.Length != 0, "native rune tables loaded");
		int num = 0;
		Enum[] array2 = array.SelectMany((ItemData d) => d.GetSubData<RuneItemData>().GetStatTypeAndValue().Keys).Distinct().ToArray();
		Enum[] array3 = array2;
		ItemData[] array4;
		foreach (Enum obj in array3)
		{
			component.selected.Clear();
			component.selected.Add(obj);
			array4 = array;
			foreach (ItemData itemData in array4)
			{
				ItemInfo item = new ItemInfo
				{
					ItemIndex = itemData.Index
				};
				bool flag = (bool)method.Invoke(component4, new object[2] { component.selected, itemData });
				Check(Matches(runeListPanel, item) == flag, "ticket stat matching " + itemData.Index + " " + obj);
				num++;
			}
		}
		ItemData itemData2 = array.First((ItemData d) => d.GetSubData<RuneItemData>().GetStatTypeAndValue().Count == 2);
		component.selected.Clear();
		component.selected.AddRange(itemData2.GetSubData<RuneItemData>().GetStatTypeAndValue().Keys);
		array4 = array;
		foreach (ItemData itemData3 in array4)
		{
			Check(Matches(runeListPanel, new ItemInfo
			{
				ItemIndex = itemData3.Index
			}) == (bool)method.Invoke(component4, new object[2] { component.selected, itemData3 }), "two-stat AND semantics");
		}
		RuneSelectItemFilterOptionList component5 = UnityEngine.Object.Instantiate(VespaResource.Load<GameObject>("Resources/Prefab2/Window/Inventory/RuneSelectItemFilterOptionList"), gameObject.transform, worldPositionStays: false).GetComponent<RuneSelectItemFilterOptionList>();
		typeof(RuneSelectItemFilterOptionList).GetMethod("Init", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(component5, new object[3]
		{
			component,
			new List<Enum>(component.selected),
			2
		});
		Check(component5.GridOptionList.GetComponentsInChildren<RuneSelectItemFilterMenuComponent>(includeInactive: true).Length != 0, "native stat picker populated");
		ItemData itemData4 = itemData2;
		Check((bool)method2.Invoke(runeListPanel, new object[1]
		{
			new ItemInfo
			{
				ItemIndex = itemData4.Index
			}
		}), "matching rune accepted by native selector");
		FieldInfo field = typeof(HeroRuneListPanel).GetField("_dataProvider", BindingFlags.Instance | BindingFlags.NonPublic);
		RuneSlotType value = RuneSlotType.None;
		field.SetValue(runeListPanel, new ProbeSlots(value));
		Check(!(bool)method2.Invoke(runeListPanel, new object[1]
		{
			new ItemInfo
			{
				ItemIndex = itemData4.Index
			}
		}), "slot restrictions preserved");
		field.SetValue(runeListPanel, null);
		component5.OnClickResetFilter();
		component5.OnClickConfirm();
		Check(component.selected.Count == 0 && component.buttonLabel.text == "Sort Option", "native Deselect and OK restore full list");
		array4 = array;
		foreach (ItemData itemData5 in array4)
		{
			Check(Matches(runeListPanel, new ItemInfo
			{
				ItemIndex = itemData5.Index
			}), "unfiltered restore");
		}
		ItemData itemData6 = GameRoot.Instance.ItemDataContainer.GetAllData().First((ItemData d) => d.Type == ItemType.PunishmentRune);
		PunishmentRuneOptionData punishmentRuneOptionData = GameRoot.Instance.PunishmentRuneOptionDataContainer.GetAllData().First((PunishmentRuneOptionData d) => d.StatTypes != null && d.StatTypes.Length != 0);
		EquipItemInfo equipItemInfo = new EquipItemInfo
		{
			ItemIndex = itemData6.Index,
			PunishmentRuneOption = new PunishmentRuneOptionInfo
			{
				MainOptionIndex = punishmentRuneOptionData.Index,
				MainOptionValue = 100
			}
		};
		Dictionary<Enum, long> rolledStats = RuneHelper.GetRuneStat(itemData6, equipItemInfo.PunishmentRuneOption);
		Check(rolledStats.Count > 0, "crafted main stat fixture");
		component.selected.Add(rolledStats.Keys.First());
		Check(Matches(runeListPanel, equipItemInfo), "crafted rolled stat matches");
		component.selected.Clear();
		component.selected.Add(array2.First((Enum s) => !rolledStats.ContainsKey(s)));
		Check(!Matches(runeListPanel, equipItemInfo), "crafted nonmatching stat hidden");
		Close(runeListPanel);
		Check(!component.opened && component.selected.Count == 0, "close clears filter state");
		component.OnClickConfirm(new List<Enum> { array2[0] }, delegate
		{
		});
		Check(component.selected.Count == 0, "closed page rejects stale picker callback");
		Open(runeListPanel);
		Check(component.opened && component.selected.Count == 0, "reopen starts unfiltered");
		HeroRunehengeWindow instance = SceneAutoBehaviour<MyHeroManagement>.instance.GetInstance<HeroRunehengeWindow>();
		instance.Open();
		HeroRuneListPanel runeListPanel2 = instance.RuneListPanel;
		runeListPanel2.gameObject.SetActive(value: true);
		Open(runeListPanel2);
		runeListPanel2.Label_SelectedCategory.text = SprkText.Get("Normal Rune");
		SprkHeroRuneFilter component6 = runeListPanel2.GetComponent<SprkHeroRuneFilter>();
		component6.OnClick();
		Check(component6.popup != null && component6.popup.IsOpen(), "management-owned stat picker opens");
		Close(runeListPanel2);
		Check(component6.popup == null, "owned stat picker closed with page");
		Open(runeListPanel2);
		GameObject gameObject2 = new GameObject("RuneFilterVisual");
		gameObject2.layer = 30;
		gameObject2.transform.position = new Vector3(10000f, 0f, 0f);
		gameObject2.AddComponent<UIPanel>().depth = 5000;
		Camera camera = new GameObject("RuneFilterVisualCamera").AddComponent<Camera>();
		camera.orthographic = true;
		camera.orthographicSize = 240f;
		camera.nearClipPlane = 1f;
		camera.farClipPlane = 2000f;
		camera.cullingMask = 1073741824;
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = new Color(0.15f, 0.12f, 0.1f, 1f);
		camera.depth = 100000f;
		camera.transform.position = new Vector3(10000f, 0f, -1000f);
		runeListPanel.Label_SelectedCategory.text = SprkText.Get("Normal Rune");
		GameObject obj2 = UnityEngine.Object.Instantiate(componentInParent.transform.parent.gameObject, gameObject2.transform, worldPositionStays: false);
		obj2.transform.localPosition = Vector3.zero;
		obj2.transform.localScale = Vector3.one;
		NGUITools.SetLayer(obj2, 30);
		list.Add("PASS: native header button/dropdown layout and reuse; " + array.Length + " runes, " + array2.Length + " stat choices, " + num + " ticket comparisons; two-stat matching, native picker Deselect/OK, slot eligibility, crafted rolled main stats and close/reopen lifecycle.");
		UnityEngine.Object.Destroy(gameObject);
		return string.Join("\n", list.ToArray());
	}

	private static void Check(bool condition, string message)
	{
		if (!condition)
		{
			throw new Exception("Rune filter: " + message);
		}
	}
}
