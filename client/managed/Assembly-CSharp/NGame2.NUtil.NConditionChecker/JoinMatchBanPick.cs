using System;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NUtil;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class JoinMatchBanPick : ConditionChecker.ConditionBase
{
	private const string _condition = "JoinMatchBanPick";

	public override bool Check(string[] values)
	{
		int count = 0;
		int minutes = 0;
		ConditionChecker instance = SingletonAuto<ConditionChecker>.instance;
		if (instance == null)
		{
			return false;
		}
		if (!instance.CheckGetValues(values, ref count, ref minutes))
		{
			return false;
		}
		CheckType checkType = CheckType.None;
		CheckType checkType2 = CheckType.None;
		if (!SingletonAuto<ConditionChecker>.instance.CheckGetCheckType(values[2], ref checkType) || !SingletonAuto<ConditionChecker>.instance.CheckGetCheckType(values[4], ref checkType2))
		{
			return false;
		}
		if (!instance.CheckCount(StaminaType.Sword2, count, checkType) || !CheckJoinMatchBanPickProgressTime(minutes, checkType2))
		{
			return false;
		}
		return true;
	}

	private bool CheckJoinMatchBanPickProgressTime(int minutes, CheckType minutesCheckType)
	{
		string tutorialCompletedTime = SingletonAuto<ConditionChecker>.instance.GetTutorialCompletedTime("JoinMatchBanPick");
		if (tutorialCompletedTime != "")
		{
			int totalMinutes = SingletonAuto<ConditionChecker>.instance.GetTotalMinutes(tutorialCompletedTime);
			long userValue = totalMinutes;
			if (SingletonAuto<ConditionChecker>.instance.CheckCheckType(minutesCheckType, minutes, userValue))
			{
				int num = 0;
				long currentValue = num;
				int num2 = 0;
				long maxValue = num2;
				if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(StaminaType.Sword2, out currentValue, out maxValue) && currentValue >= maxValue)
				{
					int totalMinutes2 = SingletonAuto<ConditionChecker>.instance.GetTotalMinutes(GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.PlayTimeUpdatedTime);
					userValue = totalMinutes2;
					return SingletonAuto<ConditionChecker>.instance.CheckCheckType(minutesCheckType, minutes, userValue);
				}
				DateTime fullRechargeTime = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetFullRechargeTime(StaminaType.Sword2);
				TimeSpan timeSpan = fullRechargeTime - Common.GetUTCNow();
				return SingletonAuto<ConditionChecker>.instance.CheckCheckType(minutesCheckType, minutes, -(long)timeSpan.TotalMinutes);
			}
		}
		return false;
	}
}
