using System.Collections.Generic;
using NGameAsset;

public static class GameAsset_AsyncLoad_ExtensionMethod
{
	public static string KEYDESC(this IAsyncLoadBase This)
	{
		if (This == null)
		{
			return string.Empty;
		}
		return This.KeyDesc;
	}

	public static bool ISDONE(this IAsyncLoadBase This)
	{
		return This?.IsDone ?? true;
	}

	public static bool ISVALID(this IAsyncLoadBase This)
	{
		return This?.IsValid ?? false;
	}

	public static string LOADMESSAGE(this IAsyncLoadBase This)
	{
		if (This == null)
		{
			return string.Empty;
		}
		return This.LoadMessage;
	}

	public static bool AsyncIsDone<T>(this IList<T> This) where T : IAsyncLoadBase
	{
		if (This == null)
		{
			return true;
		}
		for (int i = 0; i < This.Count; i++)
		{
			if (!This[i].IsDone)
			{
				return false;
			}
		}
		return true;
	}

	public static bool AsyncIsValid<T>(this IList<T> This) where T : IAsyncLoadBase
	{
		if (This == null)
		{
			return true;
		}
		for (int i = 0; i < This.Count; i++)
		{
			if (!This[i].IsValid)
			{
				return false;
			}
		}
		return true;
	}

	public static T VALUE<T>(this IAsyncLoad<T> This)
	{
		if (This == null)
		{
			return default(T);
		}
		return This.Value;
	}
}
