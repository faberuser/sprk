using System.Collections;
using NShared;

public class CustomJsonMarshaler
{
	public static T CustomParse<T>(IDictionary d)
	{
		return (T)JsonMarshaler.Parse(typeof(T), d);
	}

	public static T CustomParseString<T>(string s)
	{
		return (T)JsonMarshaler.Parse(typeof(T), s);
	}

	public static bool CustomUserInfoParseField<T>(UserInfo uInfo, string name, out T val)
	{
		return JM_NShared_UserInfo.ParseField<T>(uInfo, name, out val);
	}

	public static bool CustomUserInfoSetField<T>(UserInfo uInfo, string name, T val)
	{
		return JM_NShared_UserInfo.SetField(uInfo, name, val);
	}
}
