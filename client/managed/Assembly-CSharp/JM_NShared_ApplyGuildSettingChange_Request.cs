using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ApplyGuildSettingChange;

public static class JM_NShared_ApplyGuildSettingChange_Request
{
	public static NShared.ApplyGuildSettingChange.Request Parse(string s)
	{
		return Parse(s, new NShared.ApplyGuildSettingChange.Request());
	}

	public static NShared.ApplyGuildSettingChange.Request Parse(string s, NShared.ApplyGuildSettingChange.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ApplyGuildSettingChange.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ApplyGuildSettingChange.Request());
	}

	public static NShared.ApplyGuildSettingChange.Request Parse(IDictionary d, NShared.ApplyGuildSettingChange.Request o)
	{
		o.Logo = BaseJsonMarshaler.ParseValueByte(d, "Logo");
		o.Back = BaseJsonMarshaler.ParseValueByte(d, "Back");
		o.Flag = BaseJsonMarshaler.ParseValueByte(d, "Flag");
		o.JoinWay = BaseJsonMarshaler.ParseValueByte(d, "JoinWay");
		o.TeamLevel = BaseJsonMarshaler.ParseValueInt(d, "TeamLevel");
		o.Introduce = BaseJsonMarshaler.ParseValueString(d, "Introduce");
		o.Notice = BaseJsonMarshaler.ParseValueString(d, "Notice");
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ApplyGuildSettingChange.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ApplyGuildSettingChange.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Logo", o.Logo.ToString());
		dictionary.Add("Back", o.Back.ToString());
		dictionary.Add("Flag", o.Flag.ToString());
		dictionary.Add("JoinWay", o.JoinWay.ToString());
		dictionary.Add("TeamLevel", o.TeamLevel.ToString());
		if (o.Introduce != null)
		{
			dictionary.Add("Introduce", o.Introduce);
		}
		if (o.Notice != null)
		{
			dictionary.Add("Notice", o.Notice);
		}
		if (o.CountryCode != null)
		{
			dictionary.Add("CountryCode", o.CountryCode);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
