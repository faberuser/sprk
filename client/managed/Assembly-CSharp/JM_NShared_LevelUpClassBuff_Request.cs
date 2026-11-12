using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.LevelUpClassBuff;

public static class JM_NShared_LevelUpClassBuff_Request
{
	public static NShared.LevelUpClassBuff.Request Parse(string s)
	{
		return Parse(s, new NShared.LevelUpClassBuff.Request());
	}

	public static NShared.LevelUpClassBuff.Request Parse(string s, NShared.LevelUpClassBuff.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.LevelUpClassBuff.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.LevelUpClassBuff.Request());
	}

	public static NShared.LevelUpClassBuff.Request Parse(IDictionary d, NShared.LevelUpClassBuff.Request o)
	{
		o.TagType = BaseJsonMarshaler.ParseValueInt(d, "TagType");
		o.ClassBuffIndex = BaseJsonMarshaler.ParseValueInt(d, "ClassBuffIndex");
		o.ClassBuffLevel = BaseJsonMarshaler.ParseValueByte(d, "ClassBuffLevel");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.LevelUpClassBuff.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.LevelUpClassBuff.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("TagType", o.TagType.ToString());
		dictionary.Add("ClassBuffIndex", o.ClassBuffIndex.ToString());
		dictionary.Add("ClassBuffLevel", o.ClassBuffLevel.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
