using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleCancelNot
{
	public static BattleCancelNot Parse(string s)
	{
		return Parse(s, new BattleCancelNot());
	}

	public static BattleCancelNot Parse(string s, BattleCancelNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleCancelNot Parse(IDictionary d)
	{
		return Parse(d, new BattleCancelNot());
	}

	public static BattleCancelNot Parse(IDictionary d, BattleCancelNot o)
	{
		o.Reason = BaseJsonMarshaler.ParseEnum<BattleCancelNot.ReasonType>(d, "Reason");
		return o;
	}

	public static string EncodeJsonString(BattleCancelNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleCancelNot o)
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
