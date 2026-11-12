using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ReinforceTeamLevelBuff;

public static class JM_NShared_ReinforceTeamLevelBuff_Request
{
	public static NShared.ReinforceTeamLevelBuff.Request Parse(string s)
	{
		return Parse(s, new NShared.ReinforceTeamLevelBuff.Request());
	}

	public static NShared.ReinforceTeamLevelBuff.Request Parse(string s, NShared.ReinforceTeamLevelBuff.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ReinforceTeamLevelBuff.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ReinforceTeamLevelBuff.Request());
	}

	public static NShared.ReinforceTeamLevelBuff.Request Parse(IDictionary d, NShared.ReinforceTeamLevelBuff.Request o)
	{
		o.BuffTeamLevel = BaseJsonMarshaler.ParseValueInt(d, "BuffTeamLevel");
		o.OptionIndex = BaseJsonMarshaler.ParseValueInt(d, "OptionIndex");
		o.BonusOptionIndex = BaseJsonMarshaler.ParseValueInt(d, "BonusOptionIndex");
		o.Reinforce = BaseJsonMarshaler.ParseValueInt(d, "Reinforce");
		o.ReinforcePrice = BaseJsonMarshaler.ParseValueInt(d, "ReinforcePrice");
		o.ReinforcePriceType = BaseJsonMarshaler.ParseValueString(d, "ReinforcePriceType");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ReinforceTeamLevelBuff.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ReinforceTeamLevelBuff.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BuffTeamLevel", o.BuffTeamLevel.ToString());
		dictionary.Add("OptionIndex", o.OptionIndex.ToString());
		dictionary.Add("BonusOptionIndex", o.BonusOptionIndex.ToString());
		dictionary.Add("Reinforce", o.Reinforce.ToString());
		dictionary.Add("ReinforcePrice", o.ReinforcePrice.ToString());
		if (o.ReinforcePriceType != null)
		{
			dictionary.Add("ReinforcePriceType", o.ReinforcePriceType);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
