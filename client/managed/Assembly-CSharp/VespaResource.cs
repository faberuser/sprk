using System;
using NGameAsset;
using NVespa;
using UnityEngine;

public class VespaResource
{
	public static T Load<T>(string i_path) where T : UnityEngine.Object
	{
		return Load(i_path, typeof(T)) as T;
	}

	public static UnityEngine.Object Load(string i_path, Type systemTypeInstance)
	{
		int num = i_path.ToLower().IndexOf("resources");
		string text = i_path;
		if (num != -1)
		{
			text = i_path.Substring(num + "Resources/".Length);
		}
		if (!SingletonUpdater<AssetCacheService>.instance.GetCachedResource(text, out var o))
		{
			o = Resources.Load(text, systemTypeInstance);
			SingletonUpdater<AssetCacheService>.instance.CacheResource(text, o);
		}
		return o;
	}

	public static ResourceRequest LoadAsync<T>(string i_path) where T : UnityEngine.Object
	{
		return LoadAsync(i_path, typeof(T));
	}

	public static ResourceRequest LoadAsync(string i_path, Type systemTypeInstance)
	{
		int num = i_path.ToLower().IndexOf("resources");
		if (num != -1)
		{
			return Resources.LoadAsync(i_path.Substring(num + "Resources/".Length), systemTypeInstance);
		}
		return Resources.LoadAsync(i_path, systemTypeInstance);
	}

	public static string RemoveExtensionIfSame(string i_orig, string extension)
	{
		if (string.Compare(i_orig, extension, ignoreCase: true) == 0)
		{
			return i_orig.Substring(0, i_orig.Length - extension.Length);
		}
		return i_orig;
	}
}
