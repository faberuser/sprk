using System;
using System.Collections.Generic;
using System.Reflection;
using NGame2.NUI.NAtlasAssignComponent;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NShared;
using UnityEngine;

public sealed class SprkForgeAwakenChance : MonoBehaviour
{
	private sealed class Choice
	{
		public int Index;

		public int Count;

		public long Points;

		public Choice Previous;
	}

	private const BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly FieldInfo Inventory = typeof(HeroEquipAwaken).GetField("_ListAwakeInvenItems", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly FieldInfo Stones = typeof(HeroEquipAwaken).GetField("_ListPublicAwakenItemComponents", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly MethodInfo ClearMaterials = typeof(HeroEquipAwaken).GetMethod("ClearMaterialEquipIcon", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly MethodInfo AddMaterial = typeof(HeroEquipAwaken).GetMethod("SetupMaterialEquipIcon", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly MethodInfo CanUseStone = typeof(HeroEquipAwaken).GetMethod("CanUseAwakenStone", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly MethodInfo Rate = typeof(HeroEquipAwaken).GetMethod("GetTotalAwakenSuccessRate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly MethodInfo[] Refresh = new MethodInfo[3]
	{
		typeof(HeroEquipAwaken).GetMethod("UpdateAwakeUITask", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
		typeof(HeroEquipAwaken).GetMethod("UpdateAwakeEquipItemListTask", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
		typeof(HeroEquipAwaken).GetMethod("UpdatePublicAwakeItemListTask", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
	};

	private HeroEquipAwaken window;

	private GameObject root;

	private UISlider slider;

	private UILabel label;

	private int slot = -1;

	private int star = -1;

	private int bonus = -1;

	private bool updating;

	private readonly List<ItemIconComponent> candidates = new List<ItemIconComponent>();

	private int[][] plans;

	private int[] chances;

	public static void Sync(HeroEquipAwaken window)
	{
		SprkForgeAwakenChance sprkForgeAwakenChance = window.GetComponent<SprkForgeAwakenChance>();
		if (sprkForgeAwakenChance == null)
		{
			sprkForgeAwakenChance = window.gameObject.AddComponent<SprkForgeAwakenChance>();
		}
		if (sprkForgeAwakenChance.updating)
		{
			return;
		}
		sprkForgeAwakenChance.window = window;
		sprkForgeAwakenChance.updating = true;
		try
		{
			sprkForgeAwakenChance.SyncTarget();
		}
		finally
		{
			sprkForgeAwakenChance.updating = false;
		}
	}

	private void SyncTarget()
	{
		EquipItemInfo equipItemInfo = ((window.TargetItem == null) ? null : window.TargetItem.EquipItemInfo);
		if (equipItemInfo == null || !equipItemInfo.CLI_CanAwaken())
		{
			slot = (star = (bonus = -1));
			if (root != null)
			{
				root.SetActive(value: false);
			}
			return;
		}
		EnsureSlider();
		root.SetActive(value: true);
		if (slot != equipItemInfo.SlotIndex || star != equipItemInfo.Star || bonus != equipItemInfo.UpgradeStarFailBonus)
		{
			slot = equipItemInfo.SlotIndex;
			star = equipItemInfo.Star;
			bonus = equipItemInfo.UpgradeStarFailBonus;
			BuildPlans(equipItemInfo);
			Apply(plans.Length - 1);
		}
		else
		{
			UpdateLabel();
		}
	}

	private void BuildPlans(EquipItemInfo target)
	{
		candidates.Clear();
		HashSet<int> hashSet = new HashSet<int>();
		foreach (ItemIconComponent item in (List<ItemIconComponent>)Inventory.GetValue(window))
		{
			EquipItemInfo equipItemInfo = item.EquipItemInfo;
			if (equipItemInfo != null && equipItemInfo.SlotIndex != target.SlotIndex && equipItemInfo.ItemIndex == target.ItemIndex && equipItemInfo.CLI_CanUse_as_AwakenMaterial() && hashSet.Add(equipItemInfo.SlotIndex))
			{
				candidates.Add(item);
			}
		}
		foreach (ItemIconComponent item2 in (List<ItemIconComponent>)Stones.GetValue(window))
		{
			ItemData itemData = item2.ItemData;
			if (itemData != null && ((itemData.Type == ItemType.PublicUniqueEquipAwaken && target.IsUniqueWeaponWithoutClassWeapon) || (itemData.Type == ItemType.PublicTreasureAwaken && target.IsUniqueTreasure)) && (bool)CanUseStone.Invoke(window, new object[1] { item2 }))
			{
				candidates.Add(item2);
			}
		}
		candidates.Sort(delegate(ItemIconComponent a, ItemIconComponent b)
		{
			int num7 = ((a.EquipItemInfo == null) ? a.ItemData.AwakenStar : a.EquipItemInfo.Star);
			int value = ((b.EquipItemInfo == null) ? b.ItemData.AwakenStar : b.EquipItemInfo.Star);
			int num8 = num7.CompareTo(value);
			if (num8 != 0)
			{
				return num8;
			}
			if (a.EquipItemInfo == null)
			{
				if (b.EquipItemInfo != null)
				{
					return 1;
				}
				return a.ItemData.Index.CompareTo(b.ItemData.Index);
			}
			return (b.EquipItemInfo != null) ? a.EquipItemInfo.SlotIndex.CompareTo(b.EquipItemInfo.SlotIndex) : (-1);
		});
		int maxAwakenPoint = GameRoot.Instance.EquipAwakenRatioDataContainer.GetMaxAwakenPoint(star + 1);
		int[] array = new int[candidates.Count];
		for (int num = 0; num < array.Length; num++)
		{
			ItemIconComponent itemIconComponent = candidates[num];
			array[num] = GameRoot.Instance.EquipAwakenPointDataContainer.GetData((itemIconComponent.EquipItemInfo == null) ? itemIconComponent.ItemData.AwakenStar : itemIconComponent.EquipItemInfo.Star).AwakenPoint;
		}
		int[] array2 = new int[maxAwakenPoint + 1];
		for (int num2 = 1; num2 <= maxAwakenPoint; num2++)
		{
			array2[num2] = GameRoot.Instance.EquipAwakenRatioDataContainer.GetData(star + 1, num2)?.SuccessRatio ?? array2[num2 - 1];
		}
		plans = Plan(array, array2, bonus);
		chances = new int[plans.Length];
		for (int num3 = 0; num3 < plans.Length; num3++)
		{
			long num4 = 0L;
			int[] array3 = plans[num3];
			foreach (int num6 in array3)
			{
				num4 += array[num6];
			}
			chances[num3] = Math.Min(100, array2[(int)Math.Min(maxAwakenPoint, num4)] + bonus);
		}
		slider.numberOfSteps = plans.Length;
		slider.enabled = plans.Length > 1;
		Collider[] componentsInChildren = slider.GetComponentsInChildren<Collider>(includeInactive: true);
		for (int num5 = 0; num5 < componentsInChildren.Length; num5++)
		{
			componentsInChildren[num5].enabled = plans.Length > 1;
		}
		Collider2D[] componentsInChildren2 = slider.GetComponentsInChildren<Collider2D>(includeInactive: true);
		for (int num5 = 0; num5 < componentsInChildren2.Length; num5++)
		{
			componentsInChildren2[num5].enabled = plans.Length > 1;
		}
	}

	public static int[][] Plan(int[] points, int[] rates, int failBonus)
	{
		int num = rates.Length - 1;
		Choice[] array = new Choice[num + 1];
		array[0] = new Choice
		{
			Index = -1
		};
		for (int i = 0; i < points.Length; i++)
		{
			if (points[i] <= 0)
			{
				continue;
			}
			for (int num2 = num - 1; num2 >= 0; num2--)
			{
				Choice choice = array[num2];
				if (choice != null)
				{
					long num3 = choice.Points + points[i];
					int num4 = (int)Math.Min(num, num3);
					Choice choice2 = array[num4];
					if (choice2 == null || num3 < choice2.Points || (num3 == choice2.Points && choice.Count + 1 < choice2.Count))
					{
						array[num4] = new Choice
						{
							Index = i,
							Previous = choice,
							Points = num3,
							Count = choice.Count + 1
						};
					}
				}
			}
		}
		SortedDictionary<int, Choice> sortedDictionary = new SortedDictionary<int, Choice>();
		for (int j = 1; j <= num; j++)
		{
			Choice choice3 = array[j];
			if (choice3 != null)
			{
				int num5 = Math.Min(100, rates[j] + failBonus);
				if (num5 > 0 && (!sortedDictionary.TryGetValue(num5, out var value) || choice3.Points < value.Points || (choice3.Points == value.Points && choice3.Count < value.Count)))
				{
					sortedDictionary[num5] = choice3;
				}
			}
		}
		List<int[]> list = new List<int[]>();
		foreach (Choice value2 in sortedDictionary.Values)
		{
			List<int> list2 = new List<int>();
			Choice choice4 = value2;
			while (choice4 != null && choice4.Index >= 0)
			{
				list2.Add(choice4.Index);
				choice4 = choice4.Previous;
			}
			list2.Reverse();
			list.Add(list2.ToArray());
		}
		return list.ToArray();
	}

	private void Apply(int step)
	{
		ClearMaterials.Invoke(window, null);
		if (plans != null && step >= 0 && step < plans.Length)
		{
			int[] array = plans[step];
			foreach (int index in array)
			{
				AddMaterial.Invoke(window, new object[1] { candidates[index] });
			}
		}
		MethodInfo[] refresh = Refresh;
		for (int i = 0; i < refresh.Length; i++)
		{
			refresh[i].Invoke(window, null);
		}
		slider.value = ((plans.Length == 0) ? 0f : ((plans.Length == 1) ? 1f : ((float)step / (float)(plans.Length - 1))));
		UpdateLabel();
	}

	private void UpdateLabel()
	{
		int value = ((window.GetMaterialEquipSlotIndexArray() != null) ? Math.Min(100, (int)Rate.Invoke(window, null)) : 0);
		label.text = SprkText.Format("Success: {0}%", value);
		if (chances != null && chances.Length > 1)
		{
			int num = Array.IndexOf(chances, value);
			slider.value = ((num < 0) ? 0f : ((float)num / (float)(chances.Length - 1)));
		}
	}

	private void OnSliderChanged()
	{
		if (updating || plans == null || plans.Length < 2)
		{
			return;
		}
		updating = true;
		try
		{
			Apply(Mathf.RoundToInt(slider.value * (float)(plans.Length - 1)));
		}
		finally
		{
			updating = false;
		}
	}

	private void EnsureSlider()
	{
		if (root != null)
		{
			return;
		}
		Transform parent = window.Label_UpperMsg.transform.parent;
		string[] array = new string[5] { "WidgetAwakeItems", "StatChange", "Sprite_Arrow", "Label_UpperMsg", "Sprite_Bg" };
		foreach (string text in array)
		{
			Transform obj = parent.Find(text);
			if (obj == null)
			{
				throw new InvalidOperationException("Missing Forge layout section: " + text);
			}
			obj.localPosition += new Vector3(0f, 55f, 0f);
		}
		if (window.EventRoot != null && window.EventRoot.gameObject.activeSelf)
		{
			Vector3 localPosition = window.Label_UpperMsg.transform.localPosition;
			window.Label_UpperMsg.transform.localPosition = new Vector3(-240f, localPosition.y, localPosition.z);
			window.Label_UpperMsg.width = 500;
			window.Label_UpperMsg.fontSize = 20;
			window.EventRoot.localPosition = new Vector3(245f, localPosition.y, window.EventRoot.localPosition.z);
			window.EventRoot.localScale *= 0.5f;
		}
		root = new GameObject("SprkForgeAwakenChance");
		root.transform.SetParent(parent, worldPositionStays: false);
		root.transform.localPosition = new Vector3(-16f, -332f, 0f);
		GameObject gameObject = new GameObject("SprkForgeSliderSource");
		gameObject.SetActive(value: false);
		try
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(GameOptionSliderComponent.PrefabAuto.Value.LoadGo, gameObject.transform, worldPositionStays: false);
			slider = gameObject2.GetComponent<GameOptionSliderComponent>().Slider;
			slider.transform.SetParent(root.transform, worldPositionStays: false);
		}
		finally
		{
			UnityEngine.Object.Destroy(gameObject);
		}
		AtlasAssignerBase[] componentsInChildren = root.GetComponentsInChildren<AtlasAssignerBase>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Assign();
		}
		slider.name = "SprkSuccessRateSlider";
		slider.onChange.Clear();
		slider.numberOfSteps = 0;
		EventDelegate.Add(slider.onChange, OnSliderChanged);
		int num = window.Label_UpperMsg.depth + 1;
		int num2 = int.MaxValue;
		UIWidget[] componentsInChildren2 = slider.GetComponentsInChildren<UIWidget>(includeInactive: true);
		UIWidget[] array2 = componentsInChildren2;
		foreach (UIWidget uIWidget in array2)
		{
			num2 = Math.Min(num2, uIWidget.depth);
		}
		array2 = componentsInChildren2;
		foreach (UIWidget uIWidget2 in array2)
		{
			uIWidget2.leftAnchor.target = null;
			uIWidget2.rightAnchor.target = null;
			uIWidget2.topAnchor.target = null;
			uIWidget2.bottomAnchor.target = null;
			uIWidget2.depth = num + uIWidget2.depth - num2;
		}
		UIWidget component = parent.Find("WidgetGold").GetComponent<UIWidget>();
		UIWidget component2 = window.Button_Awaken.GetComponent<UIWidget>();
		float x = root.transform.InverseTransformPoint(component.worldCorners[0]).x;
		float x2 = root.transform.InverseTransformPoint(component2.worldCorners[2]).x;
		float num3 = (float)slider.thumb.GetComponent<UIWidget>().width * 0.5f;
		slider.backgroundWidget = slider.transform.Find("GaugeBg").GetComponent<UIWidget>();
		slider.foregroundWidget = slider.transform.Find("Gauge").GetComponent<UIWidget>();
		int num4 = slider.backgroundWidget.width - slider.foregroundWidget.width;
		int num5 = Mathf.FloorToInt((x2 - x - 180f - 24f - num3) / 0.5f);
		slider.foregroundWidget.width = num5;
		slider.backgroundWidget.width = num5 + num4;
		slider.GetComponent<UIWidget>().width = num5;
		slider.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
		slider.transform.localPosition = new Vector3(x + (num3 + (float)num5 * 0.5f) * 0.5f, 0f, 0f);
		array2 = componentsInChildren2;
		for (int i = 0; i < array2.Length; i++)
		{
			NGUITools.UpdateWidgetCollider(array2[i].gameObject);
		}
		slider.ForceUpdate();
		GameObject gameObject3 = UnityEngine.Object.Instantiate(window.Label_UpperMsg.gameObject, root.transform, worldPositionStays: false);
		label = gameObject3.GetComponent<UILabel>();
		label.name = "SprkSuccessRateLabel";
		label.leftAnchor.target = null;
		label.rightAnchor.target = null;
		label.topAnchor.target = null;
		label.bottomAnchor.target = null;
		label.pivot = UIWidget.Pivot.Right;
		label.alignment = NGUIText.Alignment.Right;
		label.fontSize = 24;
		label.width = 180;
		label.height = 30;
		label.overflowMethod = UILabel.Overflow.ShrinkContent;
		label.depth = num;
		gameObject3.transform.localScale = Vector3.one;
		gameObject3.transform.localPosition = new Vector3(x2, 0f, 0f);
		gameObject3.SetActive(value: true);
		NGUITools.SetLayer(root, window.gameObject.layer);
		NGUITools.MarkParentAsChanged(root);
	}
}
