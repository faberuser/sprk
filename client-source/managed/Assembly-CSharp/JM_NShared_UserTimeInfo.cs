using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_UserTimeInfo
{
	public static UserTimeInfo Parse(string s)
	{
		return Parse(s, new UserTimeInfo());
	}

	public static UserTimeInfo Parse(string s, UserTimeInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static UserTimeInfo Parse(IDictionary d)
	{
		return Parse(d, new UserTimeInfo());
	}

	public static UserTimeInfo Parse(IDictionary d, UserTimeInfo o)
	{
		o.LastLoginTime = BaseJsonMarshaler.ParseValueString(d, "LastLoginTime");
		o.LastLogoutTime = BaseJsonMarshaler.ParseValueString(d, "LastLogoutTime");
		o.InactiveTime = BaseJsonMarshaler.ParseValueString(d, "InactiveTime");
		return o;
	}

	public static string EncodeJsonString(UserTimeInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(UserTimeInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.LastLoginTime != null)
		{
			dictionary.Add("LastLoginTime", o.LastLoginTime);
		}
		if (o.LastLogoutTime != null)
		{
			dictionary.Add("LastLogoutTime", o.LastLogoutTime);
		}
		if (o.InactiveTime != null)
		{
			dictionary.Add("InactiveTime", o.InactiveTime);
		}
		return dictionary;
	}
}
