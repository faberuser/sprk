using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class LanguagePatternDataContainer : JitDataContainerDefault<LanguagePatternKey, LanguagePatternData>
{
	public const string TABLENAME = "LanguagePatternTable";

	private JitDictionary<string, string> _patterns_by_ServerName = new JitDictionary<string, string>();

	public override TablePathDefine PathDefine => new TablePathDefine("LanguagePatternTable");

	public LanguagePatternDataContainer()
	{
		base.Packer.RegisterAndBind(_patterns_by_ServerName);
	}

	protected override void OnEndLoadFromJsonData()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (LanguagePatternData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (!dictionary.TryGetValue(item.ServerName, out var value))
			{
				value = string.Empty;
			}
			dictionary[item.ServerName] = value + item.PatternRange;
		}
		string format = "[{0}\\s]+";
		foreach (KeyValuePair<string, string> item2 in dictionary)
		{
			_patterns_by_ServerName.JsonLoad_Get().Add(item2.Key, string.Format(format, item2.Value));
		}
	}

	public string GetData(string serverName)
	{
		if (string.IsNullOrEmpty(serverName))
		{
			return null;
		}
		return _patterns_by_ServerName.GetOrDefault(serverName);
	}
}
