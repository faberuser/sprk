using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NHelper.NPet;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopResourceList : MonoBehaviour
{
	[Serializable]
	public class ResourceInfo
	{
		public CurrencyType Currency;

		public GameObject Root;

		public UILabel LabelValue;
	}

	[Serializable]
	public class SwitchResourceInfo
	{
		public CurrencyType Currency;

		public GameObject Root;

		public UIWidget IconProxyRoot;

		[HideInInspector]
		public IconProxy IconProxyComp;
	}

	public interface IListener
	{
		void OnClickStamina();

		void OnClickRuby();

		void OnClickGold();
	}

	private IListener _listener;

	public List<ResourceInfo> ResourceInfoList = new List<ResourceInfo>();

	public List<SwitchResourceInfo> SwitchResourceInfoList = new List<SwitchResourceInfo>();

	public UIWidget GemTooltip;

	public CurrencyTooltipComponent GemTooltipComponent;

	public UILabel Label_ToolTipPayGem;

	public UILabel Label_ToolTipFreeGem;

	public UIWidget ChickenStaminaTooltip;

	public UILabel Label_StaminaTeamLevel;

	public UILabel Label_StaminaBonus;

	public UILabel Label_StaminaTotal;

	public UILabel Label_FullChickenStaminaRecoverTime;

	public UILabel Label_ChickenStaminaRecoverTime;

	public UIWidget PetFeedTooltip;

	public UILabel Label_PetFeedTotalRechargeTime;

	public UILabel Label_PetFeedRechargeTime;

	private bool _onShowPetFeedToopTip;

	public void Open(IListener listener)
	{
		_listener = listener;
		_onShowPetFeedToopTip = false;
		InitCurrency();
	}

	public void InitCurrency(CurrencyType InitSwitchResource = CurrencyType.ShopEventPoint)
	{
		UserManager userManager = GlobalAutoBehaviour<AccountManager>.instance.UserManager;
		if (userManager == null)
		{
			return;
		}
		foreach (ResourceInfo resourceInfo in ResourceInfoList)
		{
			CurrencyType currency = resourceInfo.Currency;
			if (currency == CurrencyType.Gem)
			{
				SetCurrency(CurrencyType.Gem, userManager.TotalGem);
				continue;
			}
			CurrencyTypeData data = GameRoot.Instance.CurrencyTypeDataContainer.GetData(currency);
			if (data == null)
			{
				continue;
			}
			if (!String_Extension_Method.IsNullOrEmpty(data.UserStatType))
			{
				long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(data.UserStatType);
				SetCurrency(currency, userStat);
			}
			else if (!String_Extension_Method.IsNullOrEmpty(data.StaminaType))
			{
				StaminaType o_value = StaminaType.None;
				EnumTools.TryParse<StaminaType>(data.StaminaType, out o_value);
				if (o_value > StaminaType.None)
				{
					long currentStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(o_value);
					SetCurrency(currency, currentStamina);
				}
			}
		}
		foreach (SwitchResourceInfo switchResourceInfo in SwitchResourceInfoList)
		{
			GameObject root = switchResourceInfo.Root;
			CurrencyType currency2 = switchResourceInfo.Currency;
			UIWidget iconProxyRoot = switchResourceInfo.IconProxyRoot;
			IconProxy comp = switchResourceInfo.IconProxyComp;
			if (!(iconProxyRoot == null))
			{
				if (comp != null)
				{
					FactoryBase.SafeReleaseComponent(ref comp);
				}
				comp = IconProxy.Create(iconProxyRoot);
				if (!(comp == null))
				{
					comp.SetupCurrencyNoBg(currency2);
					UIHelper.EnableGameObject(root, InitSwitchResource == currency2);
				}
			}
		}
	}

	public void ChangeSwitchCurrency(CurrencyType switchCurrencyType = CurrencyType.ShopEventPoint)
	{
		foreach (SwitchResourceInfo switchResourceInfo in SwitchResourceInfoList)
		{
			GameObject root = switchResourceInfo.Root;
			CurrencyType currency = switchResourceInfo.Currency;
			UIHelper.EnableGameObject(root, switchCurrencyType == currency);
		}
	}

	public void SetCurrency(CurrencyType currency, long value)
	{
		ResourceInfo resourceInfo = ResourceInfoList.Find((ResourceInfo info) => info.Currency == currency);
		if (resourceInfo != null)
		{
			string empty = string.Empty;
			if (resourceInfo.Currency == CurrencyType.Stamina)
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(StaminaType.Chicken, out var currentValue, out var maxValue);
				empty = $"{currentValue}/{maxValue}";
			}
			else if (resourceInfo.Currency == CurrencyType.PetFeedKey)
			{
				long num = GlobalAutoBehaviour<AccountManager>.instance.PetManager.RemainPetFoodCount;
				long num2 = GlobalAutoBehaviour<AccountManager>.instance.PetManager.MaxPetFoodCount;
				empty = PetHelper.GetRemainPetFoodCountStr(num, num2);
			}
			else if (currency == CurrencyType.Gold && value >= 1000000000)
			{
				value /= 1000;
				empty = Vespa_Long_Extension_Method.ToCommaString(value) + "K";
			}
			else
			{
				empty = Vespa_Long_Extension_Method.ToCommaString(value);
			}
			UIHelper.SetLabelText(resourceInfo.LabelValue, empty);
		}
	}

	public void OnChanged(UserManager.UserStatInfo userStatInfo)
	{
		switch (userStatInfo.UserStatType)
		{
		case UserManager.UserStatType.Stamina:
			if (userStatInfo.StaminaType == StaminaType.Chicken)
			{
				SetCurrency(CurrencyType.Stamina, userStatInfo.AfterValue);
			}
			else if (userStatInfo.StaminaType == StaminaType.PetFeedKey)
			{
				SetCurrency(CurrencyType.PetFeedKey, userStatInfo.AfterValue);
			}
			break;
		case UserManager.UserStatType.Gold:
			SetCurrency(CurrencyType.Gold, userStatInfo.AfterValue);
			break;
		case UserManager.UserStatType.Gem:
		case UserManager.UserStatType.PayGem:
		{
			long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
			SetCurrency(CurrencyType.Gem, totalGem);
			break;
		}
		case UserManager.UserStatType.Mileage:
			SetCurrency(CurrencyType.Mileage, userStatInfo.AfterValue);
			break;
		case UserManager.UserStatType.LuaPoint:
			SetCurrency(CurrencyType.LuaPoint, userStatInfo.AfterValue);
			break;
		case UserManager.UserStatType.ShopEventPoint:
			SetCurrency(CurrencyType.ShopEventPoint, userStatInfo.AfterValue);
			break;
		}
	}

	public void OnChanged(NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo userStatInfo)
	{
		SetCurrency(userStatInfo.currencyType, userStatInfo.Amount);
	}

	public void Update()
	{
		if (_onShowPetFeedToopTip)
		{
			UpdatePetFeedToolTipState();
		}
	}

	private void UpdatePetFeedToolTipState()
	{
		LobbyTopmostMenu.UpdateStaminaTimeInfo(PetFeedTooltip, Label_PetFeedTotalRechargeTime, Label_PetFeedRechargeTime, StaminaType.PetFeedKey);
	}

	public void OnClickStamina()
	{
		if (_listener != null)
		{
			_listener.OnClickStamina();
		}
	}

	public void OnClickRuby()
	{
		if (_listener != null)
		{
			_listener.OnClickRuby();
		}
	}

	public void OnClickGold()
	{
		if (_listener != null)
		{
			_listener.OnClickGold();
		}
	}

	public void ClearAll()
	{
		foreach (SwitchResourceInfo switchResourceInfo in SwitchResourceInfoList)
		{
			IconProxy comp = switchResourceInfo.IconProxyComp;
			FactoryBase.SafeReleaseComponent(ref comp);
		}
	}

	private bool IsPaidRubyPrint()
	{
		return LobbyManagement.GetCountryServerData()?.IsPaidRubyPrint ?? false;
	}

	public void OnShowGemTooltip()
	{
		if (IsPaidRubyPrint())
		{
			SetToolTipPayGem();
			UIHelper.EnableGameObject(GemTooltip, enable: true);
		}
		else if (GemTooltipComponent != null)
		{
			GemTooltipComponent.OnShowTooltip();
		}
	}

	public void OnHideGemTooltip()
	{
		if (IsPaidRubyPrint())
		{
			UIHelper.EnableGameObject(GemTooltip, enable: false);
		}
		else if (GemTooltipComponent != null)
		{
			GemTooltipComponent.OnHideTooltip();
		}
	}

	private void SetToolTipPayGem()
	{
		UIHelper.SetLabelText(Label_ToolTipPayGem, GlobalAutoBehaviour<AccountManager>.instance.UserManager.PayGem);
		UIHelper.SetLabelText(Label_ToolTipFreeGem, GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gem);
	}

	public void OnShowStaminaTooltip()
	{
		UIHelper.EnableGameObject(ChickenStaminaTooltip, enable: true);
		LobbyTopmostMenu.UpdateChickenStaminaTimeInfo(ChickenStaminaTooltip, Label_StaminaTeamLevel, Label_StaminaBonus, Label_StaminaTotal, Label_FullChickenStaminaRecoverTime, Label_ChickenStaminaRecoverTime, StaminaType.Chicken);
	}

	public void OnHideStaminaTooltip()
	{
		UIHelper.EnableGameObject(ChickenStaminaTooltip, enable: false);
	}

	public void OnShowPetFeedTooltip()
	{
		_onShowPetFeedToopTip = true;
		UIHelper.EnableGameObject(PetFeedTooltip, enable: true);
	}

	public void OnHidePetFeedTooltip()
	{
		_onShowPetFeedToopTip = false;
		UIHelper.EnableGameObject(PetFeedTooltip, enable: false);
	}
}
