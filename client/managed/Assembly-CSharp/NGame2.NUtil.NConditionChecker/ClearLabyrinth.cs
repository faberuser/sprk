using System;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NUtil;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class ClearLabyrinth : ConditionChecker.ConditionBase
{
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
		if (!instance.CheckCount(StaminaType.HideoutKey, count, checkType) || !CheckLabyrinthProgressTime(minutes, checkType2))
		{
			return false;
		}
		return true;
	}

	private bool CheckLabyrinthProgressTime(int minutes, CheckType minutesCheckType)
	{
		using (TemporaryList<TowerData> temporaryList = GameRoot.Instance.TowerDataContainer.GetTowers(TowerType.Labyrinth))
		{
			if (temporaryList.Count > 0)
			{
				TowerInfo towerInfo = GlobalAutoBehaviour<AccountManager>.instance.TowerManager.Get(temporaryList[0].TowerIndex);
				if (towerInfo != null)
				{
					if (String_Extension_Method.IsNullOrEmpty(towerInfo.CompletedTime))
					{
						return true;
					}
					if (Vespa_String_Extension_Method2.TryParseNetworkTime2(towerInfo.CompletedTime, out var o_dateTime))
					{
						TimeSpan timeSpan = Common.GetUTCNow() - o_dateTime;
						return SingletonAuto<ConditionChecker>.instance.CheckCheckType(minutesCheckType, minutes, (long)timeSpan.TotalMinutes);
					}
				}
			}
		}
		return false;
	}
}
