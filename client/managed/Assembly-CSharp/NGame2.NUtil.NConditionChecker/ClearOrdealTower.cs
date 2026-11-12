using NGame2.NAccount;
using NShared;
using NShared.NUtil;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class ClearOrdealTower : ConditionChecker.ConditionBase
{
	private const string _condition = "ClearOrdealTower";

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
		if (!instance.CheckCount(StaminaType.None, count, checkType) || !CheckOrdealTowerProgressTime(minutes, checkType2))
		{
			return false;
		}
		return true;
	}

	private bool CheckOrdealTowerProgressTime(int minutes, CheckType minutesCheckType)
	{
		using (TemporaryList<TowerData> temporaryList = GameRoot.Instance.TowerDataContainer.GetTowers(TowerType.Ordeal))
		{
			if (temporaryList.Count > 0)
			{
				string tutorialCompletedTime = SingletonAuto<ConditionChecker>.instance.GetTutorialCompletedTime("ClearOrdealTower");
				TowerInfo towerInfo = GlobalAutoBehaviour<AccountManager>.instance.TowerManager.Get(temporaryList[0].TowerIndex);
				if (towerInfo != null)
				{
					if (String_Extension_Method.IsNullOrEmpty(towerInfo.CompletedTime))
					{
						ConditionChecker instance = SingletonAuto<ConditionChecker>.instance;
						int totalMinutes = SingletonAuto<ConditionChecker>.instance.GetTotalMinutes(tutorialCompletedTime);
						return instance.CheckCheckType(minutesCheckType, minutes, totalMinutes);
					}
					ConditionChecker instance2 = SingletonAuto<ConditionChecker>.instance;
					int totalMinutes2 = SingletonAuto<ConditionChecker>.instance.GetTotalMinutes(towerInfo.CompletedTime);
					return instance2.CheckCheckType(minutesCheckType, minutes, totalMinutes2);
				}
				ConditionChecker instance3 = SingletonAuto<ConditionChecker>.instance;
				int totalMinutes3 = SingletonAuto<ConditionChecker>.instance.GetTotalMinutes(tutorialCompletedTime);
				return instance3.CheckCheckType(minutesCheckType, minutes, totalMinutes3);
			}
		}
		return false;
	}
}
