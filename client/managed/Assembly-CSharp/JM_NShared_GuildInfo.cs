using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildInfo
{
	public static GuildInfo Parse(string s)
	{
		return Parse(s, new GuildInfo());
	}

	public static GuildInfo Parse(string s, GuildInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildInfo());
	}

	public static GuildInfo Parse(IDictionary d, GuildInfo o)
	{
		o.Id = BaseJsonMarshaler.ParseValueLong(d, "Id");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.MasterNick = BaseJsonMarshaler.ParseValueString(d, "MasterNick");
		o.Level = BaseJsonMarshaler.ParseValueByte(d, "Level");
		o.Logo = BaseJsonMarshaler.ParseValueByte(d, "Logo");
		o.Back = BaseJsonMarshaler.ParseValueByte(d, "Back");
		o.Flag = BaseJsonMarshaler.ParseValueByte(d, "Flag");
		o.Introduce = BaseJsonMarshaler.ParseValueString(d, "Introduce");
		o.ReqTeamLevel = BaseJsonMarshaler.ParseValueInt(d, "ReqTeamLevel");
		o.JoinWay = BaseJsonMarshaler.ParseValueByte(d, "JoinWay");
		o.CurMember = BaseJsonMarshaler.ParseValueByte(d, "CurMember");
		return o;
	}

	public static string EncodeJsonString(GuildInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Id", o.Id.ToString());
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		if (o.MasterNick != null)
		{
			dictionary.Add("MasterNick", o.MasterNick);
		}
		dictionary.Add("Level", o.Level.ToString());
		dictionary.Add("Logo", o.Logo.ToString());
		dictionary.Add("Back", o.Back.ToString());
		dictionary.Add("Flag", o.Flag.ToString());
		if (o.Introduce != null)
		{
			dictionary.Add("Introduce", o.Introduce);
		}
		dictionary.Add("ReqTeamLevel", o.ReqTeamLevel.ToString());
		dictionary.Add("JoinWay", o.JoinWay.ToString());
		dictionary.Add("CurMember", o.CurMember.ToString());
		return dictionary;
	}
}
