using System;
using System.Text;
using NShared.NUtil;

namespace NShared;

public class MessageBuilder
{
	public static string Build<T>(T data, Func<T, string> serializer) where T : class
	{
		if (data == null)
		{
			return null;
		}
		if (serializer == null)
		{
			return null;
		}
		Type typeFromHandle = typeof(T);
		string serialized = serializer(data);
		return Build(typeFromHandle, serialized);
	}

	public static string Build(Type type, string serialized)
	{
		if (string.IsNullOrEmpty(serialized))
		{
			return null;
		}
		string name = type.Name;
		string value = Base64Encoder.ToBase64String(serialized);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(name);
		stringBuilder.Append(" ");
		stringBuilder.Append(value);
		stringBuilder.Append("\r\n");
		return stringBuilder.ToString();
	}
}
