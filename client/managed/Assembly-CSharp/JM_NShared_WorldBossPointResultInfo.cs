using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WorldBossPointResultInfo
{
	public static WorldBossPointResultInfo Parse(string s)
	{
		return Parse(s, new WorldBossPointResultInfo());
	}

	public static WorldBossPointResultInfo Parse(string s, WorldBossPointResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WorldBossPointResultInfo Parse(IDictionary d)
	{
		return Parse(d, new WorldBossPointResultInfo());
	}

	public static WorldBossPointResultInfo Parse(IDictionary d, WorldBossPointResultInfo o)
	{
		o.AddValue = BaseJsonMarshaler.ParseValueLong(d, "AddValue");
		o.NewValue = BaseJsonMarshaler.ParseValueLong(d, "NewValue");
		return o;
	}

	public static string EncodeJsonString(WorldBossPointResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WorldBossPointResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AddValue", o.AddValue.ToString());
		dictionary.Add("NewValue", o.NewValue.ToString());
		return dictionary;
	}
}
