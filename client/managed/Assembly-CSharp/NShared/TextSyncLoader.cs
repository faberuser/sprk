using System;
using System.IO;
using NVespa;

namespace NShared;

public class TextSyncLoader : ITextLoader
{
	private readonly string _prefix;

	public TextSyncLoader(string prefix)
	{
		_prefix = prefix;
	}

	void ITextLoader.Load(string path, Action<string> onLoad)
	{
		if (onLoad == null)
		{
			return;
		}
		string path2 = Path.Combine(_prefix, path);
		if (!File.Exists(path2))
		{
			onLoad(null);
			return;
		}
		try
		{
			string obj = File.ReadAllText(path2);
			onLoad(obj);
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Failed to load file sync : " + ex);
			onLoad(null);
		}
	}
}
