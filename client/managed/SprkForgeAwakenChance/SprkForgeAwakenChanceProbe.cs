using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NJit;
using NVespa.NSingleton;
using UnityEngine;

public static class SprkForgeAwakenChanceProbe
{
	private const BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static void Check(bool ok, string message)
	{
		if (!ok)
		{
			throw new Exception("Forge chance: " + message);
		}
	}

	public static string Run(string mode)
	{
		TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		object value = instance.GetType().GetProperty("AccountManagers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(instance, null);
		FieldInfo field = value.GetType().GetField("UserManager");
		FieldInfo field2 = value.GetType().GetField("EquipItemManager");
		FieldInfo field3 = value.GetType().GetField("ItemManager");
		FieldInfo field4 = value.GetType().GetField("HeroManager");
		object value2 = field.GetValue(value);
		object value3 = field2.GetValue(value);
		object value4 = field3.GetValue(value);
		object value5 = field4.GetValue(value);
		GameObject gameObject = new GameObject("ForgeChanceFixture");
		gameObject.SetActive(value: false);
		bool flag = mode == "preview";
		UIRoot uIRoot = UnityEngine.Object.FindObjectOfType<UIRoot>();
		if (uIRoot != null)
		{
			gameObject.transform.SetParent(uIRoot.transform, worldPositionStays: false);
		}
		try
		{
			field.SetValue(value, new UserManager(instance, new UserInfo
			{
				AccountId = 123456L,
				Gold = 1000000000L
			}, new PlayerMiscInfo(), new PlayerGuildPointInfo()));
			field2.SetValue(value, new NGame2.NAccount.UserEquipItemManager());
			instance.InitItemInfos(new ItemInfo[0]);
			instance.InitHeroInfos(new HeroInfo[0]);
			ItemData<EquipItemData>[] source = (from d in GameRoot.Instance.ItemDataContainer.GetAllData().OfType<ItemData<EquipItemData>>()
				where d.SubData != null
				select d).ToArray();
			List<string> list = new List<string>();
			int[] array = new int[3] { 0, 1, 2 };
			foreach (int category in array)
			{
				ItemData<EquipItemData> row = source.First(delegate(ItemData<EquipItemData> d)
				{
					EquipItemInfo equipItemInfo = new EquipItemInfo
					{
						ItemIndex = d.Index,
						SlotIndex = 1,
						Star = 3,
						Level = 90
					};
					bool num9;
					if (category != 1)
					{
						if (category != 2)
						{
							if (equipItemInfo.IsUniqueWeaponWithoutClassWeapon)
							{
								goto IL_006f;
							}
							num9 = !equipItemInfo.IsUniqueTreasure;
						}
						else
						{
							num9 = equipItemInfo.IsUniqueTreasure;
						}
					}
					else
					{
						num9 = equipItemInfo.IsUniqueWeaponWithoutClassWeapon;
					}
					if (num9 && equipItemInfo.CLI_CanAwaken())
					{
						return equipItemInfo.CLI_CanUse_as_AwakenMaterial();
					}
					goto IL_006f;
					IL_006f:
					return false;
				});
				EquipItemInfo[] array2 = (from i in Enumerable.Range(1, 13)
					select new EquipItemInfo
					{
						ItemIndex = row.Index,
						SlotIndex = i,
						Star = ((i == 1) ? 3 : 0),
						Level = 90
					}).ToArray();
				array2[12].Locked = 1;
				instance.InitEquipItemInfos(array2);
				GameObject gameObject2 = VespaResource.Load<GameObject>("Resources/Prefab2/Window/Equip/HeroEquipAwaken");
				Check(gameObject2 != null, "native prefab missing");
				HeroEquipAwaken component = UnityEngine.Object.Instantiate(gameObject2, gameObject.transform, worldPositionStays: false).GetComponent<HeroEquipAwaken>();
				typeof(HeroEquipAwaken).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(component, null);
				component.SetTargetItem(array2[0]);
				if (mode == "inspect-layout")
				{
					List<string> list2 = new List<string>();
					Transform[] componentsInChildren = component.GetComponentsInChildren<Transform>(includeInactive: true);
					foreach (Transform transform in componentsInChildren)
					{
						string text = transform.name;
						Transform parent = transform.parent;
						int num3 = 0;
						while (parent != null && parent != component.transform)
						{
							text = parent.name + "/" + text;
							parent = parent.parent;
							num3++;
						}
						if (num3 <= 6)
						{
							UIWidget component2 = transform.GetComponent<UIWidget>();
							UIPanel component3 = transform.GetComponent<UIPanel>();
							list2.Add(text + " pos=" + transform.localPosition.ToString() + " scale=" + transform.localScale.ToString() + ((component2 == null) ? "" : (" widget=" + component2.width + "x" + component2.height + " pivot=" + component2.pivot.ToString() + " anchors=" + component2.isAnchored)) + ((component3 == null) ? "" : (" clip=" + component3.baseClipRegion.ToString())));
						}
					}
					return string.Join("\n", list2.ToArray());
				}
				SprkForgeAwakenChance component4 = component.GetComponent<SprkForgeAwakenChance>();
				Check(component4 != null, "helper absent");
				UISlider componentInChildren = component4.GetComponentInChildren<UISlider>(includeInactive: true);
				int[] array3 = (int[])typeof(SprkForgeAwakenChance).GetField("chances", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(component4);
				Check(componentInChildren != null && componentInChildren.numberOfSteps == array3.Length && array3.Length != 0, "dynamic slider steps");
				Check(array3.SequenceEqual(from x in array3.Distinct()
					orderby x
					select x) && array3.All((int x) => x > 0), "stops must be unique positive chances in ascending order");
				MethodInfo method = typeof(HeroEquipAwaken).GetMethod("GetTotalAwakenSuccessRate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				int num4 = Math.Min(100, (int)method.Invoke(component, null));
				Check(num4 > 0 && component.GetMaterialEquipSlotIndexArray().Length != 0, "maximum default selected no materials");
				for (int num5 = 0; num5 < array3.Length; num5++)
				{
					componentInChildren.value = ((array3.Length == 1) ? 1f : ((float)num5 / (float)(array3.Length - 1)));
					EventDelegate.Execute(componentInChildren.onChange);
					int num6 = Math.Min(100, (int)method.Invoke(component, null));
					int[] array4 = component.GetMaterialEquipSlotIndexArray() ?? new int[0];
					Check(!array4.Contains(1) && !array4.Contains(13) && array4.Distinct().Count() == array4.Length, "target, locked or duplicate selected");
					Check(array4.Length != 0 && num6 == array3[num5], "stop differs from actual native chance");
				}
				UILabel uILabel = component4.GetComponentsInChildren<UILabel>(includeInactive: true).First((UILabel l) => l.name == "SprkSuccessRateLabel");
				Check(uILabel.text == "Success: " + num4 + "%", "readout differs from native chance");
				List<ItemIconComponent> list3 = (List<ItemIconComponent>)typeof(HeroEquipAwaken).GetField("_ListMaterialItems", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(component);
				typeof(HeroEquipAwaken).GetMethod("OnClickEquipItem", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(component, new object[1] { list3[0] });
				int num7 = ((component.GetMaterialEquipSlotIndexArray() != null) ? component.GetMaterialEquipSlotIndexArray().Length : 0);
				typeof(HeroEquipAwaken).GetMethod("UpdateAwakeUITask", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(component, null);
				Check((component.GetMaterialEquipSlotIndexArray() ?? new int[0]).Length == num7, "manual selection overwritten");
				array2[0].UpgradeStarFailBonus = 10;
				component.SetTargetItem(array2[0]);
				Check(componentInChildren.value == 1f, "failure refresh did not reset maximum");
				component.SetTargetItem(null);
				Check(!componentInChildren.transform.parent.gameObject.activeSelf, "cleared target remains interactive");
				array2[0].Star = 1;
				array2[0].UpgradeStarFailBonus = 0;
				instance.InitEquipItemInfos(array2.Take(3).ToArray());
				component.RefreshItemList();
				component.SetTargetItem(array2[0]);
				array3 = (int[])typeof(SprkForgeAwakenChance).GetField("chances", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(component4);
				Check(componentInChildren.numberOfSteps == 2 && array3.SequenceEqual(new int[2] { 50, 100 }) && componentInChildren.value == 1f, "two copies must give exactly 50% and 100% stops");
				componentInChildren.value = 0f;
				EventDelegate.Execute(componentInChildren.onChange);
				Check((int)method.Invoke(component, null) == 50 && component.GetMaterialEquipSlotIndexArray().Length == 1, "50% stop should select one copy");
				componentInChildren.value = 1f;
				EventDelegate.Execute(componentInChildren.onChange);
				Check((int)method.Invoke(component, null) == 100 && component.GetMaterialEquipSlotIndexArray().Length == 2, "100% stop should select two copies");
				for (int num8 = 2; num8 <= 4; num8++)
				{
					array2[0].Star = num8;
					component.SetTargetItem(array2[0]);
					array3 = (int[])typeof(SprkForgeAwakenChance).GetField("chances", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(component4);
					Check(array3.Length == 2 && componentInChildren.numberOfSteps == 2 && componentInChildren.value == 1f, "next star should retain just two achievable material counts");
					componentInChildren.value = 0f;
					EventDelegate.Execute(componentInChildren.onChange);
					Check((int)method.Invoke(component, null) == array3[0] && component.GetMaterialEquipSlotIndexArray().Length == 1, "next-star first stop");
					componentInChildren.value = 1f;
					EventDelegate.Execute(componentInChildren.onChange);
					Check((int)method.Invoke(component, null) == array3[1] && component.GetMaterialEquipSlotIndexArray().Length == 2, "next-star maximum stop");
				}
				instance.InitEquipItemInfos(array2.Take(2).ToArray());
				component.RefreshItemList();
				component.SetTargetItem(array2[0]);
				Check(componentInChildren.numberOfSteps == 1 && !componentInChildren.enabled && componentInChildren.value == 1f && component.GetMaterialEquipSlotIndexArray().Length == 1, "single stop must select the only available copy without dragging");
				instance.InitEquipItemInfos(new EquipItemInfo[1] { array2[0] });
				component.RefreshItemList();
				component.SetTargetItem(array2[0]);
				Check(componentInChildren.numberOfSteps == 0 && !componentInChildren.enabled && uILabel.text == "Success: 0%" && component.GetMaterialEquipSlotIndexArray() == null, "empty inventory must disable dragging and select nothing");
				if (category != 0)
				{
					ItemData itemData = GameRoot.Instance.ItemDataContainer.GetAllData().First((ItemData d) => d.Type == (ItemType)((category == 1) ? 42 : 43) && d.GetSubData<AwakenStoneData>() != null && (d.GetSubData<AwakenStoneData>().ItemIndices == null || d.GetSubData<AwakenStoneData>().ItemIndices.Length == 0));
					instance.InitEquipItemInfos(new EquipItemInfo[2]
					{
						array2[0],
						array2[12]
					});
					instance.InitItemInfos(new ItemInfo[1]
					{
						new ItemInfo
						{
							ItemIndex = itemData.Index,
							Count = 5
						}
					});
					component.RefreshItemList();
					component.SetTargetItem(array2[0]);
					int[] materialPublicAwakenItemIndexArray = component.GetMaterialPublicAwakenItemIndexArray();
					Check(materialPublicAwakenItemIndexArray != null && materialPublicAwakenItemIndexArray.Length == 2 && materialPublicAwakenItemIndexArray[0] == itemData.Index && materialPublicAwakenItemIndexArray[1] > 0 && materialPublicAwakenItemIndexArray[1] <= 5, "stone quantities missing from native request");
					Check(component.GetMaterialEquipSlotIndexArray().Length == 0, "stone plan selected locked gear");
					componentInChildren.value = 0f;
					EventDelegate.Execute(componentInChildren.onChange);
					Check(component.GetMaterialPublicAwakenItemIndexArray()[1] == 1, "leftmost stone stop must select one material");
					componentInChildren.value = 1f;
					EventDelegate.Execute(componentInChildren.onChange);
					instance.InitItemInfos(new ItemInfo[0]);
				}
				list.Add(((category == 0) ? "normal" : ((category == 1) ? "UW" : "UT")) + "=" + row.Index + ": " + num4 + "%");
				if (flag && category == 2)
				{
					gameObject.SetActive(value: true);
				}
				else
				{
					UnityEngine.Object.Destroy(component.gameObject);
				}
			}
			return "PASS: native Forge dynamic stops, exact 50%/100% two-copy example, next-star rates, single/empty inventories, maximum defaults, native request arrays, stones, locks, manual removal and failure refresh for " + string.Join(", ", list.ToArray());
		}
		finally
		{
			field.SetValue(value, value2);
			field2.SetValue(value, value3);
			field3.SetValue(value, value4);
			field4.SetValue(value, value5);
			if (!flag)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
	}
}
