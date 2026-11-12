namespace NVespa;

public class GenericWrapper<T> : IWrapper
{
	public T Value;

	public GenericWrapper()
	{
	}

	public GenericWrapper(T value)
	{
		Value = value;
	}
}
