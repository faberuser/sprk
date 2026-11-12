using NShared;

public static class __Extension_Method_NShared_IDataKeySelector__
{
	public static K GETKEY<K>(this IDataKeySelector<K> This)
	{
		if (This == null)
		{
			return default(K);
		}
		return This.Key;
	}
}
