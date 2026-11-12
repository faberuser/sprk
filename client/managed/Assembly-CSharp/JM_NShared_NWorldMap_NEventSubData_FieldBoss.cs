using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NWorldMap.NEventSubData;

public static class JM_NShared_NWorldMap_NEventSubData_FieldBoss
{
	public static FieldBoss Parse(string s)
	{
		return Parse(s, new FieldBoss());
	}

	public static FieldBoss Parse(string s, FieldBoss o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static FieldBoss Parse(IDictionary d)
	{
		return Parse(d, new FieldBoss());
	}

	public static FieldBoss Parse(IDictionary d, FieldBoss o)
	{
		o.BossCreatureIndex = BaseJsonMarshaler.ParseValueInt(d, "BossCreatureIndex");
		return o;
	}

	public static string EncodeJsonString(FieldBoss o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(FieldBoss o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BossCreatureIndex", o.BossCreatureIndex.ToString());
		return dictionary;
	}
}
