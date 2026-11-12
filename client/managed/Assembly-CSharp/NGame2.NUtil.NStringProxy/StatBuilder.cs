using NShared;
using NVespa.NGlobal;

namespace NGame2.NUtil.NStringProxy;

public class StatBuilder : IBuilder
{
	private StatType _statType;

	private long _value;

	private bool _appendLine;

	bool IBuilder.AppendLine => _appendLine;

	public StatBuilder(StatType statType, long value, bool appendLine)
	{
		_statType = statType;
		_value = value;
		_appendLine = appendLine;
	}

	public string Build()
	{
		return GetString(_statType, _value);
	}

	public static string GetString(StatType statType, long value, bool isShowSign = false)
	{
		string statDisplayValue = StatData.GetStatDisplayValue(statType, value, isShowSign);
		string arg = LocalizationManager.GetString(statType);
		return $"{arg} {statDisplayValue}";
	}
}
