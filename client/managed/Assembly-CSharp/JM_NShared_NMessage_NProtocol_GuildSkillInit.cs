using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_GuildSkillInit
{
	public static GuildSkillInit Parse(string s)
	{
		return Parse(s, new GuildSkillInit());
	}

	public static GuildSkillInit Parse(string s, GuildSkillInit o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSkillInit Parse(IDictionary d)
	{
		return Parse(d, new GuildSkillInit());
	}

	public static GuildSkillInit Parse(IDictionary d, GuildSkillInit o)
	{
		o.ActivityPoint = BaseJsonMarshaler.ParseValueInt(d, "ActivityPoint");
		o.SuppressPoint = BaseJsonMarshaler.ParseValueInt(d, "SuppressPoint");
		return o;
	}

	public static string EncodeJsonString(GuildSkillInit o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSkillInit o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ActivityPoint", o.ActivityPoint.ToString());
		dictionary.Add("SuppressPoint", o.SuppressPoint.ToString());
		return dictionary;
	}
}
