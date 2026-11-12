using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_PartyBattleStart
{
	public static PartyBattleStart Parse(string s)
	{
		return Parse(s, new PartyBattleStart());
	}

	public static PartyBattleStart Parse(string s, PartyBattleStart o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PartyBattleStart Parse(IDictionary d)
	{
		return Parse(d, new PartyBattleStart());
	}

	public static PartyBattleStart Parse(IDictionary d, PartyBattleStart o)
	{
		o.BattleServerAddress = BaseJsonMarshaler.ParseValueString(d, "BattleServerAddress");
		o.BattleServerPort = BaseJsonMarshaler.ParseValueInt(d, "BattleServerPort");
		o.RewardItems = BaseJsonMarshaler.ParseArray(d, "RewardItems", JM_NShared_RewardItemInfo.Parse);
		o.MasterServerAddress = BaseJsonMarshaler.ParseValueString(d, "MasterServerAddress");
		o.MasterServerPort = BaseJsonMarshaler.ParseValueInt(d, "MasterServerPort");
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.RaidLevel = BaseJsonMarshaler.ParseValueInt(d, "RaidLevel");
		o.OnetimeBooster = BaseJsonMarshaler.ParseValueBool(d, "OnetimeBooster");
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		o.AccountIds = BaseJsonMarshaler.ParseArrayCast(d, "AccountIds", BaseJsonMarshaler.CastToLong);
		return o;
	}

	public static string EncodeJsonString(PartyBattleStart o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PartyBattleStart o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.BattleServerAddress != null)
		{
			dictionary.Add("BattleServerAddress", o.BattleServerAddress);
		}
		dictionary.Add("BattleServerPort", o.BattleServerPort.ToString());
		dictionary.Add("RewardItems", BaseJsonMarshaler.EncodeArray(o.RewardItems, JM_NShared_RewardItemInfo.EncodeJsonObject));
		if (o.MasterServerAddress != null)
		{
			dictionary.Add("MasterServerAddress", o.MasterServerAddress);
		}
		dictionary.Add("MasterServerPort", o.MasterServerPort.ToString());
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		dictionary.Add("RaidLevel", o.RaidLevel.ToString());
		dictionary.Add("OnetimeBooster", o.OnetimeBooster.ToString());
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		dictionary.Add("AccountIds", BaseJsonMarshaler.EncodeArray(o.AccountIds));
		return dictionary;
	}
}
