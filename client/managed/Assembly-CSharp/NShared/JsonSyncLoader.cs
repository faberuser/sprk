using System;
using System.Collections.Generic;
using System.IO;
using MiniJSON;
using NVespa;

namespace NShared;

public class JsonSyncLoader : IJsonLoader
{
	private readonly string _prefix;

	public JsonSyncLoader(string prefix)
	{
		_prefix = prefix;
	}

	void IJsonLoader.Load(string path, Action<object> onLoad)
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
			List<object> list = JsonPool.instance.CreateList();
			foreach (string partialAssetPath in GetPartialAssetPathList(path2))
			{
				if (Json.Deserialize(File.ReadAllText(partialAssetPath)) is List<object> collection)
				{
					list.AddRange(collection);
				}
			}
			onLoad(list);
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Failed to load json file sync : " + ex);
			onLoad(null);
		}
	}

	private static List<string> GetPartialAssetPathList(string path)
	{
		List<string> list = new List<string>();
		if (string.IsNullOrEmpty(path))
		{
			return list;
		}
		list.Add(path);
		int num = path.LastIndexOf('.');
		if (num == -1)
		{
			return list;
		}
		string text = path.Substring(0, num);
		string text2 = path.Substring(num);
		int num2 = 100;
		for (int i = 1; i <= num2; i++)
		{
			string text3 = text + i + text2;
			if (!File.Exists(text3))
			{
				break;
			}
			list.Add(text3);
		}
		return list;
	}
}
