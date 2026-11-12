using System;
using System.IO;
using NShared.NUtil;
using NVespa;

namespace NShared;

public class AiScriptContainer
{
	public const string ScriptPath = "AiScript";

	public const string DefaultAiScriptName = "DefaultAi";

	public const string Extension = ".xml";

	private readonly IndexableDictionay<string, string> _aiScriptDictionary;

	private readonly object locker = new object();

	private ITextLoader _textLoader;

	private string _defaultAiScript;

	public AiScriptContainer()
	{
		_aiScriptDictionary = new IndexableDictionay<string, string>(16);
	}

	public void SetTextLoader(ITextLoader textSyncLoader)
	{
		_textLoader = textSyncLoader;
	}

	public string GetDefaultAiScript()
	{
		if (string.IsNullOrEmpty(_defaultAiScript))
		{
			_defaultAiScript = GetAiScriptByCreatureCode("DefaultAi");
		}
		return _defaultAiScript;
	}

	public string GetAiScriptByCreatureCode(string code)
	{
		lock (locker)
		{
			string aiScript = null;
			string aiScriptPath = GetAiScriptPath(code);
			if (_aiScriptDictionary.ContainsKey(aiScriptPath))
			{
				aiScript = _aiScriptDictionary[aiScriptPath];
			}
			else
			{
				try
				{
					_textLoader.Load(aiScriptPath, delegate(string text)
					{
						aiScript = text;
						_aiScriptDictionary.Add(aiScriptPath, aiScript);
					});
				}
				catch (Exception obj)
				{
					VespaDebug.LogError(obj);
				}
			}
			return aiScript;
		}
	}

	private string GetAiScriptByPath(string path)
	{
		string value = null;
		_aiScriptDictionary.TryGetValue(path, out value);
		return value;
	}

	private string GetAiScriptPath(string code)
	{
		return Path.Combine("AiScript", code) + ".xml";
	}
}
