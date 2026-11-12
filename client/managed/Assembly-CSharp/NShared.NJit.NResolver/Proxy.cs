namespace NShared.NJit.NResolver;

public static class Proxy<T>
{
	private static IKeyComparerT<T> _keyComparer;

	private static IDataResolverT<T> _dataResolver;

	public static IKeyComparerT<T> KeyComparer
	{
		get
		{
			if (_keyComparer == null)
			{
				_keyComparer = ResolverContainer.GetComparer<T>();
			}
			return _keyComparer;
		}
	}

	public static IDataResolverT<T> DataResolver
	{
		get
		{
			if (_dataResolver == null)
			{
				_dataResolver = ResolverContainer.Get<T>();
			}
			return _dataResolver;
		}
	}
}
