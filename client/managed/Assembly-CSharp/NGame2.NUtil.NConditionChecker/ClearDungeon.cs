using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class ClearDungeon : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 2)
		{
			return false;
		}
		int result = 0;
		if (!int.TryParse(values[1], out result))
		{
			return false;
		}
		int num = 0;
		int num2 = 8;
		int num3 = 10;
		for (int i = 1; i <= num2; i++)
		{
			for (int j = 1; j <= num3; j++)
			{
				ChapterDungeonInfo chapterDungeonInfo = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.Get(i, j);
				num += chapterDungeonInfo.DailyCompletedCount;
				if (num >= result)
				{
					return true;
				}
			}
		}
		return false;
	}
}
