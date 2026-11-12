using System;
using System.Collections.Generic;
using NGame2.NDropBonusEvent;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NCondition;

public class EventExist : ICondition
{
	bool ICondition.Check(CampaignNodeData nodeData, string[] value)
	{
		return Check(nodeData, value);
	}

	public static bool Check(CampaignNodeData nodeData, string[] value)
	{
		return GetEffectiveEvent(value, 1) != null;
	}

	public static DropBonusEvent GetEffectiveEvent(string[] value, int tokenStartIndex)
	{
		EventBonusType valueEnum = ArgParser.GetValueEnum(value, tokenStartIndex, EventBonusType.None);
		List<DropBonusEvent> allEffectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetAllEffectiveEvent(valueEnum);
		if (__Extension_Method_ICollection__.COUNT(allEffectiveEvent) == 0)
		{
			return null;
		}
		string text = string.Empty;
		int num = tokenStartIndex + 1;
		while (true)
		{
			string valueString = ArgParser.GetValueString(value, num);
			if (String_Extension_Method.IsNullOrEmpty(valueString))
			{
				break;
			}
			text += (String_Extension_Method.IsNullOrEmpty(text) ? "" : ",");
			text += valueString.Trim();
			num++;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allEffectiveEvent); i++)
		{
			DropBonusEvent dropBonusEvent = allEffectiveEvent[i];
			if (dropBonusEvent == null)
			{
				continue;
			}
			if (String_Extension_Method.IsNullOrEmpty(dropBonusEvent.Condition) && String_Extension_Method.IsNullOrEmpty(text))
			{
				return dropBonusEvent;
			}
			string a = string.Empty;
			string[] array = dropBonusEvent.Condition.Split(new char[1] { ',' });
			if (__Extension_Method_ICollection__.COUNT(array) > 0)
			{
				for (int j = 0; j < __Extension_Method_ICollection__.COUNT(array); j++)
				{
					array[j] = array[j].Trim();
				}
				a = Vespa_String_Extension_Method.Join(array, ",");
			}
			if (string.Equals(a, text, StringComparison.InvariantCultureIgnoreCase))
			{
				return dropBonusEvent;
			}
		}
		return null;
	}
}
