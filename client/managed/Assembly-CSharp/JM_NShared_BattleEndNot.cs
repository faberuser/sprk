using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleEndNot
{
	public static BattleEndNot Parse(string s)
	{
		return Parse(s, new BattleEndNot());
	}

	public static BattleEndNot Parse(string s, BattleEndNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleEndNot Parse(IDictionary d)
	{
		return Parse(d, new BattleEndNot());
	}

	public static BattleEndNot Parse(IDictionary d, BattleEndNot o)
	{
		o.Reason = BaseJsonMarshaler.ParseEnum<BattleEndNot.ReasonType>(d, "Reason");
		return o;
	}

	public static string EncodeJsonString(BattleEndNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleEndNot o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Reason", o.Reason.ToString());
		return dictionary;
	}
}
