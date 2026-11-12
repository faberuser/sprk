using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_PartyMemberSelectReward
{
	public static PartyMemberSelectReward Parse(string s)
	{
		return Parse(s, new PartyMemberSelectReward());
	}

	public static PartyMemberSelectReward Parse(string s, PartyMemberSelectReward o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PartyMemberSelectReward Parse(IDictionary d)
	{
		return Parse(d, new PartyMemberSelectReward());
	}

	public static PartyMemberSelectReward Parse(IDictionary d, PartyMemberSelectReward o)
	{
		o.RewardItemNo = BaseJsonMarshaler.ParseValueInt(d, "RewardItemNo");
		return o;
	}

	public static string EncodeJsonString(PartyMemberSelectReward o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PartyMemberSelectReward o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RewardItemNo", o.RewardItemNo.ToString());
		return dictionary;
	}
}
