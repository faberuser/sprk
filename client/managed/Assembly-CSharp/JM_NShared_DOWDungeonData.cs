using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_DOWDungeonData
{
	public static DOWDungeonData Parse(string s)
	{
		return Parse(s, new DOWDungeonData());
	}

	public static DOWDungeonData Parse(string s, DOWDungeonData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static DOWDungeonData Parse(IDictionary d)
	{
		return Parse(d, new DOWDungeonData());
	}

	public static DOWDungeonData Parse(IDictionary d, DOWDungeonData o)
	{
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.OpenDOWs = BaseJsonMarshaler.ParseArrayCast(d, "OpenDOWs", BaseJsonMarshaler.CastToInt);
		o.ClassType = BaseJsonMarshaler.ParseEnum<CreatureTagType>(d, "ClassType");
		return o;
	}

	public static string EncodeJsonString(DOWDungeonData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DOWDungeonData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("OpenDOWs", BaseJsonMarshaler.EncodeArray(o.OpenDOWs));
		dictionary.Add("ClassType", o.ClassType.ToString());
		return dictionary;
	}
}
