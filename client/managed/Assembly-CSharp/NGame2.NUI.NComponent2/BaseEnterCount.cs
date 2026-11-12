using NVespa.NGlobal;

namespace NGame2.NUI.NComponent2;

public class BaseEnterCount
{
	public UILabel ResetCountLabel;

	public UILabel EnterCountLabel;

	protected void SetResetCountLabel(int day, int maxEnterCount)
	{
		string stringDirect = LocalizationManager.GetStringDirect("GUILD_LASTLOGINTIME_DAY", day);
		string stringDirect2 = LocalizationManager.GetStringDirect("NUMBER_COUNT", maxEnterCount);
		UIHelper.SetLabelText(ResetCountLabel, stringDirect + " " + stringDirect2);
	}

	protected void SetResetCountLabel(int maxEnterCount, string dayTextKey)
	{
		string stringDirect = LocalizationManager.GetStringDirect(dayTextKey, maxEnterCount);
		UIHelper.SetLabelText(ResetCountLabel, stringDirect);
	}

	protected void SetEnterCountLabel(int maxEnterCount, int completedCount)
	{
		int num = maxEnterCount - completedCount;
		if (num < 0)
		{
			num = 0;
		}
		UIHelper.SetLabelText(EnterCountLabel, num + "/" + maxEnterCount);
	}
}
