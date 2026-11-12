using System.Collections.Generic;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NAtlasAssignComponent;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class IconProxy : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate, IInspectorGuiDrawer
{
	public const string PrefabPath = "Prefab2/Component/Icon/Proxy/IconProxy";

	public static PrefabAuto<IconProxy> PrefabAuto = new PrefabAuto<IconProxy>("Prefab2/Component/Icon/Proxy/IconProxy");

	[HideInInspector]
	public List<UISpriteSelector.SelectData> CurrencyList = new List<UISpriteSelector.SelectData>();

	[HideInInspector]
	public List<UISpriteSelector.SelectData> CurrencyNoBgList = new List<UISpriteSelector.SelectData>();

	[HideInInspector]
	public List<UISpriteSelector.SelectData> StaminaList = new List<UISpriteSelector.SelectData>();

	[HideInInspector]
	public List<UISpriteSelector.SelectData> ClassBuffList = new List<UISpriteSelector.SelectData>();

	public UIWidget WidgetRoot;

	public UISpriteSelector UISpriteSelector;

	public UISpriteSelector CopyUiSpriteSelector;

	public UISprite UISprite;

	public static IconProxy Create(UIWidget parentWidget)
	{
		if (parentWidget == null)
		{
			VespaDebug.LogError("Widget Is Null");
			return null;
		}
		IconProxy iconProxy = PrefabAuto.Factory.Create(parentWidget.transform);
		if (iconProxy != null)
		{
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(iconProxy.transform);
			iconProxy.SetDepth(parentWidget.depth);
			AtlasAssignerController component = iconProxy.GetComponent<AtlasAssignerController>();
			if (component != null)
			{
				component.Assign();
			}
		}
		return iconProxy;
	}

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
	}

	public void SetDepth(int depth)
	{
		if (UISprite != null)
		{
			UISprite.depth = depth;
		}
	}

	public void SetupStamina(StaminaType staminaType)
	{
		if (UISpriteSelector != null)
		{
			UISpriteSelector.SelectList = StaminaList;
			UISpriteSelector.SelectEnum(staminaType);
		}
	}

	public void SetupStamina(string staminaName)
	{
		if (UISpriteSelector != null)
		{
			UISpriteSelector.SelectList = StaminaList;
			UISpriteSelector.Select(staminaName);
		}
	}

	public void SetupCurrencyNoBg(CurrencyType currencyType)
	{
		if (UISpriteSelector != null)
		{
			UISpriteSelector.SelectList = CurrencyNoBgList;
			UISpriteSelector.SelectEnum(currencyType);
		}
	}

	public void SetupCurrencyNoBg(string currencyName)
	{
		if (UISpriteSelector != null)
		{
			UISpriteSelector.SelectList = CurrencyNoBgList;
			UISpriteSelector.Select(currencyName);
		}
	}

	public void SetClassBuffIcon(string name)
	{
		if (!(UISpriteSelector == null))
		{
			UISpriteSelector.SelectList = ClassBuffList;
			UISpriteSelector.Select(name);
		}
	}

	public void SetupCurrency(CurrencyType currencyType)
	{
		if (UISpriteSelector != null)
		{
			UISpriteSelector.SelectList = CurrencyList;
			UISpriteSelector.SelectEnum(currencyType);
		}
		if (currencyType != CurrencyType.EventDungeonPoint && currencyType != CurrencyType.EventDungeonPoint2)
		{
			return;
		}
		List<DropBonusEvent> allEffectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetAllEffectiveEvent(EventBonusType.EventDungeon);
		int result = -1;
		if (allEffectiveEvent != null)
		{
			for (int i = 0; i < allEffectiveEvent.Count; i++)
			{
				DropBonusEvent dropBonusEvent = allEffectiveEvent[i];
				if (!String_Extension_Method.IsNullOrEmpty(dropBonusEvent.Condition))
				{
					int.TryParse(dropBonusEvent.Condition, out result);
					break;
				}
			}
		}
		if (result == -1)
		{
			return;
		}
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(result);
		if (data != null)
		{
			string resultInfoIconSprite = data.GetResultInfoIconSprite(currencyType.ToString());
			if (!String_Extension_Method.IsNullOrEmpty(resultInfoIconSprite) && UISpriteSelector != null)
			{
				UISpriteSelector.GetComponent<UISprite>().spriteName = resultInfoIconSprite;
			}
		}
	}
}
