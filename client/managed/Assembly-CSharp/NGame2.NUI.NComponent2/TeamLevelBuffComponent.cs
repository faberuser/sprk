using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class TeamLevelBuffComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public interface IListener
	{
		void ReinforceTeamLevelBuff(TeamLevelReinforceData reinforceData, Action onComplete);
	}

	public static string PrefabPath = "Prefab2/Component/GameOption/TeamLevelBuffComponent";

	public static PrefabAuto<TeamLevelBuffComponent> PrefabAuto = new PrefabAuto<TeamLevelBuffComponent>(PrefabPath);

	[Header("[Labels]")]
	public UILabel Label_TeamLevel;

	public UILabel Label_Grade;

	public UILabel Label_Option;

	public UILabel Label_OptionValue;

	public UILabel Label_NextOptionValue;

	public UILabel Label_Price;

	public UILabel Label_MaxReinforce;

	[Header("[GameObjects]")]
	public GameObject LockRoot;

	public GameObject UnLockRoot;

	[Header("[Etc]")]
	public UIButton Button_Phurase;

	public UIWidget IconProxyWidget;

	public UIGrid StarGrid;

	private IListener _listener;

	private TeamLevelBuffCompData _buffData;

	private IconProxy _currencyIconProxy;

	private List<TeamLevelBuffStarComponent> _starComponentList = new List<TeamLevelBuffStarComponent>();

	public int TeamLevel { get; private set; }

	public bool IsEnableReinforce
	{
		get
		{
			if (_buffData == null)
			{
				return false;
			}
			return TeamLevel <= GlobalAutoBehaviour<AccountManager>.instance.UserManager.TeamLevel;
		}
	}

	public void Open(IListener listener, int openLevel)
	{
		_listener = listener;
		TeamLevel = openLevel;
		_buffData = GetTeamLevelBuffCompData();
		InitUI();
	}

	public void Refresh()
	{
		_buffData = GetTeamLevelBuffCompData();
		UpdateUI();
	}

	public void RefreshPhuraseButton()
	{
		if (_buffData != null)
		{
			UIHelper.EnableButton(Button_Phurase, IsEnoughCurrency(_buffData.CurrencyType, _buffData.Price), grayscaleIfDisable: true, !IsEnableReinforce);
		}
	}

	private void InitUI()
	{
		if (_buffData != null)
		{
			if (IconProxyWidget != null)
			{
				_currencyIconProxy = IconProxy.Create(IconProxyWidget);
				_currencyIconProxy.SetupCurrencyNoBg(_buffData.CurrencyType);
			}
			UIHelper.SetLabelText(Label_TeamLevel, TeamLevel);
			UIHelper.SetLabelText(Label_Grade, LocalizationManager.GetStringDirect(GetGradeColorLocalizationKey(_buffData.Grade)));
			UIHelper.SetLabelText(Label_Option, _buffData.Option);
			UpdateUI();
			GenerateComponents();
		}
	}

	private void UpdateUI()
	{
		if (_buffData != null)
		{
			UIHelper.SetLabelText(Label_OptionValue, _buffData.Value);
			UIHelper.SetLabelText(Label_NextOptionValue, _buffData.NextValue);
			UIHelper.SetLabelText(Label_Price, _buffData.Price);
			bool flag = _buffData.Reinforce >= _buffData.MaxReinforce;
			UIHelper.EnableGameObject(LockRoot, _buffData.IsLock);
			UIHelper.EnableGameObject(UnLockRoot, !_buffData.IsLock);
			UIHelper.EnableGameObject(Label_NextOptionValue, !flag);
			UIHelper.EnableGameObject(Button_Phurase, !flag);
			UIHelper.EnableGameObject(Label_MaxReinforce, flag);
			RefreshPhuraseButton();
		}
	}

	private void GenerateComponents()
	{
		if (_buffData == null || StarGrid == null)
		{
			return;
		}
		for (int i = 1; i < _buffData.MaxReinforce + 1; i++)
		{
			TeamLevelBuffStarComponent teamLevelBuffStarComponent = TeamLevelBuffStarComponent.PrefabAuto.Factory.Create(StarGrid.transform);
			if (!(teamLevelBuffStarComponent == null))
			{
				teamLevelBuffStarComponent.Open(i <= _buffData.Reinforce);
				_starComponentList.Add(teamLevelBuffStarComponent);
			}
		}
	}

	public void OnClickReinforce()
	{
		if (_buffData == null || _listener == null)
		{
			return;
		}
		if (!IsEnableReinforce)
		{
			string stringDirect = LocalizationManager.GetStringDirect("ECLIPSE_BATTLE_INFO_POPUP_TITLE");
			string stringDirect2 = LocalizationManager.GetStringDirect("VETERAN_LEVEL_POPUP_DESC2");
			MessagePopup.Open(stringDirect, stringDirect2);
			return;
		}
		CurrencyType currencyType = _buffData.CurrencyType;
		int price = _buffData.Price;
		TeamLevelReinforceData reinforceData = new TeamLevelReinforceData(TeamLevel, _buffData.Reinforce + 1, _buffData.Price, _buffData.CurrencyType);
		if (!IsEnoughCurrency(currencyType, price))
		{
			return;
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect3 = LocalizationManager.GetStringDirect("VETERAN_BUFF_UPGRADE_POPUP");
		string stringDirect4 = LocalizationManager.GetStringDirect("COMMON_CONFIRM_01");
		PreserveMessagePopup2.EPriceType priceType = UIHelper.ConvertToPriceType(currencyType);
		PreserveMessagePopup2.Open(title, stringDirect3, priceType, price, stringDirect4, delegate
		{
			_listener.ReinforceTeamLevelBuff(reinforceData, delegate
			{
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(_starComponentList))
				{
					int num = _buffData.Reinforce - 1;
					if (num >= 0)
					{
						_starComponentList[num].Enable();
					}
				}
			});
		});
	}

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		FactoryBase.ReleaseAndClearList(_starComponentList);
		_starComponentList.Clear();
	}

	public bool IsEnoughCurrency(CurrencyType currencyType, int currencyPrice)
	{
		UserManager userManager = GlobalAutoBehaviour<AccountManager>.instance.UserManager;
		bool result = true;
		switch (currencyType)
		{
		case CurrencyType.Gold:
			if (currencyPrice > userManager.Gold)
			{
				result = false;
			}
			break;
		case CurrencyType.Gem:
		{
			long totalGem = userManager.TotalGem;
			if (currencyPrice > totalGem)
			{
				result = false;
			}
			break;
		}
		case CurrencyType.Mileage:
		{
			long mileage = userManager.Mileage;
			if (currencyPrice > mileage)
			{
				result = false;
			}
			break;
		}
		case CurrencyType.LuaPoint:
		{
			long luaPoint = userManager.LuaPoint;
			if (currencyPrice > luaPoint)
			{
				result = false;
			}
			break;
		}
		default:
			result = false;
			break;
		}
		return result;
	}

	public string GetGradeColorLocalizationKey(TeamLevelGrade grade)
	{
		return grade switch
		{
			TeamLevelGrade.Uncommon => "GRADE_GREEN2", 
			TeamLevelGrade.Rare => "PET_GRADE_COLOR_RARE_TEXT", 
			TeamLevelGrade.Hero => "PET_GRADE_COLOR_HERO_TEXT", 
			TeamLevelGrade.Ancient => "PET_GRADE_COLOR_ANCIENT_TEXT", 
			TeamLevelGrade.Legend => "PET_GRADE_COLOR_LEGEND_TEXT", 
			_ => "", 
		};
	}

	private TeamLevelBuffCompData GetTeamLevelBuffCompData()
	{
		TeamLevelBuffCompData teamLevelBuffCompData = new TeamLevelBuffCompData();
		TeamLevelBuff teamLevelBuff = GlobalAutoBehaviour<AccountManager>.instance.TeamLevelBuffManager.Get(TeamLevel);
		if (teamLevelBuff != null)
		{
			teamLevelBuffCompData.Reinforce = teamLevelBuff.BuffReinforce;
			teamLevelBuffCompData.IsLock = false;
		}
		else
		{
			teamLevelBuffCompData.Reinforce = 0;
			teamLevelBuffCompData.IsLock = true;
		}
		TeamLevelBuffOptionData buffOptionData = GlobalAutoBehaviour<TeamLevelBuffManager>.instance.GetBuffOptionData(TeamLevel);
		if (buffOptionData != null)
		{
			int index = Math.Min(teamLevelBuffCompData.Reinforce, buffOptionData.GetMaxReinforce());
			int index2 = Math.Min(teamLevelBuffCompData.Reinforce + 1, buffOptionData.GetMaxReinforce());
			int optionValue = buffOptionData.GetOptionValue(index);
			int optionValue2 = buffOptionData.GetOptionValue(index2);
			teamLevelBuffCompData.MaxReinforce = buffOptionData.GetMaxReinforce();
			teamLevelBuffCompData.CurrencyType = buffOptionData.ReinforcePriceType;
			teamLevelBuffCompData.Grade = buffOptionData.Grade;
			teamLevelBuffCompData.Price = buffOptionData.GetReinforcePrice(index);
			GetStatDesc(buffOptionData.OptionType, optionValue, optionValue2, buffOptionData.ContentsOptionDescKey, out teamLevelBuffCompData.Option, out teamLevelBuffCompData.Value, out teamLevelBuffCompData.NextValue);
			return teamLevelBuffCompData;
		}
		TeamLevelBonusBuffOptionData bonusBuffOptionData = GlobalAutoBehaviour<TeamLevelBuffManager>.instance.GetBonusBuffOptionData(TeamLevel);
		if (bonusBuffOptionData != null)
		{
			int index3 = Math.Min(teamLevelBuffCompData.Reinforce, bonusBuffOptionData.GetMaxReinforce());
			int index4 = Math.Min(teamLevelBuffCompData.Reinforce + 1, bonusBuffOptionData.GetMaxReinforce());
			int bonusValue = bonusBuffOptionData.GetBonusValue(index3);
			int bonusValue2 = bonusBuffOptionData.GetBonusValue(index4);
			teamLevelBuffCompData.MaxReinforce = bonusBuffOptionData.GetMaxReinforce();
			teamLevelBuffCompData.CurrencyType = bonusBuffOptionData.ReinforcePriceType;
			teamLevelBuffCompData.Grade = bonusBuffOptionData.Grade;
			teamLevelBuffCompData.Price = bonusBuffOptionData.GetReinforcePrice(index3);
			GetStatDesc(bonusBuffOptionData.BonusNameKey, bonusBuffOptionData.BonusDescKey, bonusValue, bonusValue2, out teamLevelBuffCompData.Option, out teamLevelBuffCompData.Value, out teamLevelBuffCompData.NextValue);
			return teamLevelBuffCompData;
		}
		return null;
	}

	private void GetStatDesc(OperationType type, int value, int nextValue, string contentsOptionDescKey, out string optionName, out string optionValue, out string optionNextValue)
	{
		if (String_Extension_Method.IsNullOrEmpty(contentsOptionDescKey))
		{
			optionName = LocalizationManager.GetString(type);
		}
		else
		{
			string text = LocalizationManager.GetString(type);
			optionName = LocalizationManager.GetStringDirect(contentsOptionDescKey, text);
		}
		optionValue = StatData.GetStatDisplayValue(type, value, showSign: true, isCommaString: true);
		optionNextValue = StatData.GetStatDisplayValue(type, nextValue, showSign: true, isCommaString: true);
	}

	private void GetStatDesc(string nameKey, string descKey, int value, int nextValue, out string optionName, out string optionValue, out string optionNextValue)
	{
		optionName = LocalizationManager.GetStringDirect(nameKey);
		optionValue = LocalizationManager.GetStringDirect(descKey, value);
		optionNextValue = LocalizationManager.GetStringDirect(descKey, nextValue);
	}
}
