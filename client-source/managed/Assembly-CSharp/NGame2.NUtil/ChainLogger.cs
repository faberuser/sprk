using System.Collections.Generic;

namespace NGame2.NUtil;

public class ChainLogger
{
	public static int maxDefaultStringLength = 900;

	private static Dictionary<string, ChainLogger> _loggerDictionary = new Dictionary<string, ChainLogger>();

	private List<string> _logs;

	private int _stringLength;

	private int _maxStringLength;

	public static ChainLogger GetLogger(string name)
	{
		if (!_loggerDictionary.ContainsKey(name))
		{
			_loggerDictionary.Add(name, new ChainLogger(maxDefaultStringLength));
		}
		return _loggerDictionary[name];
	}

	public ChainLogger(int maxStringLength = 900)
	{
		_logs = new List<string>();
		_stringLength = 0;
		_maxStringLength = maxStringLength;
	}

	public string RecordLog(float time, params string[] values)
	{
		string text = string.Format("({0}){1}", time.ToString("#.##"), string.Join("-", values));
		_logs.Insert(0, text);
		_stringLength += text.Length + 1;
		while (_stringLength > _maxStringLength && _logs.Count > 1)
		{
			int index = _logs.Count - 1;
			_stringLength -= _logs[index].Length + 1;
			_logs.RemoveAt(index);
		}
		return ToString();
	}

	public override string ToString()
	{
		string text = string.Join(",", _logs.ToArray());
		if (text.Length > _maxStringLength)
		{
			return text.Substring(0, _maxStringLength);
		}
		return text;
	}
}
