using NGame2.NAccount;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEventdungeon.NCondition;

public class OpenPrevDungeonClear : IEventDungeonCondition
{
	bool IEventDungeonCondition.Check(EventDungeonData dungeonData, string[] values, out string labelText)
	{
		labelText = "";
		if (dungeonData == null)
		{
			return false;
		}
		return Check(dungeonData.ChapterIndex, dungeonData.DungeonIndex, values, out labelText);
	}

	private bool Check(int chapaterIndex, int dungeonIdex, string[] values, out string labelText)
	{
		labelText = "";
		if (values == null || values.Length != 1)
		{
			return false;
		}
		if (dungeonIdex == 1)
		{
			return true;
		}
		bool num = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(chapaterIndex, dungeonIdex - 1);
		if (!num)
		{
			string valueString = ArgParser.GetValueString(values, 0);
			labelText = LocalizationManager.GetStringDirect(valueString);
		}
		return num;
	}
}
