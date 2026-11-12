using NGame2.NAccount;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEventdungeon.NCondition;

public class OpenOwnedHero : IEventDungeonCondition
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
		if (__Extension_Method_ICollection__.IsNullOrEmpty(values))
		{
			return false;
		}
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return false;
		}
		bool flag = true;
		for (int i = 1; i < __Extension_Method_ICollection__.COUNT(values); i++)
		{
			int valueInt = ArgParser.GetValueInt(values, i);
			if (valueInt == 0)
			{
				VespaDebug.LogError($"{GetType().ToString()}::Check // Invalid HeroIndex {valueInt}");
				flag = false;
				break;
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(valueInt) == null)
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			string valueString = ArgParser.GetValueString(values, 0);
			labelText = LocalizationManager.GetStringDirect(valueString);
		}
		return flag;
	}
}
