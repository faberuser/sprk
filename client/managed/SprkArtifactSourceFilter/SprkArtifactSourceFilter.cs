using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NGame2.NUI;
using NGame2.NUI.NWindow;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using UnityEngine;

public static class SprkArtifactSourceFilter
{
	private static readonly string[] Groups = new string[7] { "GACHA_ALL_IN_ONE_ALL_220920_NORMAL_ARTIFACT_GROUP", "GACHA_ALL_IN_ONE_ALL_220920_EVENT_ARTIFACT_GROUP", "GACHA_ALL_IN_ONE_ALL_220920_WORLD_1_ARTIFACT_GROUP", "GACHA_ALL_IN_ONE_ALL_220920_VELKAZAR_1_ARTIFACT_GROUP", "GACHA_ALL_IN_ONE_ALL_220920_SHAKMEH_1_ARTIFACT_GROUP", "GACHA_ALL_IN_ONE_ALL_220920_KINGSPASS_ARTIFACT_GROUP", "GACHA_ALL_IN_ONE_ALL_220920_PUNISHMENT_1_ARTIFACT_GROUP" };

	private static readonly string[] Labels = new string[8] { "Normal", "Event", "World Boss", "Velkazar", "Shakmeh", "King's Pass", "Divine Punishment", "Other" };

	private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

	private static readonly FieldInfo TicketField = typeof(WeaponUniqueSelect).GetField("_useItemData", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo SelectionField = typeof(WeaponUniqueSelect).GetField("_buyItemData", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo ClearDescription = typeof(WeaponUniqueSelect).GetMethod("SetItemDescription", BindingFlags.Instance | BindingFlags.NonPublic);

	private static Dictionary<string, int> categories;

	private static void AddGroup(Dictionary<string, int> result, string group, int category, HashSet<string> visiting)
	{
		if (!visiting.Add(group))
		{
			return;
		}
		ItemGroupData data = GameRoot.Instance.ItemGroupDataContainer.GetData(group);
		if (data != null && data.SubDataList != null)
		{
			foreach (ItemGroupData.SubData subData in data.SubDataList)
			{
				if (GameRoot.Instance.ItemGroupDataContainer.GetData(subData.ItemCode) != null)
				{
					AddGroup(result, subData.ItemCode, category, visiting);
				}
				else if (!string.IsNullOrEmpty(subData.ItemCode))
				{
					result[subData.ItemCode] = category;
				}
			}
		}
		visiting.Remove(group);
	}

	public static int Category(string code)
	{
		if (categories == null)
		{
			Dictionary<string, int> result = new Dictionary<string, int>(StringComparer.Ordinal);
			for (int i = 0; i < Groups.Length; i++)
			{
				AddGroup(result, Groups[i], 101 + i, new HashSet<string>());
			}
			categories = result;
		}
		if (code == null || !categories.TryGetValue(code, out var value))
		{
			return 108;
		}
		return value;
	}

	public static void Reset(SelectFilter filter)
	{
		SprkArtifactSourceState component = filter.GetComponent<SprkArtifactSourceState>();
		if (component != null)
		{
			component.Active = false;
			component.Fresh = true;
		}
	}

	public static bool Build(SelectFilter filter, UIComboBox sub, int type, SelectFilter.Callback callback)
	{
		SprkArtifactSourceState sprkArtifactSourceState = filter.GetComponent<SprkArtifactSourceState>();
		WeaponUniqueSelect componentInParent = filter.GetComponentInParent<WeaponUniqueSelect>(includeInactive: true);
		if (type != 4 || componentInParent == null || sub == null)
		{
			if (sprkArtifactSourceState != null)
			{
				sprkArtifactSourceState.Active = false;
			}
			return false;
		}
		WeaponUniqueSelectItemData weaponUniqueSelectItemData = ((!(TicketField.GetValue(componentInParent) is ItemData itemData)) ? null : itemData.GetSubData<WeaponUniqueSelectItemData>());
		if (weaponUniqueSelectItemData == null || weaponUniqueSelectItemData.ItemIndices == null)
		{
			return false;
		}
		if (sprkArtifactSourceState == null)
		{
			sprkArtifactSourceState = filter.gameObject.AddComponent<SprkArtifactSourceState>();
		}
		int key = ((!sprkArtifactSourceState.Fresh) ? sub.CurrentKey : 0);
		sprkArtifactSourceState.Active = true;
		sprkArtifactSourceState.Fresh = false;
		HashSet<int> hashSet = new HashSet<int>();
		int[] itemIndices = weaponUniqueSelectItemData.ItemIndices;
		foreach (int index in itemIndices)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(index);
			if (data != null)
			{
				hashSet.Add(Category(data.Code));
			}
		}
		sub.Clear();
		sub.AddItem(0, LocalizationManager.GetString(FilterSelect.FILTER_SELECT_ALL));
		for (int j = 0; j < Labels.Length; j++)
		{
			if (hashSet.Contains(101 + j))
			{
				sub.AddItem(101 + j, SprkText.Get(Labels[j]));
			}
		}
		sub.Reposition();
		if (!sub.SelectItem(key, isNotify: false))
		{
			sub.SelectFirstItem(isNotify: false);
		}
		callback?.Invoke();
		return true;
	}

	public static bool Matches(SelectFilter filter, EquipItemData equip)
	{
		SprkArtifactSourceState component = filter.GetComponent<SprkArtifactSourceState>();
		if (component == null || !component.Active || filter.SubComboBoxCurrentKey <= 0)
		{
			return true;
		}
		if (equip != null)
		{
			return Category(equip.EquipCode) == filter.SubComboBoxCurrentKey;
		}
		return false;
	}

	private static bool IsActive(WeaponUniqueSelect window)
	{
		return window.GetComponentsInChildren<SprkArtifactSourceState>(includeInactive: true).Any((SprkArtifactSourceState s) => s.Active);
	}

	public static void BeforeList(WeaponUniqueSelect window)
	{
		if (IsActive(window))
		{
			SelectionField.SetValue(window, null);
			ClearDescription.Invoke(window, new object[1]);
			UIHelper.EnableButton(window.Button_Buy, enable: false, grayscaleIfDisable: true);
		}
	}

	public static void AfterList(WeaponUniqueSelect window)
	{
		if (IsActive(window) && !(window.ItemGrid == null) && !(window.ItemGrid.ScrollView == null))
		{
			UIScrollView scrollView = window.ItemGrid.ScrollView;
			scrollView.DisableSpring();
			scrollView.currentMomentum = Vector3.zero;
			scrollView.ResetPosition();
		}
	}
}
