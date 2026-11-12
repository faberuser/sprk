using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NHelper.NEventGrowWorldTree;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class StaminaConsumePointInfoComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate, LobbyRightGridWindow.IRightGridSort, IUserObserver<NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo>, IUserObserver
{
	public const string PrefabPath = "Prefab2/Component/EventCraft/StaminaConsumePointInfoComponent";

	public static PrefabAuto<StaminaConsumePointInfoComponent> PrefabAuto = new PrefabAuto<StaminaConsumePointInfoComponent>("Prefab2/Component/EventCraft/StaminaConsumePointInfoComponent");

	public UIWidget IconProxyWidget;

	public UILabel LabelName;

	public UILabel LabelDailyValue;

	public UILabel LabelMissionScoreValue;

	public UILabel LabelDailyMaxValue;

	public UILabel LabelMaxValue;

	private IconProxy _iconProxy;

	private CurrencyType _currencyType;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
		GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.ObserverManager.AddObserver(this);
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		_currencyType = CurrencyType.None;
		if (_iconProxy != null)
		{
			FactoryBase.SafeReleaseComponent(ref _iconProxy);
		}
		GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.ObserverManager.RemoveObserver(this);
	}

	public void Open(CurrencyType currenyType)
	{
		_currencyType = currenyType;
		SetIconProxy(currenyType);
		if (currenyType == CurrencyType.GrowWorldTreePoint)
		{
			SetLabelForGrowTreeEvent();
		}
		else
		{
			SetLabelForConsumeEvent(currenyType);
		}
	}

	public void UpdateWorldGrowTreeEventInfo()
	{
		SetLabelForGrowTreeEvent();
	}

	private void Refresh()
	{
		SetLabelForConsumeEvent(_currencyType);
	}

	private void SetIconProxy(CurrencyType currencyType)
	{
		if (!(IconProxyWidget == null))
		{
			if (_iconProxy == null)
			{
				_iconProxy = IconProxy.Create(IconProxyWidget);
			}
			if (_iconProxy != null)
			{
				_iconProxy.SetupCurrencyNoBg(currencyType);
			}
		}
	}

	private void SetLabelForConsumeEvent(CurrencyType currencyType)
	{
		StaminaConsumePointData data = GameRoot.Instance.StaminaConsumePointDataContainer.GetData(currencyType);
		if (data != null)
		{
			PlayerCurrencyInfo playerCurrencyInfo = GetPlayerCurrencyInfo(currencyType);
			string stringDirect = LocalizationManager.GetStringDirect($"{currencyType.ToString().ToUpper()}_NAME_TITLE");
			long num2;
			if (playerCurrencyInfo == null)
			{
				int num = 0;
				num2 = num;
			}
			else
			{
				num2 = playerCurrencyInfo.DailyAmount;
			}
			long num3 = num2;
			long num4 = playerCurrencyInfo?.Amount ?? 0;
			UIHelper.SetLabelText(LabelName, stringDirect);
			UIHelper.SetLabelText(LabelDailyMaxValue, $"{num3}/{data.DailyMaxValue}");
			UIHelper.SetLabelText(LabelMaxValue, $"{num4}/{data.MaxValue}");
		}
	}

	private void SetLabelForGrowTreeEvent()
	{
		EventStepInfo playerEventStepInfo = EventGrowWorldTreeHelper.GetPlayerEventStepInfo();
		int num = ((playerEventStepInfo == null) ? 1 : (playerEventStepInfo.DailyStep + 1));
		int eventStepTypeDataCount = EventGrowWorldTreeHelper.GetEventStepTypeDataCount(EventStepType.Daily);
		num = ((num >= eventStepTypeDataCount) ? eventStepTypeDataCount : num);
		PlayerCurrencyInfo playerCurrencyInfo = EventGrowWorldTreeHelper.GetPlayerCurrencyInfo(_currencyType);
		long num2;
		if (playerCurrencyInfo != null)
		{
			num2 = playerCurrencyInfo.Amount;
		}
		else
		{
			int num3 = 0;
			num2 = num3;
		}
		long num4 = num2;
		int targetScore = EventGrowWorldTreeHelper.GetTargetScore(EventStepType.Daily, num);
		int num5 = playerEventStepInfo?.TotalCount ?? 0;
		string stringDirect = LocalizationManager.GetStringDirect("EVENT_WORLD_TREE_SINGLE_MISSION_SCORE", num5);
		UIHelper.EnableGameObject(LabelMaxValue, enable: false);
		UIHelper.SetLabelText(LabelDailyValue, $"{num4}/{targetScore}");
		UIHelper.SetLabelText(LabelMissionScoreValue, stringDirect);
	}

	private PlayerCurrencyInfo GetPlayerCurrencyInfo(CurrencyType currencyType)
	{
		if (!AccountManager.IsLogin())
		{
			return null;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.Get(currencyType);
	}

	public void OnClickComponent()
	{
		string stringDirect = LocalizationManager.GetStringDirect("MENU_EVENT_HELP");
		string empty = string.Empty;
		empty = ((_currencyType != CurrencyType.GrowWorldTreePoint) ? "EVENTCRAFT_HELP_DESC" : "EVENT_WORLD_TREE_NOTI_BOARD_MESSAGE");
		string stringDirect2 = LocalizationManager.GetStringDirect(empty);
		MessagePopup.Open(stringDirect, stringDirect2);
	}

	bool LobbyRightGridWindow.IRightGridSort.IsDestroy()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(GetEventTypeList(EventBonusType.GrowWorldTree)))
		{
			return true;
		}
		return false;
	}

	void LobbyRightGridWindow.IRightGridSort.ChainUpdate()
	{
	}

	int LobbyRightGridWindow.IRightGridSort.GetSortIndex()
	{
		return 2;
	}

	public static List<CurrencyType> GetEventTypeList(EventBonusType bonusType)
	{
		if (!GlobalAutoBehaviour<DropBonusEventManager>.isValidInstance)
		{
			return null;
		}
		if (!GlobalAutoBehaviour<DropBonusEventManager>.instance.AnyEffectiveEventByEventBonusType(bonusType))
		{
			return null;
		}
		List<DropBonusEvent> allEffectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetAllEffectiveEvent(bonusType);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(allEffectiveEvent))
		{
			return null;
		}
		return DropBonusEventManager.GetEventConditionStringSplit<CurrencyType>(allEffectiveEvent[0].Condition);
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo>.OnChanged(NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo changedInfo)
	{
		if (changedInfo != null && changedInfo.currencyType == _currencyType)
		{
			if (_currencyType == CurrencyType.GrowWorldTreePoint)
			{
				UpdateWorldGrowTreeEventInfo();
			}
			else
			{
				Refresh();
			}
		}
	}
}
