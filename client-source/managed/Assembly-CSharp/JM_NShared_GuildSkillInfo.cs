using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSkillInfo
{
	public static GuildSkillInfo Parse(string s)
	{
		return Parse(s, new GuildSkillInfo());
	}

	public static GuildSkillInfo Parse(string s, GuildSkillInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSkillInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildSkillInfo());
	}

	public static GuildSkillInfo Parse(IDictionary d, GuildSkillInfo o)
	{
		o.SkillIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex");
		o.SkillLevel = BaseJsonMarshaler.ParseValueByte(d, "SkillLevel");
		o.ReqCount = BaseJsonMarshaler.ParseValueByte(d, "ReqCount");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		return o;
	}

	public static string EncodeJsonString(GuildSkillInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSkillInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SkillIndex", o.SkillIndex.ToString());
		dictionary.Add("SkillLevel", o.SkillLevel.ToString());
		dictionary.Add("ReqCount", o.ReqCount.ToString());
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		return dictionary;
	}
}
