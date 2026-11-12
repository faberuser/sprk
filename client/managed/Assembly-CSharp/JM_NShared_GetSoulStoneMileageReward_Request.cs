using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetSoulStoneMileageReward;

public static class JM_NShared_GetSoulStoneMileageReward_Request
{
	public static NShared.GetSoulStoneMileageReward.Request Parse(string s)
	{
		return Parse(s, new NShared.GetSoulStoneMileageReward.Request());
	}

	public static NShared.GetSoulStoneMileageReward.Request Parse(string s, NShared.GetSoulStoneMileageReward.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetSoulStoneMileageReward.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetSoulStoneMileageReward.Request());
	}

	public static NShared.GetSoulStoneMileageReward.Request Parse(IDictionary d, NShared.GetSoulStoneMileageReward.Request o)
	{
		o.SoulStoneRestoreIndex = BaseJsonMarshaler.ParseValueInt(d, "SoulStoneRestoreIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetSoulStoneMileageReward.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetSoulStoneMileageReward.Request o)
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
