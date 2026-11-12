using System;
using System.Collections.Generic;
using NGame2.NUI.NHelper;
using NShared;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NUtil.NStringProxy;

public class RuneItemDataBuilder : IBuilder
{
	public enum EMethod
	{
		Desc1
	}

	private ItemData _itemData;

	private PunishmentRuneOptionInfo _punishmentRuneOptionInfo;

	private EMethod _method;

	private bool _appendLine;

	bool IBuilder.AppendLine => _appendLine;

	public RuneItemDataBuilder(ItemData itemData, PunishmentRuneOptionInfo optionInfo, EMethod method, bool appendLine)
	{
		_itemData = itemData;
		_punishmentRuneOptionInfo = optionInfo;
		_method = method;
		_appendLine = appendLine;
	}

	public string Build()
	{
		Dictionary<Enum, long> runeStat = RuneHelper.GetRuneStat(_itemData, _punishmentRuneOptionInfo);
		string text = string.Empty;
		foreach (KeyValuePair<Enum, long> item in runeStat)
		{
			string text2 = LocalizationManager.GetString(item.Key);
			string statDisplayValue = StatData.GetStatDisplayValue(item.Key, item.Value);
			text = text + text2 + "  " + statDisplayValue + "\n";
		}
		string runeAddSkill = RuneHelper.GetRuneAddSkill(_itemData, _punishmentRuneOptionInfo);
		if (!String_Extension_Method.IsNullOrEmpty(runeAddSkill))
		{
			text += "\n";
			text += runeAddSkill;
		}
		if (_method == EMethod.Desc1)
		{
			return text;
		}
		VespaDebug.LogError("Not Implemented Yet: " + Vespa_String_Extension_Method.GetEnumString(_method));
		return string.Empty;
	}
}
