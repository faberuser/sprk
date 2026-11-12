using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_DestroyCreatureNot
{
	public static DestroyCreatureNot Parse(string s)
	{
		return Parse(s, new DestroyCreatureNot());
	}

	public static DestroyCreatureNot Parse(string s, DestroyCreatureNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static DestroyCreatureNot Parse(IDictionary d)
	{
		return Parse(d, new DestroyCreatureNot());
	}

	public static DestroyCreatureNot Parse(IDictionary d, DestroyCreatureNot o)
	{
		o.CreatureUid = BaseJsonMarshaler.ParseValueInt(d, "CreatureUid");
		return o;
	}

	public static string EncodeJsonString(DestroyCreatureNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DestroyCreatureNot o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		return dictionary;
	}
}
