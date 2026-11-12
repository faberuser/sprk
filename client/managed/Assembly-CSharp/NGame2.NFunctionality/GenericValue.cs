using System;

namespace NGame2.NFunctionality;

public class GenericValue<T> : IFunctionalityValue where T : struct
{
	public T Value;

	private Func<string, T> _parser;

	private Func<T, T, bool> _equals;

	public GenericValue(Func<string, T> parser, Func<T, T, bool> equals)
	{
		_parser = parser;
		_equals = equals;
	}

	public bool Check(T value)
	{
		return _equals(Value, value);
	}

	void IFunctionalityValue.SetValue(string value)
	{
		try
		{
			Value = _parser(value);
		}
		catch (Exception)
		{
		}
	}

	string IFunctionalityValue.Dump()
	{
		return Value.ToString() + " (" + typeof(T).Name + ")";
	}
}
