using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RestoreSoulStone;

public static class JM_NShared_RestoreSoulStone_Request
{
	public static NShared.RestoreSoulStone.Request Parse(string s)
	{
		return Parse(s, new NShared.RestoreSoulStone.Request());
	}

	public static NShared.RestoreSoulStone.Request Parse(string s, NShared.RestoreSoulStone.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RestoreSoulStone.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RestoreSoulStone.Request());
	}

	public static NShared.RestoreSoulStone.Request Parse(IDictionary d, NShared.RestoreSoulStone.Request o)
	{
		o.SoulStoneRestoreIndex = BaseJsonMarshaler.ParseValueInt(d, "SoulStoneRestoreIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RestoreSoulStone.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RestoreSoulStone.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SoulStoneRestoreIndex", o.SoulStoneRestoreIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
