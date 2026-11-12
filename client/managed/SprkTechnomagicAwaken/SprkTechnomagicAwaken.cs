using System;
using System.Collections.Generic;
using System.Reflection;
using NGame2.NAccount;
using NGame2.NUI.NAtlasAssignComponent;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NShared;
using NShared.ValanceAwaken;
using NVespa.NSingleton;
using UnityEngine;

public sealed class SprkTechnomagicAwaken : MonoBehaviour
{
	private const BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private const int SliderWidth = 380;

	private static readonly MethodInfo SetAfterStar = typeof(ValanceAwakenWindow).GetProperty("AfterStar").GetSetMethod(nonPublic: true);

	private static readonly MethodInfo SetAfterStat = typeof(ValanceAwakenWindow).GetProperty("AfterStat").GetSetMethod(nonPublic: true);

	private static readonly MethodInfo SetData = typeof(ValanceAwakenWindow).GetProperty("ValanceAwakenData").GetSetMethod(nonPublic: true);

	private static readonly MethodInfo SetupMaterialIcon = typeof(ValanceAwakenWindow).GetMethod("SetupMaterialIcon", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly FieldInfo ResultComp = typeof(ValanceAwakenWindow).GetField("_resultComp", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private static int pendingSlot = -1;

	private static int pendingTarget;

	private ValanceAwakenWindow window;

	private GameObject root;

	private UISlider slider;

	private UILabel valueLabel;

	private readonly List<ValanceAwakenData> steps = new List<ValanceAwakenData>();

	private int baseStar;

	private int target;

	private bool updating;

	public static void Setup(ValanceAwakenWindow window)
	{
		pendingSlot = -1;
		SprkTechnomagicAwaken sprkTechnomagicAwaken = window.GetComponent<SprkTechnomagicAwaken>();
		if (sprkTechnomagicAwaken == null)
		{
			sprkTechnomagicAwaken = window.gameObject.AddComponent<SprkTechnomagicAwaken>();
		}
		sprkTechnomagicAwaken.window = window;
		sprkTechnomagicAwaken.Load();
	}

	public static void Clear(ValanceAwakenWindow window)
	{
		pendingSlot = -1;
		SprkTechnomagicAwaken component = window.GetComponent<SprkTechnomagicAwaken>();
		if (!(component == null))
		{
			component.steps.Clear();
			component.Show(visible: false);
		}
	}

	public static void Remember(ValanceAwakenWindow window)
	{
		pendingSlot = -1;
		EquipItemInfo equipItemInfo = window.EquipItemInfo;
		if (equipItemInfo != null && window.ValanceAwakenData != null && window.AfterStar > window.BeforeStar + 1)
		{
			pendingSlot = equipItemInfo.SlotIndex;
			pendingTarget = window.AfterStar;
		}
	}

	public static void Encode(Dictionary<string, object> json, NShared.ValanceAwaken.Request request)
	{
		if (json != null && request != null && pendingSlot >= 0 && request.EquipItemSlotIndex == pendingSlot)
		{
			json["TargetStar"] = pendingTarget.ToString();
		}
	}

	private void Load()
	{
		steps.Clear();
		EquipItemInfo equipItemInfo = window.EquipItemInfo;
		EquipItemData equipItemData = window.EquipItemData;
		ValanceAwakenData valanceAwakenData = window.ValanceAwakenData;
		if (equipItemInfo == null || equipItemData == null || valanceAwakenData == null)
		{
			Show(visible: false);
			return;
		}
		baseStar = window.BeforeStar;
		int num = equipItemInfo.CLI_GetMaxStar_for_Awaken();
		CreatureTagType tagType = ((equipItemData.TagType != null && equipItemData.TagType.Length != 0) ? equipItemData.TagType[0] : CreatureTagType.None);
		for (int i = baseStar + 1; i <= num; i++)
		{
			ValanceAwakenData data = GameRoot.Instance.ValanceAwakenDataContainer.GetData(new ValanceAwakenDataKey((byte)equipItemData.Tier, equipItemData.DetailIndex, tagType, i));
			if (data == null || data.ItemCode1 != valanceAwakenData.ItemCode1)
			{
				break;
			}
			steps.Add(data);
		}
		if (steps.Count < 2)
		{
			Show(visible: false);
			return;
		}
		long gold = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gold;
		long num2 = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(valanceAwakenData.ItemCode1);
		int num3 = baseStar + 1;
		long num4 = 0L;
		long num5 = 0L;
		for (int j = 0; j < steps.Count; j++)
		{
			num4 += steps[j].ReqGold;
			num5 += steps[j].ItemCount1;
			if (num4 <= gold && num5 <= num2)
			{
				num3 = baseStar + 1 + j;
			}
		}
		EnsureSlider();
		Show(visible: true);
		updating = true;
		slider.numberOfSteps = steps.Count;
		slider.value = (float)(num3 - baseStar - 1) / (float)(steps.Count - 1);
		updating = false;
		Apply(num3, refresh: false);
	}

	private void Apply(int star, bool refresh)
	{
		target = Mathf.Clamp(star, baseStar + 1, baseStar + steps.Count);
		long num = 0L;
		long num2 = 0L;
		for (int i = 0; i < target - baseStar; i++)
		{
			num += steps[i].ReqGold;
			num2 += steps[i].ItemCount1;
		}
		ValanceAwakenData valanceAwakenData = steps[0];
		ValanceAwakenData valanceAwakenData2 = new ValanceAwakenData
		{
			Tier = valanceAwakenData.Tier,
			DetailIndex = valanceAwakenData.DetailIndex,
			CreatureTagType = valanceAwakenData.CreatureTagType,
			Star = target,
			ReqGold = (int)Math.Min(num, 2147483647L),
			ItemCode1 = valanceAwakenData.ItemCode1,
			ItemCount1 = (int)Math.Min(num2, 2147483647L)
		};
		EquipItemInfo equipItemInfo = window.EquipItemInfo;
		SetData.Invoke(window, new object[1] { valanceAwakenData2 });
		SetAfterStar.Invoke(window, new object[1] { target });
		SetAfterStat.Invoke(window, new object[1] { StatCalculator.GetEquipItemStatData(equipItemInfo.ItemIndex, target, equipItemInfo.Level) });
		ItemIconComponent itemIconComponent = ResultComp.GetValue(window) as ItemIconComponent;
		if (itemIconComponent != null)
		{
			EquipItemInfo equipItemInfo2 = equipItemInfo.Clone();
			equipItemInfo2.Star = target;
			equipItemInfo2.SoulWeaponInfo = equipItemInfo.SoulWeaponInfo;
			equipItemInfo2.Identified = 1;
			equipItemInfo2.ExtraOptionIndex1 = equipItemInfo.ExtraOptionIndex1;
			equipItemInfo2.ExtraOptionIndex2 = equipItemInfo.ExtraOptionIndex2;
			equipItemInfo2.ExtraOptionStep1 = equipItemInfo.ExtraOptionStep1;
			equipItemInfo2.ExtraOptionStep2 = equipItemInfo.ExtraOptionStep2;
			itemIconComponent.SetStarBlink(isEnabled: false);
			itemIconComponent.Open(window.ItemData, equipItemInfo2, null);
			itemIconComponent.SetStarBlink();
		}
		if (valueLabel != null)
		{
			valueLabel.text = "★" + target;
		}
		if (refresh)
		{
			SetupMaterialIcon.Invoke(window, null);
			window.SetReqGoldLabel();
			window.SetButton();
			window.SetItemStat();
		}
	}

	private void OnSliderChanged()
	{
		if (!updating && steps.Count >= 2 && !(window == null))
		{
			int num = baseStar + 1 + Mathf.RoundToInt(slider.value * (float)(steps.Count - 1));
			if (num != target)
			{
				Apply(num, refresh: true);
			}
		}
	}

	private void Show(bool visible)
	{
		if (root != null && root.activeSelf != visible)
		{
			root.SetActive(visible);
		}
	}

	private void EnsureSlider()
	{
		if (!(root != null))
		{
			Transform parent = window.LabelReqMaterialCount.transform.parent.parent;
			root = new GameObject("SprkTechnomagicTarget");
			root.transform.SetParent(parent, worldPositionStays: false);
			root.transform.localPosition = new Vector3(5f, 17f, 0f);
			GameObject gameObject = new GameObject("SprkTechnomagicSliderSource");
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
			slider.name = "SprkTargetStarSlider";
			slider.onChange.Clear();
			EventDelegate.Add(slider.onChange, OnSliderChanged);
			int num = window.LabelReqMaterialCount.depth + 1;
			int num2 = int.MaxValue;
			UIWidget[] componentsInChildren2 = slider.GetComponentsInChildren<UIWidget>(includeInactive: true);
			UIWidget[] array = componentsInChildren2;
			foreach (UIWidget uIWidget in array)
			{
				num2 = Math.Min(num2, uIWidget.depth);
			}
			array = componentsInChildren2;
			foreach (UIWidget uIWidget2 in array)
			{
				uIWidget2.leftAnchor.target = null;
				uIWidget2.rightAnchor.target = null;
				uIWidget2.topAnchor.target = null;
				uIWidget2.bottomAnchor.target = null;
				uIWidget2.depth = num + uIWidget2.depth - num2;
			}
			slider.transform.localPosition = Vector3.zero;
			slider.transform.localScale = Vector3.one;
			Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(root.transform, slider.transform, considerInactive: true);
			float num3 = ((bounds.size.x > 380f) ? (380f / bounds.size.x) : 1f);
			slider.transform.localScale = new Vector3(num3, num3, 1f);
			slider.transform.localPosition = new Vector3((0f - bounds.center.x) * num3, (0f - bounds.center.y) * num3, 0f);
			GameObject gameObject3 = UnityEngine.Object.Instantiate(window.LabelReqMaterialCount.gameObject, root.transform, worldPositionStays: false);
			gameObject3.name = "SprkTargetStarLabel";
			valueLabel = gameObject3.GetComponent<UILabel>();
			valueLabel.color = window.ColorGoldEnough;
			valueLabel.pivot = UIWidget.Pivot.Left;
			valueLabel.overflowMethod = UILabel.Overflow.ResizeFreely;
			valueLabel.depth = num;
			gameObject3.transform.localScale = Vector3.one;
			gameObject3.transform.localPosition = new Vector3(214f, 0f, 0f);
			NGUITools.SetLayer(root, window.gameObject.layer);
			NGUITools.MarkParentAsChanged(root);
		}
	}
}
