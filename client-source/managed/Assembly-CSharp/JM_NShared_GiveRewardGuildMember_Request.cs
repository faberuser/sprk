using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GiveRewardGuildMember;

public static class JM_NShared_GiveRewardGuildMember_Request
{
	public static NShared.GiveRewardGuildMember.Request Parse(string s)
	{
		return Parse(s, new NShared.GiveRewardGuildMember.Request());
	}

	public static NShared.GiveRewardGuildMember.Request Parse(string s, NShared.GiveRewardGuildMember.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GiveRewardGuildMember.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GiveRewardGuildMember.Request());
	}

	public static NShared.GiveRewardGuildMember.Request Parse(IDictionary d, NShared.GiveRewardGuildMember.Request o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GiveRewardGuildMember.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GiveRewardGuildMember.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("Index", o.Index.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
