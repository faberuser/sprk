using System;

namespace NGame2.NUtil.NStringProxy;

public class StringFuncBuilder : IBuilder
{
	private Func<string> _func;

	private bool _appendLine;

	bool IBuilder.AppendLine => _appendLine;

	public StringFuncBuilder(Func<string> func, bool appendLine)
	{
		_func = func;
		_appendLine = appendLine;
	}

	public string Build()
	{
		string text = Vespa_System_Action_Extension_Method.fire(_func);
		if (text != null)
		{
			return text;
		}
		return string.Empty;
	}
}
