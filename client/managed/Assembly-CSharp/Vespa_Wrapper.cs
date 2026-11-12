using NVespa;

public static class Vespa_Wrapper
{
	public static bool TryGetValue<T>(this IWrapper This, out T value)
	{
		if (This == null)
		{
			value = default(T);
			return false;
		}
		if (!(This is GenericWrapper<T> genericWrapper))
		{
			value = default(T);
			return false;
		}
		value = genericWrapper.Value;
		return true;
	}

	public static T GetValue<T>(this IWrapper This, T defaultValue)
	{
		if (!This.TryGetValue<T>(out var value))
		{
			return defaultValue;
		}
		return value;
	}
}
