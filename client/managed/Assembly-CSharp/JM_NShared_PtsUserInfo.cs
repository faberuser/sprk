using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PtsUserInfo
{
	public static PtsUserInfo Parse(string s)
	{
		return Parse(s, new PtsUserInfo());
	}

	public static PtsUserInfo Parse(string s, PtsUserInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PtsUserInfo Parse(IDictionary d)
	{
		return Parse(d, new PtsUserInfo());
	}

	public static PtsUserInfo Parse(IDictionary d, PtsUserInfo o)
	{
		o.Region = BaseJsonMarshaler.ParseValueString(d, "Region");
		o.IsEntry = BaseJsonMarshaler.ParseValueBool(d, "IsEntry");
		o.AccountId = BaseJsonMarshaler.ParseValueInt(d, "AccountId");
		o.NickName = BaseJsonMarshaler.ParseValueString(d, "NickName");
		o.TeamLevel = BaseJsonMarshaler.ParseValueInt(d, "TeamLevel");
		return o;
	}

	public static string EncodeJsonString(PtsUserInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PtsUserInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Region != null)
		{
			dictionary.Add("Region", o.Region);
		}
		dictionary.Add("IsEntry", o.IsEntry.ToString());
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.NickName != null)
		{
			dictionary.Add("NickName", o.NickName);
		}
		dictionary.Add("TeamLevel", o.TeamLevel.ToString());
		return dictionary;
	}
}
