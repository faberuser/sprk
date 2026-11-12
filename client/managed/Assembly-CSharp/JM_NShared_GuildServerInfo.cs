using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildServerInfo
{
	public static GuildServerInfo Parse(string s)
	{
		return Parse(s, new GuildServerInfo());
	}

	public static GuildServerInfo Parse(string s, GuildServerInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildServerInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildServerInfo());
	}

	public static GuildServerInfo Parse(IDictionary d, GuildServerInfo o)
	{
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.Logo = BaseJsonMarshaler.ParseValueByte(d, "Logo");
		o.Back = BaseJsonMarshaler.ParseValueByte(d, "Back");
		o.Flag = BaseJsonMarshaler.ParseValueByte(d, "Flag");
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		o.MemberCount = BaseJsonMarshaler.ParseValueByte(d, "MemberCount");
		return o;
	}

	public static string EncodeJsonString(GuildServerInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildServerInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		dictionary.Add("Logo", o.Logo.ToString());
		dictionary.Add("Back", o.Back.ToString());
		dictionary.Add("Flag", o.Flag.ToString());
		if (o.CountryCode != null)
		{
			dictionary.Add("CountryCode", o.CountryCode);
		}
		dictionary.Add("MemberCount", o.MemberCount.ToString());
		return dictionary;
	}
}
