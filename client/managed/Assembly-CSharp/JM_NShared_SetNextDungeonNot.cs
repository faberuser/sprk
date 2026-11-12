using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_SetNextDungeonNot
{
	public static SetNextDungeonNot Parse(string s)
	{
		return Parse(s, new SetNextDungeonNot());
	}

	public static SetNextDungeonNot Parse(string s, SetNextDungeonNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static SetNextDungeonNot Parse(IDictionary d)
	{
		return Parse(d, new SetNextDungeonNot());
	}

	public static SetNextDungeonNot Parse(IDictionary d, SetNextDungeonNot o)
	{
		o.ChatperIndex = BaseJsonMarshaler.ParseValueInt(d, "ChatperIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		return o;
	}

	public static string EncodeJsonString(SetNextDungeonNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SetNextDungeonNot o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChatperIndex", o.ChatperIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		return dictionary;
	}
}
