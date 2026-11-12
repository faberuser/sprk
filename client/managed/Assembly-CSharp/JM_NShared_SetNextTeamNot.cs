using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_SetNextTeamNot
{
	public static SetNextTeamNot Parse(string s)
	{
		return Parse(s, new SetNextTeamNot());
	}

	public static SetNextTeamNot Parse(string s, SetNextTeamNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static SetNextTeamNot Parse(IDictionary d)
	{
		return Parse(d, new SetNextTeamNot());
	}

	public static SetNextTeamNot Parse(IDictionary d, SetNextTeamNot o)
	{
		o.ChatperIndex = BaseJsonMarshaler.ParseValueInt(d, "ChatperIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.DeckIndex = BaseJsonMarshaler.ParseValueInt(d, "DeckIndex");
		return o;
	}

	public static string EncodeJsonString(SetNextTeamNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SetNextTeamNot o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChatperIndex", o.ChatperIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("DeckIndex", o.DeckIndex.ToString());
		return dictionary;
	}
}
