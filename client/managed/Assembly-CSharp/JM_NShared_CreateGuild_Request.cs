using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CreateGuild;

public static class JM_NShared_CreateGuild_Request
{
	public static NShared.CreateGuild.Request Parse(string s)
	{
		return Parse(s, new NShared.CreateGuild.Request());
	}

	public static NShared.CreateGuild.Request Parse(string s, NShared.CreateGuild.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CreateGuild.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CreateGuild.Request());
	}

	public static NShared.CreateGuild.Request Parse(IDictionary d, NShared.CreateGuild.Request o)
	{
		o.logo = BaseJsonMarshaler.ParseValueByte(d, "logo");
		o.logoBackground = BaseJsonMarshaler.ParseValueByte(d, "logoBackground");
		o.joinWay = BaseJsonMarshaler.ParseValueByte(d, "joinWay");
		o.introduce = BaseJsonMarshaler.ParseValueString(d, "introduce");
		o.name = BaseJsonMarshaler.ParseValueString(d, "name");
		o.reqTeamLevel = BaseJsonMarshaler.ParseValueInt(d, "reqTeamLevel");
		o.countryCode = BaseJsonMarshaler.ParseValueString(d, "countryCode");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CreateGuild.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CreateGuild.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("logo", o.logo.ToString());
		dictionary.Add("logoBackground", o.logoBackground.ToString());
		dictionary.Add("joinWay", o.joinWay.ToString());
		if (o.introduce != null)
		{
			dictionary.Add("introduce", o.introduce);
		}
		if (o.name != null)
		{
			dictionary.Add("name", o.name);
		}
		dictionary.Add("reqTeamLevel", o.reqTeamLevel.ToString());
		if (o.countryCode != null)
		{
			dictionary.Add("countryCode", o.countryCode);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
