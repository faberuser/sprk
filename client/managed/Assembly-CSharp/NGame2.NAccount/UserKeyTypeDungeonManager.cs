using System;
using System.Collections.Generic;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NAccount;

public class UserKeyTypeDungeonManager
{
	private UserConquestDungeonManager _conquestManager;

	public UserKeyTypeDungeonManager()
	{
		_conquestManager = new UserConquestDungeonManager();
	}

	public void Apply(ConquestDungeonInfo info)
	{
		if (info != null && _conquestManager != null)
		{
			_conquestManager.Apply(info);
		}
	}

	public void Apply(IList<ConquestDungeonInfo> info)
	{
		if (_conquestManager != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(info); i++)
			{
				_conquestManager.Apply(info[i]);
			}
		}
	}

	public int GetChapterRemainCount(StaminaType staminaType, int reqChapterIndex = 0)
	{
		if (staminaType == StaminaType.ConquestKey)
		{
			if (_conquestManager == null)
			{
				return 0;
			}
			return _conquestManager.GetChapterRemainCount(reqChapterIndex);
		}
		return 0;
	}

	public void GetChapterRemainCount(StaminaType staminaType, out long remainCount, out long maxCount, int reqChpaterIndex = 0)
	{
		int num = 0;
		remainCount = num;
		int num2 = 0;
		maxCount = num2;
		if (staminaType == StaminaType.ConquestKey && _conquestManager != null)
		{
			_conquestManager.GetChapterRemainCount(out remainCount, out maxCount, reqChpaterIndex);
		}
	}

	public int GetCurrentChapterRechargeGem(StaminaType staminaType, int reqChapterIndex = 0)
	{
		int result = 0;
		if (staminaType == StaminaType.ConquestKey)
		{
			if (_conquestManager == null)
			{
				return 0;
			}
			result = _conquestManager.GetCurrentChapterRechargeGem(reqChapterIndex);
		}
		return result;
	}

	public int GetChapterMaxCount(StaminaType staminaType)
	{
		if (staminaType == StaminaType.ConquestKey)
		{
			if (_conquestManager == null)
			{
				return 0;
			}
			return _conquestManager.GetChapterMaxCount();
		}
		return 0;
	}

	public bool GetChapterCount(StaminaType staminaType, int chapterIndex, out int remainCount, out int maxCount)
	{
		remainCount = 0;
		maxCount = 0;
		if (staminaType == StaminaType.ConquestKey)
		{
			if (_conquestManager == null)
			{
				return false;
			}
			return _conquestManager.GetChapterCount(chapterIndex, out remainCount, out maxCount);
		}
		return false;
	}

	public void Update()
	{
		if (_conquestManager != null)
		{
			_conquestManager.Update();
		}
	}

	public bool IsKeyTypeDungeon(StaminaType staminaType)
	{
		if (staminaType == StaminaType.ConquestKey)
		{
			return true;
		}
		return false;
	}

	public void GetKeyTooltipInfo(StaminaType staminaType, out string FullStaminaRechargeRemainTime, out string OneStaminaRechargeRemainTime)
	{
		FullStaminaRechargeRemainTime = LocalizationManager.GetStringDirect("ALLRECHARGE_TIME_ANOTHER");
		OneStaminaRechargeRemainTime = string.Empty;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("DOWDungeonResetHour");
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		int num = (int)(NVespa.Math.CreateDateTime(currentServerLocalTime.Year, currentServerLocalTime.Month, currentServerLocalTime.Day, valueInt, 0, 0) - currentServerLocalTime).TotalSeconds;
		if (num < 0)
		{
			num += 86400;
		}
		int num2 = num / 3600;
		int num3 = (num - num2 * 3600) / 60;
		num %= 60;
		LocalizationManager.GetString(Tower.TOWER_REMAIN_TIME, num2, num3);
		string text = $": {num2:D2}:{num3:D2}:{num:D2}";
		FullStaminaRechargeRemainTime += text;
		int num4 = 0;
		if (staminaType == StaminaType.ConquestKey && _conquestManager != null)
		{
			num4 = _conquestManager.GetCurrentChapterRechargeGem();
		}
		if (num4 > 0)
		{
			OneStaminaRechargeRemainTime = LocalizationManager.GetString(NShared.NLocalization.System.FULLRECHARGE_COST_RUBY_DESC, num4);
		}
	}
}
