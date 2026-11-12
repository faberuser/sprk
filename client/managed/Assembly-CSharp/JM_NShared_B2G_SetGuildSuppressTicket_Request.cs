using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2G_SetGuildSuppressTicket;

public static class JM_NShared_B2G_SetGuildSuppressTicket_Request
{
	public static NShared.B2G_SetGuildSuppressTicket.Request Parse(string s)
	{
		return Parse(s, new NShared.B2G_SetGuildSuppressTicket.Request());
	}

	public static NShared.B2G_SetGuildSuppressTicket.Request Parse(string s, NShared.B2G_SetGuildSuppressTicket.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2G_SetGuildSuppressTicket.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2G_SetGuildSuppressTicket.Request());
	}

	public static NShared.B2G_SetGuildSuppressTicket.Request Parse(IDictionary d, NShared.B2G_SetGuildSuppressTicket.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.DungeonDifficulty = BaseJsonMarshaler.ParseValueInt(d, "DungeonDifficulty");
		return o;
	}

	public static string EncodeJsonString(NShared.B2G_SetGuildSuppressTicket.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2G_SetGuildSuppressTicket.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("DungeonDifficulty", o.DungeonDifficulty.ToString());
		return dictionary;
	}
}
