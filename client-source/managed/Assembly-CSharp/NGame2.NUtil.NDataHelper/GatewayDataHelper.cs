using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NDataHelper;

public class GatewayDataHelper
{
	public static bool IsCleared(GatewayToTrialData data)
	{
		if (data == null)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager == null)
		{
			return false;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(data.ChapterIndex, data.DungeonIndex);
	}

	public static bool IsOpened(GatewayToTrialData data)
	{
		if (data == null)
		{
			return false;
		}
		int minStep = GameRoot.Instance.GatewayToTrialDataContainer.MinStep;
		if (data.Step > minStep)
		{
			int step = data.Step - 1;
			GatewayToTrialData dataByStep = GameRoot.Instance.GatewayToTrialDataContainer.GetDataByStep(step);
			if (dataByStep == null)
			{
				return false;
			}
			if (!IsCleared(dataByStep))
			{
				return false;
			}
		}
		return true;
	}
}
