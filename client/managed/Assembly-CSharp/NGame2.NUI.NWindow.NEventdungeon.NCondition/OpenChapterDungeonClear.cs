using NGame2.NAccount;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEventdungeon.NCondition;

public class OpenChapterDungeonClear : IEventDungeonCondition
{
	bool IEventDungeonCondition.Check(EventDungeonData dungeonData, string[] values, out string labelText)
	{
		labelText = "";
		if (dungeonData == null)
		{
			return false;
		}
		return Check(values, out labelText);
	}

	private bool Check(string[] values, out string labelText)
	{
		labelText = "";
		if (values == null || values.Length != 3)
		{
			return false;
		}
		int valueInt = ArgParser.GetValueInt(values, 0);
		int valueInt2 = ArgParser.GetValueInt(values, 1);
		bool num = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(valueInt, valueInt2);
		if (!num)
		{
			string valueString = ArgParser.GetValueString(values, 2);
			labelText = LocalizationManager.GetStringDirect(valueString);
		}
		return num;
	}
}
