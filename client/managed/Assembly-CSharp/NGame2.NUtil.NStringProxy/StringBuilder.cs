namespace NGame2.NUtil.NStringProxy;

public class StringBuilder : IBuilder
{
	private string _str;

	private bool _appendLine;

	bool IBuilder.AppendLine => _appendLine;

	public StringBuilder(string str, bool appendLine)
	{
		_str = str;
		_appendLine = appendLine;
	}

	public string Build()
	{
		if (_str != null)
		{
			return _str;
		}
		return string.Empty;
	}
}
