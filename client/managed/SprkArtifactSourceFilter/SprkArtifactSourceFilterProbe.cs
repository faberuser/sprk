using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NNguiExtension;
using NShared;
using NShared.NJit;
using NVespa.NSingleton;
using UnityEngine;

public static class SprkArtifactSourceFilterProbe
{
	private sealed class PurchaseProbe : WeaponUniqueSelect.Listener
	{
		public int Count;

		public int Ticket;

		public int Artifact;

		public void OnSelectWeaponUnique(int useItemIndex, int buyItemIndex, Action onSelect)
		{
			Count++;
			Ticket = useItemIndex;
			Artifact = buyItemIndex;
		}
	}

	private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

	private static void Check(bool value, string message)
	{
		if (!value)
		{
			throw new Exception("Artifact source filter: " + message);
		}
	}

	public static string Run()
	{
		TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		object value = instance.GetType().GetProperty("AccountManagers", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance, null);
		FieldInfo field = value.GetType().GetField("EquipItemManager");
		FieldInfo field2 = value.GetType().GetField("ItemManager");
		object value2 = field.GetValue(value);
		object value3 = field2.GetValue(value);
		int[] owned = new int[3] { 150001, 150043, 150900 };
		NGame2.NAccount.UserEquipItemManager userEquipItemManager = new NGame2.NAccount.UserEquipItemManager();
		userEquipItemManager.Reset(owned.Select((int id, int index) => new EquipItemInfo
		{
			ItemIndex = id,
			SlotIndex = index + 1
		}).ToArray());
		field.SetValue(value, userEquipItemManager);
		field2.SetValue(value, new NGame2.NAccount.UserItemManager());
		GameObject gameObject = new GameObject("ArtifactSourceFilterFixture");
		gameObject.SetActive(value: false);
		try
		{
			GameObject gameObject2 = VespaResource.Load<GameObject>("Resources/Prefab2/Window/Inventory/WeaponUniqueSelect");
			Check(gameObject2 != null, "native ticket prefab");
			WeaponUniqueSelect window = UnityEngine.Object.Instantiate(gameObject2, gameObject.transform, worldPositionStays: false).GetComponent<WeaponUniqueSelect>();
			typeof(WeaponUniqueSelect).GetField("SelectType", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(window, SelectFilter.SelectType.SELECT_EQUIP_ARTIFACT);
			SelectFilter selectFilter = (SelectFilter)typeof(WeaponUniqueSelect).GetField("SelectFilter", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(window);
			UIComboBox uIComboBox = (UIComboBox)typeof(SelectFilter).GetField("ComboBoxMain", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(selectFilter);
			UIComboBox uIComboBox2 = (UIComboBox)typeof(SelectFilter).GetField("ComboBoxSub", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(selectFilter);
			MethodInfo method = typeof(WeaponUniqueSelect).GetMethod("Init", BindingFlags.Instance | BindingFlags.NonPublic);
			MethodInfo rebuild = typeof(WeaponUniqueSelect).GetMethod("AddEquipItemByFilter", BindingFlags.Instance | BindingFlags.NonPublic);
			FieldInfo field3 = typeof(WeaponUniqueSelect).GetField("_shopItemComponent", BindingFlags.Instance | BindingFlags.NonPublic);
			FieldInfo itemField = typeof(WeaponUniqueItemComponent).GetField("_itemData", BindingFlags.Instance | BindingFlags.NonPublic);
			FieldInfo field4 = typeof(WeaponUniqueSelect).GetField("_buyItemData", BindingFlags.Instance | BindingFlags.NonPublic);
			List<string> list = new List<string>();
			int num = 0;
			int[] array = new int[2] { 2013, 3030 };
			for (int num2 = 0; num2 < array.Length; num2++)
			{
				int num3 = array[num2];
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(num3);
				Check((bool)method.Invoke(window, new object[1] { data }), "ticket initialized " + num3);
				int[] itemIndices = data.GetSubData<WeaponUniqueSelectItemData>().ItemIndices;
				selectFilter.InitFilter(SelectFilter.SelectType.SELECT_EQUIP_ARTIFACT, delegate
				{
					rebuild.Invoke(window, null);
				});
				Check(uIComboBox2.CurrentKey == 0, "ticket starts at All");
				int[] array2 = (from id in itemIndices.Select((int id) => SprkArtifactSourceFilter.Category(GameRoot.Instance.ItemDataContainer.GetData(id).Code)).Distinct()
					orderby id
					select id).ToArray();
				Check(uIComboBox2.GetItemCount() == array2.Length + 1, "only available source categories " + num3);
				Check(!array2.Contains(108), "every Lua/normal artifact has a known reward source");
				foreach (int source in new int[1].Concat(array2))
				{
					Check(uIComboBox2.SelectItem(source), "source choice " + source);
					int[] array3 = new int[3] { 0, 5, 4 };
					for (int num4 = 0; num4 < array3.Length; num4++)
					{
						int ownership = array3[num4];
						Check(uIComboBox.SelectItem(ownership), "ownership choice " + ownership);
						Check(uIComboBox2.CurrentKey == source, "source survives ownership changes");
						int[] array4 = (from WeaponUniqueItemComponent c in (IEnumerable)field3.GetValue(window)
							select ((ItemData)itemField.GetValue(c)).Index).ToArray();
						int[] second = itemIndices.Where((int id) => (source == 0 || SprkArtifactSourceFilter.Category(GameRoot.Instance.ItemDataContainer.GetData(id).Code) == source) && (ownership == 0 || ((ownership != 5) ? (!owned.Contains(id)) : owned.Contains(id)))).ToArray();
						Check(array4.SequenceEqual(second), "native visible list/pool/order " + num3 + "/" + source + "/" + ownership);
						Check(array4.Distinct().Count() == array4.Length, "no duplicate cards");
						num++;
					}
				}
				uIComboBox.SelectFirstItem();
				uIComboBox2.SelectFirstItem();
				PurchaseProbe purchaseProbe = new PurchaseProbe();
				typeof(WeaponUniqueSelect).GetField("_listener", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(window, purchaseProbe);
				window.OnSelect(GameRoot.Instance.ItemDataContainer.GetData(itemIndices[0]));
				Check(window.Button_Buy.isEnabled, "choosing a visible artifact enables Acquire");
				window.OnClickBuy();
				Check(purchaseProbe.Count == 1 && purchaseProbe.Ticket == num3 && purchaseProbe.Artifact == itemIndices[0], "native Acquire uses selected ticket and artifact");
				uIComboBox2.SelectItem(array2.Last());
				Check(field4.GetValue(window) == null && !window.Button_Buy.isEnabled, "filter clears selection and disables Acquire");
				window.OnClickBuy();
				Check(purchaseProbe.Count == 1, "hidden selection cannot be acquired");
				method.Invoke(window, new object[1] { data });
				selectFilter.InitFilter(SelectFilter.SelectType.SELECT_EQUIP_ARTIFACT, delegate
				{
					rebuild.Invoke(window, null);
				});
				Check(uIComboBox2.CurrentKey == 0 && uIComboBox.CurrentKey == 0, "reopening ticket resets filters");
				list.Add("ticket=" + num3 + " choices=" + itemIndices.Length + " categories=" + string.Join(",", array2.Select((int s) => s.ToString()).ToArray()));
			}
			int[][] array5 = new int[7][]
			{
				new int[2] { 1, 101 },
				new int[2] { 51, 102 },
				new int[2] { 24, 103 },
				new int[2] { 43, 104 },
				new int[2] { 517, 105 },
				new int[2] { 900, 106 },
				new int[2] { 1000, 107 }
			};
			foreach (int[] array6 in array5)
			{
				Check(SprkArtifactSourceFilter.Category("ARTIFACT_UNIQUE_" + array6[0]) == array6[1], "native reward-group mapping " + array6[0]);
			}
			int num5 = 0;
			foreach (WeaponUniqueSelectItemData allDatum in GameRoot.Instance.WeaponUniqueSelectItemDataContainer.GetAllData())
			{
				if (allDatum.ItemCode == null || !allDatum.ItemCode.StartsWith("ARTIFACT_") || allDatum.ItemIndices == null || allDatum.ItemIndices.Length == 0)
				{
					continue;
				}
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(allDatum.ItemCode);
				if (dataByCode == null)
				{
					continue;
				}
				Check((bool)method.Invoke(window, new object[1] { dataByCode }), "restricted ticket initialization");
				selectFilter.InitFilter(SelectFilter.SelectType.SELECT_EQUIP_ARTIFACT, delegate
				{
				});
				int[] array7 = (from id in allDatum.ItemIndices
					select GameRoot.Instance.ItemDataContainer.GetData(id) into item
					where item != null
					select SprkArtifactSourceFilter.Category(item.Code)).Distinct().ToArray();
				Check(uIComboBox2.GetItemCount() == array7.Length + 1, "restricted dropdown contains only ticket sources " + allDatum.ItemCode);
				array = array7;
				foreach (int key in array)
				{
					Check(uIComboBox2.SelectItem(key), "restricted source available");
				}
				foreach (int item in Enumerable.Range(101, 8).Except(array7))
				{
					Check(!uIComboBox2.SelectItem(item), "outside-pool source absent");
				}
				num5++;
			}
			Check(num5 > 2, "restricted ticket coverage");
			Check(SprkArtifactSourceFilter.Category("UNKNOWN_FUTURE_ARTIFACT") == 108, "unknown artifacts remain accessible as Other");
			selectFilter.InitFilter(SelectFilter.SelectType.SELECT_EQUIP_UNIQUE, delegate
			{
			});
			Check(!selectFilter.GetComponent<SprkArtifactSourceState>().Active, "unique weapon filter stays native");
			Check(SprkArtifactSourceFilter.Matches(selectFilter, new EquipItemData
			{
				EquipCode = "UNKNOWN"
			}), "non-artifact filtering unchanged");
			list.Add("PASS: " + num + " native source/ownership list comparisons, " + num5 + " artifact ticket dropdowns, source preservation, ticket pool/order, no duplicates, clearing selection, reopen reset, seven reward-group sources and non-artifact isolation.");
			return string.Join("\n", list.ToArray());
		}
		finally
		{
			field.SetValue(value, value2);
			field2.SetValue(value, value3);
			UnityEngine.Object.Destroy(gameObject);
		}
	}
}
