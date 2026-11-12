using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CreatureSetAiBattleLog
{
	public static CreatureSetAiBattleLog Parse(string s)
	{
		return Parse(s, new CreatureSetAiBattleLog());
	}

	public static CreatureSetAiBattleLog Parse(string s, CreatureSetAiBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CreatureSetAiBattleLog Parse(IDictionary d)
	{
		return Parse(d, new CreatureSetAiBattleLog());
	}

	public static CreatureSetAiBattleLog Parse(IDictionary d, CreatureSetAiBattleLog o)
	{
		o.PlayerId = BaseJsonMarshaler.ParseValueLong(d, "PlayerId");
		o.SetAi = BaseJsonMarshaler.ParseValueBool(d, "SetAi");
		return o;
	}

	public static string EncodeJsonString(CreatureSetAiBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CreatureSetAiBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PlayerId", o.PlayerId.ToString());
		dictionary.Add("SetAi", o.SetAi.ToString());
		return dictionary;
	}
}
