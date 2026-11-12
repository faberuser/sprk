using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class CurrencyTypeComponent : MonoBehaviour, NVespa.NPrefab2.InitOnCreate
{
	public interface Listener
	{
		void OnClick(CurrencyTypeComponent component);
	}

	public enum ResourceType
	{
		Stamina,
		UserStat
	}

	public const string PrefabPath = "Prefab2/Component/TopMostMenu/CurrencyTypeComponent";

	public static PrefabAuto<CurrencyTypeComponent> PrefabAuto = new PrefabAuto<CurrencyTypeComponent>("Prefab2/Component/TopMostMenu/CurrencyTypeComponent");

	public GameObject CurrencyTypeComponenet;

	public GameObject Root;

	public UISprite Sprite_BackGroud;

	public UIWidget IconRoot;

	public IconProxy IconProxy;

	public UIGame3DWidget Effect;

	public CurrencyTooltipComponent CurrencyTooltipComponent;

	public StaminaTooltipComponent StaminaTooltipComponent;

	public UITooltipTrigger ToolTipTrigger;

	public UILabel Label_Value;

	public UIButton Button_Buy;

	[HideInInspector]
	public CurrencyTypeData currencyTypeData;

	[HideInInspector]
	public CurrencyType _currencyType;

	[HideInInspector]
	public StaminaType _staminaType;

	[HideInInspector]
	public UserManager.UserStatType _userStatType;

	private bool isEventCurrency;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		if (IconProxy != null)
		{
			FactoryBase.SafeReleaseComponent(ref IconProxy);
		}
		IconProxy = null;
	}

	public void Open(CurrencyType currencyType)
	{
		currencyTypeData = GameRoot.Instance.CurrencyTypeDataContainer.GetCurrencyData(currencyType);
		if (currencyTypeData == null)
		{
			return;
		}
		_currencyType = currencyTypeData.CurrencyType;
		if (String_Extension_Method.IsNullOrEmpty(currencyTypeData.StaminaType) && String_Extension_Method.IsNullOrEmpty(currencyTypeData.UserStatType))
		{
			UIHelper.EnableGameObject(CurrencyTypeComponenet, enable: false);
			return;
		}
		if (!String_Extension_Method.IsNullOrEmpty(currencyTypeData.StaminaType) && !String_Extension_Method.IsNullOrEmpty(currencyTypeData.UserStatType))
		{
			UserManager.UserStatType o_value = UserManager.UserStatType.None;
			EnumTools.TryParse<UserManager.UserStatType>(currencyTypeData.UserStatType, out o_value);
			_userStatType = o_value;
			UIHelper.EnableGameObject(CurrencyTooltipComponent, enable: true);
			UIHelper.EnableGameObject(StaminaTooltipComponent, enable: false);
		}
		else if (!String_Extension_Method.IsNullOrEmpty(currencyTypeData.StaminaType))
		{
			StaminaType o_value2 = StaminaType.None;
			EnumTools.TryParse<StaminaType>(currencyTypeData.StaminaType, out o_value2);
			_staminaType = o_value2;
			UIHelper.EnableGameObject(CurrencyTooltipComponent, enable: false);
			UIHelper.EnableGameObject(StaminaTooltipComponent, enable: true);
		}
		else if (!String_Extension_Method.IsNullOrEmpty(currencyTypeData.UserStatType))
		{
			UserManager.UserStatType o_value3 = UserManager.UserStatType.None;
			EnumTools.TryParse<UserManager.UserStatType>(currencyTypeData.UserStatType, out o_value3);
			_userStatType = o_value3;
			UIHelper.EnableGameObject(CurrencyTooltipComponent, enable: true);
			UIHelper.EnableGameObject(StaminaTooltipComponent, enable: false);
		}
		CheckCondition();
		Init();
	}

	public void CheckCondition()
	{
		List<string> conditonList = currencyTypeData.GetConditonList();
		if (conditonList == null)
		{
			return;
		}
		for (int i = 0; i < conditonList.Count(); i++)
		{
			string text = Vespa_String_Extension_Method.TRIM(conditonList[i]);
			_ = string.Empty;
			if (String_Extension_Method.IsNullOrEmpty(text))
			{
				continue;
			}
			string[] array = text.Split('_');
			if (array != null && array.Count() >= 3)
			{
				string text2 = array[0];
				if (NGame2.NUI.NWindow.EntryMenu.CheckEntryMenuCondition(null, text2.ToString()))
				{
					int result = 300;
					int.TryParse(array[1], out result);
					currencyTypeData.Width = result;
					currencyTypeData.SpriteName = array[2];
					isEventCurrency = true;
					break;
				}
			}
		}
	}

	public void Init()
	{
		SetSprite();
		SetWidth();
		SetButtonAction();
		SetEffectCode();
		UpdateValue();
	}

	public void SetSprite()
	{
		if (IconProxy == null)
		{
			IconProxy = IconProxy.Create(IconRoot);
		}
		if (IconProxy != null)
		{
			IconProxy.SetupCurrencyNoBg(currencyTypeData.SpriteName);
		}
	}

	public void SetWidth()
	{
		if (Sprite_BackGroud != null)
		{
			int num = Sprite_BackGroud.GetComponent<UIWidget>().width - currencyTypeData.Width;
			Vector3 localPosition = IconRoot.transform.localPosition;
			localPosition = new Vector2(localPosition.x + (float)num, localPosition.y);
			IconRoot.transform.localPosition = localPosition;
			Sprite_BackGroud.GetComponent<UIWidget>().width = currencyTypeData.Width;
		}
	}

	public void SetEffectCode()
	{
		if (Effect != null)
		{
			TransformData transformData = new TransformData();
			transformData.Position = new Vector3(0f, 0f, 0f);
			transformData.RotationEuler = new Vector3(0f, 0f, 0f);
			transformData.Scale = 60f;
			Effect.PlayInstantEffect(currencyTypeData.EffectCode, transformData, null);
		}
	}

	public void SetButtonAction()
	{
		if (String_Extension_Method.IsNullOrEmpty(currencyTypeData.ButtonAction))
		{
			UIHelper.EnableGameObject(Button_Buy, enable: false);
			return;
		}
		UIHelper.EnableGameObject(Button_Buy, enable: true);
		if (currencyTypeData.ButtonAction.Equals("BuyGold"))
		{
			EventDelegate.Set(Button_Buy.onClick, SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().OnClickButton_BuyGold);
		}
		else if (currencyTypeData.ButtonAction.Equals("BuyGem"))
		{
			EventDelegate.Set(Button_Buy.onClick, SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().OnClickButton_BuyGem);
		}
		else if (currencyTypeData.ButtonAction.Equals("BuyStamina"))
		{
			EventDelegate.Set(Button_Buy.onClick, SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().OnClickButton_BuyStamina);
		}
		else if (currencyTypeData.ButtonAction.Equals("RechargeUndergroundPrisonKey"))
		{
			EventDelegate.Set(Button_Buy.onClick, SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().OnClickRechargeUndergroundPrisonKey);
		}
		else if (currencyTypeData.ButtonAction.Equals("Chat"))
		{
			EventDelegate.Set(Button_Buy.onClick, SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().OnClickButton_Chat);
		}
		else if (currencyTypeData.ButtonAction.Equals("AvatarHero"))
		{
			EventDelegate.Set(Button_Buy.onClick, SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().OnClickAvatarHero);
		}
		else if (currencyTypeData.ButtonAction.Equals("Mail"))
		{
			EventDelegate.Set(Button_Buy.onClick, SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().OnClickButton_Mail);
		}
		else if (currencyTypeData.ButtonAction.Equals("BackButton"))
		{
			EventDelegate.Set(Button_Buy.onClick, SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().OnClickBackButton);
		}
	}

	public void UpdateValue()
	{
		if (_staminaType != StaminaType.None)
		{
			long currentStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(_staminaType);
			long maxStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetMaxStamina(_staminaType);
			UIHelper.SetLabelText(Label_Value, $"{currentStamina}/{maxStamina}");
		}
		if (_userStatType != UserManager.UserStatType.None)
		{
			long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(_userStatType);
			string text = Vespa_Long_Extension_Method.ToCommaString(userStat);
			if (_userStatType == UserManager.UserStatType.FriendshipPoint)
			{
				int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxFriendshipPoint");
				text = $"{userStat}/{valueInt}";
			}
			UIHelper.SetLabelText(Label_Value, text);
		}
	}

	public void OnClickIcon()
	{
	}

	public void OnShowCurrencyTooltip()
	{
		if (!(CurrencyTooltipComponent == null))
		{
			string eventCurrency = string.Empty;
			if (isEventCurrency)
			{
				eventCurrency = currencyTypeData.SpriteName;
			}
			CurrencyTooltipComponent.OnShowTooltip_CurrencyTypeComponent(_currencyType, CurrencyTooltipComponent.PositionType.Icon, eventCurrency);
		}
	}

	public void OnHideCurrencyTooltip()
	{
		if (!(CurrencyTooltipComponent == null))
		{
			CurrencyTooltipComponent.OnHideTooltip();
		}
	}

	public void OnShowStaminaTooltip()
	{
		if (!(StaminaTooltipComponent == null))
		{
			StaminaTooltipComponent.OnShowTooltip(_staminaType);
		}
	}

	public void OnHideStaminaTooltip()
	{
		if (!(StaminaTooltipComponent == null))
		{
			StaminaTooltipComponent.OnHideTooltip();
		}
	}
}
