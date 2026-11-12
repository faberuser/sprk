using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NJit;
using UnityEngine;

public sealed class SprkPetPortalTests : MonoBehaviour, PortalContentsComponent.Listener
{
	private PortalRenewalData clicked;

	private bool editing;

	public void OnClickContentsButton(PortalRenewalData row)
	{
		clicked = row;
	}

	public void ChangeContentsTitle(string title)
	{
	}

	public void MoveNode(int chapter, int dungeon)
	{
	}

	public bool IsRunningEditMode()
	{
		return editing;
	}

	private IEnumerator Start()
	{
		yield return new WaitForSecondsRealtime(3f);
		string environmentVariable = Environment.GetEnvironmentVariable("SPRK_PET_PORTAL_TEST");
		Directory.CreateDirectory(environmentVariable);
		try
		{
			GameObject obj = Resources.Load<GameObject>("Prefab2/Window/Portal/PortalRenewalComponents/PortalPethouseComponent");
			if (obj == null)
			{
				throw new Exception("Native Pet Portal prefab missing");
			}
			GameObject gameObject = new GameObject("Inactive Pet Portal fixture");
			gameObject.SetActive(value: false);
			PortalPethouseComponent component = UnityEngine.Object.Instantiate(obj, gameObject.transform).GetComponent<PortalPethouseComponent>();
			typeof(PortalContentsComponent).GetField("_listener", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(component, this);
			PortalRenewalSubCategory[] array = new PortalRenewalSubCategory[3]
			{
				PortalRenewalSubCategory.Pet,
				PortalRenewalSubCategory.PetIncubator,
				PortalRenewalSubCategory.PetHouse
			};
			PortalRenewalDataContainer portalRenewalDataContainer = new PortalRenewalDataContainer();
			Type type = portalRenewalDataContainer.GetType();
			while (type != null)
			{
				PropertyInfo property = type.GetProperty("Packer", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
				if (!(property == null))
				{
					((JitPacker)property.GetValue(portalRenewalDataContainer, null)).Deserialize(File.ReadAllBytes(Path.Combine(Application.dataPath, "Documents/Patch/StandaloneWindows/TableJit/PortalRenewalTable.jit")));
					break;
				}
				type = type.BaseType;
			}
			IList<PortalRenewalData> data = portalRenewalDataContainer.GetData(PortalRenewalCategory.Pet);
			if (data == null || data.Count != 4)
			{
				throw new Exception("Actual baked Portal lookup did not repair Pet destinations");
			}
			if (SprkPetPresentation.Contents(null, PortalRenewalCategory.Pet).Count != 4)
			{
				throw new Exception("Null Pet table not repaired");
			}
			List<PortalRenewalData> list = new List<PortalRenewalData>();
			if (SprkPetPresentation.Contents(list, PortalRenewalCategory.Raid) != list || SprkPetPresentation.Contents(null, PortalRenewalCategory.Raid) != null)
			{
				throw new Exception("Unrelated Portal category changed");
			}
			int num = 0;
			for (int i = 0; i < 8; i++)
			{
				List<PortalRenewalData> list2 = new List<PortalRenewalData>();
				for (int j = 0; j < array.Length; j++)
				{
					if ((i & (1 << j)) != 0)
					{
						list2.Add(new PortalRenewalData
						{
							Category = PortalRenewalCategory.Pet,
							SubCategory = array[j],
							MoveTaskType = ((array[j] == PortalRenewalSubCategory.PetIncubator) ? ClientActivityType.OpenPetIncubator : ClientActivityType.OpenPetHouse),
							OpenCondition = new string[1] { "native-condition" }
						});
					}
				}
				IList<PortalRenewalData> list3 = SprkPetPresentation.Contents(list2, PortalRenewalCategory.Pet);
				if (list3.Count != 4 || list2.Count != CountBits(i))
				{
					throw new Exception("Missing/duplicate Pet destinations or input mutated");
				}
				foreach (PortalRenewalData item in list2)
				{
					if (!list3.Contains(item) || item.OpenCondition[0] != "native-condition")
					{
						throw new Exception("Native row/conditions replaced");
					}
				}
				if (SprkPetPresentation.Contents(list3, PortalRenewalCategory.Pet).Count != list3.Count)
				{
					throw new Exception("Pet reconstruction not idempotent");
				}
				if (component.ButtonDatas.Count != 2)
				{
					throw new Exception("Expected two native Pet buttons");
				}
				foreach (PortalShakmehComponent.ButtonData buttonData in component.ButtonDatas)
				{
					buttonData.portalData = null;
					foreach (PortalRenewalData item2 in list3)
					{
						if (item2.SubCategory == buttonData.subCategory)
						{
							buttonData.portalData = item2;
						}
					}
					if (buttonData.portalData == null)
					{
						throw new Exception("Native button has no matching Portal row: " + buttonData.subCategory);
					}
					ClientActivityType clientActivityType = ((buttonData.subCategory == PortalRenewalSubCategory.PetIncubator) ? ClientActivityType.OpenPetIncubator : ((buttonData.subCategory == PortalRenewalSubCategory.PetHouse) ? ClientActivityType.OpenPetHouse : ClientActivityType.None));
					if (buttonData.portalData.MoveTaskType != clientActivityType)
					{
						throw new Exception("Incorrect Pet navigation activity");
					}
					buttonData.LockedState = PortalRenewal.LockedState.NONE;
					clicked = null;
					editing = false;
					component.OnClickPetContentsButton(buttonData.ContentsButton);
					if (clicked != buttonData.portalData)
					{
						throw new Exception("Native button failed to dispatch destination");
					}
					num++;
					clicked = null;
					editing = true;
					component.OnClickPetContentsButton(buttonData.ContentsButton);
					if (clicked != null)
					{
						throw new Exception("Edit mode navigation guard bypassed");
					}
				}
			}
			UnityEngine.Object.Destroy(gameObject);
			File.WriteAllText(Path.Combine(environmentVariable, "result.txt"), "PASS native Pet Portal: actual baked table lookup; 8 empty/partial/complete table combinations; " + num + " Hatching/House callbacks; null table, unrelated categories, idempotency, native row/condition preservation, edit guard");
		}
		catch (Exception ex)
		{
			File.WriteAllText(Path.Combine(environmentVariable, "result.txt"), "FAIL " + ex);
		}
		Application.Quit();
	}

	private static int CountBits(int value)
	{
		int num = 0;
		while (value != 0)
		{
			num += value & 1;
			value >>= 1;
		}
		return num;
	}
}
