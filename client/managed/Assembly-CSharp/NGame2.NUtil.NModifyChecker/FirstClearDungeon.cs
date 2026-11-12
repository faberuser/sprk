using System;
using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NModifyChecker;

public class FirstClearDungeon : ModifyChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 4)
		{
			return false;
		}
		if (!int.TryParse(values[1], out var result) || !int.TryParse(values[2], out var result2) || string.IsNullOrEmpty(values[3]))
		{
			return false;
		}
		DungeonDifficultyType difficulty = (DungeonDifficultyType)Enum.Parse(typeof(DungeonDifficultyType), values[3]);
		int originalDungeonIndex = GameRoot.Instance.CampaignDungeonDataContainer.GetOriginalDungeonIndex(result2);
		if (GameRoot.Instance.CampaignDungeonDataContainer.GetData(result, originalDungeonIndex) == null)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager != null)
		{
			return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.FirstClearRewarded(result, originalDungeonIndex, difficulty);
		}
		return false;
	}
}
