using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.EquipGachaCeilingReward;

public static class JM_NShared_EquipGachaCeilingReward_Request
{
	public static NShared.EquipGachaCeilingReward.Request Parse(string s)
	{
		return Parse(s, new NShared.EquipGachaCeilingReward.Request());
	}

	public static NShared.EquipGachaCeilingReward.Request Parse(string s, NShared.EquipGachaCeilingReward.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.EquipGachaCeilingReward.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.EquipGachaCeilingReward.Request());
	}

	public static NShared.EquipGachaCeilingReward.Request Parse(IDictionary d, NShared.EquipGachaCeilingReward.Request o)
	{
		o.GachaIndex = BaseJsonMarshaler.ParseValueInt(d, "GachaIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.EquipGachaCeilingReward.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.EquipGachaCeilingReward.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GachaIndex", o.GachaIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
