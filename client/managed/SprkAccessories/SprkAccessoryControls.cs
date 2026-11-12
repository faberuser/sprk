using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NCustomizing;
using NGame2.NUtil.NCostume;
using NShared;
using NShared.BuyCustomizingCostumes;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;

public sealed class SprkAccessoryControls : MonoBehaviour
{
	public HeroCostumeView View;

	public AccessoryCostumeViewWidget Current;

	private UIButton remove;

	private UILabel[] removeLabels;

	private bool pending;

	private readonly Dictionary<UILabel, string> originalLabels = new Dictionary<UILabel, string>();

	public void ApplyLabels(UIButton button)
	{
		UILabel[] componentsInChildren = button.GetComponentsInChildren<UILabel>(includeInactive: true);
		foreach (UILabel uILabel in componentsInChildren)
		{
			if (!originalLabels.ContainsKey(uILabel))
			{
				originalLabels.Add(uILabel, uILabel.text);
			}
			uILabel.text = "Apply";
		}
	}

	private void RestoreLabels()
	{
		foreach (KeyValuePair<UILabel, string> originalLabel in originalLabels)
		{
			if (originalLabel.Key != null)
			{
				originalLabel.Key.text = originalLabel.Value;
			}
		}
		originalLabels.Clear();
	}

	public void Refresh()
	{
		if (View == null || View.ButtonResetCostumes == null)
		{
			return;
		}
		if (remove == null)
		{
			UIButton buttonResetCostumes = View.ButtonResetCostumes;
			GameObject gameObject = Object.Instantiate(buttonResetCostumes.gameObject);
			gameObject.name = "SprkRemoveViewedAccessory";
			gameObject.transform.SetParent(buttonResetCostumes.transform.parent, worldPositionStays: false);
			gameObject.transform.localPosition = buttonResetCostumes.transform.localPosition;
			gameObject.transform.localScale = buttonResetCostumes.transform.localScale;
			gameObject.transform.localRotation = buttonResetCostumes.transform.localRotation;
			remove = gameObject.GetComponent<UIButton>();
			remove.onClick.Clear();
			EventDelegate.Add(remove.onClick, Remove);
			UIWidget component = gameObject.GetComponent<UIWidget>();
			if (component != null)
			{
				component.leftAnchor.target = null;
				component.rightAnchor.target = null;
				component.topAnchor.target = null;
				component.bottomAnchor.target = null;
			}
			UILocalize[] componentsInChildren = gameObject.GetComponentsInChildren<UILocalize>(includeInactive: true);
			foreach (UILocalize obj in componentsInChildren)
			{
				obj.enabled = false;
				Object.Destroy(obj);
			}
			UILocalizeEx[] componentsInChildren2 = gameObject.GetComponentsInChildren<UILocalizeEx>(includeInactive: true);
			foreach (UILocalizeEx obj2 in componentsInChildren2)
			{
				obj2.enabled = false;
				Object.Destroy(obj2);
			}
			removeLabels = gameObject.GetComponentsInChildren<UILabel>(includeInactive: true);
			UITweener[] componentsInChildren3 = gameObject.GetComponentsInChildren<UITweener>(includeInactive: true);
			for (int i = 0; i < componentsInChildren3.Length; i++)
			{
				componentsInChildren3[i].enabled = false;
			}
		}
		bool flag = Current != null && Current.gameObject.activeInHierarchy && Current.CurrentComponentAccesoryCostumeData != null;
		remove.gameObject.SetActive(flag);
		remove.isEnabled = flag && !pending && SprkAccessories.Contains(Current.HeroInfo, Current.CurrentComponentAccesoryCostumeData.Index);
		UILabel[] array = removeLabels;
		foreach (UILabel uILabel in array)
		{
			if (uILabel != null && uILabel.text != "Remove")
			{
				uILabel.text = "Remove";
			}
		}
		AlignRemoveButton();
		if (!flag)
		{
			RestoreLabels();
		}
	}

	private void AlignRemoveButton()
	{
		if (!(remove == null) && !(View.ButtonBuyCostumes == null))
		{
			Transform transform = View.ButtonResetCostumes.transform;
			Transform parent = transform.parent;
			Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(parent, transform, considerInactive: true);
			Bounds bounds2 = NGUIMath.CalculateRelativeWidgetBounds(parent, View.ButtonBuyCostumes.transform, considerInactive: true);
			Vector3 localPosition = remove.transform.localPosition;
			localPosition.y = transform.localPosition.y;
			localPosition.z = transform.localPosition.z;
			remove.transform.localPosition = localPosition;
			Bounds bounds3 = NGUIMath.CalculateRelativeWidgetBounds(parent, remove.transform, considerInactive: true);
			float num = bounds2.min.x - bounds.max.x;
			localPosition.x += bounds.min.x - num - bounds3.max.x;
			remove.transform.localPosition = localPosition;
		}
	}

	private void LateUpdate()
	{
		Refresh();
	}

	private void OnDisable()
	{
		if (remove != null)
		{
			remove.gameObject.SetActive(value: false);
		}
		RestoreLabels();
	}

	private void OnDestroy()
	{
		if (remove != null)
		{
			Object.Destroy(remove.gameObject);
		}
	}

	private void Remove()
	{
		if (pending || Current == null || Current.CurrentComponentAccesoryCostumeData == null)
		{
			return;
		}
		int id = Current.CurrentComponentAccesoryCostumeData.Index;
		CachedHeroInfo hero = Current.HeroInfo;
		AccessoryCostumeViewWidget viewed = Current;
		AccountManager account = GlobalAutoBehaviour<AccountManager>.instance;
		if (!SprkAccessories.Contains(hero, id))
		{
			return;
		}
		NShared.BuyCustomizingCostumes.Request request = new NShared.BuyCustomizingCostumes.Request
		{
			HeroIndex = hero.HeroIndex,
			CostumeIndex = hero.CostumeIndex,
			HairCostumeIndex = hero.HairCostumeIndex,
			WeaponCostumeIndex = hero.WeaponCostumeIndex,
			HideUniqueWeapon = hero.HideUniqueWeapon,
			UnsetAccessoryCostumeIndices = new int[1] { id },
			BuyGem = 0,
			BuyGold = 0
		};
		pending = true;
		remove.isEnabled = false;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BuyCustomizingCostumes.Response response)
		{
			pending = false;
			if (GlobalAutoBehaviour<AccountManager>.isValidInstance && (object)account == GlobalAutoBehaviour<AccountManager>.instance)
			{
				if (response.Result != ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
					Refresh();
				}
				else
				{
					CustomizingHelper.OnResponse(default(CustomizingCostumesStock), request, response);
					if (viewed != null && Current == viewed && viewed.HeroInfo != null && viewed.HeroInfo.HeroIndex == hero.HeroIndex && viewed.CurrentComponentAccesoryCostumeData != null && viewed.CurrentComponentAccesoryCostumeData.Index == id)
					{
						SprkAccessories.ShowEquipped(viewed);
					}
				}
			}
		});
	}
}
