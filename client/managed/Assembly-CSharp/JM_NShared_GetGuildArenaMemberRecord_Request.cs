using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildArenaMemberRecord;

public static class JM_NShared_GetGuildArenaMemberRecord_Request
{
	public static NShared.GetGuildArenaMemberRecord.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGuildArenaMemberRecord.Request());
	}

	public static NShared.GetGuildArenaMemberRecord.Request Parse(string s, NShared.GetGuildArenaMemberRecord.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildArenaMemberRecord.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildArenaMemberRecord.Request());
	}

	public static NShared.GetGuildArenaMemberRecord.Request Parse(IDictionary d, NShared.GetGuildArenaMemberRecord.Request o)
	{
		o.SessionIndex = BaseJsonMarshaler.ParseValueInt(d, "SessionIndex");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.RecordType = BaseJsonMarshaler.ParseEnum<GuildArenaRecordType>(d, "RecordType");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGuildArenaMemberRecord.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGuildArenaMemberRecord.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		dictionary.Add("RecordType", o.RecordType.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
