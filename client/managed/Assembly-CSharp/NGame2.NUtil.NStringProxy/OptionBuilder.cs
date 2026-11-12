using NShared;

namespace NGame2.NUtil.NStringProxy;

public class OptionBuilder : IBuilder
{
	private OperationType _optionType;

	private int _optionValue;

	private bool _opened;

	private int _openLevel;

	private bool _appendLine;

	bool IBuilder.AppendLine => _appendLine;

	public OptionBuilder(OperationType optionType, int optionValue, bool opened = true, int openLevel = 0, bool appendLine = false)
	{
		_optionType = optionType;
		_optionValue = optionValue;
		_opened = opened;
		_openLevel = openLevel;
		_appendLine = appendLine;
	}

	public string Build()
	{
		return StatData.GetOptionDisplay(_optionType, _optionValue, _opened, _openLevel);
	}
}
