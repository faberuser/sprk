using System;
using System.IO;
using NVespa;

namespace NGameAsset;

public class WebTextureCache : SingletonAuto<WebTextureCache>
{
	public string GetCachedPath(string url)
	{
		try
		{
			string text = ConvertUrlToCachedPath(url);
			if (!File.Exists(text))
			{
				return null;
			}
			return text;
		}
		catch (Exception obj)
		{
			VespaDebug.LogError(obj);
		}
		return null;
	}

	public void AddCache(string url, byte[] bytes)
	{
		try
		{
			string path = ConvertUrlToCachedPath(url);
			string directoryName = Path.GetDirectoryName(path);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			File.WriteAllBytes(path, bytes);
		}
		catch (Exception obj)
		{
			VespaDebug.LogError(obj);
		}
	}

	private string ConvertUrlToCachedPath(string url)
	{
		url = url.Replace("http://", "");
		url = url.Replace("https://", "https_");
		url = url.Replace('/', '_');
		url = url.Replace('\\', '_');
		return Path.Combine(UnityCache.persistentDataPath, "CachedTexture/" + url).Replace('/', Path.DirectorySeparatorChar);
	}
}
