using System;

namespace NShared;

public static class EnumParser
{
	public static T Parse<T>(string input)
	{
		return (T)Enum.Parse(typeof(T), input);
	}
}
