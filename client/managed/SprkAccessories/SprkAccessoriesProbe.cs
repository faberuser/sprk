using System;
using System.Collections.Generic;
using System.Reflection;
using NGame2.N3D;
using NGame2.NUI.NComponent2.NCostume;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NCustomizing;
using NGame2.NUtil.NCostume;
using NGameAsset;
using NShared;
using NShared.NJit;
using NVespa.NNGUIExtension;
using UnityEngine;

public static class SprkAccessoriesProbe
{
	private sealed class ResetListener : HeroCostumeView.Listener, HeroFullSceneViewBase.Listener
	{
		public int Calls;

		public int Hero;

		public void OnOpen()
		{
		}

		public void OnClose()
		{
		}

		public void OnClickPlusGold()
		{
		}

		public void OnClickPlusGem()
		{
		}

		public void OnClickViewMotion(HeroCostumeView caller)
		{
		}

		public void ProcessSelectedCustomizingCostumes(HeroCustomizingDataGetter getter, Action complete)
		{
		}

		public void OpenLegendCostumeReservationPooup(int index)
		{
		}

		public void ResetHeroCostumes(int hero, Action complete)
		{
			Calls++;
			Hero = hero;
		}
	}

	public static string Run()
	{
		TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
		GameObject gameObject = new GameObject("SprkAccessoriesFixture");
		gameObject.SetActive(value: false);
		try
		{
			GameObject gameObject2 = VespaResource.Load<GameObject>("Resources/Prefab2/Window/FullScene/HeroCostumeView");
			if (gameObject2 == null)
			{
				throw new Exception("Dressing room prefab missing");
			}
			HeroCostumeView component = UnityEngine.Object.Instantiate(gameObject2, gameObject.transform, worldPositionStays: false).GetComponent<HeroCostumeView>();
			List<EventDelegate> list = new List<EventDelegate>(component.ButtonResetCostumes.onClick);
			SprkAccessoryControls sprkAccessoryControls = component.gameObject.AddComponent<SprkAccessoryControls>();
			sprkAccessoryControls.View = component;
			AccessoryCostumeViewWidget obj = (sprkAccessoryControls.Current = component.AccessoryCostumeViewWidget[0]);
			List<SprkAccessoryEntry> list2 = new List<SprkAccessoryEntry>();
			for (int i = 1; i <= 4; i++)
			{
				int num = 0;
				foreach (AccessoryCostumeData allDatum in GameRoot.Instance.AccessoryCostumeDataContainer.GetAllData())
				{
					if (allDatum.PartType != (AccessoryPartType)i)
					{
						continue;
					}
					CustomizeStock stock = default(CustomizeStock);
					if (RenderFacadeBuilder.TryAssignCustomizeStock(ref stock, 1, allDatum, PositionInfo.Create()))
					{
						list2.Add(new SprkAccessoryEntry
						{
							Index = allDatum.Index,
							Part = allDatum.PartType,
							Position = PositionInfo.Create()
						});
						if (i == 1 && num == 0)
						{
							typeof(AccessoryCostumeViewWidget).GetProperty("CurrentComponentOpenData").SetValue(obj, AccessoryCostumeComponent.OpenData.CreateCostumeType(allDatum), null);
						}
						if (++num == 2)
						{
							break;
						}
					}
				}
				if (num != 2)
				{
					throw new Exception("Missing renderable accessories for part " + i);
				}
			}
			CachedHeroInfo cachedHeroInfo = new CachedHeroInfo();
			cachedHeroInfo.HeroIndex = 1;
			SprkAccessories.Store(cachedHeroInfo, list2);
			foreach (SprkAccessoryEntry item in list2)
			{
				cachedHeroInfo.SetAccessoryCostume(item.Part, item.Index, item.Position);
			}
			CachedHeroInfoCostumeStock costumeStock = cachedHeroInfo.GetCostumeStock();
			if (costumeStock.IsDefaultCostume() || !cachedHeroInfo.IsEqualCostumeStock(costumeStock))
			{
				throw new Exception("Native Remove All eligibility/equality failed");
			}
			if (SprkAccessories.Stocks(cachedHeroInfo).Length != 8)
			{
				throw new Exception("Native stock builder lost layers");
			}
			CreatureStock stock2 = default(CreatureStock);
			SprkAccessories.Build(cachedHeroInfo, ref stock2);
			if (stock2.CustomizeStockValues().Count != 8)
			{
				throw new Exception("Native creature stock lost layers");
			}
			CachedHeroInfo value = JsonMarshaler.Clone(cachedHeroInfo);
			if (SprkAccessories.Fingerprint(cachedHeroInfo) != SprkAccessories.Fingerprint(value))
			{
				throw new Exception("Native CachedHeroInfo clone lost accessory positions");
			}
			sprkAccessoryControls.Refresh();
			Transform transform = component.ButtonResetCostumes.transform.parent.Find("SprkRemoveViewedAccessory");
			if (transform == null || transform.GetComponent<UIButton>().onClick.Count != 1)
			{
				throw new Exception("Remove button missing or inherited Remove All action");
			}
			if (transform.localPosition.x >= component.ButtonResetCostumes.transform.localPosition.x)
			{
				throw new Exception("Remove must be left of Remove All");
			}
			UILabel[] componentsInChildren = transform.GetComponentsInChildren<UILabel>(includeInactive: true);
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				if (componentsInChildren[j].text != "Remove")
				{
					throw new Exception("Remove label was localized incorrectly");
				}
			}
			UILocalizeEx[] componentsInChildren2 = transform.GetComponentsInChildren<UILocalizeEx>(includeInactive: true);
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				if (componentsInChildren2[j].enabled)
				{
					throw new Exception("Remove inherited active Remove All localization");
				}
			}
			Transform parent = transform.parent;
			for (int k = 0; k < 3; k++)
			{
				if (k == 1)
				{
					parent.localScale = new Vector3(1.3f, 0.8f, 1f);
				}
				if (k == 2)
				{
					component.ButtonBuyCostumes.transform.localPosition += new Vector3(37f, 0f, 0f);
				}
				transform.gameObject.SetActive(value: true);
				transform.gameObject.BroadcastMessage("OnReloadLocalization", SendMessageOptions.DontRequireReceiver);
				sprkAccessoryControls.Refresh();
				componentsInChildren = transform.GetComponentsInChildren<UILabel>(includeInactive: true);
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					if (componentsInChildren[j].text != "Remove")
					{
						throw new Exception("Remove label did not survive localization reload");
					}
				}
				Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(parent, component.ButtonResetCostumes.transform, considerInactive: true);
				Bounds bounds2 = NGUIMath.CalculateRelativeWidgetBounds(parent, component.ButtonBuyCostumes.transform, considerInactive: true);
				Bounds bounds3 = NGUIMath.CalculateRelativeWidgetBounds(parent, transform, considerInactive: true);
				float num2 = bounds2.min.x - bounds.max.x;
				float num3 = bounds.min.x - bounds3.max.x;
				if (Mathf.Abs(num2 - num3) > 0.05f)
				{
					throw new Exception("Unequal footer button gaps: " + num3 + " vs " + num2);
				}
			}
			if (list.Count == 0 || component.ButtonResetCostumes.onClick.Count != list.Count)
			{
				throw new Exception("Remove All click binding was lost");
			}
			for (int l = 0; l < list.Count; l++)
			{
				if (list[l] != component.ButtonResetCostumes.onClick[l])
				{
					throw new Exception("Remove All click binding was replaced");
				}
			}
			ResetListener resetListener = new ResetListener();
			typeof(HeroFullSceneViewBase).GetProperty("HeroInfo").SetValue(component, cachedHeroInfo, null);
			typeof(HeroCostumeView).GetField("_listener", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic).SetValue(component, resetListener);
			HashSet<int> hashSet = new HashSet<int>();
			MessagePopup[] array = UnityEngine.Object.FindObjectsOfType<MessagePopup>();
			foreach (MessagePopup messagePopup in array)
			{
				hashSet.Add(messagePopup.GetInstanceID());
			}
			EventDelegate.Execute(component.ButtonResetCostumes.onClick);
			MessagePopup messagePopup2 = null;
			array = UnityEngine.Object.FindObjectsOfType<MessagePopup>();
			foreach (MessagePopup messagePopup3 in array)
			{
				if (!hashSet.Contains(messagePopup3.GetInstanceID()))
				{
					messagePopup2 = messagePopup3;
				}
			}
			if (messagePopup2 == null)
			{
				throw new Exception("Remove All click did not open native confirmation");
			}
			if (resetListener.Calls != 0)
			{
				throw new Exception("Remove All bypassed confirmation");
			}
			messagePopup2.OnClickYes();
			if (resetListener.Calls != 1 || resetListener.Hero != 1)
			{
				throw new Exception("Confirmed Remove All did not request the selected hero reset exactly once");
			}
			return "PASS: native Remove All serialized click, confirmation and selected-hero reset request; Remove label/localization, equal footer gaps, eight accessory stocks and independent clone transforms.";
		}
		finally
		{
			UnityEngine.Object.Destroy(gameObject);
		}
	}
}
